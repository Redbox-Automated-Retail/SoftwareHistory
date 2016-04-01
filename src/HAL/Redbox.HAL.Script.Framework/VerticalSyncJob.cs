using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "vertical-sync", Operand = "VERTSYNC")]
internal sealed class VerticalSyncJob : NativeJobAdapter, IPutObserver
{
	private IGetResult GetResult;

	public void OnSuccessfulPut(IPutResult result, IFormattedLog log)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		result.PutLocation.Flags = GetResult.Flags;
		result.PutLocation.ReturnDate = GetResult.ReturnTime;
		service.Save(result.PutLocation);
	}

	public void OnFailedPut(IPutResult result, IFormattedLog log)
	{
	}

	protected override void ExecuteInner()
	{
		int errors = 0;
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: false, "Sync", flushOnWrite: true, null);
		int slot = GetTop<int>();
		applicationLog.Write($"Start vertical sync for slot {slot}");
		base.DecksService.ForAllDecksDo(delegate(IDeck deck)
		{
			if (deck.IsQlm)
			{
				return true;
			}
			ErrorCodes errorCodes = base.MotionService.MoveTo(deck.Number, slot, MoveMode.Get, base.AppLog);
			if (errorCodes != ErrorCodes.Success)
			{
				HandleError(deck.Number, slot, $"Move error: {errorCodes.ToString()}.");
				errors++;
				return false;
			}
			GetResult = ControllerService.Instance.Get(base.AppLog);
			if (!GetResult.Success)
			{
				HandleError(deck.Number, slot, "Retrieving the disk failed.");
				errors++;
				return false;
			}
			if (base.MotionService.MoveTo(deck.Number, slot, MoveMode.Put, base.AppLog) != ErrorCodes.Success)
			{
				HandleError(deck.Number, slot, "Move to PUT position failed.");
				errors++;
				return false;
			}
			base.ControlSystem.TrackCycle();
			if (!ControllerService.Instance.Put(GetResult.Previous, base.AppLog).Success)
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
			base.MotionService.MoveVend(MoveMode.Get, applicationLog);
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
