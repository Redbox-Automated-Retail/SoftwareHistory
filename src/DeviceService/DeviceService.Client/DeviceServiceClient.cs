using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using DeviceService.Client.Jobs;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Commands;
using DeviceService.ComponentModel.Requests;
using DeviceService.ComponentModel.Responses;
using Newtonsoft.Json;

namespace DeviceService.Client;

public class DeviceServiceClient : IDeviceServiceClient
{
	private class CommandData
	{
		public Action<BaseResponseEvent> CommandCompleteCallback;

		public BaseCommandRequest CommandRequest { get; set; }

		public AutoResetEvent AutoResetEvent { get; set; }

		public BaseResponseEvent ResponseEvent { get; set; }

		public bool IsAsync => AutoResetEvent == null;

		public bool IsTimedOut
		{
			get
			{
				double value = (CommandRequest.CommandTimeout.HasValue ? CommandRequest.CommandTimeout.Value : 180000);
				return CommandRequest.CreatedDate.AddMilliseconds(value) < DateTime.Now;
			}
		}
	}

	private Task _connectionTask;

	private int _reconnectionAttemptInterval;

	private System.Timers.Timer _asyncCommandTimeoutTimer;

	private object _lockConnectionObject = new object();

	private object _lockSendCommand = new object();

	private AutoResetEvent _connectionAutoResetEvent;

	private int _defaultCommandTimeout = 180000;

	private string _url;

	private int _connectionTimeout;

	private ConcurrentDictionary<Guid, CommandData> _activeCommands = new ConcurrentDictionary<Guid, CommandData>();

	private SignalRClient _signalRClient;

	private DateTime? _lastCardReaderDisconnect;

	private bool _autoReconnect;

	public bool IsConnectedToDeviceService
	{
		get
		{
			if (_signalRClient != null)
			{
				return _signalRClient.IsConnectedToDeviceService;
			}
			return false;
		}
	}

	public string Url => _url;

	public int DefaultCommandTimeout
	{
		get
		{
			return _defaultCommandTimeout;
		}
		set
		{
			_defaultCommandTimeout = value;
		}
	}

	public bool AutoReconnect
	{
		get
		{
			return _autoReconnect;
		}
		set
		{
			_autoReconnect = value;
		}
	}

	public Version AssemblyVersion => GetType().Assembly.GetName().Version;

	public event LogHandler OnLog;

	public event OnConnected OnConnectedHandler;

	public event OnDisconnected OnDisconnectedHandler;

	public event OnConnected OnCardReaderConnectedHandler;

	public event OnDisconnected OnCardReaderDisconnectedHandler;

	public DeviceServiceClient()
	{
		_signalRClient = new SignalRClient();
		_signalRClient.OnLog += delegate(string logText)
		{
			Log(logText);
		};
		_signalRClient.OnConnectedHandler += SignalRConnected;
		_signalRClient.OnDisconnectedHandler += _signalRClient_OnDisconnectedHandler;
		_signalRClient.OnCardReaderConnectedHandler += _signalRClient_OnCardReaderConnectedHandler;
		_signalRClient.OnCardReaderDisconnectedHandler += _signalRClient_OnCardReaderDisconnectedHandler;
		_signalRClient.OnIsConnectedResponseEventHandler += _signalRClient_OnIsConnectedResponseEventHandler;
		_signalRClient.OnGetUnitHealthResponseEventHandler += _signalRClient_OnGetUnitHealthResponseEventHandler;
		_signalRClient.OnCardReadResponseEventHandler += _signalRClient_OnCardReadResponseEventHandler;
		_signalRClient.OnReadConfigurationEventHandler += _signalRClient_OnReadConfigurationResponseEventHandler;
		_signalRClient.OnWriteConfigurationEventHandler += _signalRClient_OnWriteConfigurationResponseEventHandler;
		_signalRClient.OnCancelCommandResponseEventHandler += _signalRClient_OnCancelCommandResponseEventHandler;
		_signalRClient.OnRebootCardReaderResponseEventHandler += _signalRClient_OnRebootCardReaderResponseEventHandler;
		_signalRClient.OnReportAuthorizationResultResponseEventHandler += _signalRClient_OnReportAuthorizationResultResponseEventHandler;
		_signalRClient.OnValidateVersionResponseEventHandler += _signalRClient_OnValidateVersionResponseEventHandler;
	}

