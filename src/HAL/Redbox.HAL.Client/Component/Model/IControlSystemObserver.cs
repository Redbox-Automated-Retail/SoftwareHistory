namespace Redbox.HAL.Component.Model;

internal interface IControlSystemObserver
{
	void OnInitialize(ErrorCodes initError);

	void OnShutdown();
}
