namespace Redbox.HAL.Component.Model;

internal interface IControlSystemService
{
	bool IsInitialized { get; }

	void AddHandler(IControlSystemObserver observer);

	void RemoveHandler(IControlSystemObserver observer);
}