	private void _signalRClient_OnValidateVersionResponseEventHandler(ValidateVersionResponseEvent validateVersionResponseEvent)
	{
		HandleResponseEvent(validateVersionResponseEvent);
	}

	private void _signalRClient_OnReportAuthorizationResultResponseEventHandler(ReportAuthorizationResultResponseEvent reportAuthorizationResultResponseEvent)
	{
		HandleAsyncCommandResponseEvent(reportAuthorizationResultResponseEvent);
	}

	private void _signalRClient_OnRebootCardReaderResponseEventHandler(RebootCardReaderResponseEvent rebootCardReaderResponseEvent)
	{
		HandleResponseEvent(rebootCardReaderResponseEvent);
	}

	private void _signalRClient_OnCancelCommandResponseEventHandler(CancelCommandResponseEvent cancelCommandResponseEvent)
	{
		HandleAsyncCommandResponseEvent(cancelCommandResponseEvent);
	}

	private void _signalRClient_OnCardReaderDisconnectedHandler(BaseEvent baseEvent)
	{
		HandleEvent(baseEvent, delegate
		{
			Task.Run(delegate
			{
				OnCardReaderDisconnectedHandler?.Invoke(null);
			});
			_lastCardReaderDisconnect = DateTime.Now;
		});
	}

	private void _signalRClient_OnCardReaderConnectedHandler(BaseEvent baseEvent)
	{
		HandleEvent(baseEvent, delegate
		{
			if (_lastCardReaderDisconnect.HasValue)
			{
				TimeSpan timeSpan = DateTime.Now.Subtract(_lastCardReaderDisconnect.Value);
				Log($"<<< Duration of card reader disconnection: {timeSpan}");
			}
			Task.Run(delegate
			{
				OnCardReaderConnectedHandler?.Invoke();
			});
		});
	}

	private void HandleEvent(BaseEvent baseEvent, Action action = null)
	{
		LogEvent(baseEvent);
		action?.Invoke();
	}

	private void _signalRClient_OnWriteConfigurationResponseEventHandler(SimpleResponseEvent evt)
	{
		HandleResponseEvent(evt);
	}

	private void _signalRClient_OnReadConfigurationResponseEventHandler(SimpleResponseEvent evt)
	{
		HandleResponseEvent(evt);
	}

	private void _signalRClient_OnCardReadResponseEventHandler(BaseResponseEvent crre)
	{
		HandleAsyncCommandResponseEvent(crre);
	}

	public bool ConnectToDeviceService(string url)
	{
		return ConnectToDeviceService(url, _defaultCommandTimeout);
	}

	public bool ConnectToDeviceService(string url, int connectionTimeout)
	{
		bool flag = false;
		lock (_lockConnectionObject)
		{
			_connectionAutoResetEvent = new AutoResetEvent(initialState: false);
			StartConnectionToDeviceService(url, connectionTimeout);
			_connectionAutoResetEvent.WaitOne(connectionTimeout);
			return IsConnectedToDeviceService;
		}
	}

	public void StartConnectionToDeviceService(string url, int? connectionTimeout)
	{
		_url = url;
		_connectionTimeout = (connectionTimeout.HasValue ? connectionTimeout.Value : ((_connectionTimeout != 0) ? _connectionTimeout : _defaultCommandTimeout));
		if (_connectionTask == null || _connectionTask.IsCompleted)
		{
			Log($"Begin Connecting to DeviceService at url: {_url}   timeout: {_connectionTimeout}");
			_connectionTask = _signalRClient.Connect(_url, _connectionTimeout);
			_connectionTask.ContinueWith(delegate
			{
				if (!IsConnectedToDeviceService && AutoReconnect)
				{
					AttemptReconnection();
				}
			});
		}
		else
		{
			Log("DeviceService Connect attempt rejected.  Only one connection attempt allowed at a time");
		}
	}

