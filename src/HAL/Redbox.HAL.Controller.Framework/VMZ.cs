using System;
using System.Collections.Generic;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class VMZ : IDisposable
{
	private bool Disposed;

	private readonly Range m_slots = new Range(1, 15);

	private readonly Location[] m_thinPattern;

	public static VMZ Instance => Singleton<VMZ>.Instance;

	public Location First => m_thinPattern[0];

	public Range Bounds => new Range(m_slots.Start, m_slots.End);

	internal int LastIndex => m_thinPattern.Length - 1;

	public void Dispose()
	{
		DisposeInner(fromDispose: true);
	}

	~VMZ()
	{
		DisposeInner(fromDispose: false);
	}

	public int ReserveUnload(int lowDeck)
	{
		int reserved = 0;
		using (ExecutionTimer executionTimer = new ExecutionTimer())
		{
			DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
			{
				if (deck.Number <= lowDeck)
				{
					reserved += deck.SwapEmptyWith("UNKNOWN", MerchFlags.Unload, m_slots).Count;
				}
				return true;
			});
			executionTimer.Stop();
			LogHelper.Instance.Log("Reserved unloads through deck {0} ( {1}ms )", lowDeck, executionTimer.ElapsedMilliseconds);
		}
		return reserved;
	}

	public List<Location> FindFarthestEmptyOutsideOf(Location top)
	{
		List<Location> list = new List<Location>();
		int returnSlotBuffer = ControllerConfiguration.Instance.ReturnSlotBuffer;
		int num = ControllerConfiguration.Instance.GetMachineEmptyCount() - returnSlotBuffer;
		if (num <= 0)
		{
			LogHelper.Instance.Log("FindFarthestEmptyOutsideOf: There are no empty slots available.");
			return list;
		}
		if (num > ControllerConfiguration.Instance.PutAwayItemAttempts)
		{
			num = ControllerConfiguration.Instance.PutAwayItemAttempts;
		}
		foreach (ESPNode item in ControllerConfiguration.Instance.ESPHelper.EmptyPattern)
		{
			Location location = item;
			if (!top.Equals(location))
			{
				if (location.IsEmpty)
				{
					list.Add(location);
				}
				if (list.Count == num)
				{
					break;
				}
				continue;
			}
			break;
		}
		return list;
	}

	public void ForAllBetweenDo(Predicate<Location> callback, ref VMZSearchResult end)
	{
		if (end.Index >= 0 && end.Index < m_thinPattern.Length)
		{
			Execute(callback, 0, end.Index);
		}
	}

	public void ForAllBetweenDo(Predicate<Location> callback, ref VMZSearchResult low, ref VMZSearchResult high)
	{
		if (low.Index >= 0 && low.Index < m_thinPattern.Length && high.Index >= 0 && high.Index < m_thinPattern.Length)
		{
			Execute(callback, low.Index, high.Index);
		}
	}

	public void FindFirstLocation(MerchFlags flags, ref VMZSearchResult result)
	{
		for (int i = 0; i < m_thinPattern.Length; i++)
		{
			if (m_thinPattern[i].Flags == flags)
			{
				result.Index = i;
				return;
			}
		}
		result.Invalidate();
	}

	public void FindHighestLocation(MerchFlags flags, ref VMZSearchResult result)
	{
		for (int num = m_thinPattern.Length - 1; num >= 0; num--)
		{
			Location location = m_thinPattern[num];
			if (location.Flags == flags)
			{
				result.Index = num;
				return;
			}
		}
		result.Invalidate();
	}

	public int ResetCompressedZone(ref VMZSearchResult high)
	{
		int num = 0;
		using ExecutionTimer executionTimer = new ExecutionTimer();
		int deckCount = DecksManager.Instance.DeckCount;
		List<Location>[] locs = new List<Location>[deckCount];
		for (int i = 0; i < deckCount; i++)
		{
			locs[i] = new List<Location>();
		}
		try
		{
			Execute(delegate(Location loc)
			{
				if (!loc.IsEmpty)
				{
					locs[loc.Deck - 1].Add(loc);
				}
				return true;
			}, 0, high.Index);
			for (int num2 = 0; num2 < locs.Length; num2++)
			{
				if (locs[num2].Count > 0)
				{
					Deck deck = DecksManager.Instance.GetDeck(num2 + 1);
					num += deck.BulkUpdate(locs[num2], "EMPTY", MerchFlags.None);
				}
			}
		}
		finally
		{
			List<Location>[] array = locs;
			foreach (List<Location> list in array)
			{
				list.Clear();
			}
			locs = null;
		}
		executionTimer.Stop();
		LogHelper.Instance.Log("VMZ.ClearZone: reset {0} locations ( time = {1}ms ).", num, executionTimer.ElapsedMilliseconds);
		return num;
	}

	public void DumpZone(IFormattedLog log)
	{
		if (!ControllerConfiguration.Instance.DumpVMZState)
		{
			return;
		}
		log.WriteFormatted("-- Dump VMZ Locations --");
		using VMZIterator vMZIterator = new VMZIterator();
		do
		{
			log.WriteFormatted("Deck {0} Slot {1} Inventory {2} Flags {3} Stuck {4}", vMZIterator.Location.Deck, vMZIterator.Location.Slot, vMZIterator.Location.Matrix, vMZIterator.Location.Flags.ToString(), vMZIterator.Location.StuckCount);
		}
		while (vMZIterator.Up());
	}

	internal Location LocationAt(int idx)
	{
		if (idx >= 0 && idx < m_thinPattern.Length)
		{
			return m_thinPattern[idx];
		}
		return null;
	}

	internal int FindIndex(Location loc)
	{
		for (int i = 0; i < m_thinPattern.Length; i++)
		{
			if (m_thinPattern[i].Slot == loc.Slot && m_thinPattern[i].Deck == loc.Deck)
			{
				return i;
			}
		}
		return -1;
	}

	private bool LocIsLowerThan(Location loc, Location highMark)
	{
		if (loc.Equals(highMark))
		{
			return false;
		}
		if (loc.Deck < highMark.Deck)
		{
			return false;
		}
		if (loc.Deck > highMark.Deck)
		{
			return true;
		}
		return loc.Slot < highMark.Slot;
	}

	private void Execute(Predicate<Location> match, int startIndex, int endIndex)
	{
		for (int i = startIndex; i <= endIndex && match(m_thinPattern[i]); i++)
		{
		}
	}

	private void DisposeInner(bool fromDispose)
	{
		if (!Disposed)
		{
			Disposed = true;
		}
	}

	private VMZ()
	{
		if (m_thinPattern != null)
		{
			return;
		}
		int num = DecksManager.Instance.DeckCount * m_slots.Size;
		m_thinPattern = new Location[num];
		int num2 = 0;
		for (int num3 = DecksManager.Instance.DeckCount; num3 >= 1; num3--)
		{
			for (int i = m_slots.Start; i <= m_slots.End; i++)
			{
				m_thinPattern[num2++] = new Location(num3, i);
			}
		}
	}
}
