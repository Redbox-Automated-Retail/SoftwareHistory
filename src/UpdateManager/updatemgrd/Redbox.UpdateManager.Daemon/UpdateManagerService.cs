using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.Command;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Daemon.Properties;
using Redbox.UpdateManager.Domain;
using Redbox.UpdateManager.DownloadFile;
using Redbox.UpdateManager.DownloadService;
using Redbox.UpdateManager.Environment;
using Redbox.UpdateManager.FileCache;
using Redbox.UpdateManager.FileSets;

namespace Redbox.UpdateManager.Daemon;

public class UpdateManagerService : ServiceBase
{
	internal const string ServiceMoniker = "updatemgrd$default";

	internal const string StopMessage = "Stop Update Manager Service.";

	internal const string StartMessage = "Start Update Manager Service.";

	private const string ServiceApplicationId = "{C423B0E3-95B3-4191-B326-AB3EA516F825}";

	private Thread m_thread;

	private IPCServiceHost m_listener;

	private static NamedLock m_instanceLock;

	private IContainer components;

	public UpdateManagerService()
	{
		InitializeComponent();
	}

	protected override void OnStart(string[] args)
	{
		StartService();
	}

	protected override void OnStop()
	{
		StopService();
	}

	protected override bool OnPowerEvent(PowerBroadcastStatus powerStatus)
	{
		switch (powerStatus)
		{
		case PowerBroadcastStatus.QuerySuspend:
			StopService();
			return true;
		case PowerBroadcastStatus.ResumeCritical:
		case PowerBroadcastStatus.ResumeSuspend:
		case PowerBroadcastStatus.ResumeAutomatic:
			StartService();
			return true;
		default:
			return base.OnPowerEvent(powerStatus);
		}
	}

	protected override void OnShutdown()
	{
		StopService();
		base.OnShutdown();
	}

	internal void StartService()
	{
		m_instanceLock = new NamedLock("{C423B0E3-95B3-4191-B326-AB3EA516F825}", LockScope.Local);
		m_thread = new Thread(ExecuteListener);
		m_thread.TrySetApartmentState(ApartmentState.MTA);
		m_thread.Start();
	}

	internal void StopService()
	{
		Redbox.UpdateManager.Domain.UpdateManagerService.Instance.RunShutdownScript();
		try
		{
			LogHelper.Instance.Log("Stopping fileset service");
			FileSetService.Instance.Stop();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error stopping fileset service.", e);
		}
		try
		{
			LogHelper.Instance.Log("Stopping Download service");
			Redbox.UpdateManager.DownloadService.DownloadService.Instance.Stop();
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("Error stopping Download service.", e2);
		}
		try
		{
			LogHelper.Instance.Log("Stopping downloadfile service");
			DownloadFileService.Instance.Stop();
		}
		catch (Exception e3)
		{
			LogHelper.Instance.Log("Error stopping downloadfile service.", e3);
		}
		try
		{
			LogHelper.Instance.Log("Stopping health service");
			HealthService.Instance.Stop();
		}
		catch (Exception e4)
		{
			LogHelper.Instance.Log("Error stopping health service.", e4);
		}
		try
		{
			LogHelper.Instance.Log("Stopping work service");
			WorkService.Instance.Stop();
		}
		catch (Exception e5)
		{
			LogHelper.Instance.Log("Error stopping work service.", e5);
		}
		try
		{
			LogHelper.Instance.Log("Stop Update Manager Service.", LogEntryType.Info);
			EventLog.WriteEntry("Stop Update Manager Service.", EventLogEntryType.Information);
		}
		catch (Exception e6)
		{
			LogHelper.Instance.Log("Error writing stop message to event log.", e6);
		}
		try
		{
			Redbox.UpdateManager.Domain.UpdateManagerService.Instance.StopScheduler();
			Redbox.UpdateManager.Domain.UpdateManagerService.Instance.Dispose();
		}
		catch (Exception e7)
		{
			LogHelper.Instance.Log("Unhandled exception while stopping scheduler or disposing Update Manager Domain.", e7);
		}
		try
		{
			if (m_listener != null)
			{
				m_listener.Stop();
			}
		}
		catch (Exception e8)
		{
			LogHelper.Instance.Log("Unhandled exception in StopService.", e8);
		}
		try
		{
			if (m_instanceLock != null)
			{
				m_instanceLock.Dispose();
			}
		}
		catch (Exception e9)
		{
			LogHelper.Instance.Log("Unhandled exception in releasing instance lock.", e9);
		}
	}

