namespace Redbox.HAL.Component.Model;

internal interface IPowerEventService
{
	void Add(IPowerEventObserver observer);

	void Remove(IPowerEventObserver observer);
}
