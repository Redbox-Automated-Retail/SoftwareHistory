using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class QlmThinHelper : ThinHelper
{
	private QLM QLM => QLM.Instance;

	protected override MerchandizeResult ThinDiskInner(string id, Location source, Location notUsed, MerchFlags mf)
	{
		int thinSlot = GetThinSlot();
		try
		{
			MerchandizeResult merchandizeResult;
			do
			{
				if (thinSlot > QLM.Deck.NumberOfSlots)
				{
					return MerchandizeResult.QLMFull;
				}
				Location target = new Location(QLM.Deck.Number, thinSlot++);
				merchandizeResult = ThinDiskToLocation(source, target, id, mf);
			}
			while (MerchandizeResult.SlotInUse == merchandizeResult);
			return merchandizeResult;
		}
		finally
		{
			ExecutionEngine.Instance.ActiveContext.SetSymbolValue("THIN-SLOT", thinSlot);
		}
	}

	protected override MerchandizeResult ProcessSuccessfulThin(Location loc, string id, MerchFlags notUsedHere)
	{
		ResultHelper.Instance.CreateResult("ThinTransferSuccessful", "The item was transferred from storage to the QLM.", loc.Deck, loc.Slot, id, null, null);
		return MerchandizeResult.Success;
	}

	protected override Location PrepTarget(MerchFlags notUsed, out MerchandizeResult result)
	{
		int thinSlot = GetThinSlot();
		result = MerchandizeResult.Success;
		return new Location(QLM.Deck.Number, thinSlot);
	}

	private int GetThinSlot()
	{
		int result = 1;
		ErrorList errorList = new ErrorList();
		object symbolValue = ExecutionEngine.Instance.ActiveContext.GetSymbolValue("THIN-SLOT", errorList);
		if (errorList.Count == 0)
		{
			result = (int)symbolValue;
		}
		return result;
	}
}
