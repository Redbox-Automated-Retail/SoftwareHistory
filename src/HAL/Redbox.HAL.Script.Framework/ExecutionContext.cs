using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class ExecutionContext : IMessageSink, ISerializable
{
	public delegate void ContextStatusChange(ExecutionContextStatus newStatus);

	public delegate void MessageHandler(string msg);

	internal class ScriptMessage
	{
		public string Message { get; set; }

		public DateTime EventTime { get; set; }

		public static ScriptMessage Parse(string message)
		{
			string[] array = message.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			ScriptMessage scriptMessage = new ScriptMessage();
			scriptMessage.EventTime = DateTime.Parse(array[0]);
			scriptMessage.Message = array[1];
			return scriptMessage;
		}

		public override string ToString()
		{
			return $"{EventTime}|{Message}";
		}
	}

	public readonly ExecutionStatus Status = new ExecutionStatus();

	public ContextStatusChange StatusChangeCallback;

	private Deque<object> m_dataStack;

	private ProgramPriority m_priority;

	private Deque<ScriptMessage> m_messages;

	private ConnectionState m_connectionState;

	private EventWaitHandle m_completionWaitHandle;

	private IDictionary<string, object> m_symbolTable;

	private IDictionary<string, ProgramInstruction> m_programCache;

	private string m_contextDirectory;

	private List<string> m_receivedSignals;

	private List<string> m_jobResults;

	private ApplicationLog m_appLog;

	private int m_currentResult;

	public string ID { get; private set; }

	public string ParentID { get; set; }

	public string Label { get; set; }

	public int StackDepth => DataStack.Count;

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

	public bool IsImmediate => ID == ScriptConstants.ExecutionContexts.ImmediateModeContext;

	public DateTime? ExecutionStart { get; private set; }

	public DateTime CreatedOn { get; private set; }

	public string ProgramName { get; private set; }

	public Registers Registers { get; private set; }

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

	public IDictionary<string, object> SymbolTable
	{
		get
		{
			if (m_symbolTable == null)
			{
				m_symbolTable = new Dictionary<string, object>();
			}
			return m_symbolTable;
		}
		private set
		{
			m_symbolTable = value;
		}
	}

	public ManualResetEvent SignalEvent { get; private set; }

	internal ApplicationLog AppLog
	{
		get
		{
			if (m_appLog == null)
			{
				LogHelper.Instance.Log($"The applog isn't set for context {ID}; creating new.", LogEntryType.Info);
				m_appLog = new ApplicationLog(this);
			}
			return m_appLog;
		}
		set
		{
			m_appLog = value;
		}
	}

	internal ExecutionContextStatus? DeferredStatusChangeRequested { get; set; }

	internal IDictionary<string, ProgramInstruction> ProgramCache
	{
		get
		{
			if (m_programCache == null)
			{
				m_programCache = new Dictionary<string, ProgramInstruction>();
			}
			return m_programCache;
		}
	}

	internal EventWaitHandle CompletionWaitHandle
	{
		get
		{
			if (m_completionWaitHandle == null)
			{
				m_completionWaitHandle = new EventWaitHandle(initialState: false, EventResetMode.AutoReset);
			}
			return m_completionWaitHandle;
		}
	}

	internal Deque<ScriptMessage> Messages
	{
		get
		{
			if (m_messages == null)
			{
				m_messages = new Deque<ScriptMessage>();
			}
			return m_messages;
		}
	}

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

	internal List<string> CachedResults
	{
		get
		{
			if (m_jobResults == null)
			{
				m_jobResults = new List<string>();
			}
			return m_jobResults;
		}
	}

	private Deque<object> DataStack
	{
		get
		{
			if (m_dataStack == null)
			{
				m_dataStack = new Deque<object>();
			}
			return m_dataStack;
		}
	}

	private DateTime? LastEventTime { get; set; }

	private List<string> ReceivedSignals
	{
		get
		{
			if (m_receivedSignals == null)
			{
				m_receivedSignals = new List<string>();
			}
			return m_receivedSignals;
		}
	}

	private bool IsCooperative { get; set; }

	public void Save()
	{
		lock (this)
		{
			try
			{
				if (IsImmediate)
				{
					return;
				}
				string path = Path.Combine(ExecutionEngine.Instance.DataDirectory, "contexts");
				string text = Path.Combine(path, ID);
				try
				{
					if (!Directory.Exists(text))
					{
						Directory.CreateDirectory(text);
					}
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("Unable to create context directory.", e);
					return;
				}
				try
				{
					using FileStream serializationStream = File.OpenWrite(Path.Combine(text, "ctx.data"));
					BinaryFormatter binaryFormatter = new BinaryFormatter();
					binaryFormatter.Serialize(serializationStream, this);
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
		while (!CompletionWaitHandle.WaitOne(50, exitContext: false) && !IsComplete)
		{
			Thread.Sleep(0);
		}
		return true;
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
			LogHelper.Instance.Log($"The job {ID} ( {ProgramName} ) will not be trashed - it is the init job.", LogEntryType.Info);
			return;
		}
		LogHelper.Instance.Log($"context {ID} ( {ProgramName} ) being trashed", LogEntryType.Info);
		ChangeStatus(ExecutionContextStatus.Garbage);
		CleanupLog();
		if (CachedResults.Count > 0)
		{
			CachedResults.Clear();
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
					Registers.ActiveFrame.IP.Execute(Result);
				}
				if (Result.Errors.ContainsError())
				{
					Status.NextValue = ExecutionContextStatus.Errored;
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
						Status.NextValue = ExecutionContextStatus.Completed;
						break;
					}
				}
			}
			Status.Promote();
			Result.UpdateExecutionTime(executionTimer.Elapsed);
			SendStatusMessage(Status.Value);
			Save();
			CleanupLog();
			if (IsComplete)
			{
				CompletionWaitHandle.Set();
				if (!IsImmediate)
				{
					CleanupHardware();
				}
			}
		}
		catch (Exception ex)
		{
			Result.Errors.Add(Error.NewError("E001", "An unhandled exception was raised by the execution engine.", ex.ToString()));
			LogHelper.Instance.Log($"An instruction in program {ProgramName} ( ID = {ID} ) threw an unhandled exception.", ex);
			Status.Promote(ExecutionContextStatus.Errored);
			Result.UpdateExecutionTime(executionTimer.Elapsed);
			Save();
			CleanupHardware();
			CleanupLog();
			CompletionWaitHandle.Set();
		}
		finally
		{
			executionTimer.Dispose();
			if (IsComplete && !IsImmediate)
			{
				LogHelper.Instance.Log($"Job {ProgramName} ID {ID} ends with status {Status.ToString()}", LogEntryType.Info);
			}
		}
	}

	public bool Send(string message)
	{
		DateTime dateTime = DateTime.Now;
		if (LastEventTime.HasValue && dateTime.Second <= LastEventTime.Value.Second)
		{
			dateTime = LastEventTime.Value.AddSeconds(1.0);
		}
		LastEventTime = dateTime;
		Messages.PushTop(new ScriptMessage
		{
			EventTime = dateTime,
			Message = message
		});
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
		foreach (ScriptMessage each in Messages)
		{
			yield return each.ToString();
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

	public void AddSignalMessage(string msg)
	{
		lock (this)
		{
			ReceivedSignals.Add(msg);
			SignalEvent.Set();
		}
	}

	public string GetPendingSignal()
	{
		lock (this)
		{
			if (ReceivedSignals.Count > 0)
			{
				string result = ReceivedSignals[0];
				ReceivedSignals.RemoveAt(0);
				return result;
			}
			return string.Empty;
		}
	}

	internal void Delete()
	{
		lock (this)
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

	internal bool CleanupLog()
	{
		if (AppLog == null)
		{
			return false;
		}
		if (IsComplete)
		{
			AppLog.Write($"Job has finished with status {Status.ToString().ToUpper()}");
		}
		AppLog.Close();
		return true;
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
				LogHelper.Instance.Log($"Job ID = {ID}, Message = {text}", LogEntryType.Info);
			}
			try
			{
				MessageSink.Send(text);
				num++;
			}
			catch (Exception ex)
			{
				LogHelper.Instance.Log($"Job {ProgramName} ID {ID} failed to send message: exception message = {ex.Message}", LogEntryType.Error);
			}
		}
		return num > 0;
	}

	internal void SendDebugTraceMessage()
	{
		if (ConnectionState != ConnectionState.Disconnected && TraceExecution && EnableDebugging)
		{
			Send($">DBG TRACE< {ProgramName}@{Registers.ActiveFrame.IP.LineNumber}");
		}
	}

	internal void PromoteDeferredStatusChange()
	{
		Status.NextValue = DeferredStatusChangeRequested ?? ExecutionContextStatus.Pending;
		DeferredStatusChangeRequested = null;
	}

	internal void CleanupHardware()
	{
		PickerBoard.Instance.StopRoller();
		ControllerService.Instance.ToggleRingLight(RinglightState.Off, null);
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
		SignalEvent = new ManualResetEvent(initialState: false);
	}

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		if (IsImmediate)
		{
			return;
		}
		lock (this)
		{
			info.AddValue("ResultIndex", m_currentResult);
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
			bool isValid = AppLog.IsValid;
			info.AddValue("AppHasLog", isValid);
			if (isValid)
			{
				info.AddValue("AppLogDirectory", AppLog.Directory);
				info.AddValue("AppLogName", AppLog.FileName);
				info.AddValue("AppLogFlushOnWrite", AppLog.FlushOnWrite);
				info.AddValue("AppLogAppendToExisting", AppLog.AppendToExisting);
			}
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
		SignalEvent = new ManualResetEvent(initialState: false);
		AppLog = new ApplicationLog(this);
		ConnectionState = ConnectionState.Disconnected;
		Result = new ExecutionResult();
		Registers = new Registers();
		Label = info.GetString("Label");
		TraceExecution = info.GetBoolean("TraceExecution");
		ProgramName = info.GetString("ProgramName");
		EnableDebugging = info.GetBoolean("EnableDebugging");
		ID = info.GetString("ID");
		string value = info.GetString("Priority");
		try
		{
			Priority = Enum<ProgramPriority>.Parse(value, ProgramPriority.Normal);
		}
		catch (Exception)
		{
			Priority = ProgramPriority.Normal;
		}
		m_currentResult = info.GetInt32("ResultIndex");
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
			if (array != null && array.Length > 0)
			{
				Result.Errors.AddRange(ErrorList.NewFromStrings(array));
			}
		}
		string text3 = info.GetString("Messages");
		if (!string.IsNullOrEmpty(text3))
		{
			string[] array2 = text3.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			string[] array3 = array2;
			foreach (string text4 in array3)
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
			string[] array4 = text5.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			string[] array5 = array4;
			foreach (string text6 in array5)
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
			string text7 = info.GetString("Programs");
			string[] array6 = text7.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			string[] array7 = array6;
			foreach (string text8 in array7)
			{
				string path = Path.Combine(ContextDirectory, text8);
				try
				{
					using FileStream serializationStream = File.OpenRead(path);
					ProgramCache[text8] = (ProgramInstruction)binaryFormatter.Deserialize(serializationStream);
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
		string text9 = info.GetString("Stack");
		string[] array8 = text9.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		for (int num = array8.Length - 1; num >= 0; num--)
		{
			string[] array9 = array8[num].Split("=".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
			if (array9.Length >= 2)
			{
				PushTop(ConversionHelper.ChangeType(array9[1], Type.GetType(array9[0])));
			}
		}
		if (flag)
		{
			string text10 = info.GetString("CallFrames");
			string[] array10 = text10.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			for (int num2 = array10.Length - 1; num2 >= 0; num2--)
			{
				string text11 = array10[num2];
				string[] array11 = text11.Split('|');
				string text12 = array11[3];
				int parentContextLineNumber = int.Parse(array11[4]);
				CallFrame callFrame = new CallFrame
				{
					PC = int.Parse(array11[0]),
					X = int.Parse(array11[1]),
					Y = int.Parse(array11[2]),
					ProgramName = text12
				};
				ProgramInstruction program = (ProgramCache.ContainsKey(text12) ? ProgramCache[text12] : null);
				callFrame.LoadInstructionPointer(parentContextLineNumber, program);
				Registers.CallStack.Push(callFrame);
			}
		}
		if (info.GetBoolean("AppHasLog"))
		{
			AppLog.Directory = info.GetString("AppLogDirectory");
			AppLog.FileName = info.GetString("AppLogName");
			AppLog.AppendToExisting = info.GetBoolean("AppLogAppendToExisting");
			AppLog.FlushOnWrite = info.GetBoolean("AppLogFlushOnWrite");
		}
	}

	private void SendStatusMessage(ExecutionContextStatus status)
	{
		Send($">STATUS CHANGE< {status}");
		if (StatusChangeCallback != null)
		{
			StatusChangeCallback(status);
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
			if (instruction2 is CallInstruction callInstruction)
			{
				string value = callInstruction.Operands[0].Value;
				ProgramInstruction program = ExecutionEngine.Instance.GetProgram(value);
				if (program != null)
				{
					ProgramCache[value] = (ProgramInstruction)program.Clone();
					CopyReferences(ProgramCache[value]);
				}
			}
		}
	}

	private void ChangeStatus(ExecutionContextStatus status)
	{
		lock (this)
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
