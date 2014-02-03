using System.Collections.Generic;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class GetNonBarcodeInventory : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		_ = ExecutionEngine.Instance.ActiveContext;
		List<Location> unknowns = ControllerConfiguration.Instance.GetUnknowns();
		try
		{
			foreach (Location item in unknowns)
			{
				ResultHelper.Instance.CreateResult("UnknownLocation", "The location is UNKNOWN.", item.Deck, item.Slot, "UNKNOWN", null, null);
			}
		}
		finally
		{
			unknowns.Clear();
		}
		List<Location> emptySlots = ControllerConfiguration.Instance.GetEmptySlots();
		try
		{
			foreach (Location item2 in emptySlots)
			{
				ResultHelper.Instance.CreateResult("EmptyLocation", "The location is EMPTY.", item2.Deck, item2.Slot, "EMPTY", null, null);
			}
		}
		finally
		{
			emptySlots.Clear();
		}
	}
}
