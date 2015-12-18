using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class QlmThinHelper : ThinHelper
{
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
			base.Context.SetSymbolValue("THIN-SLOT", thinSlot);
		}
	}

	protected override Location FindEmptyTarget(MerchFlags notUsed, out MerchandizeResult result)
	{
		int thinSlot = GetThinSlot();
		result = MerchandizeResult.Success;
		return new Location(QLM.Deck.Number, thinSlot);
	}

	internal QlmThinHelper(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}

	private int GetThinSlot()
	{
		int result = 1;
		ErrorList errorList = new ErrorList();
		object symbolValue = base.Context.GetSymbolValue("THIN-SLOT", errorList);
		if (errorList.Count == 0)
		{
			result = (int)symbolValue;
		}
		return result;
	}
}
