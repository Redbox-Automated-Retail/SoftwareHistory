using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;
using Redbox.HAL.Script.Framework;
using Redbox.HAL.Service.Framework;
using log4net;

namespace Redbox.HAL.Service.Win32;

public class HALService : ServiceBase, ILogger
{
	private enum PowerState
	{
		Normal,
		Suspended
	}

	private const string StopMessage = "Stop Hardware Abstraction Layer (HAL) Service.";

	private const string StartMessage = "Start Hardware Abstraction Layer (HAL) Service";

	private const string ConfigurationFile = "HAL.xml";

	private string ServiceURI = "rcp://127.0.0.1:7001";

	private Thread ListenerThread;

	private IPCProxy Proxy;

	private PowerState State;

	private static readonly ILog m_errorLog = LogManager.GetLogger("ErrorLog");

	private static readonly ILog m_log = LogManager.GetLogger("ServiceLog");

	private readonly object PowerStateLock = new object();

	private IContainer components;

	public HALService()
	{
		InitializeComponent();
	}

	public void Log(string message, Exception e)
	{
		m_log.Error(message, e);
	}

	public void Log(string message, LogEntryType type)
	{
		switch (type)
		{
		case LogEntryType.Info:
			m_log.Info(message);
			break;
		case LogEntryType.Debug:
			m_log.Debug(message);
			break;
		case LogEntryType.Error:
			LogServiceError(message);
			m_log.Error(message);
			break;
		case LogEntryType.Fatal:
			m_log.Fatal(message);
			break;
		}
	}

	public void Log(string message, Exception e, LogEntryType type)
	{
		switch (type)
		{
		case LogEntryType.Info:
			m_log.Info(message, e);
			break;
		case LogEntryType.Debug:
			m_log.Debug(message, e);
			break;
		case LogEntryType.Error:
			LogServiceError(message, e);
			m_log.Error(message, e);
			break;
		case LogEntryType.Fatal:
			m_log.Fatal(message, e);
			break;
		}
	}

	public bool IsLevelEnabled(LogEntryType entryLogLevel)
	{
		return entryLogLevel switch
		{
			LogEntryType.Info => m_log.IsInfoEnabled, 
			LogEntryType.Debug => m_log.IsDebugEnabled, 
			LogEntryType.Error => m_log.IsErrorEnabled, 
			LogEntryType.Fatal => m_log.IsFatalEnabled, 
			_ => false, 
		};
	}

	protected override void OnStart(string[] args)
	{
		ServiceURI = "rcp://127.0.0.1:7001";
		if (args != null)
		{
			for (int i = 0; i < args.Length; i++)
			{
				if (args[i].StartsWith("--uri"))
				{
					ServiceURI = CommandLineOption.GetOptionVal(args[i], ServiceURI);
				}
			}
		}
		StartService();
	}

	protected override void OnStop()
	{
		LogHelper.Instance.Log("Stop service request from OnStop.");
		StopService();
	}

	protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
	{
		LogHelper.Instance.Log("Stop service request from OnPowerEvent; powerStatus = {0}.", powerStatus.ToString());
		lock (PowerStateLock)
		{
			switch (powerStatus)
			{
			case PowerBroadcastStatus.Suspend:
				if (State == PowerState.Normal)
				{
					ExecutionEngine.Teardown();
					ControllerService.Instance.ParkHardware();
					State = PowerState.Suspended;
				}
				return true;
			case PowerBroadcastStatus.ResumeSuspend:
				if (PowerState.Suspended == State)
				{
					StartEngine();
					State = PowerState.Normal;
				}
				return true;
			default:
				return base.OnPowerEvent(powerStatus);
			}
		}
	}

	protected override void OnShutdown()
	{
		LogHelper.Instance.Log("Stop service request from OnShutdown.");
		StopService();
		base.OnShutdown();
	}

	internal void StopService()
	{
		ExecutionEngine.Teardown();
		ControllerService.Instance.ParkHardware();
		StoreManager.Instance.CloseStores();
		Log("Stop Hardware Abstraction Layer (HAL) Service.", LogEntryType.Info);
		WriteEventLogEntry("Stop Hardware Abstraction Layer (HAL) Service.", EventLogEntryType.Information);
		Proxy.Dispose();
	}

	internal void StartService()
	{
		ListenerThread = new Thread(ExecuteListener);
		ListenerThread.TrySetApartmentState(ApartmentState.MTA);
		ListenerThread.Start();
	}

	internal void ExecuteListener()
	{
		string message = string.Format("{0} version {1} (.NET version {2}; OS = {3})", "Start Hardware Abstraction Layer (HAL) Service", typeof(HALService).Assembly.GetName().Version, Environment.Version.ToString(), RuntimeService.Instance.GetPlatform().ToString());
		try
		{
			LogHelper.Instance.Logger = this;
			Log(message, LogEntryType.Info);
			WriteEventLogEntry("Start Hardware Abstraction Layer (HAL) Service", EventLogEntryType.Information);
			StoreManager.Instance.Initialize();
			Proxy = IPCProxy.Parse(ServiceURI);
			if (!Proxy.ProtocolValid)
			{
				string text = "ProtocolURI is malformed; please fix. HAL service is exiting.";
				Log(text, LogEntryType.Fatal);
				WriteEventLogEntry(text, EventLogEntryType.Error);
				return;
			}
			string path = RuntimeService.Instance.RuntimePath("HAL.xml");
			ConfigurationCommand configurationCommand = new ConfigurationCommand();
			ErrorList errorList = configurationCommand.LoadConfiguration(path);
			if (errorList.ContainsError())
			{
				foreach (Error item in errorList)
				{
					Log($"Error = {item}", LogEntryType.Info);
				}
				Halt("Failed to load configuration.");
			}
			else
			{
				StartEngine();
				string serviceStartup = RuntimeService.Instance.RuntimePath("ServiceStartup.txt");
				Proxy.ExecuteStartupFile(serviceStartup);
				Proxy.StartListener();
			}
		}
		catch (Exception e)
		{
			Log("Top level exception caught", e);
		}
	}

	private void LogServiceError(string msg, Exception exception)
	{
		string msg2 = $"{msg}: {exception.Message} ({exception.StackTrace})";
		LogServiceError(msg2);
	}

	private void StartEngine()
	{
		ErrorList errorList = new ErrorList();
		string fullPath = Path.GetFullPath(RuntimeService.Instance.AssemblyDirectory + "\\..\\Scripts");
		ExecutionEngine.NewEngine(fullPath, errorList);
		if (errorList.ContainsError())
		{
			Halt("Failed to load base scripts.");
		}
		else
		{
			ExecutionEngine.Instance.ExecuteStartupInit();
		}
	}

	private void Halt(string msg)
	{
		Log("The HAL service encountered startup problems, and is stopping.", LogEntryType.Error);
		Log(msg, LogEntryType.Error);
		WriteEventLogEntry("HAL Service encountered startup problems, and is stopping.", EventLogEntryType.Error);
		Stop();
	}

	private void LogServiceError(string msg)
	{
		try
		{
			m_errorLog.Error(msg);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unable to log error message to diagnostic log.", e);
		}
	}

	private bool WriteEventLogEntry(string msg, EventLogEntryType type)
	{
		bool result = false;
		try
		{
			EventLog.WriteEntry(msg, type);
			result = true;
		}
		catch (Exception)
		{
			LogHelper.Instance.Log("Unable to write to the event log; might be full?", LogEntryType.Info);
		}
		return result;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		base.CanHandlePowerEvent = true;
		base.CanShutdown = true;
		base.ServiceName = "HalSvc$Default";
	}
}
