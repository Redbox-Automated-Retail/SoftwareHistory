using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;
using Redbox.WUA;

namespace Redbox.UpdateManager.DownloadFile;

internal class WuaDownloadFile : DownloadFileBase
{
	private const string DownloadFileTypeName = "WUA download";

	private List<Update> _availableList;

	public WuaDownloadFile(IDownloadFileData downloadFileData)
		: base(downloadFileData)
	{
	}

	public override Redbox.UpdateManager.ComponentModel.ErrorList ProcessDownloadFile()
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		if (RebootRequired())
		{
			return errorList;
		}
		errorList.AddRange(CheckUpdateErrors());
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.None)
		{
			errorList.AddRange(CheckNone());
		}
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.PendingDownload)
		{
			errorList.AddRange(CheckPendingDownload());
		}
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.Downloading)
		{
			errorList.AddRange(CheckDownloading());
		}
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.PostDownload)
		{
			errorList.AddRange(CheckPostDownload());
		}
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.PendingInstall)
		{
			errorList.AddRange(CheckPendingInstall());
		}
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.Installing)
		{
			errorList.AddRange(CheckInstalling());
		}
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.PostInstall)
		{
			errorList.AddRange(CheckPostInstall());
		}
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.Complete)
		{
			errorList.AddRange(CheckComplete());
		}
		if (base.DownloadFileData.DownloadFileDataState == DownloadFileDataState.Error)
		{
			errorList.AddRange(CheckError());
		}
		return errorList;
	}

	private Redbox.UpdateManager.ComponentModel.ErrorList GetAvailable(out Update update)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		update = null;
		try
		{
			List<Update> availableList = GetAvailableList();
			Guid guid = default(Guid);
			ref Guid reference = ref guid;
			reference = new Guid(base.DownloadFileData.FileKey);
			update = availableList.Where((Update u) => u.Id == guid).FirstOrDefault();
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("WDF001", "An unhandled exception occurred in WuaDownloadFile.CheckAvailable", e));
		}
		return errorList;
	}

	private List<Update> GetAvailableList()
	{
		_availableList = new List<Update>();
		Redbox.WUA.ErrorList errors = Manager.Instance.ListAvailable(out var updates);
		errors.ToLogHelper();
		updates?.ForEach(delegate(Update u)
		{
			_availableList.Add(u);
		});
		return _availableList;
	}

	private Redbox.UpdateManager.ComponentModel.ErrorList CheckNone()
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.None)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "WUA download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		try
		{
			if (Manager.Instance.IsBusy)
			{
				LogHelper.Instance.Log("WUA is busy");
				return errorList;
			}
			errorList.AddRange(GetAvailable(out var update));
			if (errorList.ContainsError())
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
				base.DownloadFileData.Message = errorList[0].Description;
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			if (update == null)
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Complete;
				base.DownloadFileData.Message = "Update is not available";
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			if (update.IsInstalled)
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Complete;
				base.DownloadFileData.Message = "Update is already installed";
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			if (update.IsDownloaded)
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.PostDownload;
				errorList.AddRange(SendDownloadFileStatus(StatusMessage.StatusMessageType.Info, "Update is downloaded, post download", null));
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			Redbox.WUA.ErrorList errors = Manager.Instance.EnqueueDownload(new Guid(base.DownloadFileData.FileKey));
			errorList.AddRange(errors.ToErrors());
			if (errorList.ContainsError())
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
				base.DownloadFileData.Message = errorList[0].Description;
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.PendingDownload;
			errorList.AddRange(SaveDownloadFile());
			errorList.AddRange(SendDownloadFileStatus(StatusMessage.StatusMessageType.Info, "Update is available, pending download", null));
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("WDF001", "An unhandled exception occurred in WuaDownloadFile.CheckNone", e));
		}
		return errorList;
	}

	private Redbox.UpdateManager.ComponentModel.ErrorList CheckPendingDownload()
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.PendingDownload)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "WUA download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		List<Update> source = Manager.Instance.CurrentlyDownloading();
		Update update = source.Where((Update u) => u.Id == new Guid(base.DownloadFileData.FileKey)).FirstOrDefault();
		if (update != null)
		{
			base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Downloading;
			base.DownloadFileData.Message = "Update is downloading";
			errorList.AddRange(SaveDownloadFile());
		}
		List<Update> source2 = Manager.Instance.ListDownloadQueue();
		update = source2.Where((Update u) => u.Id == new Guid(base.DownloadFileData.FileKey)).FirstOrDefault();
		if (update == null)
		{
			base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Downloading;
			base.DownloadFileData.Message = "Update is downloading";
			errorList.AddRange(SaveDownloadFile());
		}
		return errorList;
	}

	private Redbox.UpdateManager.ComponentModel.ErrorList CheckDownloading()
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.Downloading)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "WUA download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		if (Manager.Instance.IsBusy)
		{
			LogHelper.Instance.Log("WUA is busy");
			return errorList;
		}
		try
		{
			List<Update> source = Manager.Instance.CurrentlyDownloading();
			Update update = source.Where((Update u) => u.Id == new Guid(base.DownloadFileData.FileKey)).FirstOrDefault();
			if (update != null)
			{
				return errorList;
			}
			errorList.AddRange(GetAvailable(out update));
			if (errorList.ContainsError())
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
				base.DownloadFileData.Message = errorList[0].Description;
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			if (update == null)
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Complete;
				base.DownloadFileData.Message = "Update is not available";
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			if (update.IsInstalled)
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Complete;
				base.DownloadFileData.Message = "Update is already installed";
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			if (update.IsDownloaded)
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.PostDownload;
				errorList.AddRange(SendDownloadFileStatus(StatusMessage.StatusMessageType.Info, "Update is downloaded", null));
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("WDF001", "An unhandled exception occurred in WuaDownloadFile.CheckDownloading.", e));
		}
		return errorList;
	}

	private Redbox.UpdateManager.ComponentModel.ErrorList CheckPostDownload()
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.PostDownload)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "WUA download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		try
		{
			if (Manager.Instance.IsBusy)
			{
				LogHelper.Instance.Log("WUA is busy");
				return errorList;
			}
			if (!IsValidTime())
			{
				LogHelper.Instance.Log("WuaDownloadFile: CheckPostDownload is skipping download file {0} until start: {1} and end time: {2} are valid.", base.DownloadFileData.Name, base.DownloadFileData.StartTime, base.DownloadFileData.EndTime);
				return errorList;
			}
			ServiceLocator.Instance.GetService<IUpdateService>();
			errorList.AddRange(GetAvailable(out var update));
			if (errorList.ContainsError())
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
				base.DownloadFileData.Message = errorList[0].Description;
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			if (update == null)
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Complete;
				base.DownloadFileData.Message = "Update is not available";
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			if (update.IsInstalled)
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Complete;
				base.DownloadFileData.Message = "Update is already installed";
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			if (!update.IsDownloaded)
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
				base.DownloadFileData.Message = "Update is not downloaded but is in a state is should be";
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			Redbox.WUA.ErrorList errors = Manager.Instance.EnqueueInstall(new Guid(base.DownloadFileData.FileKey));
			errorList.AddRange(errors.ToErrors());
			if (errorList.ContainsError())
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
				base.DownloadFileData.Message = errorList[0].Description;
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.PendingInstall;
			errorList.AddRange(SaveDownloadFile());
			errorList.AddRange(SendDownloadFileStatus(StatusMessage.StatusMessageType.Info, "Update is downloaded, pending install", null));
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("WDF999", "An unhandled exception occurred in WuaDownloadFile.CheckPendingInstall.", e));
		}
		return errorList;
	}

	private Redbox.UpdateManager.ComponentModel.ErrorList CheckPendingInstall()
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.PendingInstall)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "WUA download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		List<Update> source = Manager.Instance.CurrentlyInstalling();
		Update update = source.Where((Update u) => u.Id == new Guid(base.DownloadFileData.FileKey)).FirstOrDefault();
		if (update != null)
		{
			base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Installing;
			base.DownloadFileData.Message = "Update is installing";
			errorList.AddRange(SaveDownloadFile());
		}
		List<Update> source2 = Manager.Instance.ListInstallQueue();
		update = source2.Where((Update u) => u.Id == new Guid(base.DownloadFileData.FileKey)).FirstOrDefault();
		if (update == null)
		{
			base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Installing;
			base.DownloadFileData.Message = "Update is installing";
			errorList.AddRange(SaveDownloadFile());
		}
		return errorList;
	}

	private Redbox.UpdateManager.ComponentModel.ErrorList CheckInstalling()
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.Installing)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "WUA download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		if (Manager.Instance.IsBusy)
		{
			LogHelper.Instance.Log("WUA is busy");
			return errorList;
		}
		try
		{
			List<Update> source = Manager.Instance.CurrentlyInstalling();
			Update update = source.Where((Update u) => u.Id == new Guid(base.DownloadFileData.FileKey)).FirstOrDefault();
			if (update != null)
			{
				return errorList;
			}
			errorList.AddRange(GetAvailable(out update));
			if (errorList.ContainsError())
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
				base.DownloadFileData.Message = errorList[0].Description;
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			if (update == null)
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Complete;
				base.DownloadFileData.Message = "Update is not available";
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			if (update.IsInstalled)
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.PostInstall;
				errorList.AddRange(SendDownloadFileStatus(StatusMessage.StatusMessageType.Info, "Update is installed", null));
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			LogHelper.Instance.Log(string.Format("{0} download for {1} added back into queue guid: {1} ", "WUA download", base.DownloadFileData.Name, base.DownloadFileData.FileKey));
			Redbox.WUA.ErrorList errors = Manager.Instance.EnqueueInstall(new Guid(base.DownloadFileData.FileKey));
			errorList.AddRange(errors.ToErrors());
			if (errorList.ContainsError())
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
				base.DownloadFileData.Message = errorList[0].Description;
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("WDF001", "An unhandled exception occurred in WuaDownloadFile.CheckDownloading.", e));
		}
		return errorList;
	}

	private Redbox.UpdateManager.ComponentModel.ErrorList CheckPostInstall()
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.PostInstall)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "WUA download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		if (!string.IsNullOrEmpty(base.DownloadFileData.ActivateScript))
		{
			errorList.AddRange(RunScript(out var scriptCompleted));
			if (errorList.ContainsError())
			{
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
				base.DownloadFileData.Message = errorList[0].Description;
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
			if (!scriptCompleted)
			{
				return errorList;
			}
		}
		base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Complete;
		errorList.AddRange(SaveDownloadFile());
		return errorList;
	}

	private Redbox.UpdateManager.ComponentModel.ErrorList CheckComplete()
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.Complete)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "WUA download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		errorList.AddRange(SendDownloadFileStatus(StatusMessage.StatusMessageType.Info, "DownloadFile success", null));
		errorList.AddRange(DeleteDownloadFile());
		return errorList;
	}

	private Redbox.UpdateManager.ComponentModel.ErrorList CheckError()
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		if (base.DownloadFileData.DownloadFileDataState != DownloadFileDataState.Error)
		{
			return errorList;
		}
		LogHelper.Instance.Log(string.Format("{0} checking state: {1} for {2}", "WUA download", base.DownloadFileData.DownloadFileDataState.ToString(), base.DownloadFileData.Name));
		errorList.AddRange(SendDownloadFileStatus(StatusMessage.StatusMessageType.Error, "DownloadFile error", base.DownloadFileData.Message));
		errorList.AddRange(DeleteDownloadFile());
		return errorList;
	}

	private Redbox.UpdateManager.ComponentModel.ErrorList CheckUpdateErrors()
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		try
		{
			List<UpdateError> downloadUpdateErrors = Manager.Instance.GetDownloadUpdateErrors(new Guid(base.DownloadFileData.FileKey));
			downloadUpdateErrors.AddRange(Manager.Instance.GetInstallUpdateErrors(new Guid(base.DownloadFileData.FileKey)));
			if (downloadUpdateErrors.Count > 0)
			{
				UpdateError updateError = downloadUpdateErrors.First();
				base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
				string message = "";
				updateError.Errors.ForEach(delegate(Redbox.WUA.Error e)
				{
					message += $"Code: {e.Code}, Message: {e.Description}, Details: {e.Details}\n";
				});
				base.DownloadFileData.Message = message;
				errorList.AddRange(SaveDownloadFile());
				return errorList;
			}
		}
		catch (Exception ex)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("WDF999", "An unhandled exception occurred in WuaDownloadFile.CheckUpdateErrors.", ex));
			base.DownloadFileData.DownloadFileDataState = DownloadFileDataState.Error;
			base.DownloadFileData.Message = $"An unhandled exception occurred in WuaDownloadFile.CheckUpdateErrors with exception message: {ex.Message}";
			errorList.AddRange(SaveDownloadFile());
		}
		return errorList;
	}

	private bool RebootRequired()
	{
		if (Manager.Instance.RebootRequiredToFinishInstall)
		{
			DownloadFileService.Instance.MarkRebootRequired();
			return true;
		}
		return false;
	}

	public static Redbox.UpdateManager.ComponentModel.ErrorList StartDownloads()
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		if (Manager.Instance.IsBusy)
		{
			LogHelper.Instance.Log("WUA is busy");
			return errorList;
		}
		List<Update> list = Manager.Instance.ListDownloadQueue();
		if (list.Count > 0)
		{
			errorList.AddRange(Manager.Instance.BeginDownload().ToErrors());
		}
		return errorList;
	}

	public static Redbox.UpdateManager.ComponentModel.ErrorList StartInstalls()
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		if (Manager.Instance.IsBusy)
		{
			LogHelper.Instance.Log("WUA is busy");
			return errorList;
		}
		List<Update> list = Manager.Instance.ListInstallQueue();
		if (list.Count > 0)
		{
			errorList.AddRange(Manager.Instance.BeginInstall().ToErrors());
		}
		return errorList;
	}

	public static Redbox.UpdateManager.ComponentModel.ErrorList GetAsyncErrors()
	{
		return Manager.Instance.GetAsyncErrors().ToErrors();
	}
}