	public bool DisconnectFromDeviceService()
	{
		Log("Begin disconnect from DeviceService.");
		if (_signalRClient != null)
		{
			return _signalRClient.Disconnect();
		}
		return false;
	}

	public bool RebootCardReader()
	{
		return (RunCommand(new BaseCommandRequest("RebootCardReader", AssemblyVersion)) as SimpleResponseEvent)?.Success ?? false;
	}

	public bool IsCardReaderConnected()
	{
		return (RunCommand(new BaseCommandRequest("IsConnected", AssemblyVersion)) as IsConnectedResponseEvent)?.IsConnected ?? false;
	}

	public void ReadCard(Guid requestId, CardReadRequest request, Action<BaseResponseEvent> completeCallback, int? timeout = null)
	{
		RunAsyncReadCardCommand(requestId, request, timeout.HasValue ? timeout.Value : _defaultCommandTimeout, completeCallback);
	}

	public IReadCardJob CreateReadCardJob(CardReadRequest request, Action<BaseResponseEvent> readCardJobCompleteCallback, int? timeout = null)
	{
		return new ReadCardJob(this, request, readCardJobCompleteCallback, timeout);
	}

	public void ReportAuthorizeResult(bool success, Action<BaseResponseEvent> completeCallback)
	{
		ReportAuthorizeResultCommand baseCommandRequest = new ReportAuthorizeResultCommand(AssemblyVersion)
		{
			Success = success
		};
		RunAsyncCommand(baseCommandRequest, completeCallback);
	}

	public UnitHealthModel GetUnitHealth()
	{
		return (RunCommand(new BaseCommandRequest("GetUnitHealth", AssemblyVersion)) as GetUnitHealthResponseEvent)?.UnitHealthModel;
	}

	public string ReadConfig(string group, string index)
	{
		return (RunCommand(new ConfigCommand("ReadConfiguration", AssemblyVersion)) as SimpleResponseEvent)?.Data;
	}

	public string WriteConfig(string group, string index, string value)
	{
		return (RunCommand(new ConfigCommand("ReadConfiguration", AssemblyVersion)) as SimpleResponseEvent)?.Data;
	}

	public bool CancelCommand(Guid commandToCancelRequestId, Action<BaseResponseEvent> cancelCompleteCallback)
	{
		CancelCommandRequest baseCommandRequest = new CancelCommandRequest(AssemblyVersion)
		{
			CommandToCancelRequestId = commandToCancelRequestId,
			RequestId = Guid.NewGuid()
		};
		return RunAsyncCommand(baseCommandRequest, cancelCompleteCallback);
	}

	public ValidateVersionModel ValidateVersion()
	{
		ValidateVersionModel result = new ValidateVersionModel
		{
			IsCompatible = false,
			DeviceServiceVersion = new Version(0, 0)
		};
		Version assemblyVersion = AssemblyVersion;
		if (assemblyVersion != null)
		{
			ValidateVersionResponseEvent validateVersionResponseEvent = RunCommand(new ValidateVersionCommand(assemblyVersion)) as ValidateVersionResponseEvent;
			if (validateVersionResponseEvent?.ValidateVersionModel != null)
			{
				result = validateVersionResponseEvent.ValidateVersionModel;
			}
		}
		return result;
	}

	private void Log(string logText)
	{
		OnLog?.Invoke(logText);
	}

	private void _signalRClient_OnDisconnectedHandler(Exception exception)
	{
		_lastCardReaderDisconnect = null;
		Log($"DeviceService disconnected. {exception}");
		if (_autoReconnect)
		{
			AttemptReconnection();
		}
		Task.Run(delegate
		{
			OnDisconnectedHandler?.Invoke(exception);
		});
	}

	private void AttemptReconnection()
	{
		Task.Run(async delegate
		{
			int reconnectionInterval = GetReconnectionInterval();
			Log($"Delay DeviceServiceClient reconnection attempt {TimeSpan.FromMilliseconds(reconnectionInterval)}");
			await Task.Delay(reconnectionInterval);
			StartConnectionToDeviceService(_url, null);
		});
	}

