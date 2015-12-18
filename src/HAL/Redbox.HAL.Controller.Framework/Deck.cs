using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Controller.Framework;

public sealed class Deck
{
	private class ProblemLocationsLog
	{
		private const string LogFileName = "ErrorLocationsHistory.txt";

		private TextWriter m_logFile;

		internal static ProblemLocationsLog Instance => Singleton<ProblemLocationsLog>.Instance;

		private TextWriter LogFile
		{
			get
			{
				if (m_logFile == null)
				{
					string path = Path.Combine(ControllerConfiguration.Instance.KioskLogsPath, "ErrorLogs");
					string path2 = Path.Combine(path, "ErrorLocationsHistory.txt");
					m_logFile = new StreamWriter(File.Open(path2, FileMode.Append, FileAccess.Write, FileShare.Read))
					{
						AutoFlush = true
					};
				}
				return m_logFile;
			}
		}

		internal void LogFormattedMessage(string format, params object[] parms)
		{
			try
			{
				string arg = string.Format(format, parms);
				arg = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()} {arg}";
				LogFile.WriteLine(arg);
			}
			catch (Exception)
			{
			}
		}

		private ProblemLocationsLog()
		{
		}
	}

	internal const int DefaultSlot1Offset = 780;

	private const int SlotsPerQuadrantDenseDeck = 15;

	private const int SlotsPerQuadrantSparseDeck = 6;

	private readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

	private DeckData[] m_inventoryData;

	private readonly List<Quadrant> m_quadrants = new List<Quadrant>();

	public int Number { get; private set; }

	public int YOffset { get; private set; }

	public bool IsQlmDeck { get; private set; }

	public int NumberOfSlots { get; private set; }

	public decimal SlotWidth { get; private set; }

	public int? SellThruSlots { get; private set; }

	public int? SellThruOffset { get; private set; }

	public int? ApproachOffset { get; private set; }

	public int SlotsPerQuadrant { get; private set; }

	public Quadrant[] Quadrants
	{
		get
		{
			return m_quadrants.ToArray();
		}
		private set
		{
			m_quadrants.AddRange(value);
		}
	}

	internal bool IsSparse => NumberOfSlots == 72;

	private bool TrackProblemLocations
	{
		get
		{
			if (ControllerConfiguration.Instance.Mode == ControllerMode.Normal)
			{
				return ControllerConfiguration.Instance.TrackProblemLocations;
			}
			return false;
		}
	}

	private IDataStore Store => StoreManager.Instance.FindByName("HALData.vdb3");

	public override int GetHashCode()
	{
		return Number;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is Deck deck))
		{
			return false;
		}
		return deck == this;
	}

	public static bool operator !=(Deck lhs, Deck rhs)
	{
		return !(lhs == rhs);
	}

	public static bool operator ==(Deck lhs, Deck rhs)
	{
		if (object.ReferenceEquals(lhs, rhs))
		{
			return true;
		}
		if ((object)lhs == null || (object)rhs == null)
		{
			return false;
		}
		if (lhs.YOffset != rhs.YOffset)
		{
			return false;
		}
		if (lhs.NumberOfSlots != rhs.NumberOfSlots)
		{
			return false;
		}
		if (lhs.IsQlmDeck != rhs.IsQlmDeck)
		{
			return false;
		}
		if (lhs.SlotWidth != rhs.SlotWidth)
		{
			return false;
		}
		if (lhs.SellThruOffset != rhs.SellThruOffset)
		{
			return false;
		}
		if (lhs.ApproachOffset != rhs.ApproachOffset)
		{
			return false;
		}
		if (lhs.Quadrants.Length != rhs.Quadrants.Length)
		{
			return false;
		}
		for (int i = 0; i < lhs.Quadrants.Length; i++)
		{
			Quadrant quadrant = lhs.Quadrants[i];
			Quadrant quadrant2 = rhs.Quadrants[i];
			if (quadrant.Offset != quadrant2.Offset || quadrant.IsExcluded != quadrant2.IsExcluded)
			{
				return false;
			}
		}
		return true;
	}

	public Deck(int number, int yoffset, bool isQlmDeck, int? approachOffset, int numberOfSlots, decimal slotWidth, int? sellThruSlots, int? sellThruOffset, Quadrant[] quadrantOffsets)
	{
		Number = number;
		YOffset = yoffset;
		IsQlmDeck = isQlmDeck;
		SlotWidth = slotWidth;
		SellThruSlots = sellThruSlots;
		SellThruOffset = sellThruOffset;
		ApproachOffset = approachOffset;
		NumberOfSlots = numberOfSlots;
		Quadrants = quadrantOffsets ?? new Quadrant[1]
		{
			new Quadrant(780)
		};
		SlotsPerQuadrant = NumberOfSlots / Quadrants.Length;
	}

	public static Deck FromXmlNode(XmlNode node)
	{
		int attributeValue = XmlNodeExtensions.GetAttributeValue(node, "Number", 1);
		int attributeValue2 = XmlNodeExtensions.GetAttributeValue(node, "Offset", -18760);
		bool attributeValue3 = XmlNodeExtensions.GetAttributeValue(node, "IsQlm", defaultValue: false);
		int? attributeValue4 = XmlNodeExtensions.GetAttributeValue<int?>(node, "ApproachOffset", null);
		decimal attributeValue5 = XmlNodeExtensions.GetAttributeValue(node, "SlotWidth", 166.6667m);
		int attributeValue6 = XmlNodeExtensions.GetAttributeValue(node, "NumberOfSlots", 90);
		int? attributeValue7 = XmlNodeExtensions.GetAttributeValue<int?>(node, "SellThruSlots", null);
		int? attributeValue8 = XmlNodeExtensions.GetAttributeValue<int?>(node, "SellThruOffset", null);
		List<Quadrant> list = new List<Quadrant>();
		int num = 0;
		foreach (XmlNode childNode in node.ChildNodes)
		{
			int attributeValue9 = XmlNodeExtensions.GetAttributeValue(childNode, "Offset", 780);
			int? attributeValue10 = XmlNodeExtensions.GetAttributeValue<int?>(childNode, "StartSlot", null);
			int? attributeValue11 = XmlNodeExtensions.GetAttributeValue<int?>(childNode, "EndSlot", null);
			bool attributeValue12 = XmlNodeExtensions.GetAttributeValue(childNode, "IsExcluded", defaultValue: false);
			Range range = null;
			if (attributeValue10.HasValue && attributeValue11.HasValue)
			{
				range = new Range(attributeValue10.Value, attributeValue11.Value);
			}
			else
			{
				int num2 = ((90 == attributeValue6) ? 15 : 6);
				int num3 = num * num2 + 1;
				int end = num3 + num2 - 1;
				range = new Range(num3, end);
			}
			Quadrant quadrant = new Quadrant(attributeValue9, range);
			list.Add(quadrant);
			quadrant.IsExcluded = attributeValue12;
			num++;
		}
		return new Deck(attributeValue, attributeValue2, attributeValue3, attributeValue4, attributeValue6, attributeValue5, attributeValue7, attributeValue8, list.ToArray());
	}

	public void ToXmlWriter(XmlWriter writer)
	{
		writer.WriteStartElement("Deck");
		writer.WriteAttributeString("Number", XmlConvert.ToString(Number));
		writer.WriteAttributeString("Offset", XmlConvert.ToString(YOffset));
		writer.WriteAttributeString("IsQlm", XmlConvert.ToString(IsQlmDeck));
		if (ApproachOffset.HasValue)
		{
			writer.WriteAttributeString("ApproachOffset", XmlConvert.ToString(ApproachOffset.Value));
		}
		writer.WriteAttributeString("SlotWidth", XmlConvert.ToString(SlotWidth));
		writer.WriteAttributeString("NumberOfSlots", XmlConvert.ToString(NumberOfSlots));
		if (SellThruSlots.HasValue)
		{
			writer.WriteAttributeString("SellThruSlots", XmlConvert.ToString(SellThruSlots.Value));
		}
		if (SellThruOffset.HasValue)
		{
			writer.WriteAttributeString("SellThruOffset", XmlConvert.ToString(SellThruOffset.Value));
		}
		Quadrant[] quadrants = Quadrants;
		foreach (Quadrant quadrant in quadrants)
		{
			writer.WriteStartElement("Quadrant");
			writer.WriteAttributeString("Offset", XmlConvert.ToString(quadrant.Offset));
			if (quadrant.Slots != null)
			{
				writer.WriteAttributeString("StartSlot", XmlConvert.ToString(quadrant.Slots.Start));
				writer.WriteAttributeString("EndSlot", XmlConvert.ToString(quadrant.Slots.End));
			}
			if (quadrant.IsExcluded)
			{
				writer.WriteAttributeString("IsExcluded", XmlConvert.ToString(value: true));
			}
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	public List<int> SwapEmptyWith(string id, MerchFlags flags, Range range)
	{
		List<int> list = new List<int>();
		IDataStore store = Store;
		if (store == null)
		{
			return list;
		}
		using (new WithWriteLock(Lock))
		{
			using Transaction transaction = new Transaction(TransactionType.Update);
			for (int i = range.Start - 1; i < range.End; i++)
			{
				if (m_inventoryData[i].ID == "EMPTY" && !m_inventoryData[i].Excluded)
				{
					m_inventoryData[i].ID = id;
					m_inventoryData[i].MerchFlags = flags;
					transaction.Enqueue(m_inventoryData[i].UpdateCommand(store));
					list.Add(i + 1);
				}
			}
			if (transaction.Execute(store) != list.Count)
			{
				LogHelper.Instance.Log("[SwapEmptyWith]WARNING: transaction failed.", LogEntryType.Error);
				list.Clear();
			}
			return list;
		}
	}

	public void MarkInventory(string id)
	{
		using (new WithWriteLock(Lock))
		{
			for (int i = 0; i < NumberOfSlots; i++)
			{
				m_inventoryData[i].ID = id;
			}
			IDataStore store = Store;
			if (store == null)
			{
				return;
			}
			string format = "UPDATE {0} SET ID = '{1}' WHERE  Deck = '{2}'";
			using Transaction transaction = new Transaction(TransactionType.Update);
			string statement = string.Format(format, "Inventory_v1", id, Number);
			transaction.Enqueue(statement);
			if (transaction.Execute(Store) != NumberOfSlots)
			{
				LogHelper.Instance.Log($"There was an error setting the inventory on deck {Number} to {id}", LogEntryType.Info);
			}
		}
	}

	public int FindId(string id)
	{
		using (new WithReadLock(Lock))
		{
			for (int i = 0; i < m_inventoryData.Length; i++)
			{
				if (string.Compare(m_inventoryData[i].ID, id, ignoreCase: true) == 0)
				{
					return i + 1;
				}
			}
			return -1;
		}
	}

	public bool RemoveInventory()
	{
		using (new WithWriteLock(Lock))
		{
			Remove(new Range(1, NumberOfSlots));
			m_inventoryData = null;
			return true;
		}
	}

	public bool Add()
	{
		IDataStore store = Store;
		if (store == null)
		{
			return false;
		}
		using (new WithWriteLock(Lock))
		{
			m_inventoryData = new DeckData[NumberOfSlots];
			using Transaction transaction = new Transaction(TransactionType.Insert);
			for (int i = 0; i < NumberOfSlots; i++)
			{
				m_inventoryData[i] = new DeckData(Number, i + 1)
				{
					ID = "EMPTY",
					ReturnDate = null,
					Excluded = false,
					EmptyStuckCount = 0,
					MerchFlags = MerchFlags.None
				};
				transaction.Enqueue(m_inventoryData[i].InsertCommand(store));
			}
			return transaction.Execute(Store) == NumberOfSlots;
		}
	}

	public string GetIdForSlot(int slot)
	{
		using (new WithReadLock(Lock))
		{
			if (!IsSlotValid(slot))
			{
				return "SLOT OUT OF RANGE";
			}
			return m_inventoryData[slot - 1].ID;
		}
	}

	public bool IsSlotSellThru(int slot)
	{
		if (SellThruSlots.HasValue)
		{
			return slot % SellThruSlots.Value == 0;
		}
		return false;
	}

	public int GetSlotOffset(int slot)
	{
		if (!IsSlotValid(slot))
		{
			throw new ArgumentException($"The slot parameter must be between 1 and {NumberOfSlots}.");
		}
		int num = slot - 1;
		int num2 = num / SlotsPerQuadrant;
		if (num2 >= Quadrants.Length)
		{
			throw new ArgumentException($"The computed quadrant offset {num2} is outside the range of defined quadrants.  This usuallys means the deck is incorrectly configured.");
		}
		int offset = Quadrants[num2].Offset;
		Quadrant quadrant = FindQuadrantThatContainsSlot(slot);
		int num3 = num % SlotsPerQuadrant;
		if (quadrant != null)
		{
			num3 = slot - quadrant.Slots.Start;
			offset = quadrant.Offset;
		}
		decimal num4 = (decimal)num3 * SlotWidth;
		if (IsSlotSellThru(slot))
		{
			num4 = SellThruOffset ?? 915;
		}
		return (int)((decimal)offset + num4);
	}

	public bool UpdateOnTransfer(int slot, MerchFlags flags, DateTime? newTime)
	{
		using (new WithWriteLock(Lock))
		{
			DeckData deckData = m_inventoryData[slot - 1];
			deckData.MerchFlags = flags;
			deckData.ReturnDate = newTime;
			return UpdateStore(deckData);
		}
	}

	public bool ClearMerchFlag(int slot)
	{
		using (new WithWriteLock(Lock))
		{
			DeckData deckData = m_inventoryData[slot - 1];
			deckData.MerchFlags = MerchFlags.None;
			return UpdateStore(deckData);
		}
	}

	public bool UpdateEmptyStuck(int slot)
	{
		if (!TrackProblemLocations)
		{
			return false;
		}
		using (new WithWriteLock(Lock))
		{
			DeckData deckData = m_inventoryData[slot - 1];
			deckData.EmptyStuckCount++;
			ProblemLocationsLog.Instance.LogFormattedMessage("Incrementing Deck = {0} Slot = {1}; current stuck count = {2}", Number, slot, deckData.EmptyStuckCount);
			if (deckData.EmptyStuckCount >= ControllerConfiguration.Instance.MarkLocationUnknownThreshold && deckData.ID != "UNKNOWN")
			{
				ProblemLocationsLog.Instance.LogFormattedMessage("Error threshold met: set inventory to {0} at Deck = {1} Slot = {2}", "UNKNOWN", Number, slot);
				deckData.ID = "UNKNOWN";
			}
			return UpdateStore(deckData);
		}
	}

	public int GetEmptyStuckCount(int slot)
	{
		using (new WithReadLock(Lock))
		{
			return TrackProblemLocations ? m_inventoryData[slot - 1].EmptyStuckCount : 0;
		}
	}

	public bool Exclude(List<int> slots)
	{
		IDataStore store = Store;
		if (store == null)
		{
			return false;
		}
		using (new WithWriteLock(Lock))
		{
			using Transaction transaction = new Transaction(TransactionType.Update);
			foreach (int slot in slots)
			{
				m_inventoryData[slot - 1].Reset();
				m_inventoryData[slot - 1].Excluded = true;
				transaction.Enqueue(m_inventoryData[slot - 1].UpdateCommand(store));
			}
			return transaction.Execute(Store) == slots.Count;
		}
	}

	public bool UnExclude(List<int> slots)
	{
		IDataStore store = Store;
		if (store == null)
		{
			return false;
		}
		using (new WithWriteLock(Lock))
		{
			using Transaction transaction = new Transaction(TransactionType.Update);
			foreach (int slot in slots)
			{
				m_inventoryData[slot - 1].Excluded = false;
				transaction.Enqueue(m_inventoryData[slot - 1].UpdateCommand(store));
			}
			return transaction.Execute(Store) == slots.Count;
		}
	}

	public bool UpdateFromResult(GetResult result, int slot)
	{
		using (new WithWriteLock(Lock))
		{
			DeckData deckData = m_inventoryData[slot - 1];
			deckData.ID = result.Previous;
			deckData.ReturnDate = result.ReturnTime;
			deckData.MerchFlags = result.Flags;
			return UpdateStore(deckData);
		}
	}

	public bool SetSlotMatrix(int slot, string matrix)
	{
		SetSlotId(slot, matrix);
		return true;
	}

	public bool IsSlotValid(int slot)
	{
		if (slot >= 1)
		{
			return slot <= NumberOfSlots;
		}
		return false;
	}

	public bool Reset(string id)
	{
		bool flag = id == "EMPTY";
		using (new WithWriteLock(Lock))
		{
			using Transaction transaction = new Transaction(TransactionType.Update);
			for (int i = 0; i < NumberOfSlots; i++)
			{
				m_inventoryData[i].Reset();
				if (!flag)
				{
					m_inventoryData[i].ID = id;
				}
				transaction.Enqueue(m_inventoryData[i].UpdateCommand(Store));
			}
			return transaction.Execute(Store) == NumberOfSlots;
		}
	}

	public Location FindMatching(MerchFlags flags)
	{
		using (new WithReadLock(Lock))
		{
			for (int i = 0; i < NumberOfSlots; i++)
			{
				if (m_inventoryData[i].MerchFlags == flags)
				{
					return new Location(Number, i + 1);
				}
			}
			return null;
		}
	}

	internal static bool IsSuspiciousId(string id)
	{
		if (InventoryConstants.IsKnownInventoryToken(id))
		{
			return false;
		}
		try
		{
			if (Enum.IsDefined(typeof(GetResultCode), id) || Enum.IsDefined(typeof(PutResultCode), id) || Enum.IsDefined(typeof(PickerHardwareError), id))
			{
				return true;
			}
			return false;
		}
		catch (Exception)
		{
			return false;
		}
	}

	internal void Copy(Deck newProperties)
	{
		using (new WithWriteLock(Lock))
		{
			YOffset = newProperties.YOffset;
			IsQlmDeck = newProperties.IsQlmDeck;
			SlotWidth = newProperties.SlotWidth;
			SellThruOffset = newProperties.SellThruOffset;
			ApproachOffset = newProperties.ApproachOffset;
			int numberOfSlots = NumberOfSlots;
			NumberOfSlots = newProperties.NumberOfSlots;
			if (numberOfSlots < newProperties.NumberOfSlots)
			{
				Range range = new Range(numberOfSlots + 1, NumberOfSlots);
				if (!AddEntries(range))
				{
					LogHelper.Instance.Log($"Unable to add entries {range.Start} -> {range.End} on deck {Number}", LogEntryType.Error);
				}
				else if (IsQlmDeck && !newProperties.IsQlmDeck)
				{
					ResetRange(new Range(1, numberOfSlots));
				}
			}
			else if (numberOfSlots > newProperties.NumberOfSlots)
			{
				Range slots = new Range(newProperties.NumberOfSlots + 1, numberOfSlots);
				RemoveEntries(slots);
			}
			m_quadrants.Clear();
			m_quadrants.AddRange(newProperties.Quadrants);
			IDataStore store = Store;
			if (store == null)
			{
				return;
			}
			Quadrant[] quadrants = Quadrants;
			foreach (Quadrant quadrant in quadrants)
			{
				using Transaction transaction = new Transaction(TransactionType.Update);
				for (int j = quadrant.Slots.Start; j <= quadrant.Slots.End; j++)
				{
					if (m_inventoryData[j - 1].Excluded != quadrant.IsExcluded)
					{
						m_inventoryData[j - 1].Excluded = quadrant.IsExcluded;
						transaction.Enqueue(m_inventoryData[j - 1].UpdateCommand(store));
					}
				}
				transaction.Execute(store);
			}
		}
	}

	internal void Fill(GetResult gr, int slot)
	{
		using (new WithWriteLock(Lock))
		{
			DeckData deckData = m_inventoryData[slot - 1];
			gr.Previous = deckData.ID;
			gr.ReturnTime = deckData.ReturnDate;
			gr.Flags = deckData.MerchFlags;
		}
	}

	internal bool ResetLocation(int slot)
	{
		using (new WithWriteLock(Lock))
		{
			DeckData deckData = m_inventoryData[slot - 1];
			deckData.Reset();
			return UpdateStore(deckData);
		}
	}

	internal bool MarkSlotUnknown(int slot)
	{
		SetSlotId(slot, "UNKNOWN");
		return true;
	}

	internal bool UpdateReturnTime(int slot, DateTime? newDate)
	{
		using (new WithWriteLock(Lock))
		{
			if (!newDate.HasValue && !m_inventoryData[slot - 1].ReturnDate.HasValue)
			{
				return true;
			}
			m_inventoryData[slot - 1].ReturnDate = newDate;
			return UpdateStore(m_inventoryData[slot - 1]);
		}
	}

	internal DateTime? GetReturnTime(int slot)
	{
		using (new WithReadLock(Lock))
		{
			return m_inventoryData[slot - 1].ReturnDate;
		}
	}

	internal int BulkUpdate(List<Location> locs, string newId, MerchFlags newFlags)
	{
		IDataStore store = Store;
		if (store == null)
		{
			return 0;
		}
		using (new WithWriteLock(Lock))
		{
			using Transaction transaction = new Transaction(TransactionType.Update);
			foreach (Location loc in locs)
			{
				if (loc.Deck != Number)
				{
					LogHelper.Instance.Log("BulkUpdate: invalid deck {0} specified.", loc.Deck);
					continue;
				}
				int num = loc.Slot - 1;
				m_inventoryData[num].ID = newId;
				m_inventoryData[num].MerchFlags = newFlags;
				transaction.Enqueue(m_inventoryData[num].UpdateCommand(store));
			}
			return (transaction.Execute(Store) == locs.Count) ? locs.Count : 0;
		}
	}

	internal string SetSlotId(int slot, string id)
	{
		using (new WithWriteLock(Lock))
		{
			DateTime? returnDate = m_inventoryData[slot - 1].ReturnDate;
			MerchFlags merchFlags = m_inventoryData[slot - 1].MerchFlags;
			return UpdateSlotInner(slot, id, merchFlags, returnDate);
		}
	}

	internal string SetSlotId(int slot, string id, MerchFlags flags, DateTime? rv)
	{
		using (new WithWriteLock(Lock))
		{
			return UpdateSlotInner(slot, id, flags, rv);
		}
	}

	internal int GetEmptyCount()
	{
		int num = 0;
		using (new WithReadLock(Lock))
		{
			DeckData[] inventoryData = m_inventoryData;
			foreach (DeckData deckData in inventoryData)
			{
				if (!deckData.Excluded && deckData.ID == "EMPTY")
				{
					num++;
				}
			}
			return num;
		}
	}

	internal void GetUnknownLocations(List<Location> list)
	{
		using (new WithReadLock(Lock))
		{
			MatchMatrix("UNKNOWN", list);
		}
	}

	internal void GetEmptyLocations(List<Location> list)
	{
		using (new WithReadLock(Lock))
		{
			MatchMatrix("EMPTY", list);
		}
	}

	internal bool ImportInventory(List<DeckData> data)
	{
		IDataStore store = Store;
		if (store == null)
		{
			return false;
		}
		using (new WithWriteLock(Lock))
		{
			using (Transaction transaction = new Transaction(TransactionType.Update))
			{
				foreach (DeckData datum in data)
				{
					transaction.Enqueue(datum.UpdateCommand(store));
				}
				if (transaction.Execute(Store) != data.Count)
				{
					return false;
				}
			}
			m_inventoryData = new DeckData[NumberOfSlots];
			for (int i = 0; i < NumberOfSlots; i++)
			{
				DeckData deckData = data[i];
				m_inventoryData[i] = new DeckData(Number, i + 1)
				{
					ID = deckData.ID,
					ReturnDate = deckData.ReturnDate,
					Excluded = deckData.Excluded,
					EmptyStuckCount = deckData.EmptyStuckCount,
					MerchFlags = deckData.MerchFlags
				};
			}
			return true;
		}
	}

	internal void WriteInventoryData(XmlTextWriter writer)
	{
		using (new WithReadLock(Lock))
		{
			DeckData[] inventoryData = m_inventoryData;
			foreach (DeckData deckData in inventoryData)
			{
				writer.WriteStartElement("item");
				deckData.WriteToXML(writer);
				writer.WriteEndElement();
			}
		}
	}

	internal bool ResetEmptyStuckCount(int slot)
	{
		if (!TrackProblemLocations)
		{
			return false;
		}
		using (new WithUpgradeableReadLock(Lock))
		{
			DeckData deckData = m_inventoryData[slot - 1];
			if (deckData.EmptyStuckCount == 0)
			{
				return true;
			}
			using (new WithWriteLock(Lock))
			{
				deckData.EmptyStuckCount = 0;
				ProblemLocationsLog.Instance.LogFormattedMessage("Resetting Deck = {0} Slot = {1}", Number, slot);
				return UpdateStore(deckData);
			}
		}
	}

	internal bool IsStuck(int slot)
	{
		if (!TrackProblemLocations)
		{
			return false;
		}
		using (new WithReadLock(Lock))
		{
			return m_inventoryData[slot - 1].EmptyStuckCount >= ControllerConfiguration.Instance.MarkLocationUnknownThreshold;
		}
	}

	internal MerchFlags GetMerchFlags(int slot)
	{
		using (new WithReadLock(Lock))
		{
			return m_inventoryData[slot - 1].MerchFlags;
		}
	}

	internal DeckData FindFirst(string id)
	{
		using (new WithReadLock(Lock))
		{
			for (int i = 0; i < NumberOfSlots; i++)
			{
				if (m_inventoryData[i].ID == id)
				{
					return m_inventoryData[i];
				}
			}
			return m_inventoryData[NumberOfSlots - 1];
		}
	}

	internal bool UpdateMerchFlag(MerchFlags flags, int slot)
	{
		using (new WithWriteLock(Lock))
		{
			DeckData deckData = m_inventoryData[slot - 1];
			deckData.MerchFlags = flags;
			return UpdateStore(deckData);
		}
	}

	internal bool IsExcluded(int slot)
	{
		using (new WithReadLock(Lock))
		{
			return m_inventoryData[slot - 1].Excluded;
		}
	}

	internal int GetExcludedCount()
	{
		int num = 0;
		using (new WithReadLock(Lock))
		{
			for (int i = 0; i < m_inventoryData.Length; i++)
			{
				if (m_inventoryData[i].Excluded)
				{
					num++;
				}
			}
			return num;
		}
	}

	internal void ExcludeSlot(int slot, bool excluded)
	{
		using (new WithUpgradeableReadLock(Lock))
		{
			DeckData deckData = m_inventoryData[slot - 1];
			if (deckData.Excluded == excluded)
			{
				return;
			}
			using (new WithWriteLock(Lock))
			{
				deckData.Excluded = excluded;
				if (excluded)
				{
					deckData.Reset();
				}
				UpdateStore(deckData);
			}
		}
	}

	internal void ResetRange(Range slots, string id, MerchFlags flags)
	{
		IDataStore store = Store;
		if (store == null)
		{
			return;
		}
		using (new WithWriteLock(Lock))
		{
			using Transaction transaction = new Transaction(TransactionType.Update);
			for (int i = slots.Start; i <= slots.End; i++)
			{
				DeckData deckData = m_inventoryData[i - 1];
				deckData.ID = id;
				deckData.ReturnDate = null;
				deckData.EmptyStuckCount = 0;
				deckData.MerchFlags = flags;
				transaction.Enqueue(deckData.UpdateCommand(store));
			}
			if (transaction.Execute(Store) != slots.Size)
			{
				LogHelper.Instance.Log("[ResetRange]: update affected rows did not match range size.");
			}
		}
	}

	internal void LoadData()
	{
		List<DeckData> list = new List<DeckData>();
		using (new DisposeableList<DeckData>(list))
		{
			IDataStore store = Store;
			if (store == null)
			{
				LogHelper.Instance.Log("Unable to find a store to load data from.");
				throw new ApplicationException("No data stores found.");
			}
			using (new WithWriteLock(Lock))
			{
				LoadFromStore(list);
				if (list.Count == NumberOfSlots)
				{
					m_inventoryData = list.ToArray();
					return;
				}
				LogHelper.Instance.Log($"LoadInventory: res.count = {list.Count}; deck count = {NumberOfSlots}; deleting deck & initializing to EMPTY", LogEntryType.Error);
				Range slots = new Range(1, list.Count);
				Remove(slots);
				using Transaction transaction = new Transaction(TransactionType.Insert);
				m_inventoryData = new DeckData[NumberOfSlots];
				for (int i = 0; i < NumberOfSlots; i++)
				{
					m_inventoryData[i] = new DeckData(Number, i + 1)
					{
						ID = "EMPTY",
						ReturnDate = null,
						Excluded = false,
						EmptyStuckCount = 0
					};
					transaction.Enqueue(m_inventoryData[i].InsertCommand(store));
				}
				transaction.Execute(Store);
			}
		}
	}

	internal void Dump()
	{
		LogHelper.Instance.Log("Deck {0} Number of Slots {1} Y-offset {2}", Number, NumberOfSlots, YOffset.ToString());
	}

	private void MatchMatrix(string matrix, List<Location> toFill)
	{
		for (int i = 0; i < m_inventoryData.Length; i++)
		{
			if (!m_inventoryData[i].Excluded && m_inventoryData[i].ID == matrix)
			{
				toFill.Add(new Location(Number, i + 1));
			}
		}
	}

	internal void RemoveEntries(Range slots)
	{
		DeckData[] inventoryData = m_inventoryData;
		m_inventoryData = new DeckData[NumberOfSlots];
		for (int i = 0; i < NumberOfSlots; i++)
		{
			m_inventoryData[i] = inventoryData[i];
		}
		Remove(slots);
	}

	private bool AddEntries(Range slots)
	{
		IDataStore store = Store;
		if (store == null)
		{
			return false;
		}
		DeckData[] inventoryData = m_inventoryData;
		m_inventoryData = new DeckData[NumberOfSlots];
		Array.Copy(inventoryData, m_inventoryData, inventoryData.Length);
		using Transaction transaction = new Transaction(TransactionType.Insert);
		for (int i = slots.Start; i <= slots.End; i++)
		{
			m_inventoryData[i - 1] = new DeckData(Number, i)
			{
				ID = "EMPTY",
				ReturnDate = null,
				Excluded = false,
				EmptyStuckCount = 0
			};
			transaction.Enqueue(m_inventoryData[i - 1].InsertCommand(store));
		}
		return transaction.Execute(Store) == slots.Size;
	}

	private void ResetRange(Range slots)
	{
		IDataStore store = Store;
		if (store == null)
		{
			return;
		}
		using Transaction transaction = new Transaction(TransactionType.Update);
		for (int i = slots.Start; i <= slots.End; i++)
		{
			DeckData deckData = m_inventoryData[i - 1];
			deckData.ID = "EMPTY";
			deckData.ReturnDate = null;
			deckData.EmptyStuckCount = 0;
			deckData.MerchFlags = MerchFlags.None;
			transaction.Enqueue(deckData.UpdateCommand(store));
		}
		if (transaction.Execute(Store) != slots.Size)
		{
			LogHelper.Instance.Log("[ResetRange]: update affected rows did not match range size.");
		}
	}

	private Quadrant FindQuadrantThatContainsSlot(int slot)
	{
		Quadrant[] quadrants = Quadrants;
		foreach (Quadrant quadrant in quadrants)
		{
			if (quadrant.Slots == null)
			{
				return null;
			}
			if (quadrant.Slots.InRange(slot))
			{
				return quadrant;
			}
		}
		return null;
	}

	private void LoadFromStore(List<DeckData> list)
	{
		IDataStore store = Store;
		if (store == null)
		{
			return;
		}
		string query = string.Format("SELECT Slot, ID, LastReturnTime, Excluded, EmptyStuckCount, MerchFlags FROM [{0}] where Deck={1}", "Inventory_v1", Number);
		store.ExectuteSelectQuery(delegate(IDataReader reader)
		{
			DateTime? returnDate = null;
			string text = reader.ReadString(2);
			if (!string.IsNullOrEmpty(text))
			{
				try
				{
					returnDate = DateTime.Parse(text);
				}
				catch
				{
					returnDate = null;
				}
			}
			DeckData item = new DeckData(Number, reader.ReadInt(0))
			{
				ID = reader.ReadString(1),
				ReturnDate = returnDate,
				Excluded = reader.ReadBoolean(3),
				EmptyStuckCount = reader.ReadInt(4),
				MerchFlags = (MerchFlags)reader.ReadInt64(5)
			};
			list.Add(item);
		}, query);
		list.Sort((DeckData x, DeckData y) => x.Slot.CompareTo(y.Slot));
	}

	private string UpdateSlotInner(int slot, string id, MerchFlags flags, DateTime? dt)
	{
		string iD = m_inventoryData[slot - 1].ID;
		m_inventoryData[slot - 1].ID = id;
		m_inventoryData[slot - 1].MerchFlags = flags;
		m_inventoryData[slot - 1].ReturnDate = dt;
		if (!UpdateStore(m_inventoryData[slot - 1]))
		{
			LogHelper.Instance.Log($"SetSlotId: Deck={Number}, Slot={slot}, ID={id} (previous = {iD}) returned FAILURE", LogEntryType.Error);
		}
		return iD;
	}

	private bool UpdateStore(DeckData data)
	{
		IDataStore dataStore = StoreManager.Instance.FindByName("HALData.vdb3");
		return dataStore?.Update(data.UpdateCommand(dataStore)) ?? false;
	}

	private bool Remove(Range slots)
	{
		IDataStore dataStore = StoreManager.Instance.FindByName("HALData.vdb3");
		if (dataStore == null)
		{
			return false;
		}
		string deleteQuery = string.Format("DELETE FROM {0} WHERE Deck = '{1}' AND Slot >= '{2}' and Slot <= '{3}'", "Inventory_v1", Number, slots.Start, slots.End);
		return dataStore.Delete(deleteQuery) == slots.Size;
	}
}
