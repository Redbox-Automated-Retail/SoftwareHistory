using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "clear-merch-status", Operand = "CLEAR-MERCH-STATUS")]
internal sealed class ClearMerchStatusJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int num = 0;
		try
		{
			if (!ControllerConfiguration.Instance.IsVMZMachine)
			{
				return;
			}
			int top = GetTop<int>();
			for (int i = 0; i < top; i++)
			{
				string top2 = GetTop<string>();
				Location location = InventoryService.Instance.Lookup(top2);
				if (location != null && SegmentManager.Instance.InCompressedZone(location) && !SegmentManager.Instance.IsNonThinType(location.Flags))
				{
					base.Context.ServiceLog("The flags at {0} are changed from {1} -> None", location.ToString(), location.Flags.ToString());
					location.ChangeMerchFlags(MerchFlags.None);
					num++;
				}
			}
		}
		finally
		{
			base.Context.CreateInfoResult("MerchFlagsResetCount", num.ToString());
		}
	}

	internal ClearMerchStatusJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
