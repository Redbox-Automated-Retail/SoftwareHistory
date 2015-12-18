using System.Collections.Generic;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-excluded-locations", Operand = "GET-EXCLUDED-LOCS")]
internal sealed class GetExcludedLocationsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		List<Location> list = InventoryService.Instance.ComputeExcludedSlots();
		if (list.Count == 0)
		{
			base.Context.CreateInfoResult("NoExcludedSlots", "There are no excluded slots.");
			return;
		}
		list.Sort((Location x, Location y) => (x.Deck == y.Deck) ? x.Slot.CompareTo(y.Slot) : x.Deck.CompareTo(y.Deck));
		foreach (Location item in list)
		{
			base.Context.CreateResult("ExcludedSlot", "The location is excluded.", item.Deck, item.Slot, null, null, null);
		}
	}

	internal GetExcludedLocationsJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
