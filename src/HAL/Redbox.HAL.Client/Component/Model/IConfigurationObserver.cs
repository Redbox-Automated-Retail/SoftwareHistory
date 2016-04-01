namespace Redbox.HAL.Component.Model;

internal interface IConfigurationObserver
{
	void NotifyConfigurationLoaded();

	void NotifyConfigurationChangeStart();

	void NotifyConfigurationChangeEnd();
}
