using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.WUA.Callbacks;
using WUApiLib;

namespace Redbox.WUA;

internal class Manager : IManager
{
	private static Manager _manager;

	private List<UpdateError> _installUpdateErrors = new List<UpdateError>();

	private List<UpdateError> _downloadUpdateErrors = new List<UpdateError>();

	private object _lock = new object();

	internal UpdateDownloader UpdateDownloader;

	internal readonly UpdateCollection DownloadQueue = new UpdateCollectionClass();

	internal readonly object DownloadLock = new object();

	internal IDownloadJob DownloadJob;

	internal IUpdateInstaller UpdateInstaller;

	internal UpdateCollection InstallQueue = new UpdateCollectionClass();

	internal readonly object InstallLock = new object();

	internal IInstallationJob InstallJob;

	internal readonly ErrorList AsyncErrors = new ErrorList();

	private readonly IUpdateSession m_updateSession = new UpdateSessionClass();

	public static Manager Instance
	{
		get
		{
			if (_manager == null)
			{
				_manager = new Manager();
			}
			return _manager;
		}
	}

	public bool IsBusy
	{
		get
		{
			if (!DownloadInProgress && !InstallInProgress)
			{
				return RebootRequiredToFinishInstall;
			}
			return true;
		}
	}

	public bool DownloadInProgress { get; internal set; }

	public int DownloadProgress { get; internal set; }

	public bool InstallInProgress { get; internal set; }

	public int InstallProgress { get; internal set; }

	public bool RebootRequiredToFinishInstall { get; internal set; }

	public ErrorList ListAll(out List<Update> updates)
	{
		ErrorList result = Search("Type = 'Software' OR Type = 'Driver'", out var updates2);
		updates = updates2.ToListOfUpdates();
		return result;
	}

	public ErrorList ListInstalled(out List<Update> updates)
	{
		ErrorList result = Search("(Type = 'Software' AND IsInstalled = 1 ) OR (Type = 'Driver' AND IsInstalled = 1)", out var updates2);
		updates = updates2.ToListOfUpdates();
		return result;
	}

	public ErrorList ListAvailable(out List<Update> updates)
	{
		ErrorList result = Search("(Type = 'Software' AND IsInstalled = 0 ) OR (Type = 'Driver' AND IsInstalled = 0)", out var updates2);
		updates = updates2.ToListOfUpdates();
		return result;
	}

	public ErrorList ListDownloadedNotInstalled(out List<Update> updates)
	{
		ErrorList result = ListAvailable(out var updates2);
		updates = updates2.Where((Update u) => u.IsDownloaded).ToList();
		return result;
	}

	public ErrorList IsAvailable(Guid guid, out bool isAvailable)
	{
		isAvailable = false;
		ErrorList errorList = new ErrorList();
		errorList.AddRange(ListAvailable(out var updates));
		if (errorList.Count > 0)
		{
			return errorList;
		}
		Update update = updates.Where((Update u) => u.Id == guid).FirstOrDefault();
		if (update != null)
		{
			isAvailable = !update.IsInstalled;
		}
		return errorList;
	}

	public ErrorList IsInstalled(Guid guid, out bool isInstalled)
	{
		isInstalled = false;
		ErrorList errorList = new ErrorList();
		errorList.AddRange(ListAll(out var updates));
		if (errorList.Count > 0)
		{
			return errorList;
		}
		Update update = updates.Where((Update u) => u.Id == guid).FirstOrDefault();
		if (update != null)
		{
			isInstalled = update.IsInstalled;
		}
		return errorList;
	}

	public ErrorList EnqueueDownload(List<Guid> guids)
	{
		if (IsBusy)
		{
			return BusyErrors();
		}
		ErrorList errorList = new ErrorList();
		foreach (Guid guid in guids)
		{
			errorList.AddRange(EnqueueDownload(guid));
		}
		return errorList;
	}

