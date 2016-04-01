namespace Redbox.HAL.Component.Model;

public interface IControlSystemObserver
{
	void OnInitialize(ErrorCodes initError);

	void OnShutdown();
}
