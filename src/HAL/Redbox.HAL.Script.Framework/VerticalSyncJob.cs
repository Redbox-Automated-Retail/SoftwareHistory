using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class VerticalSyncJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int errors = 0;
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: false, "Sync", flushOnWrite: true, null);
		int slot = GetTop<int>();
		applicationLog.Write($"Start vertical sync for slot {slot}");
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			if (deck.IsQlmDeck)
			{
				return true;
			}
			MotionResult motionResult = MoveTo(deck.Number, slot, MoveMode.Get);
			if (motionResult != MotionResult.Success)
			{
				HandleError(deck.Number, slot, $"Move error: {motionResult.ToString()}.");
				errors++;
				return false;
			}
			GetResult getResult = ControllerService.Instance.Get(testOnEmpty: false, base.AppLog);
			if (!getResult.Success)
			{
				HandleError(deck.Number, slot, "Retrieving the disk failed.");
				errors++;
				return false;
			}
			if (MoveTo(deck.Number, slot, MoveMode.Put) != MotionResult.Success)
			{
				HandleError(deck.Number, slot, "Move to PUT position failed.");
				errors++;
				return false;
			}
			if (!ControllerService.Instance.Put(getResult.Previous, base.AppLog).Success)
			{
				HandleError(deck.Number, slot, "Filing the disk failed.");
				errors++;
				return false;
			}
			return true;
		});
		if (errors <= 0)
		{
			applicationLog.Write("The sync completed successfully.");
			ResultHelper.Instance.CreateInfoResult("SyncSuccess", "The test succeeded.");
			ControllerService.Instance.MoveToVend(MoveMode.Get, applicationLog);
		}
	}

	private void HandleError(int deck, int slot, string msg)
	{
		ResultHelper.Instance.CreateResult("SyncFailure", msg, deck, slot, null, null, null);
		AddError("Job error.");
	}
}