	public ErrorList EnqueueDownload(Guid guid)
	{
		if (IsBusy)
		{
			return BusyErrors();
		}
		ErrorList errorList = new ErrorList();
		int count = DownloadQueue.Count;
		try
		{
			string searchString = $"UpdateID = '{guid}'";
			errorList.AddRange(Search(searchString, out var updates));
			if (updates.Count > 0)
			{
				lock (DownloadLock)
				{
					DownloadQueue.Add(updates[0]);
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("WUA01", "Unhandled exception in EnqueueDownload", e));
		}
		finally
		{
			if (count == DownloadQueue.Count)
			{
				errorList.Add("WUA02", "No updates were added to download queue", "");
			}
		}
		return errorList;
	}

	public ErrorList DequeueDownload(Guid id)
	{
		if (IsBusy)
		{
			return BusyErrors();
		}
		ErrorList errorList = new ErrorList();
		try
		{
			lock (DownloadLock)
			{
				for (int i = 0; i < DownloadQueue.Count; i++)
				{
					if (id.Equals(new Guid(DownloadQueue[i].Identity.UpdateID)))
					{
						DownloadQueue.RemoveAt(i);
					}
				}
			}
		}
		catch (Exception exception)
		{
			errorList.Add("WUA06", "Unhandled exception in DequeueDownload", exception);
		}
		return errorList;
	}

	public List<Update> ListDownloadQueue()
	{
		return DownloadQueue.ToListOfUpdates();
	}

	public ErrorList BeginDownload()
	{
		if (IsBusy)
		{
			return BusyErrors();
		}
		ErrorList errorList = new ErrorList();
		if (DownloadQueue.Count == 0)
		{
			errorList.Add("WUA08", "There are no updates queued for download", "Please queue up some updates to download");
		}
		if (DownloadInProgress)
		{
			errorList.Add("WUA081", "There is a download already in progress", "Please wait for the current process to finish");
		}
		if (errorList.Any())
		{
			return errorList;
		}
		lock (DownloadLock)
		{
			try
			{
				UpdateDownloader = m_updateSession.CreateUpdateDownloader();
				UpdateDownloader.IsForced = false;
				UpdateDownloader.Priority = DownloadPriority.dpLow;
				UpdateDownloader.Updates = new UpdateCollectionClass();
				UpdateDownloader.Updates = DownloadQueue.Copy();
				DownloadQueue.Clear();
				DownloadProgressChangedCallback onProgressChanged = new DownloadProgressChangedCallback(this);
				DownloadCompletedCallback onCompleted = new DownloadCompletedCallback(this);
				DownloadJob = UpdateDownloader.BeginDownload(onProgressChanged, onCompleted, null);
				DownloadInProgress = true;
				DownloadProgress = 0;
			}
			catch (Exception exception)
			{
				errorList.Add("WUA10", "Unhandled exception in Download", exception);
				DownloadInProgress = false;
				DownloadProgress = -1;
			}
		}
		return errorList;
	}

	public List<Update> CurrentlyDownloading()
	{
		lock (DownloadLock)
		{
			return DownloadInProgress ? DownloadJob.Updates.ToListOfUpdates() : new List<Update>();
		}
	}

	public ErrorList AbortDownload()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			if (DownloadInProgress)
			{
				DownloadJob.RequestAbort();
				DownloadInProgress = false;
				DownloadProgress = -1;
			}
		}
		catch (Exception exception)
		{
			errorList.Add("WUA70", "There was an error when trying to cancel the current download", exception);
		}
		return errorList;
	}

	public ErrorList EnqueueInstall(List<Guid> guids)
	{
		if (IsBusy)
		{
			return BusyErrors();
		}
		ErrorList errorList = new ErrorList();
		foreach (Guid guid in guids)
		{
			errorList.AddRange(EnqueueInstall(guid));
		}
		return errorList;
	}

	public ErrorList EnqueueInstall(Guid guid)
	{
		if (IsBusy)
		{
			return BusyErrors();
		}
		ErrorList errorList = new ErrorList();
		int count = InstallQueue.Count;
		try
		{
			string searchString = $"UpdateID = '{guid}'";
			errorList.AddRange(Search(searchString, out var updates));
			if (updates.Count > 0)
			{
				if (updates[0].IsDownloaded)
				{
					lock (InstallLock)
					{
						InstallQueue.Add(updates[0]);
					}
				}
				else
				{
					errorList.Add("WUA03", "The update needs to be downloaded before it can be installed", "Please download the update first");
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("WUA04", "Unhandled exception in EnqueueInstall", e));
		}
		finally
		{
			if (count == InstallQueue.Count)
			{
				errorList.Add("WUA05", "No updates were added to the install queue", "");
			}
		}
		return errorList;
	}

	public ErrorList DequeueInstall(Guid id)
	{
		if (IsBusy)
		{
			return BusyErrors();
		}
		ErrorList errorList = new ErrorList();
		try
		{
			lock (InstallLock)
			{
				for (int i = 0; i < InstallQueue.Count; i++)
				{
					if (id.Equals(new Guid(InstallQueue[i].Identity.UpdateID)))
					{
						InstallQueue.RemoveAt(i);
					}
				}
			}
		}
		catch (Exception exception)
		{
			errorList.Add("WUA07", "Unhandled exception in DequeueInstall", exception);
		}
		return errorList;
	}

	public List<Update> ListInstallQueue()
	{
		return InstallQueue.ToListOfUpdates();
	}

	public ErrorList BeginInstall()
	{
		if (IsBusy)
		{
			return BusyErrors();
		}
		ErrorList errorList = new ErrorList();
		if (InstallQueue.Count == 0)
		{
			errorList.Add("WUA11", "There are no updates queued for install", "Please queue up some updates to install");
		}
		if (InstallInProgress)
		{
			errorList.Add("WUA42", "There is an installation already in progress", "Please wait for the current process to finish");
		}
		if (errorList.Any())
		{
			return errorList;
		}
		lock (InstallLock)
		{
			try
			{
				UpdateInstaller = m_updateSession.CreateUpdateInstaller();
				if (UpdateInstaller.RebootRequiredBeforeInstallation)
				{
					errorList.Add("WUA12", "Reboot Required Before Installation", "Before installing updates a reboot is required.");
					return errorList;
				}
				if (RebootRequiredToFinishInstall)
				{
					errorList.Add(Error.NewWarning("WUA121", "Reboot Required to finish previous Installation", "The previously installed updates will not take effect until reboot."));
				}
				UpdateInstaller.Updates = InstallQueue.Copy();
				InstallQueue.Clear();
				UpdateInstaller.AllowSourcePrompts = false;
				UpdateInstaller.IsForced = false;
				foreach (IUpdate update in UpdateInstaller.Updates)
				{
					update.AcceptEula();
				}
				InstallationProgressChangedCallback onProgressChanged = new InstallationProgressChangedCallback(this);
				InstallationCompletedCallback onCompleted = new InstallationCompletedCallback(this);
				InstallJob = UpdateInstaller.BeginInstall(onProgressChanged, onCompleted, null);
				InstallInProgress = true;
				InstallProgress = 0;
			}
			catch (Exception exception)
			{
				errorList.Add("WUA16", "Unhandled exception in Install", exception);
				InstallInProgress = false;
				InstallProgress = -1;
			}
		}
		return errorList;
	}

	public List<Update> CurrentlyInstalling()
	{
		lock (InstallLock)
		{
			return InstallInProgress ? InstallJob.Updates.ToListOfUpdates() : new List<Update>();
		}
	}

	public ErrorList AbortInstall()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			if (InstallInProgress)
			{
				InstallJob.RequestAbort();
				InstallInProgress = false;
				InstallProgress = -1;
			}
		}
		catch (Exception exception)
		{
			errorList.Add("WUA70", "There was an error when trying to cancel the current installation", exception);
		}
		return errorList;
	}

	public ErrorList GetAsyncErrors()
	{
		ErrorList result = (ErrorList)AsyncErrors.Clone();
		AsyncErrors.Clear();
		return result;
	}

	public List<UpdateError> GetDownloadUpdateErrors(Guid id)
	{
		lock (_lock)
		{
			List<UpdateError> list = _downloadUpdateErrors.Where((UpdateError e) => e.Id == id).ToList();
			list.ForEach(delegate(UpdateError e)
			{
				_downloadUpdateErrors.Remove(e);
			});
			return list;
		}
	}

	public List<UpdateError> GetDownloadUpdateErrors()
	{
		lock (_lock)
		{
			List<UpdateError> result = _downloadUpdateErrors.ToList();
			_downloadUpdateErrors.Clear();
			return result;
		}
	}

	public void AddDownloadUpdateError(UpdateError error)
	{
		lock (_lock)
		{
			_downloadUpdateErrors.Add(error);
		}
	}

	public List<UpdateError> GetInstallUpdateErrors(Guid id)
	{
		lock (_lock)
		{
			List<UpdateError> list = _installUpdateErrors.Where((UpdateError e) => e.Id == id).ToList();
			list.ForEach(delegate(UpdateError e)
			{
				_installUpdateErrors.Remove(e);
			});
			return list;
		}
	}

	public List<UpdateError> GetInstallUpdateErrors()
	{
		lock (_lock)
		{
			List<UpdateError> result = _installUpdateErrors.ToList();
			_installUpdateErrors.Clear();
			return result;
		}
	}

	public void AddInstallUpdateError(UpdateError error)
	{
		lock (_lock)
		{
			_installUpdateErrors.Add(error);
		}
	}

	private Manager()
	{
		DownloadInProgress = false;
		InstallInProgress = false;
		InstallProgress = -1;
		DownloadProgress = -1;
		RebootRequiredToFinishInstall = false;
	}

	private ErrorList BusyErrors()
	{
		ErrorList errorList = new ErrorList();
		if (DownloadInProgress)
		{
			errorList.Add(Error.NewError("WUA-BUSY", "WUA: A download is in progress.", ""));
		}
		if (InstallInProgress)
		{
			errorList.Add(Error.NewError("WUA-BUSY", "WUA: A install is in progress.", ""));
		}
		if (RebootRequiredToFinishInstall)
		{
			errorList.Add(Error.NewError("WUA-BUSY", "WUA: A reboot is required.", ""));
		}
		return errorList;
	}

	private ErrorList Search(string searchString, out UpdateCollection updates)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			IUpdateSearcher updateSearcher = m_updateSession.CreateUpdateSearcher();
			ISearchResult searchResult = updateSearcher.Search(searchString);
			if (searchResult.ResultCode != OperationResultCode.orcSucceeded)
			{
				errorList.Add(searchResult.ResultCode.ToError());
			}
			foreach (IUpdateException warning in searchResult.Warnings)
			{
				errorList.Add(warning);
			}
			updates = searchResult.Updates;
		}
		catch (Exception exception)
		{
			errorList.Add("WUA17", "Unhandled exception in Search", exception);
			updates = new UpdateCollectionClass();
		}
		return errorList;
	}

	internal void DownloadCleanUp()
	{
		lock (DownloadLock)
		{
			DownloadProgress = -1;
			DownloadInProgress = false;
			if (DownloadJob != null)
			{
				DownloadJob.CleanUp();
				DownloadJob = null;
			}
		}
	}

	internal void InstallCleanUp()
	{
		lock (InstallLock)
		{
			InstallProgress = -1;
			InstallQueue.Clear();
			InstallInProgress = false;
			if (InstallJob != null)
			{
				InstallJob.CleanUp();
				InstallJob = null;
			}
		}
	}
}
