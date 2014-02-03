using WUApiLib;

namespace Redbox.WUA.Callbacks;

internal class InstallationProgressChangedCallback : IInstallationProgressChangedCallback
{
	private readonly Manager m_manager;

	public InstallationProgressChangedCallback(Manager manager)
	{
		m_manager = manager;
	}

	public void Invoke(IInstallationJob installationJob, IInstallationProgressChangedCallbackArgs callbackArgs)
	{
		m_manager.InstallProgress = callbackArgs.Progress.PercentComplete;
	}
}
