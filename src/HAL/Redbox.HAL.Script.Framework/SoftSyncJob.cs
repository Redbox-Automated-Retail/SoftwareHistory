using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class SoftSyncJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ServiceLogHelper.Instance.WriteInfoMessage("Begin soft sync request.");
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			int numberOfSlots = deck.NumberOfSlots;
			for (int i = 0; i < numberOfSlots; i++)
			{
				int num = i + 1;
				string item = ControllerService.Instance.Inspect(deck.Number, num);
				ResultHelper.Instance.CreateResult("SoftSync", "The inventory location has been successfully inspected.", deck.Number, num, item, null, null);
			}
			return true;
		});
		ResultHelper.Instance.CreateInfoResult("SoftSyncEnd", "The soft sync request succeeded.");
	}
}
