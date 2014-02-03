using System;
using System.Threading;
using WUApiLib;

namespace Redbox.WUA.Callbacks;

internal class InstallationCompletedCallback : IInstallationCompletedCallback
{
	private readonly Manager m_manager;

	public InstallationCompletedCallback(Manager manager)
	{
		m_manager = manager;
	}

	public void Invoke(IInstallationJob installationJob, IInstallationCompletedCallbackArgs callbackArgs)
	{
		lock (m_manager.InstallLock)
		{
			try
			{
				IInstallationResult installationResult = m_manager.UpdateInstaller.EndInstall(installationJob);
				if (installationResult.HResult != 0)
				{
					m_manager.AsyncErrors.Add("WUA13", "There was an exception code raised during installation", "Error Code:" + installationResult.HResult);
				}
				if (installationResult.ResultCode != OperationResultCode.orcSucceeded)
				{
					m_manager.AsyncErrors.Add(installationResult.ResultCode.ToError());
					for (int i = 0; i < installationJob.Updates.Count; i++)
					{
						IUpdate update = installationJob.Updates[i];
						Guid id = new Guid(update.Identity.UpdateID);
						IUpdateInstallationResult updateResult = installationResult.GetUpdateResult(i);
						OperationResultCode resultCode = updateResult.ResultCode;
						if (resultCode != OperationResultCode.orcSucceeded && resultCode != OperationResultCode.orcInProgress && resultCode != OperationResultCode.orcNotStarted)
						{
							UpdateError updateError = new UpdateError();
							updateError.Id = id;
							updateError.UpdateOperationResultCode = (UpdateOperationResultCode)resultCode;
							UpdateError updateError2 = updateError;
							if (resultCode == OperationResultCode.orcAborted || resultCode == OperationResultCode.orcFailed)
							{
								m_manager.AddInstallUpdateError(updateError2);
							}
							if (updateResult.HResult != 0)
							{
								Error item = Error.NewError("WUA14", "There was an exception code raised during installation of update " + update.Title, "Error Code: " + updateResult.HResult);
								m_manager.AsyncErrors.Add(item);
								updateError2.Errors.Add(item);
							}
							if (resultCode != OperationResultCode.orcSucceeded)
							{
								Error item = Error.NewError("WUA15", "Installation install of " + update.Title + " was not successful", "Result Code: " + Enum.GetName(typeof(OperationResultCode), resultCode));
								m_manager.AsyncErrors.Add(item);
								updateError2.Errors.Add(item);
							}
						}
					}
				}
				m_manager.RebootRequiredToFinishInstall = installationResult.RebootRequired;
			}
			catch (Exception exception)
			{
				m_manager.AsyncErrors.Add("WUA99", "There was an exception code raised during InstallationCompletedCallback.Invoke", exception);
			}
			finally
			{
				ThreadPool.QueueUserWorkItem(delegate
				{
					m_manager.InstallCleanUp();
				});
			}
		}
	}
}
