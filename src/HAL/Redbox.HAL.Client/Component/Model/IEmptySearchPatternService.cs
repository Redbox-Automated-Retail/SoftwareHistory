namespace Redbox.HAL.Component.Model;

internal interface IEmptySearchPatternService
{
	void DumpESP(bool dumpStore);

	void AddObserver(IExcludeEmptySearchLocationObserver observer);

	void RemoveObserver(IExcludeEmptySearchLocationObserver observer);

	IEmptySearchResult FindMultipleEmptyLocations();

	IEmptySearchResult FindOneEmpty();

	IEmptySearchResult FindEmptyOutsideOf(ILocation top);
}
