namespace Redbox.HAL.Component.Model;

internal interface IControlSystemObserver
{
	void OnSystemInitialize(ErrorCodes initError);

	void OnSystemShutdown();
}
