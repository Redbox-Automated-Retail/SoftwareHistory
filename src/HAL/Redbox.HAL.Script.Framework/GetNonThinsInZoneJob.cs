using System.Collections.Generic;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-nonthins-in-vmz", Operand = "GET-NONTHINS-IN-VMZ")]
internal sealed class GetNonThinsInZoneJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		List<Location> nonThinItemsInCompressedZone = SegmentManager.Instance.GetNonThinItemsInCompressedZone();
		using (new DisposeableList<Location>(nonThinItemsInCompressedZone))
		{
			foreach (Location item in nonThinItemsInCompressedZone)
			{
				base.Context.CreateResult("NonThinInVMZ", "The item is a non-thin in the VMZ.", item.Deck, item.Slot, item.Matrix, null, null);
			}
			base.Context.CreateInfoResult("NonThinInVMZCount", nonThinItemsInCompressedZone.Count.ToString());
		}
	}

	internal GetNonThinsInZoneJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
