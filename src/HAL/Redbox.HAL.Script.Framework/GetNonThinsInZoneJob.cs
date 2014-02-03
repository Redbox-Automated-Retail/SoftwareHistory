using System.Collections.Generic;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class GetNonThinsInZoneJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		List<Location> nonThinItemsInCompressedZone = VMZ.Instance.GetNonThinItemsInCompressedZone();
		foreach (Location item in nonThinItemsInCompressedZone)
		{
			ResultHelper.Instance.CreateResult("NonThinInVMZ", "The item is a non-thin in the VMZ.", item.Deck, item.Slot, item.Matrix, null, null);
		}
		ResultHelper.Instance.CreateInfoResult("NonThinInVMZCount", nonThinItemsInCompressedZone.Count.ToString());
	}
}
