using System;
using System.Collections.Generic;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public class EmptySearchResult : IDisposable
{
	public readonly List<Location> EmptyLocations = new List<Location>();

	private bool m_disposed;

	public int FoundEmpty => EmptyLocations.Count;

	public void Dispose()
	{
		if (!m_disposed)
		{
			m_disposed = true;
			EmptyLocations.Clear();
			GC.SuppressFinalize(this);
		}
	}

	public bool FindMultipleEmptyLocations()
	{
		int returnSlotBuffer = ControllerConfiguration.Instance.ReturnSlotBuffer;
		int num = ControllerConfiguration.Instance.GetMachineEmptyCount() - returnSlotBuffer;
		if (num <= 0)
		{
			LogHelper.Instance.Log("FindEmptyOutsideOf: There are no empty slots available.");
			return false;
		}
		if (num > ControllerConfiguration.Instance.PutAwayItemAttempts)
		{
			num = ControllerConfiguration.Instance.PutAwayItemAttempts;
		}
		foreach (ESPNode item in ControllerConfiguration.Instance.ESPHelper.EmptyPattern)
		{
			Deck deck = DecksManager.Instance.GetDeck(item.Deck);
			string idForSlot = deck.GetIdForSlot(item.Slot);
			if ("EMPTY" == idForSlot)
			{
				EmptyLocations.Add(new Location(deck.Number, item.Slot));
				if (EmptyLocations.Count == num)
				{
					break;
				}
			}
		}
		return EmptyLocations.Count > 0;
	}

	public bool FindOneEmpty()
	{
		foreach (ESPNode item in ControllerConfiguration.Instance.ESPHelper.EmptyPattern)
		{
			Deck deck = DecksManager.Instance.GetDeck(item.Deck);
			string idForSlot = deck.GetIdForSlot(item.Slot);
			if ("EMPTY" == idForSlot)
			{
				EmptyLocations.Add(new Location(deck.Number, item.Slot));
				break;
			}
		}
		return EmptyLocations.Count > 0;
	}
}
