namespace Redbox.HAL.Component.Model;

internal interface IExcludeEmptySearchLocationObserver
{
	bool ShouldExclude(ILocation location);
}
