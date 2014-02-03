using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class QuickSlotTestJob : NativeJobAdapter
{
	private readonly int[] TestSlots = new int[2] { 30, 60 };

	protected override void ExecuteInner()
	{
		ApplicationLog appLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "KioskTest", flushOnWrite: false, "QuickTest");
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			if (deck.IsQlmDeck)
			{
				appLog.Write("The deck is a qlm deck - finish test.");
				return false;
			}
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
			if (!MoveAndPut(deck.Number, 1, "UNKNOWN"))
			{
				AddError("Unable to place the disk back into the source slot.");
				return false;
			}
			return true;
		});
	}
}
