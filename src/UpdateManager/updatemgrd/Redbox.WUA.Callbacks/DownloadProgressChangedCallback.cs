using WUApiLib;

namespace Redbox.WUA.Callbacks;

internal class DownloadProgressChangedCallback : IDownloadProgressChangedCallback
{
	private readonly Manager m_manager;

	public DownloadProgressChangedCallback(Manager manager)
	{
		m_manager = manager;
	}

	public void Invoke(IDownloadJob downloadJob, IDownloadProgressChangedCallbackArgs callbackArgs)
	{
		m_manager.DownloadProgress = callbackArgs.Progress.PercentComplete;
	}
}
