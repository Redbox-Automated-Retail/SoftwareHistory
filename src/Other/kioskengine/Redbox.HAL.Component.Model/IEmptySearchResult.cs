using System;
using System.Collections.Generic;

namespace Redbox.HAL.Component.Model;

internal interface IEmptySearchResult : IDisposable
{
	IList<ILocation> EmptyLocations { get; }

	int FoundEmpty { get; }
}
