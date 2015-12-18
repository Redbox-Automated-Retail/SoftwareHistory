using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "adjacent-bin-test", Operand = "ADJACENT-BIN-TEST")]
internal sealed class AdjacentDumpSlotTestJob : NativeJobAdapter
{
	private readonly int[] SourceDiskSlots = new int[2] { 30, 60 };

	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "KioskTest", flushOnWrite: false, "AdjacentDumpSlotTest");
		Deck deck = QLM.Deck;
		if (deck != null)
		{
			applicationLog.Write("The kiosk shows a QLM configuration - go no further.");
			return;
		}
		int value = DumpBin.Instance.Deck.Value;
		Deck deck2 = DecksManager.Instance.GetDeck(value);
		if (!PutToAdjacentAndTest(deck2.Number, 2, 81))
		{
			AddError("Test failure!");
		}
		else if (!PutToAdjacentAndTest(deck2.Number, 3, 85))
		{
			AddError("Test failure!");
		}
		else if (!MoveAndGet(deck2.Number, 4))
		{
			AddError("Test failure!");
		}
		else if (!TestAdjacentWithDisk(deck2.Number, 81))
		{
			AddError("Test failure!");
		}
		else if (!TestAdjacentWithDisk(deck2.Number, 85))
		{
			AddError("Test failure!");
		}
		else if (!FileToBin(deck2.Number))
		{
			AddError("Test failure!");
		}
		else if (!MoveAndGet(deck2.Number, 81))
		{
			AddError("Test failure!");
		}
		else if (!FileToBin(deck2.Number))
		{
			AddError("Test failure!");
		}
		else if (!MoveAndGet(deck2.Number, 85))
		{
			AddError("Test failure!");
		}
		else if (!FileToBin(deck2.Number))
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
		DumpBin dumpBin = SpecialLocationLocatorService.Instance.FindDumpBin();
		if (dumpBin == null)
		{
			base.AppLog.Write("The kiosk is not configured for a dumpbin.");
			AddError("The dumpbin is not configured.");
		}
		if (ControllerService.Instance.MoveTo(dumpBin.MoveableLocation(), MoveMode.Put, base.AppLog) != MotionResult.Success)
		{
			AddError("Test failure!");
			return false;
		}
		if (!ControllerService.Instance.Put("UNKNOWN", base.AppLog).Success)
		{
			AddError("Test failure.");
			return false;
		}
		return true;
	}

	private bool TestAdjacentWithDisk(int deck, int testSlot)
	{
		if (ControllerService.Instance.MoveTo(deck, testSlot, MoveMode.Put, base.AppLog) != MotionResult.Success)
		{
			AddError("Test failure!");
			return false;
		}
		PutResult putResult = ControllerService.Instance.Put("UNKNOWN", base.AppLog);
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
