using System.Collections.Generic;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class GetExcludedLocationsJob : NativeJobAdapter
{
	private class LocationSorter : IComparer<Location>
	{
		public int Compare(Location x, Location y)
		{
			if (x.Deck == y.Deck)
			{
				if (y.Slot == x.Slot)
				{
					return 0;
				}
				if (x.Slot <= y.Slot)
				{
					return -1;
				}
				return 1;
			}
			if (x.Deck <= y.Deck)
			{
				return -1;
			}
			return 1;
		}
	}

	protected override void ExecuteInner()
	{
		List<Location> list = ControllerConfiguration.Instance.ComputeExcludedSlots();
		if (list.Count == 0)
		{
			ResultHelper.Instance.CreateInfoResult("NoExcludedSlots", "There are no excluded slots.");
			return;
		}
		list.Sort(new LocationSorter());
		foreach (Location item in list)
		{
			ResultHelper.Instance.CreateResult("ExcludedSlot", "The location is excluded.", item.Deck, item.Slot, null, null, null);
		}
	}
}
