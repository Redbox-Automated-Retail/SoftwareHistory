using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Timers;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Script.Framework;

public sealed class ExecutionEngine : IStatusChangeObserver
{
	internal readonly string DataDirectory;

	private bool SchedulerEnabled;

	private bool ContextSwitchRequested;

	private Thread SchedulerThread;

	private bool PrintedRebootDiagnostic;

	private readonly ReaderWriterLockSlim ContextLock = new ReaderWriterLockSlim();

	private readonly ExecutionContextList Contexts = new ExecutionContextList();

	private readonly object ModeLock = new object();

	private readonly ManualResetEvent SchedulerResetEvent = new ManualResetEvent(initialState: false);

	private readonly ProgramCache ProgramCache = new ProgramCache();

	private readonly string[] NonSchedulableJobsWithinReboot = new string[7] { "qlm-unload", "thin", "unload-thin", "sync", "sync-locations", "thin-vmz", "clean-vmz" };

	private readonly string[] SingletonJobs = new string[1] { "clean-vmz" };

	private readonly RedboxTimer RebootWakeupTimer;

	private readonly TimeSpan? RebootWindowStart = null;

	private ExecutionContext m_immediateModeContext;

	private EngineModes m_currentMode = EngineModes.Normal;

	private static readonly object InstanceLock = new object();

	public static ExecutionEngine Instance { get; private set; }

	public ExecutionContext ActiveContext { get; private set; }

	internal ExecutionContext InitJob { get; set; }

	internal TimeSpan? KioskRebootTime { get; private set; }

