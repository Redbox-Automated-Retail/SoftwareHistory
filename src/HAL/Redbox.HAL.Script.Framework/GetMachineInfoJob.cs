using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class GetMachineInfoJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		Deck deck = QLM.Instance.Deck;
		if (deck == null)
		{
			ServiceLogHelper.Instance.WriteInfoMessage("There is no QLM deck defined for this machine.");
			ResultHelper.Instance.CreateInfoResult("QlmMissingInfo", "There is no QLM deck defined for this machine.");
		}
		else
		{
			ResultHelper.Instance.CreateResult("QlmInfo", "Designated QLM deck for this machine, slot equals maximum number for this deck.", deck.Number, deck.NumberOfSlots, null, null, null);
		}
		DecksManager.Instance.ForAllReverseDecksDo(delegate(Deck deck2)
		{
			ResultHelper.Instance.CreateResult("DeckInfo", "Slot equals maximum number for this deck.", deck2.Number, deck2.NumberOfSlots, null, null, null);
			return true;
		});
		ResultHelper.Instance.CreateResult("MaxDeckInfo", "Deck equals maximum number for this machine.", DecksManager.Instance.DeckCount, 1, null, null, null);
		ResultHelper.Instance.CreateInfoResult("BufferSlot", ControllerConfiguration.Instance.ReturnSlotBuffer.ToString());
		ResultHelper.Instance.CreateInfoResult("MerchType", (deck == null) ? "VMZ2" : "QLM");
	}
}
