using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal sealed class ClearMerchStatusJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int num = 0;
		try
		{
			if (MerchandizingMode.VMZ_Gen2 != ControllerConfiguration.Instance.MerchandizingMode)
			{
				return;
			}
			int top = GetTop<int>();
			for (int i = 0; i < top; i++)
			{
				string top2 = GetTop<string>();
				Location location = ControllerService.Instance.Lookup(top2);
				if (location != null && VMZ.Instance.LocInCompressedZone(location))
				{
					MerchFlags flags = location.Flags;
					if (MerchFlags.Thin == flags || MerchFlags.Rebalance == flags)
					{
						ServiceLogHelper.Instance.WriteInfoMessage("The flags at {0} are changed from {1} -> None", location.ToString(), flags.ToString());
						location.ChangeMerchFlags(MerchFlags.None);
						num++;
					}
				}
			}
		}
		finally
		{
			ResultHelper.Instance.CreateInfoResult("MerchFlagsResetCount", num.ToString());
		}
	}
}
