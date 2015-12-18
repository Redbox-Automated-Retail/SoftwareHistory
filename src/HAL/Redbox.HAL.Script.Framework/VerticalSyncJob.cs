using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "vertical-sync", Operand = "VERTSYNC")]
internal sealed class VerticalSyncJob : NativeJobAdapter
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
			MotionResult motionResult = ControllerService.Instance.MoveTo(deck.Number, slot, MoveMode.Get, base.AppLog);
			if (motionResult != MotionResult.Success)
			{
				HandleError(deck.Number, slot, $"Move error: {motionResult.ToString()}.");
				errors++;
				return false;
			}
			GetResult getResult = ControllerService.Instance.Get(base.AppLog);
			if (!getResult.Success)
			{
				HandleError(deck.Number, slot, "Retrieving the disk failed.");
				errors++;
				return false;
			}
			if (ControllerService.Instance.MoveTo(deck.Number, slot, MoveMode.Put, base.AppLog) != MotionResult.Success)
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
			base.Context.CreateInfoResult("SyncSuccess", "The test succeeded.");
			ControllerService.Instance.MoveToVend(MoveMode.Get, applicationLog);
		}
	}

	internal VerticalSyncJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}

	private void HandleError(int deck, int slot, string msg)
	{
		base.Context.CreateResult("SyncFailure", msg, deck, slot, null, null, null);
		AddError("Job error.");
	}
}