	internal EngineModes Mode => m_currentMode;

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
			using (new WithUpgradeableReadLock(ContextLock))
			{
				if (!Contexts.Contains(m_immediateModeContext))
				{
					using (new WithWriteLock(ContextLock))
					{
						Contexts.Add(m_immediateModeContext);
					}
				}
			}
			return m_immediateModeContext;
		}
	}

	public static ExecutionEngine NewEngine(string scriptsPath, ErrorList errors)
	{
		lock (InstanceLock)
		{
			if (Instance == null)
			{
				Instance = new ExecutionEngine();
				Instance.Initialize(scriptsPath, errors);
				LogHelper.Instance.Log("Execution service version {0} initialized.", typeof(ExecutionEngine).Assembly.GetName().Version);
			}
			return Instance;
		}
	}

	public void StatusChange(ExecutionContextStatus status)
	{
		if (ExecutionContextStatus.Completed == status || ExecutionContextStatus.Errored == status)
		{
			PollerManager.Instance.RegisterPollers();
			CleanManager.Instance.Init();
		}
	}

	public static void Teardown()
	{
		lock (InstanceLock)
		{
			if (Instance != null)
			{
				if (Instance.InitJob != null)
				{
					Instance.InitJob.RemoveObserver(Instance);
				}
				Instance.Shutdown();
				Instance = null;
			}
		}
	}

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
		InitJob.AddObserver(this);
		InitJob.Resume();
	}

	public ExecutionResult Compile(string programSource, string programName)
	{
		return ProgramCache.CompileStringSource(programName, programSource);
	}

	public ExecutionResult Compile(Stream stream, string programName)
	{
		return ProgramCache.CompileInner(stream, programName);
	}

	public void CleanupJobs(bool force)
	{
		using (new WithWriteLock(ContextLock))
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
		using (new WithReadLock(ContextLock))
		{
			return GetJobInner(jobId);
		}
	}

	public ExecutionContext[] GetJobByLabel(string label)
	{
		List<ExecutionContext> list = new List<ExecutionContext>();
		using (new WithReadLock(ContextLock))
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

	public List<ExecutionContext> SafeGetJobList()
	{
		List<ExecutionContext> list = new List<ExecutionContext>();
		using (new WithReadLock(ContextLock))
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
		CompiledProgram program = ProgramCache.GetProgram(progName);
		if (program == null)
		{
			executionContext2.Result.Errors.Add(Error.NewError("E001", $"The program '{progName}' was not found in the program cache.", "Recompile the program which will place it in the program cache."));
			return executionContext2;
		}
		using (new WithUpgradeableReadLock(ContextLock))
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
			executionContext2.CopyProgramAndReferences(progName, program.ParseTree);
			using (new WithWriteLock(ContextLock))
			{
				Contexts.Add(executionContext2);
			}
		}
		executionContext2.Save();
		return executionContext2;
	}

	public ExecutionContext ExecuteImmediate()
	{
		ImmediateModeContext.Reset();
		CompiledProgram program = ProgramCache.GetProgram("$$$immediate$$$");
		if (program == null || program.CompilerResult.Errors.ContainsError())
		{
			ImmediateModeContext.Result.Errors.Add(Error.NewError("E001", string.Format("The program '{0}' was not found in the program cache.", "$$$immediate$$$"), "Recompile the program which will place it in the program cache."));
			return ImmediateModeContext;
		}
		ImmediateModeContext.CopyProgramAndReferences("$$$immediate$$$", program.ParseTree);
		ImmediateModeContext.Priority = ProgramPriority.Immediate;
		ImmediateModeContext.Resume();
		return ImmediateModeContext;
	}

	public ProgramInstruction GetProgram(string programName)
	{
		return ProgramCache.GetProgram(programName)?.ParseTree;
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
		ProgramCache.RemoveProgram(programName);
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
		List<string> list2 = ProgramCache.ProgramList();
		using (new DisposeableList<string>(list2))
		{
			foreach (string item in list2)
			{
				if (!item.StartsWith("$"))
				{
					list.Add($"{item}|{ProgramCache.GetProgram(item)}|{false}");
				}
			}
		}
		return list.ToArray();
	}

	public void ResumeAll()
	{
		using (new WithReadLock(ContextLock))
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
		using (new WithReadLock(ContextLock))
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

	public void ChangeMode(EngineModes mode)
	{
		lock (ModeLock)
		{
			m_currentMode = mode;
			LogHelper.Instance.Log("[Execution Engine]: Change mode to {0}", m_currentMode.ToString());
			PollerManager.Instance.OnEngineModeChange(m_currentMode);
		}
	}

	public string GetActiveContextProgramName()
	{
		if (ActiveContext == null || ActiveContext.Registers.ActiveFrame == null)
		{
			return "Idle";
		}
		return ActiveContext.Registers.ActiveFrame.ProgramName;
	}

	internal NativeJobAdapter MakeJobFromOperand(ExecutionContext ctx, ExecutionResult result, string key)
	{
		return ProgramCache.MakeJobFromOperand(ctx, result, key);
	}

	internal bool OutsideRebootWindow()
	{
		return CanSchedule();
	}

	internal int ContextCount()
	{
		using (new WithReadLock(ContextLock))
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
		RebootWakeupTimer = new RedboxTimer("Reboot Wakeup", RestartJobs);
		DataDirectory = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "data");
		int rebootExecutionPreventionWindow = ControllerConfiguration.Instance.RebootExecutionPreventionWindow;
		IPersistentMap dataStore = StoreManager.Instance.GetDataStore(PersistentMapTypes.Engine);
		if (rebootExecutionPreventionWindow <= 0)
		{
			return;
		}
		string value = dataStore.GetValue("RebootTime", string.Empty);
		if (!string.IsNullOrEmpty(value))
		{
			try
			{
				KioskRebootTime = TimeSpan.Parse(value);
				RebootWindowStart = KioskRebootTime.Value.Subtract(new TimeSpan(0, rebootExecutionPreventionWindow, 0));
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Parsing kiosk reboot time failed.", e);
				KioskRebootTime = (RebootWindowStart = null);
			}
		}
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
		using (new WithReadLock(ContextLock))
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
		if (!KioskRebootTime.HasValue || !RebootWindowStart.HasValue)
		{
			return true;
		}
		TimeSpan timeOfDay = DateTime.Now.TimeOfDay;
		if (timeOfDay >= RebootWindowStart.Value && timeOfDay <= KioskRebootTime.Value)
		{
			if (!RebootWakeupTimer.Started)
			{
				TimeSpan span = KioskRebootTime.Value.Add(new TimeSpan(0, 5, 0));
				RebootWakeupTimer.ScheduleAtNext(span);
			}
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
		using (new WithReadLock(ContextLock))
		{
			foreach (ExecutionContext context in Contexts)
			{
				if (context.ProgramName.Equals(name))
				{
					list.Add(context);
				}
			}
			return list.ToArray();
		}
	}

	private void ConfigurationUpdateStart()
	{
		LogHelper.Instance.Log("ExecutionEngine::ConfigurationUpdateStart()");
		PollerManager.Instance.ConfigurationUpdateStart();
	}

	private void ConfigurationUpdateFinish()
	{
		LogHelper.Instance.Log("ExecutionEngine::ConfigurationUpdateFinish()");
		PollerManager.Instance.ConfigurationUpdateFinish();
	}

	private string GetUniqueId()
	{
		using (new WithReadLock(ContextLock))
		{
			for (int i = 0; i < 8; i++)
			{
				string text = IdHelper.GeneratePseudoUniqueId(8);
				if (text != ScriptConstants.ExecutionContexts.ImmediateModeContext && Contexts.GetByID(text) == null)
				{
					return text;
				}
			}
		}
		throw new ApplicationException("Unable to generate a unique ID for ExecutionContext.");
	}

	private void RestartJobs(object source, ElapsedEventArgs e)
	{
		RebootWakeupTimer.Disable();
		RestartYoungest(1);
	}

	private void RestartYoungest(int delay)
	{
		string[] nonSchedulableJobsWithinReboot = NonSchedulableJobsWithinReboot;
		foreach (string name in nonSchedulableJobsWithinReboot)
		{
			ExecutionContext[] byName = GetByName(name);
			if (byName.Length == 0)
			{
				continue;
			}
			Array.Sort(byName, (ExecutionContext left, ExecutionContext right) => left.CreatedOn.CompareTo(right.CreatedOn));
			ExecutionContext executionContext = null;
			for (int num = byName.Length - 1; num >= 0; num--)
			{
				executionContext = byName[num];
				if (!executionContext.IsComplete)
				{
					_ = DateTime.Now;
					if (!executionContext.StartTime.HasValue || executionContext.StartTime.Value < DateTime.Now.AddMinutes(delay))
					{
						executionContext.StartTime = DateTime.Now.AddMinutes(delay);
					}
					LogHelper.Instance.Log($"[Execution Engine] Restarting job {executionContext.ProgramName} ( ID = {executionContext.ID} )");
					executionContext.Status.NextValue = ExecutionContextStatus.Pending;
					executionContext.Status.Promote();
					break;
				}
			}
		}
	}

	private void LoadJobs()
	{
		using (new WithWriteLock(ContextLock))
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
						if (executionContext.Status.Value == ExecutionContextStatus.Suspended && (executionContext.ProgramName.Equals("init") || executionContext.ProgramName.Equals("return") || executionContext.ProgramName.Equals("vend") || executionContext.ProgramName == "soft-sync"))
						{
							LogHelper.Instance.Log("Job {0} ( {1} ) is in pending status at initialization; mark it garbage.", executionContext.ID, executionContext.ProgramName);
							executionContext.Status.NextValue = ExecutionContextStatus.Garbage;
							executionContext.Status.Promote();
						}
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
		}
	}

	private void Initialize(string scriptsPath, ErrorList errors)
	{
		ControllerConfiguration.Instance.ConfigurationChangeStart += ConfigurationUpdateStart;
		ControllerConfiguration.Instance.ConfigurationChangeEnd += ConfigurationUpdateFinish;
		ProgramCache.CompilePrograms(scriptsPath, errors);
		LoadJobs();
		RestartYoungest(5);
		CleanupJobs(force: false);
		SchedulerThread = new Thread(ProcessScheduler)
		{
			Name = "HAL Script Scheduler",
			IsBackground = true
		};
		SchedulerThread.SetApartmentState(ApartmentState.STA);
		SchedulerThread.Start();
		SchedulerEnabled = true;
	}

	private void Shutdown()
	{
		Compile("CAMERA STOP FORCE=TRUE", "$$$immediate$$$");
		ExecutionContext executionContext = ExecuteImmediate();
		executionContext.WaitForCompletion();
		PollerManager.Instance.Shutdown();
		ControllerConfiguration.Instance.ConfigurationChangeStart -= ConfigurationUpdateStart;
		ControllerConfiguration.Instance.ConfigurationChangeEnd -= ConfigurationUpdateFinish;
		SchedulerEnabled = false;
		SuspendAll(force: true);
		while (ActiveContext != null)
		{
			Thread.Sleep(0);
		}
		LogHelper.Instance.Log("ExecutionEngine.Shutdown() complete.");
	}

	private ExecutionContext GetJobInner(string id)
	{
		foreach (ExecutionContext context in Contexts)
		{
			if (context.ID == id)
			{
				return context;
			}
		}
		return null;
	}
}
