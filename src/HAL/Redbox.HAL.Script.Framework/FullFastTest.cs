using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class FullFastTest : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog appLog = ApplicationLog.ConfigureLog(base.Context, logAppends: false, "Sync", flushOnWrite: false, "fast-test");
		ServiceLogHelper.Instance.WriteInfoMessage("Begin fast sync.");
		ControllerService service = ControllerService.Instance;
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			int numberOfSlots = deck.NumberOfSlots;
			for (int i = 0; i < numberOfSlots; i++)
			{
				int slot = i + 1;
				if (service.MoveTo(deck.Number, slot, MoveMode.Get, appLog) != MotionResult.Success)
				{
					AddError("Move failed.");
					return false;
				}
				PerfFunctions.Wait(1000);
				GetResult getResult = service.Get(testOnEmpty: false, appLog);
				if (!getResult.IsSlotEmpty)
				{
					if (!getResult.Success)
					{
						AddError("GET failed.");
						return false;
					}
					service.CenterItem(CenterDiskMethod.VendDoorAndBack);
					PerfFunctions.SpinWait(50);
					service.CenterItem(CenterDiskMethod.DrumAndBack);
					PerfFunctions.Wait(50);
					if (service.MoveTo(deck.Number, slot, MoveMode.Put, appLog) != MotionResult.Success)
					{
						AddError("Move failed.");
						return false;
					}
					PutResult putResult = service.Put(getResult.Previous, appLog);
					if (!putResult.Success)
					{
						AddError("Put failed.");
						return false;
					}
					PerfFunctions.Wait(1000);
				}
			}
			return true;
		});
	}
}