	private void ResetReconnectionInterval()
	{
		_reconnectionAttemptInterval = 0;
	}

	private int GetReconnectionInterval()
	{
		if (_reconnectionAttemptInterval == 0)
		{
			_reconnectionAttemptInterval = 5000;
		}
		else
		{
			_reconnectionAttemptInterval *= 2;
		}
		int num = 300000;
		if (_reconnectionAttemptInterval > num)
		{
			_reconnectionAttemptInterval = num;
		}
		return _reconnectionAttemptInterval;
	}

	private void _signalRClient_OnGetUnitHealthResponseEventHandler(GetUnitHealthResponseEvent getUnitHealthResponseEvent)
	{
		LogResponseEvent(getUnitHealthResponseEvent);
		if (_activeCommands.TryGetValue(getUnitHealthResponseEvent.RequestId, out var value))
		{
			value.ResponseEvent = getUnitHealthResponseEvent;
			value?.AutoResetEvent.Set();
		}
		else
		{
			Log($"Response Event Handler error - unable to find active command: {JsonConvert.SerializeObject(getUnitHealthResponseEvent)}");
		}
	}

	private void HandleResponseEvent(BaseResponseEvent baseResponseEvent, Action<CommandData> action = null)
	{
		if (_activeCommands.TryGetValue(baseResponseEvent.RequestId, out var value))
		{
			LogResponseEvent(baseResponseEvent);
			value.ResponseEvent = baseResponseEvent;
			if (value.AutoResetEvent != null)
			{
				value?.AutoResetEvent.Set();
			}
			action?.Invoke(value);
		}
		else
		{
			Log($"Response Event Handler error - unable to find active command: {JsonConvert.SerializeObject(baseResponseEvent)}");
		}
	}

	private void HandleAsyncCommandResponseEvent(BaseResponseEvent baseResponseEvent)
	{
		HandleResponseEvent(baseResponseEvent, delegate(CommandData commandData)
		{
			_activeCommands.TryRemove(commandData.CommandRequest.RequestId, out var _);
			Task.Run(delegate
			{
				commandData.CommandCompleteCallback?.Invoke(baseResponseEvent);
			});
		});
	}

	private void _signalRClient_OnIsConnectedResponseEventHandler(IsConnectedResponseEvent isConnectedResponseEvent)
	{
		HandleResponseEvent(isConnectedResponseEvent);
	}

	private void LogCommand(BaseCommandRequest baseCommandRequest)
	{
		Log($">>> {baseCommandRequest.CommandName}: {JsonConvert.SerializeObject(baseCommandRequest)}");
	}

	private void LogCommandTimeout(BaseCommandRequest baseCommandRequest)
	{
		Log($"*** Timeout for {baseCommandRequest.CommandName}: {JsonConvert.SerializeObject(baseCommandRequest)}");
	}

	private void LogResponseEvent(BaseResponseEvent responseEvent)
	{
		string logText = null;
		Log(string.Format(arg1: (!(responseEvent is EMVCardReadResponseEvent responseEvent2)) ? ((!(responseEvent is EncryptedCardReadResponseEvent responseEvent3)) ? ((!(responseEvent is UnencryptedCardReadResponseEvent responseEvent4)) ? JsonConvert.SerializeObject(responseEvent) : ObfuscateUnencryptedCardReadResponseEvent(responseEvent4, logText)) : ObfuscateEncryptedCardReadResponseEvent(responseEvent3, logText)) : ObfuscateEMVCardReadResponseEvent(responseEvent2, logText), format: "<<< {0}: {1}", arg0: responseEvent.EventName));
	}

	private void LogEvent(BaseEvent baseEvent)
	{
		Log($"<<< {baseEvent.EventName}: {JsonConvert.SerializeObject(baseEvent)}");
	}

