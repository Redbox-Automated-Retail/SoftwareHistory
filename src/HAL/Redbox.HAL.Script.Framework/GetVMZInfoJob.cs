using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class GetVMZInfoJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		VMZSearchResult result = default(VMZSearchResult);
		VMZ.Instance.FindLastLocation(MerchFlags.Rebalance, ref result);
		if (!result.Found)
		{
			ResultHelper.Instance.CreateInfoResult("NoRebalances", "There are no rebalances in the VMZ.");
		}
		else
		{
			int countInZone = VMZ.Instance.GetCountInZone(MerchFlags.Rebalance);
			ResultHelper.Instance.CreateInfoResult("VMZRebalanceCount", countInZone.ToString());
			ResultHelper.Instance.CreateResult("LastRebalanceInfo", "The high rebalance was found.", result.Deck, result.Slot, result.Location.Matrix, null, null);
		}
		VMZSearchResult result2 = default(VMZSearchResult);
		VMZ.Instance.FindLastLocation(MerchFlags.Thin, ref result2);
		if (!result2.Found)
		{
			ResultHelper.Instance.CreateInfoResult("NoThins", "There are no rebalances in the VMZ.");
		}
		else
		{
			int countInZone2 = VMZ.Instance.GetCountInZone(MerchFlags.Thin);
			ResultHelper.Instance.CreateInfoResult("VMZThinCount", countInZone2.ToString());
			ResultHelper.Instance.CreateResult("LastThinInfo", "The high thin was found.", result2.Deck, result2.Slot, result2.Location.Matrix, null, null);
		}
		ResultHelper.Instance.CreateInfoResult("DumpBinInventoryCount", DumpBin.Instance.CurrentCount().ToString());
	}
}
