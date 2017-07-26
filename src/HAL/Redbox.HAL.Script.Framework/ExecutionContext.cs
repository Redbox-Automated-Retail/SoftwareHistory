using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class ExecutionContext : IMessageSink, ISerializable, IExecutionContext
{
	public readonly ExecutionStatus Status = new ExecutionStatus();

	public readonly IDictionary<string, object> SymbolTable = new Dictionary<string, object>();

	internal readonly Registers Registers;

	internal readonly ApplicationLog AppLog;

	internal readonly IDictionary<string, ProgramInstruction> ProgramCache = new Dictionary<string, ProgramInstruction>();

	internal readonly Deque<ScriptMessage> Messages = new Deque<ScriptMessage>();

	internal readonly ReaderWriterLockSlim ResultsLock = new ReaderWriterLockSlim();

	private DateTime? LastEventTime;

	private bool IsCooperative;

	private ProgramPriority m_priority;

	private ConnectionState m_connectionState;

	private string m_contextDirectory;

	private SignalHandler Handler;

	private readonly EventWaitHandle CompletionWaitHandle = new EventWaitHandle(initialState: false, EventResetMode.AutoReset);

	private readonly ReaderWriterLockSlim SignalsLock = new ReaderWriterLockSlim();

	private readonly List<string> CachedSignals = new List<string>();

	private readonly object Lock = new object();

	private readonly Deque<object> DataStack = new Deque<object>();

	private readonly List<string> CachedResults = new List<string>();

	private readonly List<IStatusChangeObserver> Observers = new List<IStatusChangeObserver>();

	private readonly ReaderWriterLockSlim ObserverLock = new ReaderWriterLockSlim();

	private const string ResultPrefix = "RESULT";

	private const string EmptyItemToken = "~";

	public string ID { get; private set; }

	public string ParentID { get; set; }

	public string Label { get; set; }

	public int StackDepth => DataStack.Count;

	public int ResultCount => CachedResults.Count;

	public bool IsComplete
	{
		get
		{
			if (!(Status == ExecutionContextStatus.Errored) && !(Status == ExecutionContextStatus.Completed) && !(Status == ExecutionContextStatus.Garbage))
			{
				return Status == ExecutionContextStatus.Stopped;
			}
			return true;
		}
	}

	public IFormattedLog ContextLog => AppLog;

	public bool IsImmediate { get; internal set; }

	public DateTime? ExecutionStart { get; private set; }

	public DateTime CreatedOn { get; private set; }

	public string ProgramName { get; private set; }

	public bool TraceExecution { get; set; }

	public DateTime? StartTime { get; set; }

	public bool EnableDebugging { get; set; }

	public ExecutionResult Result { get; private set; }

	public ProgramPriority Priority
	{
		get
		{
			if (!IsComplete)
			{
				return m_priority;
			}
			return ProgramPriority.Lowest;
		}
		set
		{
			m_priority = value;
		}
	}

	public IMessageSink MessageSink { get; set; }

	public bool EligibleForScheduling => Status == ExecutionContextStatus.Pending;

	public ConnectionState ConnectionState
	{
		get
		{
			return m_connectionState;
		}
		set
		{
			m_connectionState = value;
			if (m_connectionState == ConnectionState.Connected)
			{
				FlushMessages();
			}
		}
	}

	internal ExecutionContextStatus? DeferredStatusChangeRequested { get; set; }

	internal string ContextDirectory
	{
		get
		{
			if (string.IsNullOrEmpty(m_contextDirectory))
			{
				string path = Path.Combine(Path.GetDirectoryName(typeof(ExecutionContext).Assembly.Location), "data\\contexts");
				m_contextDirectory = Path.Combine(path, ID);
			}
			return m_contextDirectory;
		}
	}

	internal bool IsAutoScheduled
	{
		get
		{
			if (!SymbolTable.ContainsKey("_EE_AUTOSCHEDULED"))
			{
				return SymbolTable.ContainsKey("_CHECK_RUNTIME");
			}
			return true;
		}
		set
		{
			if (!SymbolTable.ContainsKey("_EE_AUTOSCHEDULED"))
			{
				SymbolTable.Add("_EE_AUTOSCHEDULED", true);
			}
		}
	}

	public void Save()
	{
		lock (Lock)
		{
			try
			{
				if (IsImmediate)
				{
					return;
				}
				try
				{
					if (!Directory.Exists(ContextDirectory))
					{
						Directory.CreateDirectory(ContextDirectory);
					}
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("Unable to create context directory.", e);
					return;
				}
				try
				{
					using FileStream serializationStream = File.OpenWrite(Path.Combine(ContextDirectory, "ctx.data"));
					new BinaryFormatter().Serialize(serializationStream, this);
				}
				catch (Exception e2)
				{
					LogHelper.Instance.Log("Unable to serialize context.", e2);
					LogHelper.Instance.Log("", e2);
				}
			}
			catch (Exception e3)
			{
				LogHelper.Instance.Log("ExecutionContext.Save threw an unhandled exception.", e3);
			}
		}
	}

	public ProgramInstruction GetProgram(string programName)
	{
		if (ProgramCache.ContainsKey(programName))
		{
			return ProgramCache[programName];
		}
		return null;
	}

	public void CopyProgramAndReferences(string programName, ProgramInstruction programInstruction)
	{
		ProgramName = programName;
		IsCooperative = programInstruction.IsCooperative;
		ProgramCache[ProgramName] = (ProgramInstruction)programInstruction.Clone();
		CopyReferences(ProgramCache[ProgramName]);
	}

	public bool WaitForCompletion()
	{
		if (IsComplete)
		{
			return true;
		}
		while (true)
		{
			try
			{
				if (CompletionWaitHandle.WaitOne(50, exitContext: false) || IsComplete)
				{
					return true;
				}
				Thread.Sleep(0);
			}
			catch (ObjectDisposedException)
			{
				return true;
			}
		}
	}

	public void Pend()
	{
		if (IsImmediate || Status == ExecutionContextStatus.Suspended)
		{
			ChangeStatus(ExecutionContextStatus.Pending);
		}
	}

	public void Trash()
	{
		if (ID == ExecutionEngine.Instance.InitJob.ID)
		{
			LogHelper.Instance.Log("The job {0} ( {1} ) will not be trashed - it is the init job.", ID, ProgramName);
			return;
		}
		LogHelper.Instance.Log("Context {0} ( {1} ) being trashed", ID, ProgramName);
		ChangeStatus(ExecutionContextStatus.Garbage);
		using (new WithWriteLock(ResultsLock))
		{
			if (CachedResults.Count > 0)
			{
				CachedResults.Clear();
			}
		}
	}

	public void Resurrect()
	{
		Result.Errors.RemoveCode("E201");
		ChangeStatus(ExecutionContextStatus.Pending);
	}

	public void Resume()
	{
		Result.Errors.RemoveCode("E201");
		if (Status != ExecutionContextStatus.Suspended)
		{
			LogHelper.Instance.Log($"Cannot resume context with status {Status.ToString()}", LogEntryType.Error);
		}
		else
		{
			ChangeStatus(ExecutionContextStatus.Pending);
		}
	}

	public void Suspend()
	{
		if (IsImmediate || Status == ExecutionContextStatus.Pending || Status == ExecutionContextStatus.Running)
		{
			ChangeStatus(ExecutionContextStatus.Suspended);
		}
		else if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
		{
			LogHelper.Instance.Log("Cannot suspend job that isn't in Pending or Running status.", LogEntryType.Error);
		}
	}

	public void Terminate()
	{
		if (!(Status == ExecutionContextStatus.Completed) && !(Status == ExecutionContextStatus.Garbage) && !(Status == ExecutionContextStatus.Errored) && !(Status == ExecutionContextStatus.Stopped))
		{
			ChangeStatus(ExecutionContextStatus.Stopped);
		}
	}

	public void Reset()
	{
		Suspend();
		CompletionWaitHandle.Reset();
		Result.Reset();
		Registers.Reset();
	}

	public void Execute()
	{
		ExecutionTimer executionTimer = new ExecutionTimer();
		ExecutionContextStatus executionContextStatus = ExecutionContextStatus.Completed;
		try
		{
			if (!ExecutionStart.HasValue)
			{
				ExecutionStart = DateTime.Now;
			}
			ChangeStatus(ExecutionContextStatus.Running);
			SendStatusMessage(ExecutionContextStatus.Running);
			AppLog.Open();
			if (Registers.ActiveFrame == null)
			{
				Registers.PushCallFrame(ProgramName, ProgramCache[ProgramName]);
			}
			while (!Status.ShouldPromote())
			{
				if (Registers.ActiveFrame.IP != null)
				{
					Registers.ActiveFrame.IP.Execute(Result, this);
				}
				if (Result.Errors.ContainsError())
				{
					Status.NextValue = (executionContextStatus = ExecutionContextStatus.Errored);
					if (!Registers.ActiveFrame.IP.ExitsContext)
					{
						LogHelper.Instance.Log($"Job {ID} had errors processing instruction at line {Registers.ActiveFrame.IP.LineNumber}.", LogEntryType.Info);
					}
					continue;
				}
				if (Result.RestartAtCurrentIP)
				{
					Result.RestartAtCurrentIP = false;
					continue;
				}
				if (Result.SwitchToContext != null)
				{
					Registers.PushCallFrame(Result.SwitchToContext.GetProgram().Operands[0].Value, Result.SwitchToContext);
					Result.SwitchToContext = null;
					continue;
				}
				while (!Registers.ActiveFrame.FetchNextInstruction())
				{
					Registers.PopCallFrame();
					if (Registers.ActiveFrame == null)
					{
						Status.NextValue = (executionContextStatus = ExecutionContextStatus.Completed);
						break;
					}
				}
			}
			Status.Promote();
			Result.UpdateExecutionTime(executionTimer.Elapsed);
			SendStatusMessage(Status.Value);
			Save();
		}
		catch (Exception ex)
		{
			Result.Errors.Add(Error.NewError("E001", "An unhandled exception was raised by the execution engine.", ex.ToString()));
			LogHelper.Instance.Log($"An instruction in program {ProgramName} ( ID = {ID} ) threw an unhandled exception.", ex);
			executionContextStatus = ExecutionContextStatus.Errored;
			Status.Promote(ExecutionContextStatus.Errored);
			Result.UpdateExecutionTime(executionTimer.Elapsed);
			Save();
		}
		finally
		{
			executionTimer.Dispose();
			if (IsComplete)
			{
				CompletionWaitHandle.Set();
				if (!IsImmediate)
				{
					CleanupHardware();
					string msg = $"Job {ProgramName} ID {ID} ends with status {executionContextStatus.ToString()}";
					LogHelper.Instance.Log(msg);
					AppLog.Write(msg);
					CompletionWaitHandle.Close();
				}
			}
			AppLog.Close();
		}
	}

	public IEnumerable<string> EnumerateResults()
	{
		foreach (string cachedResult in CachedResults)
		{
			yield return cachedResult;
		}
	}

	public void CreateResult(string code, string msg, string id)
	{
		CreateResult(StackEnd.Bottom, code, msg, null, null, id, null, null);
	}

	public bool Send(string message)
	{
		DateTime dateTime = DateTime.Now;
		if (LastEventTime.HasValue && dateTime.Second <= LastEventTime.Value.Second)
		{
			dateTime = LastEventTime.Value.AddSeconds(1.0);
		}
		LastEventTime = dateTime;
		Messages.PushTop(new ScriptMessage(dateTime, message));
		return FlushMessages();
	}

	public object PopBottom()
	{
		return DataStack.PopBottom();
	}

	public void PushBottom(object instance)
	{
		DataStack.PushBottom(instance);
	}

	public T PopTop<T>()
	{
		return (T)ConversionHelper.ChangeType(PopTop(), typeof(T));
	}

	public object PopTop()
	{
		return DataStack.PopTop();
	}

	public void PushTop(object instance)
	{
		DataStack.PushTop(instance);
	}

	public void ClearStack()
	{
		DataStack.Clear();
	}

	public void RemoveResultsFromStack()
	{
		DataStack.RemoveAll((object each) => string.Compare(each as string, 0, "RESULT|", 0, "RESULT".Length + 1, ignoreCase: true) == 0);
	}

	public IEnumerable<object> EnumerateStack()
	{
		foreach (object item in DataStack)
		{
			yield return item;
		}
	}

	public IEnumerable<string> EnumerateMessages()
	{
		foreach (ScriptMessage message in Messages)
		{
			yield return message.ToString();
		}
	}

	public ICollection<string> GetSymbolNames()
	{
		return SymbolTable.Keys;
	}

	public object GetSymbolValue(string name, ErrorList errors)
	{
		string text = name.ToUpper();
		if (!SymbolTable.TryGetValue(text, out var value))
		{
			errors.Add(Error.NewError("E002", $"The symbol {text} is not defined.", $"Line {Registers.ActiveFrame.IP.LineNumber}: {Registers.ActiveFrame.IP}"));
			return null;
		}
		return value;
	}

	public void SetSymbolValue(string name, object value)
	{
		SymbolTable[name.ToUpper()] = value;
	}

	public void ClearSymbolTable()
	{
		SymbolTable.Clear();
	}

	public void SaveDataStack()
	{
		if (!IsImmediate)
		{
			Save();
		}
	}

	public void SaveSymbolTable()
	{
		if (!IsImmediate)
		{
			Save();
		}
	}

	public void RegisterHandler(SignalHandler handler)
	{
		using (new WithUpgradeableReadLock(SignalsLock))
		{
			if (handler == null || Handler != null)
			{
				return;
			}
			using (new WithWriteLock(SignalsLock))
			{
				Handler = handler;
				foreach (string cachedSignal in CachedSignals)
				{
					Handler(cachedSignal);
				}
				CachedSignals.Clear();
			}
		}
	}

	public void Signal(string signal)
	{
		using (new WithUpgradeableReadLock(SignalsLock))
		{
			if (Handler != null)
			{
				Handler(signal);
				return;
			}
			using (new WithWriteLock(SignalsLock))
			{
				CachedSignals.Add(signal);
			}
		}
	}

	internal void AddObserver(IStatusChangeObserver o)
	{
		using (new WithWriteLock(ObserverLock))
		{
			Observers.Add(o);
		}
	}

	internal void RemoveObserver(IStatusChangeObserver o)
	{
		using (new WithWriteLock(ObserverLock))
		{
			Observers.Remove(o);
		}
	}

	internal void ServiceLog(string fmt, params object[] stuff)
	{
		if (!IsImmediate)
		{
			ServiceLog(string.Format(fmt, stuff), LogEntryType.Info);
		}
	}

	internal void ServiceLog(string msg)
	{
		if (!IsImmediate)
		{
			ServiceLog(msg, LogEntryType.Info);
		}
	}

	internal void ServiceLog(LogEntryType type, string fmt, params object[] stuff)
	{
		if (!IsImmediate)
		{
			string msg = string.Format(fmt, stuff);
			ServiceLog(msg, type);
		}
	}

	internal void ServiceLog(string msg, LogEntryType type)
	{
		if (!IsImmediate)
		{
			msg = $"[{ProgramName}, {ID}]: {msg}";
			LogHelper.Instance.Log(msg, type);
		}
	}

	internal void Delete()
	{
		lock (Lock)
		{
			try
			{
				Directory.Delete(ContextDirectory, recursive: true);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"Unable to delete context directory {ContextDirectory}", e);
			}
		}
	}

	internal bool FlushMessages()
	{
		if (ConnectionState != ConnectionState.Connected || MessageSink == null)
		{
			return false;
		}
		int num = 0;
		while (Messages.Count > 0)
		{
			string text = Messages.PopTop().ToString();
			if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
			{
				LogHelper.Instance.Log("Job ID = {0}, Message = {1}", ID, text);
			}
			try
			{
				MessageSink.Send(text);
				num++;
			}
			catch (Exception ex)
			{
				LogHelper.Instance.Log(LogEntryType.Error, "Job {0} ID {1} failed to send message: exception message = {2}", ProgramName, ID, ex.Message);
			}
		}
		return num > 0;
	}

	internal void PromoteDeferredStatusChange()
	{
		Status.NextValue = DeferredStatusChangeRequested ?? ExecutionContextStatus.Pending;
		DeferredStatusChangeRequested = null;
	}

	internal void CleanupHardware()
	{
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		service.SetSensors(on: false);
		service.StopRoller();
		service.ToggleRingLight(on: false, null);
	}

	internal string CreateCameraCaptureErrorResult()
	{
		return CreateResult(StackEnd.Bottom, "CameraCaptureFailure", "The CAMERA CAPTURE instruction failed.", null, null, null, null, null);
	}

	internal string CreateItemStuckResult(string id)
	{
		return CreateResult(StackEnd.Bottom, "ItemStuckInGripper", "There is an item stuck in the gripper", null, null, id, null, null);
	}

	internal string CreateMoveErrorResult()
	{
		return CreateResult(StackEnd.Bottom, "MachineError", "There was an error executing the MOVE instruction.", null, null, null, null, null);
	}

	internal string CreateMoveErrorResult(int deck, int slot)
	{
		return CreateResult(StackEnd.Bottom, "MachineError", "There was an error executing the MOVE instruction.", deck, slot, null, null, null);
	}

	internal string CreateInfoResult(StackEnd end, string code, string message)
	{
		return CreateResult(end, code, message, null, null, null, null, null);
	}

	internal string CreateInfoResult(string code, string message)
	{
		return CreateResult(StackEnd.Bottom, code, message, null, null, null, null, null);
	}

	internal string CreateInfoResult(string code, string message, string id)
	{
		return CreateResult(StackEnd.Bottom, code, message, null, null, id, null, null);
	}

	internal string CreateResult(string code, string message, int? deck, int? slot, string item, DateTime? returnTime, string previousID)
	{
		return CreateResult(StackEnd.Bottom, code, message, deck, slot, item, returnTime, previousID);
	}

	internal string CreateResult(StackEnd end, string code, string message, int? deck, int? slot, string item, DateTime? returnTime, string previousID)
	{
		if (IsImmediate)
		{
			return string.Empty;
		}
		using (new WithWriteLock(ResultsLock))
		{
			string text = "~";
			if (deck.HasValue && slot.HasValue)
			{
				text = $"{deck},{slot}";
			}
			string text2 = "~";
			if (returnTime.HasValue)
			{
				text2 = returnTime.ToString();
			}
			Guid guid = Guid.NewGuid();
			DateTime now = DateTime.Now;
			string text3 = item ?? "~";
			string text4 = previousID ?? "~";
			if (message.Contains("'"))
			{
				message = message.Replace('\'', ' ');
			}
			string text5 = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}", "RESULT", guid, now, code, text, text3, message, text2, text4);
			CachedResults.Add(text5);
			return text5;
		}
	}

	internal ExecutionContext(string id, string programName)
	{
		ID = id;
		CreatedOn = DateTime.Now;
		ProgramName = programName;
		Registers = new Registers();
		Result = new ExecutionResult();
		ConnectionState = ConnectionState.Disconnected;
		AppLog = new ApplicationLog(this);
	}

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (IsImmediate)
		{
			return;
		}
		lock (Lock)
		{
			info.AddValue("IsCooperative", IsCooperative);
			info.AddValue("ID", ID);
			info.AddValue("Label", Label);
			info.AddValue("TraceExecution", TraceExecution);
			info.AddValue("ProgramName", ProgramName);
			info.AddValue("EnableDebugging", EnableDebugging);
			info.AddValue("Priority", Priority.ToString());
			info.AddValue("Status", Status.ToString());
			info.AddValue("CreatedOn", CreatedOn);
			if (StartTime.HasValue)
			{
				info.AddValue("StartTime", StartTime);
				info.AddValue("HasStartTime", value: true);
			}
			else
			{
				info.AddValue("HasStartTime", value: false);
			}
			if (ExecutionStart.HasValue)
			{
				info.AddValue("ExecutionStartTime", ExecutionStart.Value);
				info.AddValue("HasExecutionStartTime", value: true);
			}
			else
			{
				info.AddValue("HasExecutionStartTime", value: false);
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Error error in Result.Errors)
			{
				stringBuilder.Append($"{error}|{error.Details};");
			}
			info.AddValue("Errors", stringBuilder.ToString());
			if (Result.ExecutionTime.HasValue)
			{
				info.AddValue("ExecutionTime", Result.ExecutionTime.ToString());
				info.AddValue("HasExecutionTime", value: true);
			}
			else
			{
				info.AddValue("HasExecutionTime", value: false);
			}
			StringBuilder stringBuilder2 = new StringBuilder();
			foreach (ScriptMessage message in Messages)
			{
				stringBuilder2.Append($"{message.ToString()};");
			}
			info.AddValue("Messages", stringBuilder2.ToString());
			StringBuilder stringBuilder3 = new StringBuilder();
			foreach (string cachedResult in CachedResults)
			{
				stringBuilder3.Append($"{cachedResult.ToString()};");
			}
			info.AddValue("Results", stringBuilder3.ToString());
			info.AddValue("ParentID", ParentID);
			if (SymbolTable.Keys.Count > 0)
			{
				info.AddValue("SymbolTable", SymbolTable, SymbolTable.GetType());
				info.AddValue("HasSymbolTable", value: true);
			}
			else
			{
				info.AddValue("HasSymbolTable", value: false);
			}
			StringBuilder stringBuilder4 = new StringBuilder();
			foreach (object item in EnumerateStack())
			{
				stringBuilder4.Append($"{item.GetType().FullName}={item};");
			}
			info.AddValue("Stack", stringBuilder4);
			if (IsCooperative)
			{
				StringBuilder stringBuilder5 = new StringBuilder();
				foreach (CallFrame item2 in Registers.CallStack)
				{
					string value = $"{item2.PC}|{item2.X}|{item2.Y}|{item2.ProgramName}|{((item2.ParentContext != null) ? item2.ParentContext.LineNumber : (-1))};";
					stringBuilder5.Append(value);
				}
				info.AddValue("CallFrames", stringBuilder5.ToString());
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				StringBuilder stringBuilder6 = new StringBuilder();
				foreach (string key in ProgramCache.Keys)
				{
					try
					{
						stringBuilder6.Append(key + ";");
						using FileStream serializationStream = File.OpenWrite(Path.Combine(ContextDirectory, key));
						binaryFormatter.Serialize(serializationStream, ProgramCache[key]);
					}
					catch (Exception e)
					{
						LogHelper.Instance.Log($"Unable to serialize program {key}.", e);
					}
				}
				info.AddValue("Programs", stringBuilder6.ToString());
			}
			AppLog.Serialize(info);
		}
	}

	protected ExecutionContext(SerializationInfo info, StreamingContext context)
	{
		bool flag = true;
		try
		{
			flag = (IsCooperative = info.GetBoolean("IsCooperative"));
		}
		catch (SerializationException e)
		{
			LogHelper.Instance.Log("Unable to hydrate context due to serialization exception", e);
			IsCooperative = true;
		}
		ConnectionState = ConnectionState.Disconnected;
		Result = new ExecutionResult();
		Registers = new Registers();
		Label = info.GetString("Label");
		TraceExecution = info.GetBoolean("TraceExecution");
		ProgramName = info.GetString("ProgramName");
		EnableDebugging = info.GetBoolean("EnableDebugging");
		ID = info.GetString("ID");
		string value = info.GetString("Priority");
		Priority = Enum<ProgramPriority>.ParseIgnoringCase(value, ProgramPriority.Normal);
		string value2 = info.GetString("Status");
		Status.NextValue = Enum<ExecutionContextStatus>.ParseIgnoringCase(value2, ExecutionContextStatus.Stopped);
		if (Status.NextValue == ExecutionContextStatus.Running)
		{
			Status.NextValue = ExecutionContextStatus.Suspended;
		}
		Status.Promote();
		CreatedOn = info.GetDateTime("CreatedOn");
		if (info.GetBoolean("HasStartTime"))
		{
			StartTime = info.GetDateTime("StartTime");
		}
		if (info.GetBoolean("HasExecutionTime"))
		{
			string text = info.GetString("ExecutionTime");
			if (!string.IsNullOrEmpty(text))
			{
				Result.ExecutionTime = TimeSpan.Parse(text);
			}
		}
		if (info.GetBoolean("HasExecutionStartTime"))
		{
			ExecutionStart = info.GetDateTime("ExecutionStartTime");
		}
		string text2 = info.GetString("Errors");
		if (!string.IsNullOrEmpty(text2))
		{
			string[] array = text2.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			if (array != null && array.Length != 0)
			{
				Result.Errors.AddRange(ErrorList.NewFromStrings(array));
			}
		}
		string text3 = info.GetString("Messages");
		if (!string.IsNullOrEmpty(text3))
		{
			string[] array2 = text3.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			foreach (string text4 in array2)
			{
				if (!string.IsNullOrEmpty(text4))
				{
					Messages.PushBottom(ScriptMessage.Parse(text4));
				}
			}
		}
		string text5 = info.GetString("Results");
		if (!string.IsNullOrEmpty(text5))
		{
			string[] array2 = text5.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			foreach (string text6 in array2)
			{
				if (!string.IsNullOrEmpty(text6))
				{
					CachedResults.Add(text6);
				}
			}
		}
		ParentID = info.GetString("ParentID");
		if (flag)
		{
			string[] array3 = info.GetString("Programs").Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			string[] array2 = array3;
			foreach (string text7 in array2)
			{
				string path = Path.Combine(ContextDirectory, text7);
				try
				{
					using FileStream serializationStream = File.OpenRead(path);
					ProgramCache[text7] = (ProgramInstruction)binaryFormatter.Deserialize(serializationStream);
				}
				catch (Exception e2)
				{
					LogHelper.Instance.Log("Unable to deserialize program.", e2);
				}
			}
		}
		if (info.GetBoolean("HasSymbolTable"))
		{
			SymbolTable = (IDictionary<string, object>)info.GetValue("SymbolTable", typeof(Dictionary<string, object>));
		}
		string[] array4 = info.GetString("Stack").Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		for (int num = array4.Length - 1; num >= 0; num--)
		{
			string[] array5 = array4[num].Split("=".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
			if (array5.Length >= 2)
			{
				PushTop(ConversionHelper.ChangeType(array5[1], Type.GetType(array5[0])));
			}
		}
		if (flag)
		{
			string[] array6 = info.GetString("CallFrames").Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			for (int num2 = array6.Length - 1; num2 >= 0; num2--)
			{
				string[] array7 = array6[num2].Split('|');
				string text8 = array7[3];
				int parentContextLineNumber = int.Parse(array7[4]);
				CallFrame callFrame = new CallFrame
				{
					PC = int.Parse(array7[0]),
					X = int.Parse(array7[1]),
					Y = int.Parse(array7[2]),
					ProgramName = text8
				};
				ProgramInstruction program = (ProgramCache.ContainsKey(text8) ? ProgramCache[text8] : null);
				callFrame.LoadInstructionPointer(parentContextLineNumber, program);
				Registers.CallStack.Push(callFrame);
			}
		}
		AppLog = ApplicationLog.Deserialize(info, this);
	}

	private void SendStatusMessage(ExecutionContextStatus status)
	{
		Send($">STATUS CHANGE< {status}");
		using (new WithReadLock(ObserverLock))
		{
			foreach (IStatusChangeObserver observer in Observers)
			{
				observer.StatusChange(status);
			}
		}
	}

	private void CopyReferences(Instruction instruction)
	{
		if (instruction == null || instruction.Instructions.Count == 0)
		{
			return;
		}
		foreach (Instruction instruction2 in instruction.Instructions)
		{
			if (instruction2.Instructions.Count > 0)
			{
				CopyReferences(instruction2);
			}
		}
	}

	private void ChangeStatus(ExecutionContextStatus status)
	{
		lock (Lock)
		{
			if (Status == ExecutionContextStatus.Running)
			{
				if (!DeferredStatusChangeRequested.HasValue)
				{
					DeferredStatusChangeRequested = status;
				}
			}
			else
			{
				Status.Promote(status);
				SendStatusMessage(status);
				Save();
			}
		}
	}
}
