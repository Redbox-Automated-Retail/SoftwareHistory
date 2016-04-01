using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "adjacent-bin-test", Operand = "ADJACENT-BIN-TEST")]
internal sealed class AdjacentDumpSlotTestJob : NativeJobAdapter
{
	private readonly int[] SourceDiskSlots = new int[2] { 30, 60 };

	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "KioskTest", flushOnWrite: false, "AdjacentDumpSlotTest");
		if (!ControllerConfiguration.Instance.IsVMZMachine)
		{
			applicationLog.Write("The kiosk shows a QLM configuration - go no further.");
			return;
		}
		IDumpbinService service = ServiceLocator.Instance.GetService<IDumpbinService>();
		int deck = service.PutLocation.Deck;
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
		else if (!FileToBin(byNumber.Number))
		{
			AddError("Test failure!");
		}
		else if (!MoveAndGet(byNumber.Number, 81))
		{
			AddError("Test failure!");
		}
		else if (!FileToBin(byNumber.Number))
		{
			AddError("Test failure!");
		}
		else if (!MoveAndGet(byNumber.Number, 85))
		{
			AddError("Test failure!");
		}
		else if (!FileToBin(byNumber.Number))
		{
			AddError("Test failure!");
		}
	}

	internal AdjacentDumpSlotTestJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}

	private bool FileToBin(int deck)
	{
		IDumpbinService service = ServiceLocator.Instance.GetService<IDumpbinService>();
		if (service == null || !service.Active)
		{
			base.AppLog.Write("The kiosk is not configured for a dumpbin.");
			AddError("The dumpbin is not configured.");
		}
		if (base.MotionService.MoveTo(service.MoveableLocation(), MoveMode.Put, base.AppLog) != ErrorCodes.Success)
		{
			AddError("Test failure!");
			return false;
		}
		base.ControlSystem.TrackCycle();
		if (!ControllerService.Instance.Put("UNKNOWN", base.AppLog).Success)
		{
			AddError("Test failure.");
			return false;
		}
		return true;
	}

	private bool TestAdjacentWithDisk(int deck, int testSlot)
	{
		if (base.MotionService.MoveTo(deck, testSlot, MoveMode.Put, base.AppLog) != ErrorCodes.Success)
		{
			AddError("Test failure!");
			return false;
		}
		base.ControlSystem.TrackCycle();
		IPutResult putResult = ControllerService.Instance.Put("UNKNOWN", base.AppLog);
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