	private string ObfuscateEMVCardReadResponseEvent(EMVCardReadResponseEvent responseEvent, string logText)
	{
		return CreateResponseEventLogText(responseEvent, delegate(string json)
		{
			EMVCardReadResponseEvent eMVCardReadResponseEvent = JsonConvert.DeserializeObject<EMVCardReadResponseEvent>(json);
			if (eMVCardReadResponseEvent != null)
			{
				EMVCardReadModel data = eMVCardReadResponseEvent.Data;
				if (data != null)
				{
					data.ObfuscateSensitiveData();
					return eMVCardReadResponseEvent;
				}
				return eMVCardReadResponseEvent;
			}
			return eMVCardReadResponseEvent;
		});
	}

	private string ObfuscateEncryptedCardReadResponseEvent(EncryptedCardReadResponseEvent responseEvent, string logText)
	{
		return CreateResponseEventLogText(responseEvent, delegate(string json)
		{
			EncryptedCardReadResponseEvent encryptedCardReadResponseEvent = JsonConvert.DeserializeObject<EncryptedCardReadResponseEvent>(json);
			if (encryptedCardReadResponseEvent != null)
			{
				EncryptedCardReadModel data = encryptedCardReadResponseEvent.Data;
				if (data != null)
				{
					data.ObfuscateSensitiveData();
					return encryptedCardReadResponseEvent;
				}
				return encryptedCardReadResponseEvent;
			}
			return encryptedCardReadResponseEvent;
		});
	}

	private string ObfuscateUnencryptedCardReadResponseEvent(UnencryptedCardReadResponseEvent responseEvent, string logText)
	{
		return CreateResponseEventLogText(responseEvent, delegate(string json)
		{
			UnencryptedCardReadResponseEvent unencryptedCardReadResponseEvent = JsonConvert.DeserializeObject<UnencryptedCardReadResponseEvent>(json);
			if (unencryptedCardReadResponseEvent != null)
			{
				UnencryptedCardReadModel data = unencryptedCardReadResponseEvent.Data;
				if (data != null)
				{
					data.ObfuscateSensitiveData();
					return unencryptedCardReadResponseEvent;
				}
				return unencryptedCardReadResponseEvent;
			}
			return unencryptedCardReadResponseEvent;
		});
	}

	private string CreateResponseEventLogText(BaseEvent responseEvent, Func<string, object> ObfuscateJsonAction)
	{
		string result = null;
		try
		{
			string arg = JsonConvert.SerializeObject(responseEvent);
			result = JsonConvert.SerializeObject(ObfuscateJsonAction(arg));
		}
		catch (Exception)
		{
			Log("Unhandled exception in DeviceServiceClient.CreateResponseEventLogText while deserializing a CardReadResponseEvent");
		}
		return result;
	}

	private void SignalRConnected()
	{
		ResetReconnectionInterval();
		_lastCardReaderDisconnect = null;
		Log("DeviceService connected.");
		Task.Run(delegate
		{
			OnConnectedHandler?.Invoke();
		});
		if (_connectionAutoResetEvent != null)
		{
			_connectionAutoResetEvent.Set();
		}
	}

	private BaseResponseEvent RunCommand(string commandName, Guid? requestId)
	{
		return RunCommand(new BaseCommandRequest(commandName, AssemblyVersion)
		{
			RequestId = (requestId ?? Guid.NewGuid())
		});
	}

	private bool RunAsyncCommand(string commandName, Guid? requestId, int timeout, Action<BaseResponseEvent> commandCompleteCallback = null)
	{
		return RunAsyncCommand(new BaseCommandRequest(commandName, AssemblyVersion)
		{
			RequestId = (requestId ?? Guid.NewGuid()),
			CommandTimeout = timeout
		}, commandCompleteCallback);
	}

	private bool RunAsyncReadCardCommand(Guid? requestId, CardReadRequest request, int timeout, Action<BaseResponseEvent> commandCompleteCallback = null)
	{
		return RunAsyncCommand(new ReadCardCommand(AssemblyVersion)
		{
			RequestId = (requestId ?? Guid.NewGuid()),
			Request = request,
			CommandTimeout = timeout
		}, commandCompleteCallback);
	}