	internal void ExecuteListener()
	{
		try
		{
			LogHelper.Instance.Log("Start Update Manager Service.", LogEntryType.Info);
			EventLog.WriteEntry("Start Update Manager Service.", EventLogEntryType.Information);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error writing start message to event log.", e);
		}
		try
		{
			MacroService macroService = new MacroService();
			ServiceLocator.Instance.AddService(typeof(IMacroService), macroService);
			string directoryName = Path.GetDirectoryName(typeof(UpdateManagerService).Assembly.Location);
			macroService["RunningPath"] = directoryName;
			LogHelper.Instance.Log("Start the daemon thread.", LogEntryType.Info);
			LogHelper.Instance.Log("Initializing status message service");
			StatusMessageService.Instance.Initialize();
			LogHelper.Instance.Log("Initializing health service");
			HealthService.Instance.Initialize();
			LogHelper.Instance.Log("Starting health service");
			HealthService.Instance.Start();
			RegisterHealthItems();
			CommandRepository.AssemblyPatterns = new string[3] { "updatemgrd.exe", "Redbox.IPC.Framework.dll", "Redbox.UpdateManager.Command.dll" };
			LogHelper.Instance.Log("Initialize the Update Manager Service domain.", LogEntryType.Info);
			Redbox.UpdateManager.Domain.UpdateManagerService.Instance.Intialize(Settings.Default.KioskEnginePath, Settings.Default.ScriptLocation, Settings.Default.UpdateServiceUrl, Settings.Default.WcfUpdateServiceUrl, Settings.Default.KioskEngineUrl, Settings.Default.DeveloperMode, Settings.Default.InitialSubscriptionState, Settings.Default.StoreNumber, Settings.Default.TrimWorkingSetInterval, Settings.Default.UpdateServiceTimeout, Settings.Default.ShutdownScriptPath, Settings.Default.MinimumRetryDelay);
			LogHelper.Instance.Log("Execute start-up scripts.", LogEntryType.Info);
			BatchCommandRunner.ExecuteStartupFiles();
			LogHelper.Instance.Log("Start Update Manager Service scheduler.", LogEntryType.Info);
			Redbox.UpdateManager.Domain.UpdateManagerService.Instance.StartScheduler();
			string url = Settings.Default.Url;
			m_listener = IPCServiceHost.Create(IPCProtocol.Parse(url), AssemblyInfoHelper.GetProductName(typeof(EngineCommand).Assembly), AssemblyInfoHelper.GetVersion(typeof(EngineCommand).Assembly), AssemblyInfoHelper.GetCopyright(typeof(EngineCommand).Assembly), Settings.Default.MaximumWorkerThreads, Settings.Default.MaximumWorkerThreads, 8192, 64, 5);
			LogHelper.Instance.Log("Traffic is non-encrypted; no valid certificate specified or found.", LogEntryType.Info);
			LogHelper.Instance.Log($"Start Service Listener for interface '{m_listener.Protocol.Host}' on port '{m_listener.Protocol.Port}'.", LogEntryType.Info);
			m_listener.Start();
			LogHelper.Instance.Log("Initializing work service");
			WorkService.Instance.Initialize();
			LogHelper.Instance.Log("Starting work service");
			WorkService.Instance.Start();
			LogHelper.Instance.Log("Downloadfile service starting");
			DownloadFileService.Instance.Initialize();
			LogHelper.Instance.Log("Starting downloadfile service");
			DownloadFileService.Instance.Start();
			LogHelper.Instance.Log("Download service starting");
			string rootPath = macroService.ExpandProperties("${RunningPath}\\..\\.store\\.downloads");
			Redbox.UpdateManager.DownloadService.DownloadService.Instance.Initialize(rootPath);
			Redbox.UpdateManager.DownloadService.DownloadService.Instance.Start();
			LogHelper.Instance.Log("FileCacheService Initialization");
			string rootPath2 = macroService.ExpandProperties("${RunningPath}\\..\\.store\\.filecache");
			FileCacheService.Instance.Initialize(rootPath2);
			LogHelper.Instance.Log("FileSet service starting");
			string rootPath3 = macroService.ExpandProperties("${RunningPath}\\..\\.store\\.filesets");
			FileSetService.Instance.Initialize(rootPath3);
			FileSetService.Instance.Start();
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("Top level exception caught", e2);
		}
	}

	internal void RegisterHealthItems()
	{
		try
		{
			HealthService.Instance.Add("UPDATE_SERVICE_SERVERPOLL", TimeSpan.FromMinutes(17.0), delegate
			{
				try
				{
					LogHelper.Instance.Log("Running Health Entry for serverpoll.");
					object value = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Update Manager\\Health Service", "RunServerPoll", "true");
					bool flag = true;
					if (value != null && value is string && bool.TryParse(value as string, out var result))
					{
						flag = result;
					}
					LogHelper.Instance.Log("Running Health Entry for ServerPoll: {0}.", flag);
					if (flag)
					{
						IPollService service = ServiceLocator.Instance.GetService<IPollService>();
						if (service != null)
						{
							Redbox.UpdateManager.ComponentModel.ErrorList errorList = service.ServerPoll();
							{
								foreach (Redbox.UpdateManager.ComponentModel.Error item in errorList)
								{
									LogHelper.Instance.Log(item.ToString(), (!item.IsWarning) ? LogEntryType.Error : LogEntryType.Info);
								}
								return;
							}
						}
					}
				}
				catch (Exception e2)
				{
					LogHelper.Instance.Log("(UMS990) Error running ServerPoll health item", e2);
				}
			});
			HealthService.Instance.Add("UPDATE_SERVICE_WATCHDOG", new TimeSpan(0, 5, 0), delegate
			{
				try
				{
					LogHelper.Instance.Log("Running Health Entry for Watchdog.");
					object value = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Update Manager\\Health Service", "RunWatchdog", "true");
					bool flag = true;
					if (value != null && value is string && bool.TryParse(value as string, out var result))
					{
						flag = result;
					}
					LogHelper.Instance.Log("Running Health Entry for watchdog: {0}.", flag);
					if (flag)
					{
						IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
						string text = Path.Combine(service.ExpandProperties("${Scripts}"), "watchdog.lua");
						if (!File.Exists(text))
						{
							LogHelper.Instance.Log("Health Entry for watchdog.  Script: {0} not found.", text);
						}
						else
						{
							IKernelService service2 = ServiceLocator.Instance.GetService<IKernelService>();
							string chunk = File.ReadAllText(text);
							LogHelper.Instance.Log("Health Entry for watchdog. Executing lua script at {0}", text);
							service2.ExecuteChunk(chunk);
						}
					}
				}
				catch (Exception e2)
				{
					LogHelper.Instance.Log("(UMS991) Error running watchdog health item", e2);
				}
			});
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("(UMS994) Error registering health items.", e);
		}
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
		base.ServiceName = "updatemgrd$default";
	}
}
