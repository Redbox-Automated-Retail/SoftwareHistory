using System.Collections.ObjectModel;

namespace Redbox.HAL.DataStorage;

internal sealed class InventoryStore : StoreDescriptor
{
	private ReadOnlyCollection<string> ReadonlyTableList;

	internal override string Store => "HALData.vdb3";

	internal override ReadOnlyCollection<string> Tables => ReadonlyTableList;

	internal InventoryStore()
	{
		ReadonlyTableList = new ReadOnlyCollection<string>(new string[1] { "Inventory_v1" });
	}
}
