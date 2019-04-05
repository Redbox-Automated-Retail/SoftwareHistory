using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "adjacent-bin-test-kent")]
internal sealed class AdjacentBinTestKentJob : NativeJobAdapter
{
	private readonly int[] SourceDiskSlots = new int[2] { 30, 60 };

	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "KioskTest", flushOnWrite: false, "AdjacentDumpSlotTest");
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		IDeck qlmDeck = service.QlmDeck;
		if (qlmDeck != null)
		{
			applicationLog.Write("The kiosk shows a QLM configuration - go no further.");
			return;
		}
		IDumpbinService service2 = ServiceLocator.Instance.GetService<IDumpbinService>();
		int deck = service2.PutLocation.Deck;
		IDeck byNumber = base.DecksService.GetByNumber(deck);
		if (!PutToAdjacentAndTest(byNumber.Number, 2, 81))
		{
			AddError("Test failure!");
		}
		else if (!PutToAdjacentAndTest(byNumber.Number, 3, 85))
		{
			AddError("Test failure!");
		}
		else if (!MoveAndGet(byNumber.Number, 4))
		{
			AddError("Test failure!");
		}
		else if (!TestAdjacentWithDisk(byNumber.Number, 81))
		{
			AddError("Test failure!");
		}
		else if (!TestAdjacentWithDisk(byNumber.Number, 85))
		{
			AddError("Test failure!");
		}
		else if (!FileDiskInPickerToBin())
		{
			AddError("Test failure!");
		}
		else if (!MoveAndGet(byNumber.Number, 81))
		{
			AddError("Test failure!");
		}
		else if (!FileDiskInPickerToBin())
		{
			AddError("Test failure!");
		}
		else if (!MoveAndGet(byNumber.Number, 85))
		{
			AddError("Test failure!");
		}
		else if (!FileDiskInPickerToBin())
		{
			AddError("Test failure!");
		}
	}

	internal AdjacentBinTestKentJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}

	private bool FileDiskInPickerToBin()
	{
		IDumpbinService service = ServiceLocator.Instance.GetService<IDumpbinService>();
		if (base.MotionService.MoveTo(service.PutLocation, MoveMode.Put, base.AppLog) != ErrorCodes.Success)
		{
			AddError("Test failure!");
			return false;
		}
		base.ControlSystem.TrackCycle();
		IControllerService service2 = ServiceLocator.Instance.GetService<IControllerService>();
		if (!service2.Put("UNKNOWN").Success)
		{
			AddError("Test failure.");
			return false;
		}
		return true;
	}

	private bool TestAdjacentWithDisk(int deck, int testSlot)
	{
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		if (service.MoveTo(deck, testSlot, MoveMode.Put, base.AppLog) != ErrorCodes.Success)
		{
			AddError("Test failure!");
			return false;
		}
		base.ControlSystem.TrackCycle();
		IControllerService service2 = ServiceLocator.Instance.GetService<IControllerService>();
		IPutResult putResult = service2.Put("UNKNOWN");
		if (!putResult.IsSlotInUse)
		{
			return false;
		}
		return true;
	}

	private bool PutToAdjacentAndTest(int deck, int slot, int targetSlot)
	{
		if (!MoveAndGet(deck, slot))
		{
			AddError("Unable to retrieve source disk.");
			return false;
		}
		if (!MoveAndPut(deck, targetSlot, "UNKNOWN"))
		{
			AddError("Unable to put disk into adjacent slot.");
			return false;
		}
		return true;
	}
}
