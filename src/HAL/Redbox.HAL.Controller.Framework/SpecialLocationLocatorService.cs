using System;
using System.Collections.Generic;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public class SpecialLocationLocatorService
{
	private readonly List<SpecialLocation> Locations;

	public static SpecialLocationLocatorService Instance => Singleton<SpecialLocationLocatorService>.Instance;

	public SpecialLocation FindByName(string name)
	{
		foreach (SpecialLocation location in Locations)
		{
			if (name.Equals(location.Name, StringComparison.CurrentCultureIgnoreCase))
			{
				return location;
			}
		}
		return null;
	}

	public SpecialLocation FindByLocation(Location target)
	{
		foreach (SpecialLocation location in Locations)
		{
			if (location.Deck.HasValue && location.Deck.Value == target.Deck && target.Slot >= location.Slots.Start && target.Slot <= location.Slots.End)
			{
				return location;
			}
		}
		return null;
	}

	public DumpBin FindDumpBin()
	{
		foreach (SpecialLocation location in Locations)
		{
			if (location.Name.Equals("DumpBin"))
			{
				return location as DumpBin;
			}
		}
		return null;
	}

	internal bool ExcludeSlot(Deck deck, int slot)
	{
		foreach (SpecialLocation location in Locations)
		{
			if (location.ExcludeFromESP(deck, slot))
			{
				return true;
			}
		}
		return false;
	}

	private SpecialLocationLocatorService()
	{
		Locations = new List<SpecialLocation>();
		Locations.Add(DumpBin.Instance);
	}
}
