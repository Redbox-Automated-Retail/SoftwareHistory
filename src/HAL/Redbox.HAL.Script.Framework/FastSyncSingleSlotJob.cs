using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "quick-single-sync", Operand = "QUICK-SINGLE-SYNC")]
internal sealed class FastSyncSingleSlotJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog log = ApplicationLog.ConfigureLog(base.Context, logAppends: false, "Sync", flushOnWrite: false, "single-fast-sync");
		base.Context.ServiceLog("Begin fast sync.");
		ControllerService instance = ControllerService.Instance;
		int top = GetTop<int>();
		int top2 = GetTop<int>();
		int top3 = GetTop<int>();
		Deck deck = DecksManager.Instance.GetDeck(top2);
		_ = deck.NumberOfSlots;
		Location location = new Location(top2, top3);
		for (int i = 0; i < top; i++)
		{
			if (instance.MoveTo(location, MoveMode.Get, log) != MotionResult.Success)
			{
				AddError("Move failed.");
				break;
			}
			PerfFunctions.Wait(1000);
			GetResult getResult = instance.Get(log);
			if (!getResult.IsSlotEmpty)
			{
				if (!getResult.Success)
				{
					AddError("GET failed.");
					break;
				}
				instance.CenterItem(CenterDiskMethod.VendDoorAndBack);
				PerfFunctions.Wait(50);
				instance.CenterItem(CenterDiskMethod.DrumAndBack);
				PerfFunctions.SpinWait(50);
				if (instance.MoveTo(location, MoveMode.Put, log) != MotionResult.Success)
				{
					AddError("Move failed.");
					break;
				}
				PutResult putResult = instance.Put(getResult.Previous, log);
				if (!putResult.Success)
				{
					AddError("Put failed.");
					break;
				}
				PerfFunctions.Wait(1000);
			}
		}
	}

	internal FastSyncSingleSlotJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
