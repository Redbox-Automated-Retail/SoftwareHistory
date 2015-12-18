using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "vmz-removed-thins", Operand = "VMZ-REMOVED-THINS")]
internal sealed class VMZRemovedThinsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "VMZ", flushOnWrite: false, "vmz-removed-disks");
		applicationLog.Write("** Remove thins from VMZ **");
		VMZ.Instance.DumpZone(applicationLog);
		int totalEmptyNow = InventoryService.Instance.GetEmptyCount();
		VMZSearchResult high = default(VMZSearchResult);
		SegmentManager.Instance.FindHighest(ref high);
		if (high.Found)
		{
			VMZ.Instance.ForAllBetweenDo(delegate(Location loc)
			{
				if (loc.IsEmpty)
				{
					return true;
				}
				if (SegmentManager.Instance.IsThinType(loc.Flags))
				{
					base.Context.CreateResult("ThinTransferSuccessful", "The disk was removed from the VMZ.", loc.Deck, loc.Slot, loc.Matrix, null, null);
					base.Context.CreateResult($"{loc.Flags.ToString()}DiskRemovedFromVMZ", "Disk merchandizing metadata.", loc.Deck, loc.Slot, loc.Matrix, null, null);
				}
				else
				{
					base.Context.CreateResult("NonThinRemoved", "The disk was removed from the VMZ.", loc.Deck, loc.Slot, loc.Matrix, null, null);
				}
				totalEmptyNow++;
				return true;
			}, ref high);
		}
		base.Context.CreateInfoResult("MachineEmptySlots", totalEmptyNow.ToString());
	}

	internal VMZRemovedThinsJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
