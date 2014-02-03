using System;
using System.Collections.Generic;
using System.IO;
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

	private DeckData[] m_inventoryData;

	public int Number { get; private set; }

	public int YOffset { get; private set; }

	public bool IsQlmDeck { get; private set; }

	public int NumberOfSlots { get; private set; }

	public decimal SlotWidth { get; private set; }

	public int? SellThruSlots { get; private set; }

	public int? SellThruOffset { get; private set; }

	public int? ApproachOffset { get; private set; }

	public int SlotsPerQuadrant { get; private set; }

	public Quadrant[] Quadrants { get; private set; }

	internal bool IsSparse => NumberOfSlots == 72;

	private object Lock { get; set; }

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
		Lock = new object();
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
		lock (Lock)
		{
			return SwapWithEmptyInner(id, flags, range);
		}
	}

	public List<int> SwapEmptyWith(string id, MerchFlags flags)
	{
		lock (Lock)
		{
			Range slots = new Range(1, NumberOfSlots);
			return SwapWithEmptyInner(id, flags, slots);
		}
	}

	public void MarkInventory(string id)
	{
		lock (Lock)
		{
			for (int i = 0; i < NumberOfSlots; i++)
			{
				m_inventoryData[i].ID = id;
			}
			InventoryManager.Instance.UpdateDeckInventory(this, id);
		}
	}

	public int FindId(string id)
	{
		lock (Lock)
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

	public bool ResetLocation(int slot)
	{
		lock (Lock)
		{
			DeckData data = m_inventoryData[slot - 1];
			return ResetDataInner(data);
		}
	}

	public bool FillResult(GetResult gr, int slot, bool reset)
	{
		lock (Lock)
		{
			DeckData deckData = m_inventoryData[slot - 1];
			gr.Previous = deckData.ID;
			gr.ReturnTime = deckData.ReturnDate;
			gr.Flags = deckData.MerchFlags;
			return !reset || ResetDataInner(deckData);
		}
	}

	public bool RemoveInventory()
	{
		lock (Lock)
		{
			InventoryManager.Instance.RemoveEntries(this, new Range(1, NumberOfSlots));
			m_inventoryData = null;
			return true;
		}
	}

	public bool Add()
	{
		lock (Lock)
		{
			m_inventoryData = new DeckData[NumberOfSlots];
			using Transaction txn = new Transaction(TransactionType.Insert);
			for (int i = 0; i < NumberOfSlots; i++)
			{
				m_inventoryData[i] = new DeckData
				{
					Deck = Number,
					Slot = i + 1,
					ID = "EMPTY",
					ReturnDate = null,
					Excluded = false,
					EmptyStuckCount = 0,
					MerchFlags = MerchFlags.None
				};
				InventoryManager.Instance.EnqueueInsert(txn, m_inventoryData[i]);
			}
			return InventoryManager.Instance.ExecuteTransaction(txn);
		}
	}

	public string GetIdForSlot(int slot)
	{
		lock (Lock)
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

	public Quadrant FindContainingQuadrant(int slot)
	{
		return FindQuadrantThatContainsSlot(slot);
	}

	public bool UpdateReturnTime(int slot, DateTime? newDate)
	{
		lock (Lock)
		{
			if (!newDate.HasValue && !m_inventoryData[slot - 1].ReturnDate.HasValue)
			{
				return true;
			}
			m_inventoryData[slot - 1].ReturnDate = newDate;
			return InventoryManager.Instance.UpdateSlot(m_inventoryData[slot - 1]);
		}
	}

	public DateTime? GetReturnTime(int slot)
	{
		lock (Lock)
		{
			return m_inventoryData[slot - 1].ReturnDate;
		}
	}

	public bool UpdateOnTransfer(int slot, MerchFlags flags, DateTime? newTime)
	{
		lock (Lock)
		{
			DeckData deckData = m_inventoryData[slot - 1];
			deckData.MerchFlags = flags;
			deckData.ReturnDate = newTime;
			return InventoryManager.Instance.UpdateSlot(deckData);
		}
	}

	public bool ClearMerchFlag(int slot)
	{
		lock (Lock)
		{
			DeckData deckData = m_inventoryData[slot - 1];
			deckData.MerchFlags = MerchFlags.None;
			return InventoryManager.Instance.UpdateSlot(deckData);
		}
	}

	public string GetMerchDescription(int slot)
	{
		lock (Lock)
		{
			return m_inventoryData[slot - 1].MerchFlags.ToString();
		}
	}

	public int GetEmptyStuckCount(int slot)
	{
		lock (Lock)
		{
			return TrackProblemLocations ? m_inventoryData[slot - 1].EmptyStuckCount : 0;
		}
	}

	public bool MarkSlotEmpty(int slot)
	{
		SetSlotId(slot, "EMPTY");
		return true;
	}

	public bool MarkSlotUnknown(int slot)
	{
		SetSlotId(slot, "UNKNOWN");
		return true;
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

	internal static bool IsSuspiciousId(string id)
	{
		if (Constants.IsKnownInventoryToken(id))
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

	internal int BulkUpdate(List<Location> locs, string newId, MerchFlags newFlags)
	{
		lock (Lock)
		{
			using Transaction txn = new Transaction(TransactionType.Update);
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
				InventoryManager.Instance.EnqueueUpdate(txn, m_inventoryData[num]);
			}
			return InventoryManager.Instance.ExecuteTransaction(txn) ? locs.Count : 0;
		}
	}

	internal string SetSlotId(int slot, string id)
	{
		lock (Lock)
		{
			string iD = m_inventoryData[slot - 1].ID;
			m_inventoryData[slot - 1].ID = id;
			if (!InventoryManager.Instance.UpdateSlot(m_inventoryData[slot - 1]))
			{
				LogHelper.Instance.Log($"SetSlotId: Deck={Number}, Slot={slot}, ID={id} (previous = {iD}) returned FAILURE", LogEntryType.Error);
			}
			return iD;
		}
	}

	internal void CopyData(Deck deck)
	{
		lock (Lock)
		{
			m_inventoryData = deck.m_inventoryData;
		}
	}

	internal int GetEmptyCount()
	{
		int num = 0;
		lock (Lock)
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

	internal List<Location> GetUnknownLocations()
	{
		lock (Lock)
		{
			return MatchInventory("UNKNOWN");
		}
	}

	internal List<Location> GetEmptyLocations()
	{
		lock (Lock)
		{
			return MatchInventory("EMPTY");
		}
	}

	internal void ApplyQuadrantExcludes()
	{
		lock (Lock)
		{
			Quadrant[] quadrants = Quadrants;
			foreach (Quadrant quadrant in quadrants)
			{
				ExcludeRange(quadrant.Slots, quadrant.IsExcluded);
			}
		}
	}

	internal bool ImportInventory(List<DeckData> data)
	{
		lock (Lock)
		{
			if (!InventoryManager.Instance.BulkUpdate(data))
			{
				return false;
			}
			m_inventoryData = new DeckData[NumberOfSlots];
			for (int i = 0; i < NumberOfSlots; i++)
			{
				DeckData deckData = data[i];
				m_inventoryData[i] = new DeckData
				{
					Deck = Number,
					Slot = i + 1,
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
		lock (Lock)
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

	internal bool UpdateEmptyStuck(int slot)
	{
		if (!TrackProblemLocations)
		{
			return false;
		}
		lock (Lock)
		{
			DeckData deckData = m_inventoryData[slot - 1];
			ProblemLocationsLog.Instance.LogFormattedMessage("Incrementing Deck = {0} Slot = {1} with problem type ; current error count = {3}", Number, slot, deckData.EmptyStuckCount);
			if (++deckData.EmptyStuckCount >= ControllerConfiguration.Instance.MarkLocationUnknownThreshold && deckData.ID != "UNKNOWN")
			{
				ProblemLocationsLog.Instance.LogFormattedMessage("Error threshold met: set inventory to {0} at Deck = {1} Slot = {2}", "UNKNOWN", Number, slot);
				deckData.ID = "UNKNOWN";
			}
			return InventoryManager.Instance.UpdateSlot(deckData);
		}
	}

	internal bool ResetEmptyStuckCount(int slot)
	{
		if (!TrackProblemLocations)
		{
			return false;
		}
		lock (Lock)
		{
			DeckData deckData = m_inventoryData[slot - 1];
			if (deckData.EmptyStuckCount == 0)
			{
				return true;
			}
			deckData.EmptyStuckCount = 0;
			ProblemLocationsLog.Instance.LogFormattedMessage("Resetting Deck = {0} Slot = {1}", Number, slot);
			return InventoryManager.Instance.UpdateSlot(deckData);
		}
	}

	internal bool IsStuck(int slot)
	{
		if (!TrackProblemLocations)
		{
			return false;
		}
		lock (Lock)
		{
			return m_inventoryData[slot - 1].EmptyStuckCount >= ControllerConfiguration.Instance.MarkLocationUnknownThreshold;
		}
	}

	internal MerchFlags GetMerchFlags(int slot)
	{
		lock (Lock)
		{
			return m_inventoryData[slot - 1].MerchFlags;
		}
	}

	internal bool UpdateMerchFlag(MerchFlags flags, int slot)
	{
		lock (Lock)
		{
			DeckData deckData = m_inventoryData[slot - 1];
			deckData.MerchFlags = flags;
			return InventoryManager.Instance.UpdateSlot(deckData);
		}
	}

	internal bool IsExcluded(int slot)
	{
		lock (Lock)
		{
			return m_inventoryData[slot - 1].Excluded;
		}
	}

	internal int GetExcludedCount()
	{
		int num = 0;
		lock (Lock)
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
		lock (Lock)
		{
			DeckData deckData = m_inventoryData[slot - 1];
			if (deckData.Excluded != excluded)
			{
				deckData.Excluded = excluded;
				InventoryManager.Instance.UpdateSlot(deckData);
			}
		}
	}

	internal void ResetRange(Range slots)
	{
		lock (Lock)
		{
			using Transaction txn = new Transaction(TransactionType.Update);
			for (int i = slots.Start; i <= slots.End; i++)
			{
				DeckData deckData = m_inventoryData[i - 1];
				deckData.ID = "EMPTY";
				deckData.ReturnDate = null;
				deckData.EmptyStuckCount = 0;
				deckData.MerchFlags = MerchFlags.None;
				InventoryManager.Instance.EnqueueUpdate(txn, deckData);
			}
			InventoryManager.Instance.ExecuteTransaction(txn);
		}
	}

	internal void ResetRange(Range slots, string id, MerchFlags flags)
	{
		lock (Lock)
		{
			using Transaction txn = new Transaction(TransactionType.Update);
			for (int i = slots.Start; i <= slots.End; i++)
			{
				DeckData deckData = m_inventoryData[i - 1];
				deckData.ID = id;
				deckData.ReturnDate = null;
				deckData.EmptyStuckCount = 0;
				deckData.MerchFlags = flags;
				InventoryManager.Instance.EnqueueUpdate(txn, deckData);
			}
			InventoryManager.Instance.ExecuteTransaction(txn);
		}
	}

	internal bool AddEntries(Range slots)
	{
		lock (Lock)
		{
			DeckData[] inventoryData = m_inventoryData;
			m_inventoryData = new DeckData[NumberOfSlots];
			Array.Copy(inventoryData, m_inventoryData, inventoryData.Length);
			using Transaction txn = new Transaction(TransactionType.Insert);
			for (int i = slots.Start; i <= slots.End; i++)
			{
				m_inventoryData[i - 1] = new DeckData
				{
					Deck = Number,
					Slot = i,
					ID = "EMPTY",
					ReturnDate = null,
					Excluded = false,
					EmptyStuckCount = 0
				};
				InventoryManager.Instance.EnqueueInsert(txn, m_inventoryData[i - 1]);
			}
			return InventoryManager.Instance.ExecuteTransaction(txn);
		}
	}

	internal void RemoveEntries(Range slots)
	{
		lock (Lock)
		{
			DeckData[] inventoryData = m_inventoryData;
			m_inventoryData = new DeckData[NumberOfSlots];
			for (int i = 0; i < NumberOfSlots; i++)
			{
				m_inventoryData[i] = inventoryData[i];
			}
			InventoryManager.Instance.RemoveEntries(this, slots);
		}
	}

	internal void LoadData()
	{
		List<DeckData> list = null;
		try
		{
			lock (Lock)
			{
				list = InventoryManager.Instance.LoadDeck(Number);
				if (list.Count == NumberOfSlots)
				{
					m_inventoryData = list.ToArray();
					return;
				}
				LogHelper.Instance.Log($"LoadInventory: res.count = {list.Count}; deck count = {NumberOfSlots}; deleting deck & initializing to EMPTY", LogEntryType.Error);
				Range slots = new Range(1, list.Count);
				InventoryManager.Instance.RemoveEntries(this, slots);
				using Transaction txn = new Transaction(TransactionType.Insert);
				m_inventoryData = new DeckData[NumberOfSlots];
				for (int i = 0; i < NumberOfSlots; i++)
				{
					m_inventoryData[i] = new DeckData
					{
						Deck = Number,
						Slot = i + 1,
						ID = "EMPTY",
						ReturnDate = null,
						Excluded = false,
						EmptyStuckCount = 0
					};
					InventoryManager.Instance.EnqueueInsert(txn, m_inventoryData[i]);
				}
				InventoryManager.Instance.ExecuteTransaction(txn);
			}
		}
		finally
		{
			list?.Clear();
		}
	}

	internal void Dump()
	{
		LogHelper.Instance.Log("Deck {0} Number of Slots {1} Y-offset {2}", Number, NumberOfSlots, YOffset.ToString());
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

	private void ExcludeRange(Range slots, bool excluded)
	{
		using Transaction transaction = new Transaction(TransactionType.Update);
		for (int i = slots.Start; i <= slots.End; i++)
		{
			if (m_inventoryData[i - 1].Excluded != excluded)
			{
				int num = (excluded ? 1 : 0);
				transaction.Enqueue($"UPDATE {InventoryManager.Instance.TableName} SET Excluded = '{num}' WHERE Deck = '{Number}' AND Slot = '{i}'");
				m_inventoryData[i - 1].Excluded = excluded;
			}
		}
		InventoryManager.Instance.ExecuteTransaction(transaction);
	}

	private bool ResetDataInner(DeckData data)
	{
		data.ReturnDate = null;
		data.ID = "EMPTY";
		data.EmptyStuckCount = 0;
		data.MerchFlags = MerchFlags.None;
		return InventoryManager.Instance.UpdateSlot(data);
	}

	private List<int> SwapWithEmptyInner(string newId, MerchFlags newFlags, Range slots)
	{
		List<int> list = new List<int>();
		using Transaction txn = new Transaction(TransactionType.Update);
		for (int i = slots.Start - 1; i < slots.End; i++)
		{
			if (m_inventoryData[i].ID == "EMPTY" && !m_inventoryData[i].Excluded)
			{
				m_inventoryData[i].ID = newId;
				m_inventoryData[i].MerchFlags = newFlags;
				InventoryManager.Instance.EnqueueUpdate(txn, m_inventoryData[i]);
				list.Add(i + 1);
			}
		}
		if (!InventoryManager.Instance.ExecuteTransaction(txn))
		{
			list.Clear();
		}
		return list;
	}

	private List<Location> MatchInventory(string toMatch)
	{
		List<Location> list = new List<Location>();
		for (int i = 0; i < m_inventoryData.Length; i++)
		{
			if (!m_inventoryData[i].Excluded && m_inventoryData[i].ID == toMatch)
			{
				list.Add(new Location(Number, i + 1));
			}
		}
		return list;
	}
}
