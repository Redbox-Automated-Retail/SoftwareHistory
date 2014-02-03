using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

public class ExecutionEngine
{
	private class TimeComparer : IComparer<ExecutionContext>
	{
		public int Compare(ExecutionContext left, ExecutionContext right)
		{
			return left.CreatedOn.CompareTo(right.CreatedOn);
		}
	}

	private readonly object ContextLock = new object();

	private TimeSpan? m_rebootWindowStart = null;

	private TimeSpan? m_rebootTime = null;

	private bool m_checkedRebootTime;

	private string m_dataDirectory;

	private ExecutionContextList m_contexts;

	private ExecutionContext m_immediateModeContext;

	private readonly string[] NonSchedulableJobsWithinReboot = new string[6] { "qlm-unload", "thin", "unload-thin", "sync", "thin-vmz", "clean-vmz" };

	private readonly string[] SingletonJobs = new string[1] { "clean-vmz" };

	private IDictionary<string, CompiledProgram> m_programCache;

	public static ExecutionEngine Instance => Singleton<ExecutionEngine>.Instance;

	public ExecutionContext ActiveContext { get; set; }

	public ILogger Logger { get; set; }

	internal ExecutionContext InitJob { get; set; }

	internal string DataDirectory
	{
		get
		{
			if (string.IsNullOrEmpty(m_dataDirectory))
			{
				m_dataDirectory = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "data");
			}
			return m_dataDirectory;
		}
	}

	internal TimeSpan? KioskRebootTime
	{
		get
		{
			if (m_checkedRebootTime)
			{
				return m_rebootTime;
			}
			m_checkedRebootTime = true;
			int rebootExecutionPreventionWindow = ControllerConfiguration.Instance.RebootExecutionPreventionWindow;
			if (rebootExecutionPreventionWindow <= 0)
			{
				m_rebootTime = null;
				m_rebootWindowStart = null;
				return null;
			}
			string secretValue = RegistryOptions.Instance.GetSecretValue<string>("SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store", "RebootTime");
			if (!string.IsNullOrEmpty(secretValue))
			{
				try
				{
					m_rebootTime = TimeSpan.Parse(secretValue);
					m_rebootWindowStart = m_rebootTime.Value.Subtract(new TimeSpan(0, rebootExecutionPreventionWindow, 0));
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("Parsing kiosk reboot time failed.", e);
					m_rebootTime = null;
				}
			}
			return m_rebootTime;
		}
	}

	private IDictionary<string, CompiledProgram> ProgramCache
	{
		get
		{
			if (m_programCache == null)
			{
				m_programCache = new Dictionary<string, CompiledProgram>();
			}
			return m_programCache;
		}
	}

	private ExecutionContext ImmediateModeContext
	{
		get
		{
			if (m_immediateModeContext == null)
			{
				m_immediateModeContext = new ExecutionContext(ScriptConstants.ExecutionContexts.ImmediateModeContext, "$$$immediate$$$")
				{
					Priority = ProgramPriority.Immediate,
					Label = "HAL Script Immediate Mode"
				};
			}
			lock (ContextLock)
			{
				if (!Contexts.Contains(m_immediateModeContext))
				{
					Contexts.Add(m_immediateModeContext);
				}
			}
			return m_immediateModeContext;
		}
	}

	private ExecutionContextList Contexts
	{
		get
		{
			if (m_contexts == null)
			{
				m_contexts = new ExecutionContextList();
			}
			return m_contexts;
		}
	}

	private ManualResetEvent SchedulerResetEvent { get; set; }

	private bool ContextSwitchRequested { get; set; }

	private Thread SchedulerThread { get; set; }

	private bool SchedulerEnabled { get; set; }

	private TimeSpan RebootWindowStart => m_rebootWindowStart.Value;

	private bool PrintedRebootDiagnostic { get; set; }

	public string GetInitJobStatus()
	{
		if (InitJob == null)
		{
			return ExecutionContextStatus.Pending.ToString().ToUpper();
		}
		if (!InitJob.IsComplete)
		{
			return ExecutionContextStatus.Running.ToString().ToUpper();
		}
		return InitJob.Status.ToString().ToUpper();
	}

	public void ExecuteStartupInit()
	{
		LogHelper.Instance.Log("Schedule init job", LogEntryType.Info);
		InitJob = ScheduleJob("init", "HAL Service Startup: Init", DateTime.Now, ProgramPriority.High);
		InitJob.StatusChangeCallback = HandleStartupInitStatusChange;
		InitJob.Resume();
	}

	public void Initialize()
	{
		ControllerConfiguration.Instance.ConfigurationChangeStart += ConfigurationUpdateStart;
		ControllerConfiguration.Instance.ConfigurationChangeEnd += ConfigurationUpdateFinish;
		LoadJobs();
		CleanupJobs(force: false);
		SchedulerResetEvent = new ManualResetEvent(initialState: false);
		SchedulerThread = new Thread(ProcessScheduler)
		{
			Name = "HAL Script Scheduler",
			IsBackground = true
		};
		SchedulerThread.SetApartmentState(ApartmentState.STA);
		SchedulerThread.Start();
		SchedulerEnabled = true;
	}

	public void Shutdown()
	{
		Instance.Compile("CAMERA STOP FORCE=TRUE", "$$$immediate$$$");
		ExecutionContext executionContext = ExecuteImmediate();
		executionContext.WaitForCompletion();
		QuickReturnPoller.Instance.Shutdown();
		MotionControlSafetyPoller.Instance.Shutdown(joinThread: false);
		WatchdogPoller.Instance.Shutdown(joinThread: false);
		TSMonitor.Instance.Stop();
		SchedulerEnabled = false;
		SuspendAll(force: true);
		while (ActiveContext != null)
		{
			Thread.Sleep(0);
		}
		LogHelper.Instance.Log("ExecutionEngine.Shutdown() complete.");
	}

	public ExecutionResult Compile(string programSource, string programName)
	{
		return CompileStringSource(programName, programSource);
	}

	public ExecutionResult Compile(Stream stream, string programName)
	{
		return CompileInner(stream, programName);
	}

	public void LoadJobs()
	{
		lock (ContextLock)
		{
			string path = Path.Combine(DataDirectory, "contexts");
			try
			{
				if (!Directory.Exists(path))
				{
					LogHelper.Instance.Log("Contexts directory doesn't exist; creating.", LogEntryType.Debug);
					Directory.CreateDirectory(path);
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Unable to create contexts directory.", e);
			}
			string[] directories = Directory.GetDirectories(path);
			string[] array = directories;
			foreach (string text in array)
			{
				string path2 = Path.Combine(text, "ctx.data");
				ExecutionContext executionContext = null;
				try
				{
					using (FileStream serializationStream = File.OpenRead(path2))
					{
						BinaryFormatter binaryFormatter = new BinaryFormatter();
						executionContext = (ExecutionContext)binaryFormatter.Deserialize(serializationStream);
					}
					if (executionContext != null)
					{
						Contexts.Add(executionContext);
					}
				}
				catch
				{
					LogHelper.Instance.Log("Unable to load context {0}; deleting.", text);
					try
					{
						Directory.Delete(text, recursive: true);
					}
					catch (Exception e2)
					{
						LogHelper.Instance.Log(" >> Can't delete: ", e2);
					}
				}
			}
			foreach (ExecutionContext context in Contexts)
			{
				if (context.Status.Value == ExecutionContextStatus.Suspended)
				{
					if (context.StartTime.HasValue)
					{
						int num = DateTime.Compare(context.StartTime.Value, DateTime.Now);
						if (num >= 0)
						{
							if (context.StartTime.Value < DateTime.Now.AddMinutes(5.0))
							{
								context.StartTime = DateTime.Now.AddMinutes(5.0);
							}
							context.Status.NextValue = ExecutionContextStatus.Pending;
							context.Status.Promote();
							LogHelper.Instance.Log("Promoting job {0} ({1}) to PENDING.", context.ID, context.ProgramName);
						}
					}
					else if (context.ProgramName.Equals("init") || context.ProgramName.Equals("return") || context.ProgramName.Equals("vend") || context.ProgramName == "soft-sync")
					{
						context.Status.NextValue = ExecutionContextStatus.Garbage;
						context.Status.Promote();
					}
				}
				else if (context.Status.Value == ExecutionContextStatus.Pending && context.ProgramName == "init")
				{
					LogHelper.Instance.Log("Job {0} ( {1} ) is in pending status at initialization; mark it garbage.", context.ID, context.ProgramName);
					context.Status.NextValue = ExecutionContextStatus.Garbage;
					context.Status.Promote();
				}
			}
			if (!ContainsExecuting("qlm-unload"))
			{
				FindYoungestJobAndPend("qlm-unload", 5);
			}
			if (!ContainsExecuting("thin"))
			{
				FindYoungestJobAndPend("thin", 5);
			}
			if (!ContainsExecuting("thin") && !ContainsExecuting("qlm-unload") && !ContainsExecuting("unload-thin"))
			{
				FindYoungestJobAndPend("unload-thin", 5);
			}
			if (!ContainsExecuting("sync"))
			{
				FindYoungestJobAndPend("sync", 5);
			}
			if (!ContainsExecuting("thin-vmz"))
			{
				FindYoungestJobAndPend("thin-vmz", 5);
			}
			if (!ContainsExecuting("clean-vmz"))
			{
				FindYoungestJobAndPend("clean-vmz", 5);
			}
		}
	}

	public void CleanupJobs(bool force)
	{
		lock (ContextLock)
		{
			IEnumerable<ExecutionContext> enumerable = Contexts.EnumerateGarbage(force);
			foreach (ExecutionContext item in enumerable)
			{
				if ((InitJob == null || !(item.ID == InitJob.ID)) && !item.IsImmediate)
				{
					item.Delete();
					Contexts.Remove(item);
				}
			}
		}
	}

	public ExecutionContext GetJob(string jobId)
	{
		lock (ContextLock)
		{
			foreach (ExecutionContext context in Contexts)
			{
				if (context.ID == jobId)
				{
					return context;
				}
			}
			return null;
		}
	}

	public ExecutionContext[] GetJobByLabel(string label)
	{
		List<ExecutionContext> list = new List<ExecutionContext>();
		lock (ContextLock)
		{
			foreach (ExecutionContext context in Contexts)
			{
				if (string.Compare(context.Label, label, ignoreCase: true) == 0)
				{
					list.Add(context);
				}
			}
			return list.ToArray();
		}
	}

	public bool RemoveJob(string jobId)
	{
		ExecutionContext job = GetJob(jobId);
		lock (ContextLock)
		{
			return job != null && Contexts.Remove(job);
		}
	}

	public List<ExecutionContext> SafeGetJobList()
	{
		List<ExecutionContext> list = new List<ExecutionContext>();
		lock (ContextLock)
		{
			foreach (ExecutionContext context in Contexts)
			{
				if (!(context.ID == InitJob.ID))
				{
					list.Add(context);
				}
			}
			return list;
		}
	}

	public ExecutionContext ScheduleJob(string progName, string label, DateTime? startTime, ProgramPriority priority)
	{
		ExecutionContext executionContext = new ExecutionContext(GetUniqueId(), progName);
		executionContext.StartTime = startTime;
		executionContext.Priority = priority;
		executionContext.Label = label;
		ExecutionContext executionContext2 = executionContext;
		if (!ProgramCache.ContainsKey(progName))
		{
			executionContext2.Result.Errors.Add(Error.NewError("E001", $"The program '{progName}' was not found in the program cache.", "Recompile the program which will place it in the program cache."));
			return executionContext2;
		}
		lock (ContextLock)
		{
			if (IsSingletonJob(progName))
			{
				foreach (ExecutionContext context in Contexts)
				{
					if (context.ProgramName.Equals(progName, StringComparison.CurrentCultureIgnoreCase) && !context.IsComplete)
					{
						string text = $"The program '{progName}' is marked singleton, instance currently running - deny new job.";
						LogHelper.Instance.Log(text);
						executionContext2.Result.Errors.Add(Error.NewError("E001", text, "Reschedule the program ."));
						return executionContext2;
					}
				}
			}
			CompiledProgram compiledProgram = ProgramCache[progName];
			executionContext2.CopyProgramAndReferences(progName, compiledProgram.ParseTree);
			Contexts.Add(executionContext2);
		}
		executionContext2.Save();
		return executionContext2;
	}

	public ExecutionContext ExecuteImmediate()
	{
		ImmediateModeContext.Reset();
		if (!ProgramCache.ContainsKey("$$$immediate$$$") || ProgramCache["$$$immediate$$$"].CompilerResult.Errors.ContainsError())
		{
			ImmediateModeContext.Result.Errors.Add(Error.NewError("E001", string.Format("The program '{0}' was not found in the program cache.", "$$$immediate$$$"), "Recompile the program which will place it in the program cache."));
			return ImmediateModeContext;
		}
		ImmediateModeContext.CopyProgramAndReferences("$$$immediate$$$", ProgramCache["$$$immediate$$$"].ParseTree);
		ImmediateModeContext.Priority = ProgramPriority.Immediate;
		ImmediateModeContext.Resume();
		return ImmediateModeContext;
	}

	public IEnumerable<ExecutionContext> EnumerateContexts()
	{
		List<ExecutionContext> contexts = new List<ExecutionContext>(Contexts);
		contexts.Sort((ExecutionContext x, ExecutionContext y) => x.Priority.CompareTo(y.Priority));
		foreach (ExecutionContext item in contexts)
		{
			yield return item;
		}
	}

	public ProgramInstruction GetProgram(string programName)
	{
		if (!ProgramCache.ContainsKey(programName))
		{
			return null;
		}
		return ProgramCache[programName].ParseTree;
	}

	public void PerformContextSwitch(bool runtimeOnly)
	{
		ActiveContext.PromoteDeferredStatusChange();
		if (!runtimeOnly)
		{
			ActiveContext.Save();
			ContextSwitchRequested = true;
			SchedulerResetEvent.Set();
		}
	}

	public void RemoveProgram(string programName)
	{
		ProgramCache.Remove(programName);
	}

	public bool IsImmediateJobPending()
	{
		if (!(ImmediateModeContext.Status == ExecutionContextStatus.Pending))
		{
			return ImmediateModeContext.Status == ExecutionContextStatus.Running;
		}
		return true;
	}

	public string[] GetPrograms()
	{
		List<string> list = new List<string>();
		foreach (string key in ProgramCache.Keys)
		{
			if (!key.StartsWith("$"))
			{
				list.Add($"{key}|{ProgramCache[key]}|{false}");
			}
		}
		return list.ToArray();
	}

	public void ResumeAll()
	{
		lock (ContextLock)
		{
			foreach (ExecutionContext context in Contexts)
			{
				if (!context.IsComplete)
				{
					context.Resume();
				}
			}
		}
	}

	public void SuspendAll(bool force)
	{
		lock (ContextLock)
		{
			foreach (ExecutionContext context in Contexts)
			{
				if (context.IsComplete)
				{
					continue;
				}
				context.Suspend();
				if (force)
				{
					if (context.Status == ExecutionContextStatus.Running)
					{
						context.PromoteDeferredStatusChange();
					}
					else
					{
						context.Save();
					}
				}
			}
		}
	}

	public void SuspendDevices()
	{
		QuickReturnPoller.Instance.Shutdown();
	}

	public void RestartDevices()
	{
		QuickReturnPoller.Instance.Startup();
	}

	public string GetActiveContextProgramName()
	{
		if (ActiveContext == null || ActiveContext.Registers.ActiveFrame == null)
		{
			return "Idle";
		}
		return ActiveContext.Registers.ActiveFrame.ProgramName;
	}

	public ErrorList CompileBaseScripts(string basePath)
	{
		ErrorList errorList = new ErrorList();
		Dictionary<string, string> nativeJobs = NativeJobFactory.Instance.NativeJobs;
		foreach (string key in nativeJobs.Keys)
		{
			string text = key;
			string programSource = nativeJobs[key];
			if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
			{
				LogHelper.Instance.Log($"Compile native program {text}", LogEntryType.Info);
			}
			ExecutionResult executionResult = CompileStringSource(text, programSource);
			errorList.AddRange(executionResult.Errors);
			if (!executionResult.Errors.ContainsError())
			{
				continue;
			}
			LogHelper.Instance.Log($"There were errors compiling native script {text}", LogEntryType.Error);
			foreach (Error error in executionResult.Errors)
			{
				LogHelper.Instance.Log(error.ToString(), LogEntryType.Error);
			}
		}
		string[] files = Directory.GetFiles(basePath, "*.hs");
		string[] array = files;
		foreach (string text2 in array)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text2);
			if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
			{
				LogHelper.Instance.Log($"Compile program {fileNameWithoutExtension} from file {text2}", LogEntryType.Info);
			}
			using MemoryStream stream = new MemoryStream(FileHelper.ReadToBuffer(text2));
			ExecutionResult executionResult2 = CompileInner(stream, fileNameWithoutExtension);
			errorList.AddRange(executionResult2.Errors);
			if (!executionResult2.Errors.ContainsError())
			{
				continue;
			}
			LogHelper.Instance.Log($"There were errors compiling script {fileNameWithoutExtension}", LogEntryType.Error);
			foreach (Error error2 in executionResult2.Errors)
			{
				LogHelper.Instance.Log(error2.ToString(), LogEntryType.Error);
			}
		}
		return errorList;
	}

	internal int ContextCount()
	{
		lock (ContextLock)
		{
			return Contexts.Count;
		}
	}

	internal void ProcessScheduler()
	{
		while (true)
		{
			Thread.Sleep(0);
			if (!CanSchedule())
			{
				SuspendCritical(force: true);
			}
			if (SchedulerEnabled)
			{
				if (ContextSwitchRequested || ActiveContext == null)
				{
					ActiveContext = Contexts.GetEligibleExecutionContext();
					ContextSwitchRequested = false;
					SchedulerResetEvent.Reset();
				}
				if (ActiveContext != null && ActiveContext.Status != ExecutionContextStatus.Running)
				{
					ActiveContext.Execute();
					ActiveContext = null;
				}
			}
			SchedulerResetEvent.WaitOne(250, exitContext: false);
		}
	}

	private ExecutionEngine()
	{
	}

	private bool IsSingletonJob(string name)
	{
		string[] singletonJobs = SingletonJobs;
		foreach (string text in singletonJobs)
		{
			if (text.Equals(name, StringComparison.CurrentCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	private void SuspendCritical(bool force)
	{
		lock (ContextLock)
		{
			foreach (ExecutionContext context in Contexts)
			{
				if (context.IsComplete || context.Status == ExecutionContextStatus.Suspended)
				{
					continue;
				}
				string[] nonSchedulableJobsWithinReboot = NonSchedulableJobsWithinReboot;
				foreach (string text in nonSchedulableJobsWithinReboot)
				{
					if (!(text == context.ProgramName))
					{
						continue;
					}
					context.Suspend();
					if (force)
					{
						if (context.Status == ExecutionContextStatus.Running)
						{
							context.PromoteDeferredStatusChange();
						}
						else
						{
							context.Save();
						}
					}
				}
			}
		}
	}

	private bool CanSchedule()
	{
		if (!KioskRebootTime.HasValue)
		{
			return true;
		}
		TimeSpan timeOfDay = DateTime.Now.TimeOfDay;
		if (timeOfDay >= RebootWindowStart && timeOfDay <= KioskRebootTime.Value)
		{
			if (!PrintedRebootDiagnostic)
			{
				LogHelper.Instance.Log($"Time {FormatTime(timeOfDay)} is within 10 minute reboot window of {FormatTime(KioskRebootTime.Value)}; disabling scheduler.", LogEntryType.Info);
				PrintedRebootDiagnostic = true;
			}
			return false;
		}
		return true;
	}

	private string FormatTime(TimeSpan t)
	{
		return $"{DateTime.Now.ToShortDateString()} {t.ToString()}";
	}

	private ExecutionContext[] GetByName(string name)
	{
		List<ExecutionContext> list = new List<ExecutionContext>();
		foreach (ExecutionContext context in Contexts)
		{
			if (context.ProgramName.Equals(name))
			{
				list.Add(context);
			}
		}
		return list.ToArray();
	}

	private void ConfigurationUpdateStart()
	{
		LogHelper.Instance.Log("ExecutionEngine::ConfigurationUpdateStart()");
		IceQubeBoardPoller.Instance.Shutdown(joinThread: false);
		QuickReturnPoller.Instance.Shutdown();
		TSMonitor.Instance.Stop();
	}

	private void ConfigurationUpdateFinish()
	{
		LogHelper.Instance.Log("ExecutionEngine::ConfigurationUpdateFinish()");
		IceQubeBoardPoller.Instance.Start();
		QuickReturnDevice.Instance.ConfigurationChange();
		QuickReturnPoller.Instance.Startup();
		TSMonitor.Instance.Start();
	}

	private bool ContainsExecuting(string name)
	{
		ExecutionContext[] byName = GetByName(name);
		if (byName.Length == 0)
		{
			return false;
		}
		ExecutionContext[] array = byName;
		foreach (ExecutionContext executionContext in array)
		{
			if (executionContext.Status.Value == ExecutionContextStatus.Pending || executionContext.Status.Value == ExecutionContextStatus.Running)
			{
				return true;
			}
		}
		return false;
	}

	private void FindYoungestJobAndPend(string jobName, int minutesInFuture)
	{
		ExecutionContext[] byName = GetByName(jobName);
		if (byName.Length == 0)
		{
			return;
		}
		Array.Sort(byName, new TimeComparer());
		ExecutionContext executionContext = null;
		int num = byName.Length - 1;
		while (num >= 0)
		{
			executionContext = byName[num];
			if (executionContext.IsComplete)
			{
				executionContext = null;
				num--;
				continue;
			}
			if (executionContext.Status.Value != ExecutionContextStatus.Pending && executionContext.Status.Value != ExecutionContextStatus.Running)
			{
				break;
			}
			return;
		}
		if (executionContext == null || executionContext.IsComplete)
		{
			return;
		}
		_ = DateTime.Now;
		if (executionContext.StartTime.HasValue)
		{
			if (executionContext.StartTime.Value < DateTime.Now.AddMinutes(minutesInFuture))
			{
				executionContext.StartTime = DateTime.Now.AddMinutes(minutesInFuture);
			}
		}
		else
		{
			executionContext.StartTime = DateTime.Now.AddMinutes(minutesInFuture);
		}
		LogHelper.Instance.Log($"Pending youngest job (name {executionContext.ProgramName}, ID={executionContext.ID})", LogEntryType.Debug);
		executionContext.Status.NextValue = ExecutionContextStatus.Pending;
		executionContext.Status.Promote();
	}

	private string GetUniqueId()
	{
		for (int i = 0; i < 8; i++)
		{
			string text = IdHelper.GeneratePseudoUniqueId(8);
			if (text != ScriptConstants.ExecutionContexts.ImmediateModeContext && Contexts.GetByID(text) == null)
			{
				return text;
			}
		}
		throw new ExecutionEngineException("Unable to generate a unique ID for ExecutionContext.");
	}

	private ExecutionResult CompileStringSource(string programName, string programSource)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using StreamWriter streamWriter = new StreamWriter(memoryStream);
		streamWriter.WriteLine(programSource);
		streamWriter.Flush();
		memoryStream.Seek(0L, SeekOrigin.Begin);
		return CompileInner(memoryStream, programName);
	}

	private ExecutionResult CompileInner(Stream stream, string programName)
	{
		if (!ProgramCache.ContainsKey(programName))
		{
			return CompileProgramInner(stream, programName);
		}
		if (programName.Equals("$$$immediate$$$", StringComparison.CurrentCultureIgnoreCase))
		{
			return CompileProgramInner(stream, programName);
		}
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Normal && ControllerConfiguration.Instance.AllowProgramCacheReplacement2)
		{
			LogHelper.Instance.Log($"Allow replacement of program {programName} - allow cache replacement2 is true.", LogEntryType.Info);
			return CompileProgramInner(stream, programName);
		}
		if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
		{
			LogHelper.Instance.Log($"Not allowing replacement of program {programName}", LogEntryType.Info);
		}
		ExecutionResult executionResult = new ExecutionResult();
		executionResult.ExecutionTime = new TimeSpan(0L);
		return executionResult;
	}

	private ExecutionResult CompileProgramInner(Stream stream, string programName)
	{
		ExecutionResult executionResult = new Compiler(stream, programName).Compile(out var compiledProgram);
		ProgramCache[programName] = new CompiledProgram
		{
			ParseTree = compiledProgram,
			CompilerResult = executionResult
		};
		return executionResult;
	}

	private void HandleStartupInitStatusChange(ExecutionContextStatus status)
	{
		if (ExecutionContextStatus.Completed == status || ExecutionContextStatus.Errored == status)
		{
			MotionControlSafetyPoller.Instance.Start();
			WatchdogPoller.Instance.Start();
			CleanManager.Instance.Init();
			TSMonitor.Instance.Start();
			if (ExecutionContextStatus.Completed == status)
			{
				QuickReturnPoller.Instance.Startup();
				IceQubeBoardPoller.Instance.Start();
			}
		}
	}
}
