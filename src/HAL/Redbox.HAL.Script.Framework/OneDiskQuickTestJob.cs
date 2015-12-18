using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "one-disk-quick-deck-test", Operand = "ONE-DISK-QUICK-DECK-TEST")]
internal sealed class OneDiskQuickTestJob : NativeJobAdapter
{
	private readonly int[] TestSlots = new int[2] { 30, 60 };

	protected override void ExecuteInner()
	{
		ApplicationLog appLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "KioskTest", flushOnWrite: false, "OneDiskQuickTest");
		int errors = 0;
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			if (deck.IsQlmDeck)
			{
				appLog.Write("The deck is a qlm deck - finish test.");
				return true;
			}
			if (!TestDeck(deck))
			{
				errors++;
				return false;
			}
			Deck deck2 = DecksManager.Instance.GetDeck(deck.Number + 1);
			if (deck2 == null || deck2.IsQlmDeck)
			{
				return true;
			}
			if (!MoveAndPut(deck2.Number, 1, "UNKNOWN"))
			{
				AddError("Unable to place the into the slot.");
				errors++;
				return false;
			}
			return true;
		});
		if (errors <= 0)
		{
			if (!MoveAndPut(1, 1, "UNKNOWN"))
			{
				AddError("Unable to place the disk back into the source slot.");
			}
			ControllerService.Instance.MoveToVend(MoveMode.Get, appLog);
		}
	}

	internal OneDiskQuickTestJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}

	private bool TestDeck(Deck deck)
	{
		if (!MoveAndGet(deck.Number, 1))
		{
			AddError("Failure to retrieve disk from start position.");
			return false;
		}
		int[] testSlots = TestSlots;
		foreach (int slot in testSlots)
		{
			if (!MoveAndPut(deck.Number, slot, "UNKNOWN"))
			{
				AddError("Unable to place the disk at a test location.");
				return false;
			}
			if (!MoveAndGet(deck.Number, slot))
			{
				AddError("Unable to get the disk from test location.");
				return false;
			}
		}
		return true;
	}
}
