using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "qlm-test-sync", Operand = "QLM-TEST-SYNC")]
internal sealed class TestQlmJob : NativeJobAdapter
{
	private readonly int[] TargetSlots = new int[4] { 1, 20, 41, 62 };

	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: false, "QLM", flushOnWrite: true, null);
		applicationLog.Write("Start QLM test sync.");
		if (ControllerConfiguration.Instance.IsVMZMachine)
		{
			applicationLog.Write("The kiosk is not configured with a QLM.");
			base.Context.CreateInfoResult("SyncFailure", "Kiosk is not configured for a QLM.");
			AddError("The kiosk is not configured for a QLM.");
			return;
		}
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		IDecksService service2 = ServiceLocator.Instance.GetService<IDecksService>();
		if (service2.QlmDeck == null)
		{
			applicationLog.Write("The kiosk is not configured with a QLM.");
			base.Context.CreateInfoResult("SyncFailure", "Kiosk is not configured for a QLM.");
			AddError("The kiosk is not configured for a QLM.");
			return;
		}
		QlmStatus qlmStatus = service.GetQlmStatus();
		if (QlmStatus.Engaged != qlmStatus)
		{
			applicationLog.Write($"The QLM status instruction returned {qlmStatus.ToString()}");
			base.Context.CreateInfoResult("SyncFailure", "The QLM is not engaged.");
			AddError("There is no QLM in the bay.");
			return;
		}
		if (!PickerBoard.IsFull())
		{
			applicationLog.Write("There is not a disk in the picker for a test - bailing.");
			AddError("There is no disk in the picker.");
			base.Context.CreateInfoResult("SyncFailure", "There is no disk in the picker.");
			return;
		}
		IRuntimeService service3 = ServiceLocator.Instance.GetService<IRuntimeService>();
		int number = service2.QlmDeck.Number;
		int[] targetSlots = TargetSlots;
		foreach (int num in targetSlots)
		{
			if (!MoveAndPut(number, num, "UNKNOWN"))
			{
				base.Context.CreateResult("SyncFailure", "Filing the disk to the QLM failed.", number, num, null, null, null);
				AddError("Unable to PUT disk.");
				return;
			}
			service3.Wait(1000);
			if (!MoveAndGet(number, num))
			{
				base.Context.CreateResult("SyncFailure", "Filing the disk to the QLM failed.", number, num, null, null, null);
				AddError("Failure to get disk.");
				return;
			}
			service3.Wait(500);
		}
		applicationLog.Write("The sync completed successfully.");
		base.MotionService.MoveVend(MoveMode.Get, applicationLog);
	}

	internal TestQlmJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}

	private void HandleError(int deck, int slot)
	{
		base.Context.CreateResult("SyncFailure", "Retrieving the disk from the QLM failed.", deck, slot, null, null, null);
		AddError("Job error.");
	}
}
