using System.Collections.Generic;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class VMZRemovedThinsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "VMZ", flushOnWrite: false, "vmz-removed-disks");
		applicationLog.Write("** Remove thins from VMZ **");
		VMZ.Instance.DumpZone(applicationLog);
		int emptyCount = ControllerConfiguration.Instance.GetEmptyCount();
		List<Location> thinItems = VMZ.Instance.GetThinItems();
		try
		{
			foreach (Location item in thinItems)
			{
				ResultHelper.Instance.CreateResult("ThinTransferSuccessful", "The disk was removed from the VMZ.", item.Deck, item.Slot, item.Matrix, null, null);
				string code = "ThinDiskRemovedFromVMZ";
				if (item.Flags == MerchFlags.Rebalance)
				{
					code = "RebalanceDiskRemovedFromVMZ";
				}
				ResultHelper.Instance.CreateResult(code, "Disk merchandizing metadata.", item.Deck, item.Slot, item.Matrix, null, null);
			}
		}
		finally
		{
			emptyCount += thinItems.Count;
			thinItems.Clear();
		}
		thinItems = VMZ.Instance.GetNonThinItemsInCompressedZone();
		try
		{
			foreach (Location item2 in thinItems)
			{
				ResultHelper.Instance.CreateResult("NonThinRemoved", "The disk was removed from the VMZ.", item2.Deck, item2.Slot, item2.Matrix, null, null);
			}
		}
		finally
		{
			emptyCount += thinItems.Count;
			thinItems.Clear();
		}
		ResultHelper.Instance.CreateInfoResult("MachineEmptySlots", emptyCount.ToString());
	}
}
