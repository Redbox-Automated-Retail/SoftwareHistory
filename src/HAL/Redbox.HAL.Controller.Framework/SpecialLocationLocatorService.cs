using System;
using System.Collections.Generic;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public class SpecialLocationLocatorService
{
	private List<SpecialLocation> m_locations;

	public static SpecialLocationLocatorService Instance => Singleton<SpecialLocationLocatorService>.Instance;

	public SpecialLocation FindByName(string name)
	{
		foreach (SpecialLocation location in m_locations)
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
		foreach (SpecialLocation location in m_locations)
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
		foreach (SpecialLocation location in m_locations)
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
		foreach (SpecialLocation location in m_locations)
		{
			if (location.ExcludeFromESP(deck, slot))
			{
				return true;
			}
		}
		return false;
	}

	private bool FindByOffset(Axis axis, int? xOffset, int? yOffset)
	{
		foreach (SpecialLocation location in m_locations)
		{
			if (location.Deck.HasValue)
			{
				Deck deck = DecksManager.Instance.GetDeck(location.Deck.Value);
				int slotOffset = deck.GetSlotOffset(location.Slots.Start);
				int slotOffset2 = deck.GetSlotOffset(location.Slots.End);
				if (xOffset.Value >= slotOffset && xOffset.Value <= slotOffset2)
				{
					return false;
				}
				if (Axis.XY == axis)
				{
					if (xOffset.Value >= slotOffset && xOffset.Value <= slotOffset2)
					{
						return false;
					}
					if (yOffset.Value <= deck.YOffset)
					{
						return false;
					}
				}
				else if (axis == Axis.X)
				{
					if (xOffset.Value >= slotOffset && xOffset.Value <= slotOffset2)
					{
						return false;
					}
				}
				else if (Axis.Y == axis && xOffset.Value <= deck.YOffset)
				{
					return false;
				}
			}
			else if (Axis.XY == axis)
			{
				if (yOffset.HasValue && yOffset.Value <= location.Offset)
				{
					LogHelper.Instance.Log($"MoveAbsolueLocation: move to {yOffset.Value.ToString()} on axis {axis.ToString()} failed.", LogEntryType.Info);
					return false;
				}
			}
			else if (Axis.Y == axis && xOffset.HasValue && xOffset.Value <= location.Offset)
			{
				LogHelper.Instance.Log($"MoveAbsolueLocation: move to {xOffset.Value.ToString()} on axis {axis.ToString()} failed.", LogEntryType.Info);
				return false;
			}
		}
		return true;
	}

	private SpecialLocationLocatorService()
	{
		m_locations = new List<SpecialLocation>();
		m_locations.Add(DumpBin.Instance);
	}
}
