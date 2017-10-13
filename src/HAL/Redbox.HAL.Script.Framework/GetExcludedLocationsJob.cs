using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-excluded-locations", Operand = "GET-EXCLUDED-LOCS")]
internal sealed class GetExcludedLocationsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IList<ILocation> excludedSlots = base.InventoryService.GetExcludedSlots();
		using (new DisposeableList<ILocation>(excludedSlots))
		{
			if (excludedSlots.Count == 0)
			{
				base.Context.CreateInfoResult("NoExcludedSlots", "There are no excluded slots.");
				return;
			}
			foreach (ILocation item in excludedSlots)
			{
				base.Context.CreateResult("ExcludedSlot", "The location is excluded.", item.Deck, item.Slot, null, null, null);
			}
		}
	}

	internal GetExcludedLocationsJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
