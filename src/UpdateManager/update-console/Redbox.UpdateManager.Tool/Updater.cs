using System;
using System.Collections.Generic;
using System.IO;
using Redbox.Core;
using Redbox.UpdateManager.BITS;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Environment;
using Redbox.UpdateManager.Kernel;
using Redbox.UpdateManager.Remoting;
using Redbox.UpdateManager.ServiceProxies;
using Redbox.UpdateManager.TaskScheduler;
using Redbox.UpdateManager.Tool.Properties;

namespace Redbox.UpdateManager.Tool;

public class Updater
{
	private const string ServiceApplicationId = "{C423B0E3-95B3-4191-B326-AB3EA516F825}";

	public string StoreNumber { get; set; }

	public ErrorList Initialize(string updateServiceUrl)
	{
		ErrorList result = new ErrorList();
		MacroService macroService = new MacroService();
		ServiceLocator.Instance.AddService(typeof(IMacroService), macroService);
		string directoryName = Path.GetDirectoryName(typeof(Updater).Assembly.Location);
		macroService["StoreNumber"] = StoreNumber;
		macroService["ProgramFiles"] = "C:\\Program Files";
		macroService["REDS"] = "C:\\Program Files\\Redbox\\REDS";
		macroService["data"] = "C:\\Program Files\\Redbox\\REDS\\data";
		macroService["RedboxProgramFiles"] = "C:\\Program Files\\Redbox";
		macroService["engine"] = "C:\\Program Files\\Redbox\\REDS\\Kiosk Engine";
		macroService["Scripts"] = Path.Combine(directoryName, "..\\scripts");
		macroService["WorkingDirectory"] = directoryName;
		WindowsTaskScheduler.Instance.Initialize();
		KioskEngineService.Instance.Initialize(Settings.Default.KioskEnginePath);
		Redbox.UpdateManager.Remoting.UpdateService.Instance.Initialize(updateServiceUrl, TimeSpan.FromSeconds(120.0), StoreNumber, Settings.Default.MinimumRetryDelay);
		KernelService.Instance.Initialize();
		if (IsServiceRunning())
		{
			DataStoreServiceProxy.Instance.Initialize(Settings.Default.UpdateManagerUrl);
			QueueServiceProxy.Instance.Initialize(Settings.Default.UpdateManagerUrl);
			TransferServiceProxy.Instance.Initialize(Settings.Default.UpdateManagerUrl);
			RepositoryServiceProxy.Instance.Initialize(Settings.Default.UpdateManagerUrl);
			SchedulerProxy.Instance.Initialize(Settings.Default.UpdateManagerUrl);
		}
		else
		{
			DataStoreService.Instance.Initialize(Settings.Default.DataStoreRoot);
			QueueService.Instance.Initialize(Settings.Default.DataStoreRoot);
			TransferService.Instance.Initialize();
			RepositoryService.Instance.Initialize(Settings.Default.ManifestRoot);
			Scheduler.Instance.Initialize();
		}
		return result;
	}

	public ErrorList StartUpdate()
	{
		IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
		return service.StartDownloads();
	}

	public ErrorList FinishUpdates()
	{
		IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
		return service.FinishDownloads();
	}

	public ErrorList FinishUpdate(Guid jobId)
	{
		IUpdateService service = ServiceLocator.Instance.GetService<IUpdateService>();
		return service.FinishDownload(jobId);
	}

	public ErrorList Activate(string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		bool deferredMove;
		return service.ActivateToHead(name, out deferredMove);
	}

	public ErrorList Activate(string name, string hash)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		bool deferredMove;
		return service.ActivateTo(name, hash, out deferredMove);
	}

	public ErrorList UpdateTo(string name, string hash)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		return service.UpdateTo(name, hash);
	}

	public ErrorList UpdateTo(string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		return service.UpdateToHead(name);
	}

	public List<string> GetManifests()
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		return service.GetAllRepositories();
	}

	public List<IRevLog> GetPendingChangeSets(string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		return service.GetPendingChanges(name);
	}

	public List<IRevLog> GetAppliedChangeSets(string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		return service.GetAppliedChanges(name);
	}

	public List<IRevLog> GetAllChangesSets(string name)
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		return service.GetAllChanges(name);
	}

	public ErrorList ResetBITS()
	{
		ITransferService service = ServiceLocator.Instance.GetService<ITransferService>();
		return service.CancelAll();
	}

	public void ResetManifest()
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		service.Reset(out var _);
	}

	public void Reset()
	{
		IRepositoryService service = ServiceLocator.Instance.GetService<IRepositoryService>();
		service.Reset(out var _);
		ITransferService service2 = ServiceLocator.Instance.GetService<ITransferService>();
		service2.CancelAll();
		IDataStoreService service3 = ServiceLocator.Instance.GetService<IDataStoreService>();
		service3.Reset();
	}

	private static bool IsServiceRunning()
	{
		NamedLock namedLock = new NamedLock("{C423B0E3-95B3-4191-B326-AB3EA516F825}", LockScope.Local);
		return !namedLock.Exists();
	}
}
