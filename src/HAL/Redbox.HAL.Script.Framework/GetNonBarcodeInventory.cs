using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-non-barcode-inventory", Operand = "GET-NON-BARCODE-INVENTORY")]
internal sealed class GetNonBarcodeInventory : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IList<ILocation> unknowns = base.InventoryService.GetUnknowns();
		using (new DisposeableList<ILocation>(unknowns))
		{
			foreach (ILocation item in unknowns)
			{
				base.Context.CreateResult("UnknownLocation", "The location is UNKNOWN.", item.Deck, item.Slot, "UNKNOWN", null, null);
			}
		}
		IList<ILocation> emptySlots = base.InventoryService.GetEmptySlots();
		using (new DisposeableList<ILocation>(emptySlots))
		{
			foreach (ILocation item2 in emptySlots)
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
