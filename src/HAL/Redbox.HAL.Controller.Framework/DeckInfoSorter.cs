using System.Collections.Generic;

namespace Redbox.HAL.Controller.Framework;

internal class DeckInfoSorter : IComparer<DeckData>
{
	public int Compare(DeckData x, DeckData y)
	{
		if (x.Deck == y.Deck)
		{
			return CompareSlot(x.Slot, y.Slot);
		}
		if (x.Deck >= y.Deck)
		{
			return 1;
		}
		return -1;
	}

	private int CompareSlot(int s1, int s2)
	{
		if (s1 == s2)
		{
			return 0;
		}
		if (s1 <= s2)
		{
			return -1;
		}
		return 1;
	}
}
