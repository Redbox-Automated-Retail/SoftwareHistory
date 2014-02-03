using System;
using System.Collections.Generic;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public class EmptySearchResult : IDisposable
{
	private List<Location> m_emptySlots = new List<Location>();

	private bool m_disposed;

	public List<Location> EmptyLocations => m_emptySlots;

	public int FoundEmpty => m_emptySlots.Count;

	public void Dispose()
	{
		if (!m_disposed)
		{
			m_disposed = true;
			m_emptySlots.Clear();
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
		ESPNode[] emptyPattern = ESPHelper.Instance.EmptyPattern;
		foreach (ESPNode eSPNode in emptyPattern)
		{
			Deck deck = DecksManager.Instance.GetDeck(eSPNode.Deck);
			string idForSlot = deck.GetIdForSlot(eSPNode.Slot);
			if ("EMPTY" == idForSlot)
			{
				m_emptySlots.Add(new Location(deck.Number, eSPNode.Slot));
				if (m_emptySlots.Count == num)
				{
					break;
				}
			}
		}
		return m_emptySlots.Count > 0;
	}

	public bool FindOneEmpty()
	{
		ESPNode[] emptyPattern = ESPHelper.Instance.EmptyPattern;
		foreach (ESPNode eSPNode in emptyPattern)
		{
			Deck deck = DecksManager.Instance.GetDeck(eSPNode.Deck);
			string idForSlot = deck.GetIdForSlot(eSPNode.Slot);
			if ("EMPTY" == idForSlot)
			{
				m_emptySlots.Add(new Location(deck.Number, eSPNode.Slot));
				break;
			}
		}
		return m_emptySlots.Count > 0;
	}
}
