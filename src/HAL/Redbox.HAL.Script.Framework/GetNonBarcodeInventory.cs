using System.Collections.Generic;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-non-barcode-inventory", Operand = "GET-NON-BARCODE-INVENTORY")]
internal sealed class GetNonBarcodeInventory : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		List<Location> unknowns = InventoryService.Instance.GetUnknowns();
		using (new DisposeableList<Location>(unknowns))
		{
			foreach (Location item in unknowns)
			{
				base.Context.CreateResult("UnknownLocation", "The location is UNKNOWN.", item.Deck, item.Slot, "UNKNOWN", null, null);
			}
		}
		List<Location> emptySlots = InventoryService.Instance.GetEmptySlots();
		using (new DisposeableList<Location>(emptySlots))
		{
			foreach (Location item2 in emptySlots)
			{
				base.Context.CreateResult("EmptyLocation", "The location is EMPTY.", item2.Deck, item2.Slot, "EMPTY", null, null);
			}
		}
	}

	internal GetNonBarcodeInventory(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
