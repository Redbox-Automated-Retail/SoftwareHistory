using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public class VMZ : IDisposable
{
	private readonly Range m_slots = new Range(1, 15);

	private readonly Location[] m_thinPattern;

	public static VMZ Instance => Singleton<VMZ>.Instance;

	internal int LastIndex => m_thinPattern.Length - 1;

	private bool Disposed { get; set; }

	public void Dispose()
	{
		DisposeInner(fromDispose: true);
	}

	~VMZ()
	{
		DisposeInner(fromDispose: false);
	}

	public int EndsWithSlot(Deck deck)
	{
		return m_slots.End + 1;
	}

	public int GetCountInZone(MerchFlags flags)
	{
		int num = 0;
		Location[] thinPattern = m_thinPattern;
		foreach (Location location in thinPattern)
		{
			if (location.Flags == flags)
			{
				num++;
			}
		}
		return num;
	}

	public List<Location> GetThinItems()
	{
		List<Location> list = new List<Location>();
		Location[] thinPattern = m_thinPattern;
		foreach (Location location in thinPattern)
		{
			if (IsThinType(location.Flags))
			{
				list.Add(location);
			}
		}
		return list;
	}

	public List<Location> GetNonThinItemsInCompressedZone()
	{
		List<Location> list = new List<Location>();
		VMZSearchResult result = default(VMZSearchResult);
		FindHighThinTypeDisk(ref result);
		if (!result.Found)
		{
			return list;
		}
		Location[] thinPattern = m_thinPattern;
		foreach (Location location in thinPattern)
		{
			if (location.Equals(result.Location))
			{
				break;
			}
			if (!IsThinType(location.Flags) && !location.IsEmpty)
			{
				list.Add(location);
			}
		}
		return list;
	}

	public bool LocInCompressedZone(Location target)
	{
		VMZSearchResult result = default(VMZSearchResult);
		FindHighThinTypeDisk(ref result);
		if (!result.Found)
		{
			return false;
		}
		Location[] thinPattern = m_thinPattern;
		foreach (Location location in thinPattern)
		{
			if (location.Equals(result.Location))
			{
				return location.Equals(target);
			}
			if (location.Equals(target))
			{
				return true;
			}
		}
		return false;
	}

	public List<string> GetState()
	{
		List<string> rv = new List<string>();
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			for (int i = m_slots.Start; i <= m_slots.End; i++)
			{
				string item = $"Deck {deck.Number} Slot {i} Inventory {deck.GetIdForSlot(i)} Flags {deck.GetMerchFlags(i).ToString()}";
				rv.Add(item);
			}
			return true;
		});
		return rv;
	}

	public void DumpZone()
	{
		StreamWriter log = new StreamWriter(GetDumpFile());
		try
		{
			log.WriteLine("-- Dump VMZ Locations --");
			DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
			{
				for (int i = m_slots.Start; i <= m_slots.End; i++)
				{
					MerchFlags merchFlags = deck.GetMerchFlags(i);
					log.WriteLine("Deck {0} Slot {1} Inventory {2} Flags {3} Stuck {4}", deck.Number, i, deck.GetIdForSlot(i), merchFlags.ToString(), deck.GetEmptyStuckCount(i));
				}
				return true;
			});
		}
		finally
		{
			if (log != null)
			{
				((IDisposable)log).Dispose();
			}
		}
	}

	public void DumpZone(IFormattedLog log)
	{
		if (!ControllerConfiguration.Instance.DumpVMZState)
		{
			return;
		}
		log.WriteFormatted("-- Dump VMZ Locations --");
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			for (int i = m_slots.Start; i <= m_slots.End; i++)
			{
				MerchFlags merchFlags = deck.GetMerchFlags(i);
				log.WriteFormatted("Deck {0} Slot {1} Inventory {2} Flags {3} Stuck {4}", deck.Number, i, deck.GetIdForSlot(i), merchFlags.ToString(), deck.GetEmptyStuckCount(i));
			}
			return true;
		});
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

	public Location FindNextUnloadLocation()
	{
		Location[] thinPattern = m_thinPattern;
		foreach (Location location in thinPattern)
		{
			if (MerchFlags.Unload == location.Flags)
			{
				return location;
			}
		}
		return null;
	}

	public List<Location> FindEmptyOutsideOfCompressedZone()
	{
		VMZSearchResult res = default(VMZSearchResult);
		FindHighTypeInner(MerchFlags.Thin, ref res);
		if (!res.Found)
		{
			FindHighTypeInner(MerchFlags.Rebalance, ref res);
			if (!res.Found)
			{
				return FindFarthestEmptyOutsideOf(m_thinPattern[0]);
			}
		}
		return FindFarthestEmptyOutsideOf(res.Location);
	}

	public List<Location> FindEmptyOutsideOf(VMZIterator iter)
	{
		return FindFarthestEmptyOutsideOf(m_thinPattern[iter.Index]);
	}

	public bool DiskInteriorToTop(MerchFlags flags, Location loc)
	{
		if (!SlotInMerchQuadrant(loc.Slot))
		{
			return false;
		}
		VMZSearchResult res = default(VMZSearchResult);
		FindHighTypeInner(flags, ref res);
		if (!res.Found)
		{
			return false;
		}
		if (loc.Equals(res.Location))
		{
			return true;
		}
		return LocIsLowerThan(loc, res.Location);
	}

	public Location FindEmptyInteriorToHighThinType(ref VMZSearchResult result)
	{
		return FindEmptyBetween(0, result.Index);
	}

	public Location FindEmptyInteriorToHighThinType(ref VMZSearchResult upper, ref VMZSearchResult lower)
	{
		return FindEmptyBetween(lower.Index + 1, upper.Index);
	}

	public List<Location> FindThinTypeInteriorTo(MerchFlags type, ref VMZSearchResult start)
	{
		List<Location> list = new List<Location>();
		for (int i = 0; i < start.Index; i++)
		{
			Location location = m_thinPattern[i];
			if (location.Flags == type)
			{
				list.Add(location);
			}
		}
		return list;
	}

	public List<Location> FindNonThinsInteriorTo(ref VMZSearchResult start)
	{
		List<Location> list = new List<Location>();
		for (int i = 0; i < start.Index; i++)
		{
			Location location = m_thinPattern[i];
			if (!IsThinType(location.Flags) && !location.IsEmpty)
			{
				list.Add(location);
			}
		}
		return list;
	}

	public List<Location> FindNonThinsInteriorTo(ref VMZSearchResult start, Location lowBound)
	{
		List<Location> list = new List<Location>();
		for (int num = start.Index - 1; num >= 0; num--)
		{
			Location location = m_thinPattern[num];
			if (location.Equals(lowBound))
			{
				break;
			}
			if (!IsThinType(location.Flags) && !location.IsEmpty)
			{
				list.Add(location);
			}
		}
		return list;
	}

	public void FindLastLocation(MerchFlags flags, ref VMZSearchResult result)
	{
		FindHighTypeInner(flags, ref result);
	}

	public int ResetCompressedZone(ref VMZSearchResult high)
	{
		int num = 0;
		using ExecutionTimer executionTimer = new ExecutionTimer();
		int deckCount = DecksManager.Instance.DeckCount;
		List<Location>[] array = new List<Location>[deckCount];
		for (int i = 0; i < deckCount; i++)
		{
			array[i] = new List<Location>();
		}
		try
		{
			Location[] thinPattern = m_thinPattern;
			foreach (Location location in thinPattern)
			{
				if (high.Location.Equals(location))
				{
					array[location.Deck - 1].Add(location);
					break;
				}
				if (!location.IsEmpty)
				{
					array[location.Deck - 1].Add(location);
				}
			}
			for (int k = 0; k < array.Length; k++)
			{
				if (array[k].Count > 0)
				{
					Deck deck = DecksManager.Instance.GetDeck(k + 1);
					num += deck.BulkUpdate(array[k], "EMPTY", MerchFlags.None);
				}
			}
		}
		finally
		{
			List<Location>[] array2 = array;
			foreach (List<Location> list in array2)
			{
				list.Clear();
			}
			array = null;
		}
		executionTimer.Stop();
		LogHelper.Instance.Log("VMZ.ClearZone: reset {0} locations ( time = {1}ms ).", num, executionTimer.ElapsedMilliseconds);
		return num;
	}

	public bool LocIsInVMZ(Location loc)
	{
		return SlotInMerchQuadrant(loc.Slot);
	}

	public bool MarkLocationAs(Location loc, MerchFlags flags)
	{
		if (!SlotInMerchQuadrant(loc.Slot))
		{
			return false;
		}
		return DecksManager.Instance.GetDeck(loc.Deck).UpdateMerchFlag(flags, loc.Slot);
	}

	public bool IsThinType(MerchFlags flags)
	{
		if (MerchFlags.Rebalance != flags)
		{
			return MerchFlags.Thin == flags;
		}
		return true;
	}

	internal void Initialize()
	{
		LogHelper.Instance.Log("VMZ.Initialize()", LogEntryType.Info);
	}

	internal Location LocationAt(int idx)
	{
		if (idx < 0 || idx >= m_thinPattern.Length)
		{
			return null;
		}
		return m_thinPattern[idx];
	}

	internal bool LocIsInVMZ(ControllerLocation loc)
	{
		return SlotInMerchQuadrant(loc.Slot);
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

	private void FindHighThinTypeDisk(ref VMZSearchResult result)
	{
		FindHighTypeInner(MerchFlags.Thin, ref result);
		if (!result.Found)
		{
			FindHighTypeInner(MerchFlags.Rebalance, ref result);
		}
	}

	private void FindHighTypeInner(MerchFlags flags, ref VMZSearchResult res)
	{
		for (int num = m_thinPattern.Length - 1; num >= 0; num--)
		{
			Location location = m_thinPattern[num];
			if (location.Flags == flags)
			{
				res.Index = num;
				return;
			}
		}
		res.Clear();
	}

	private string GetDumpFile()
	{
		DateTime now = DateTime.Now;
		string path = $"m{now.Month}d{now.Day}y{now.Year}_h{now.Hour}m{now.Minute}s{now.Second}t{now.Millisecond}.txt";
		string path2 = Path.Combine(ControllerConfiguration.Instance.KioskLogsPath, "VMZ");
		string text = Path.Combine(path2, path);
		if (File.Exists(text))
		{
			try
			{
				File.Delete(text);
			}
			catch (Exception arg)
			{
				Console.WriteLine("couldn't delete file.", arg);
			}
		}
		return text;
	}

	private Location FindEmptyBetween(int start, int end)
	{
		for (int i = start; i < end; i++)
		{
			if (m_thinPattern[i].IsEmpty)
			{
				return m_thinPattern[i];
			}
		}
		return null;
	}

	private bool SlotInMerchQuadrant(int slot)
	{
		if (slot >= m_slots.Start)
		{
			return slot <= m_slots.End;
		}
		return false;
	}

	private List<Location> FindFarthestEmptyOutsideOf(Location top)
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
		ESPNode[] emptyPattern = ESPHelper.Instance.EmptyPattern;
		foreach (ESPNode eSPNode in emptyPattern)
		{
			Location location = eSPNode;
			if (top.Equals(location))
			{
				break;
			}
			if (location.IsEmpty)
			{
				list.Add(location);
			}
			if (list.Count == num)
			{
				break;
			}
		}
		return list;
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
