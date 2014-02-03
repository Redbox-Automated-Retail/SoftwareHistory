using System.Collections.Generic;

namespace Redbox.HAL.Controller.Framework;

internal class DeckSlotSorter : IComparer<DeckData>
{
	public int Compare(DeckData x, DeckData y)
	{
		if (x.Slot > y.Slot)
		{
			return 1;
		}
		if (x.Slot >= y.Slot)
		{
			return 0;
		}
		return -1;
	}
}
