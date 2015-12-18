using System.Collections.ObjectModel;

namespace Redbox.HAL.DataStorage;

internal abstract class StoreDescriptor
{
	internal abstract string Store { get; }

	internal abstract ReadOnlyCollection<string> Tables { get; }
}
