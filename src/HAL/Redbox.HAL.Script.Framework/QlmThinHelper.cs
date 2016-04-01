using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class QlmThinHelper : ThinHelper
{
	protected override MerchandizeResult ThinDiskInner(string id, ILocation source, ILocation notUsed, MerchFlags mf)
	{
		int thinSlot = GetThinSlot();
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		try
		{
			MerchandizeResult merchandizeResult;
			do
			{
				if (thinSlot > QLM.Deck.NumberOfSlots)
				{
					return MerchandizeResult.QLMFull;
				}
				ILocation target = service.Get(QLM.Deck.Number, thinSlot++);
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

	protected override ILocation FindEmptyTarget(MerchFlags notUsed, out MerchandizeResult result)
	{
		int thinSlot = GetThinSlot();
		result = MerchandizeResult.Success;
		IDeck deck = QLM.Deck;
		if (thinSlot > deck.NumberOfSlots)
		{
			result = MerchandizeResult.QLMFull;
			return null;
		}
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		return service.Get(QLM.Deck.Number, thinSlot);
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
