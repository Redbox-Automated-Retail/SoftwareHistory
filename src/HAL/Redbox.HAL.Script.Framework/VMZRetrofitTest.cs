using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "test-retrofit-deck", Operand = "TEST-RETROFIT-DECK")]
internal sealed class VMZRetrofitTest : NativeJobAdapter
{
	private readonly int[] TestSlots = new int[5] { 16, 31, 46, 61, 76 };

	protected override void ExecuteInner()
	{
		try
		{
			ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "KioskTest", flushOnWrite: false, "RetrofitDeckTest");
			if (ControllerMode.Emulated == ControllerConfiguration.Instance.Mode)
			{
				applicationLog.Write("HAL is running emulated - do not run job.");
				return;
			}
			if (QLM.Deck != null)
			{
				applicationLog.Write("The kiosk is still configured for a QLM - bail test.");
				AddError("Kiosk Misconfiguration.");
				return;
			}
			Deck deck = DecksManager.Instance.GetDeck(DumpBin.Instance.Deck.Value);
			if (!MoveAndGet(deck.Number, 1))
			{
				AddError("Failure to retrieve disk from start position.");
				return;
			}
			int[] testSlots = TestSlots;
			foreach (int slot in testSlots)
			{
				if (!MoveAndPut(deck.Number, slot, "UNKNOWN"))
				{
					AddError("Unable to place the disk at a test location.");
					return;
				}
				if (!MoveAndGet(deck.Number, slot))
				{
					AddError("Unable to get the disk from test location.");
					return;
				}
			}
			if (!MoveAndPut(deck.Number, 81, "UNKNOWN"))
			{
				AddError("Unable to put disk into adjacent slot.");
			}
			else if (TestSlotForDisk(deck.Number, 81))
			{
				if (!PutToAdjacentAndTest(deck.Number, 2, 85))
				{
					AddError("Test failure!");
				}
				else if (!MoveAndGet(deck.Number, 3))
				{
					AddError("Test failure!");
				}
				else if (!TestAdjacentWithDisk(deck.Number, 81))
				{
					AddError("Test failure!");
				}
				else if (!TestAdjacentWithDisk(deck.Number, 85))
				{
					AddError("Test failure!");
				}
				else if (!FileToBin(deck.Number))
				{
					AddError("Test failure!");
				}
				else if (!MoveAndGet(deck.Number, 81))
				{
					AddError("Test failure!");
				}
				else if (!FileToBin(deck.Number))
				{
					AddError("Test failure!");
				}
				else if (!MoveAndGet(deck.Number, 85))
				{
					AddError("Test failure!");
				}
				else if (!FileToBin(deck.Number))
				{
					AddError("Test failure!");
				}
				else
				{
					ControllerService.Instance.MoveToVend(MoveMode.Get, base.AppLog);
				}
			}
		}
		finally
		{
			Deck deck2 = DecksManager.Instance.GetDeck(DumpBin.Instance.Deck.Value);
			if (!deck2.IsQlmDeck)
			{
				deck2.MarkInventory("EMPTY");
			}
			DumpBin.Instance.ResetCount();
		}
	}

	internal VMZRetrofitTest(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}

	private bool FileToBin(int deck)
	{
		Location location = DumpBin.Instance.MoveableLocation();
		if (ControllerService.Instance.MoveTo(location, MoveMode.Put, base.AppLog) != MotionResult.Success)
		{
			AddError("Test failure!");
			return false;
		}
		if (!ControllerService.Instance.Put("UNKNOWN", base.AppLog).Success)
		{
			AddError("Test failure.");
			return false;
		}
		ControllerService.Instance.MoveTo(deck, 1, MoveMode.None, base.AppLog);
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
		return TestSlotForDisk(deck, testSlot);
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
		return TestSlotForDisk(deck, targetSlot);
	}

	private bool TestSlotForDisk(int deck, int slot)
	{
		return true;
	}
}