	private BaseResponseEvent RunCommand(BaseCommandRequest baseCommandRequest)
	{
		CommandData commandData = new CommandData
		{
			CommandRequest = baseCommandRequest,
			AutoResetEvent = new AutoResetEvent(initialState: false)
		};
		SendCommand(commandData);
		int millisecondsTimeout = (baseCommandRequest.CommandTimeout.HasValue ? baseCommandRequest.CommandTimeout.Value : _defaultCommandTimeout);
		commandData.AutoResetEvent.WaitOne(millisecondsTimeout);
		if (commandData.ResponseEvent == null)
		{
			LogCommandTimeout(commandData.CommandRequest);
		}
		_activeCommands.TryRemove(commandData.CommandRequest.RequestId, out var _);
		return commandData?.ResponseEvent;
	}

	private bool RunAsyncCommand(BaseCommandRequest baseCommandRequest, Action<BaseResponseEvent> commandCompleteCallback)
	{
		bool result = false;
		if (baseCommandRequest != null && commandCompleteCallback != null)
		{
			CommandData commandData = new CommandData
			{
				CommandRequest = baseCommandRequest,
				CommandCompleteCallback = commandCompleteCallback
			};
			SendCommand(commandData);
			StartTimeoutTimer();
			result = true;
		}
		return result;
	}

	private void SendCommand(CommandData commandData)
	{
		lock (_lockSendCommand)
		{
			_activeCommands[commandData.CommandRequest.RequestId] = commandData;
			LogCommand(commandData.CommandRequest);
			if (commandData.CommandRequest is CancelCommandRequest cancelCommandRequest)
			{
				_signalRClient.SendCommand(cancelCommandRequest);
			}
			else if (commandData.CommandRequest != null)
			{
				_signalRClient.SendCommand(commandData.CommandRequest);
			}
			else
			{
				Log($"DeviceServiceClient.SendCommand unknown command type {commandData?.CommandRequest?.GetType()}");
			}
		}
	}

	private void StartTimeoutTimer()
	{
		if (_asyncCommandTimeoutTimer == null)
		{
			_asyncCommandTimeoutTimer = new System.Timers.Timer();
			_asyncCommandTimeoutTimer.Interval = 60000.0;
			_asyncCommandTimeoutTimer.AutoReset = true;
			_asyncCommandTimeoutTimer.Elapsed += AsyncCommandTimeoutTimer_Elapsed;
		}
		if (!_asyncCommandTimeoutTimer.Enabled)
		{
			Log("Starting DeviceServiceClient aysnc command timeout timer");
			_asyncCommandTimeoutTimer.Enabled = true;
		}
	}

	private void AsyncCommandTimeoutTimer_Elapsed(object sender, ElapsedEventArgs e)
	{
		try
		{
			int num = 0;
			foreach (KeyValuePair<Guid, CommandData> activeCommand in _activeCommands)
			{
				CommandData value = activeCommand.Value;
				if (value == null || !value.IsAsync)
				{
					continue;
				}
				if (value.IsTimedOut)
				{
					if (_activeCommands.TryRemove(value.CommandRequest.RequestId, out var removedCommandData))
					{
						TimeoutResponseEvent timeoutResponseEvent = new TimeoutResponseEvent(removedCommandData.CommandRequest)
						{
							Success = false
						};
						Task.Run(delegate
						{
							removedCommandData?.CommandCompleteCallback(timeoutResponseEvent);
						});
						Log($"DeviceServiceClient removing timed out aync command {JsonConvert.SerializeObject(removedCommandData.CommandRequest)}");
					}
				}
				else
				{
					num++;
				}
			}
			if (num == 0)
			{
				Log("DeviceServiceClient no active aysnc commands.  Stopping timeout timer");
				_asyncCommandTimeoutTimer.Enabled = false;
			}
			else
			{
				Log($"DeviceServiceClient active aysnc commands: {num}");
			}
		}
		catch (Exception arg)
		{
			Log($"Unhandled exception in DeviceServiceClient.AsyncCommandTimeoutTimer_Elapsed. {arg}");
		}
	}
}
