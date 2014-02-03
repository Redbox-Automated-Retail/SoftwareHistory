using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class TestQlmJob : NativeJobAdapter
{
	private readonly int[] TargetSlots = new int[4] { 1, 20, 41, 62 };

	private QLM QLM => QLM.Instance;

	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: false, "QLM", flushOnWrite: true, null);
		applicationLog.Write("Start QLM test sync.");
		if (ControllerConfiguration.Instance.MerchandizingMode != MerchandizingMode.QLM)
		{
			applicationLog.Write("The kiosk is not configured with a QLM.");
			ResultHelper.Instance.CreateInfoResult("SyncFailure", "Kiosk is not configured for a QLM.");
			AddError("The kiosk is not configured for a QLM.");
			return;
		}
		if (QLM.Instance.Deck == null)
		{
			applicationLog.Write("The kiosk is not configured with a QLM.");
			ResultHelper.Instance.CreateInfoResult("SyncFailure", "Kiosk is not configured for a QLM.");
			AddError("The kiosk is not configured for a QLM.");
			return;
		}
		QlmStatus status = QLM.GetStatus();
		if (QlmStatus.Engaged != status)
		{
			applicationLog.Write($"The QLM status instruction returned {status.ToString()}");
			ResultHelper.Instance.CreateInfoResult("SyncFailure", "The QLM is not engaged.");
			AddError("There is no QLM in the bay.");
			return;
		}
		if (!PickerBoard.Instance.IsFull())
		{
			applicationLog.Write("There is not a disk in the picker for a test - bailing.");
			AddError("There is no disk in the picker.");
			ResultHelper.Instance.CreateInfoResult("SyncFailure", "There is no disk in the picker.");
			return;
		}
		int number = QLM.Deck.Number;
		int[] targetSlots = TargetSlots;
		foreach (int num in targetSlots)
		{
			if (!MoveAndPut(number, num, "UNKNOWN"))
			{
				ResultHelper.Instance.CreateResult("SyncFailure", "Filing the disk to the QLM failed.", number, num, null, null, null);
				AddError("Unable to PUT disk.");
				return;
			}
			PerfFunctions.Wait(1000);
			if (!MoveAndGet(number, num))
			{
				ResultHelper.Instance.CreateResult("SyncFailure", "Filing the disk to the QLM failed.", number, num, null, null, null);
				AddError("Failure to get disk.");
				return;
			}
			PerfFunctions.Wait(500);
		}
		applicationLog.Write("The sync completed successfully.");
		ControllerService.Instance.MoveToVend(MoveMode.Get, applicationLog);
	}

	private void HandleError(int deck, int slot)
	{
		ResultHelper.Instance.CreateResult("SyncFailure", "Retrieving the disk from the QLM failed.", deck, slot, null, null, null);
		AddError("Job error.");
	}
}
