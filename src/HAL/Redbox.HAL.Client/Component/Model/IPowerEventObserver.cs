namespace Redbox.HAL.Component.Model;

internal interface IPowerEventObserver
{
	void OnSuspend();

	void OnResume();
}
