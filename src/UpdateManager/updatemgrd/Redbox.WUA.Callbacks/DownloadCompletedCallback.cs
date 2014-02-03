using System;
using System.Threading;
using WUApiLib;

namespace Redbox.WUA.Callbacks;

internal class DownloadCompletedCallback : IDownloadCompletedCallback
{
	private readonly Manager m_manager;

	public DownloadCompletedCallback(Manager manager)
	{
		m_manager = manager;
	}

	public void Invoke(IDownloadJob downloadJob, IDownloadCompletedCallbackArgs callbackArgs)
	{
		lock (m_manager.DownloadLock)
		{
			try
			{
				IDownloadResult downloadResult = m_manager.UpdateDownloader.EndDownload(downloadJob);
				if (downloadResult.HResult != 0)
				{
					m_manager.AsyncErrors.Add("WUA13", "There was an exception code raised during download", "Error Code:" + downloadResult.HResult);
				}
				if (downloadResult.ResultCode == OperationResultCode.orcSucceeded)
				{
					return;
				}
				m_manager.AsyncErrors.Add(downloadResult.ResultCode.ToError());
				for (int i = 0; i < downloadJob.Updates.Count; i++)
				{
					IUpdate update = downloadJob.Updates[i];
					Guid id = new Guid(update.Identity.UpdateID);
					IUpdateDownloadResult updateResult = downloadResult.GetUpdateResult(i);
					OperationResultCode resultCode = updateResult.ResultCode;
					if (resultCode != OperationResultCode.orcSucceeded && resultCode != OperationResultCode.orcInProgress && resultCode != OperationResultCode.orcNotStarted)
					{
						UpdateError updateError = new UpdateError();
						updateError.Id = id;
						updateError.UpdateOperationResultCode = (UpdateOperationResultCode)resultCode;
						UpdateError updateError2 = updateError;
						if (resultCode == OperationResultCode.orcAborted || resultCode == OperationResultCode.orcFailed)
						{
							m_manager.AddDownloadUpdateError(updateError2);
						}
						if (updateResult.HResult != 0)
						{
							Error item = Error.NewError("WUA14", "There was an exception code raised during download of update " + update.Title, "Error Code: " + updateResult.HResult);
							m_manager.AsyncErrors.Add(item);
							updateError2.Errors.Add(item);
						}
						if (resultCode != OperationResultCode.orcSucceeded)
						{
							Error item = Error.NewError("WUA15", "Installation download of " + update.Title + " was not successful", "Result Code: " + Enum.GetName(typeof(OperationResultCode), resultCode));
							m_manager.AsyncErrors.Add(item);
							updateError2.Errors.Add(item);
						}
					}
				}
			}
			catch (Exception exception)
			{
				m_manager.AsyncErrors.Add("WUA99", "There was an exception code raised during DownloadCompletedCallback.Invoke", exception);
			}
			finally
			{
				ThreadPool.QueueUserWorkItem(delegate
				{
					m_manager.DownloadCleanUp();
				});
			}
		}
	}
}
