using System;
using System.Xml;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Controller.Framework;

internal sealed class DeckData
{
	internal string ID { get; set; }

	internal int Deck { get; private set; }

	internal int Slot { get; private set; }

	internal DateTime? ReturnDate { get; set; }

	internal bool Excluded { get; set; }

	internal int EmptyStuckCount { get; set; }

	internal MerchFlags MerchFlags { get; set; }

	public static implicit operator Location(DeckData loc)
	{
		return new Location(loc.Deck, loc.Slot);
	}

	internal static DeckData CloneFrom(DeckData template)
	{
		DeckData deckData = new DeckData(template.Deck, template.Slot);
		deckData.ReturnDate = template.ReturnDate;
		deckData.ID = template.ID;
		deckData.MerchFlags = template.MerchFlags;
		deckData.EmptyStuckCount = template.EmptyStuckCount;
		deckData.Excluded = template.Excluded;
		return deckData;
	}

	internal static DeckData FromXML(XmlNode node, ErrorList errors)
	{
		int? attributeValue = XmlNodeExtensions.GetAttributeValue<int?>(node, "deck", null);
		int? attributeValue2 = XmlNodeExtensions.GetAttributeValue<int?>(node, "slot", null);
		string attributeValue3 = XmlNodeExtensions.GetAttributeValue<string>(node, "id", null);
		string attributeValue4 = XmlNodeExtensions.GetAttributeValue<string>(node, "ReturnTime", null);
		DateTime? returnDate = null;
		if (!string.IsNullOrEmpty(attributeValue4) && !attributeValue4.Equals("NONE"))
		{
			try
			{
				returnDate = DateTime.Parse(attributeValue4);
			}
			catch (Exception)
			{
				LogHelper.Instance.Log($"Unable to parse date time {attributeValue4}", LogEntryType.Error);
				returnDate = null;
			}
		}
		bool attributeValue5 = XmlNodeExtensions.GetAttributeValue(node, "excluded", defaultValue: false);
		int attributeValue6 = XmlNodeExtensions.GetAttributeValue(node, "emptyStuckCount", 0);
		string attributeValue7 = XmlNodeExtensions.GetAttributeValue(node, "merchFlags", "None");
		MerchFlags merchFlags = Enum<MerchFlags>.ParseIgnoringCase(attributeValue7, MerchFlags.None);
		if (!attributeValue.HasValue || !attributeValue2.HasValue || string.IsNullOrEmpty(attributeValue3))
		{
			errors.Add(Error.NewError("P004", "A valid deck, slot, and id must be specified for each item element.", "Add a valid item element with a deck, slot, and id attributes."));
			LogHelper.Instance.Log($"Attempt to import invalid item. XmlNode: {node.OuterXml}", LogEntryType.Error);
			return null;
		}
		if (attributeValue3.Length > 100)
		{
			errors.Add(Error.NewError("P005", $"Barcode {attributeValue3} exceeds the maximum length of {100} characters.", "Ensure that the length of the barcode is valid."));
			LogHelper.Instance.Log($"Barcode {attributeValue3} exceeds the maximum length of {100} characters.", LogEntryType.Error);
			return null;
		}
		Deck deckNumber = DecksManager.Instance.GetDeckNumber(attributeValue.Value);
		if (null == deckNumber)
		{
			errors.Add(Error.NewError("P003", $"The deck value must be between 1 and {DecksManager.Instance.Count}.", "Ensure the deck attribute value is within the valid range."));
			LogHelper.Instance.Log($"Deck {attributeValue} doesn't match any decks in the configuration.", LogEntryType.Error);
			return null;
		}
		if (attributeValue2 < 1 || deckNumber.NumberOfSlots < attributeValue2)
		{
			errors.Add(Error.NewError("P003", $"The slot must be between 1 and {deckNumber.NumberOfSlots}.", "Ensure the slot number is within the valid range."));
			LogHelper.Instance.Log($"Deck {deckNumber.Number} expects a value between 1 and {deckNumber.NumberOfSlots}, received {attributeValue2}", LogEntryType.Error);
			return null;
		}
		DeckData deckData = new DeckData();
		deckData.Deck = attributeValue.Value;
		deckData.Slot = attributeValue2.Value;
		deckData.ReturnDate = returnDate;
		deckData.Excluded = attributeValue5;
		deckData.EmptyStuckCount = attributeValue6;
		deckData.MerchFlags = merchFlags;
		deckData.ID = attributeValue3;
		return deckData;
	}

	public override bool Equals(object o)
	{
		if (!(o is DeckData deckData))
		{
			return false;
		}
		if (object.ReferenceEquals(this, deckData))
		{
			return true;
		}
		if (Deck != deckData.Deck || Slot != deckData.Slot)
		{
			return false;
		}
		if (ID != deckData.ID)
		{
			return false;
		}
		if (Excluded != deckData.Excluded)
		{
			return false;
		}
		if (EmptyStuckCount != deckData.EmptyStuckCount)
		{
			return false;
		}
		if (!ReturnDate.HasValue)
		{
			if (!deckData.ReturnDate.HasValue)
			{
				return true;
			}
			return false;
		}
		return ReturnDate.Equals(deckData.ReturnDate);
	}

	internal void WriteToXML(XmlTextWriter writer)
	{
		writer.WriteAttributeString("deck", XmlConvert.ToString(Deck));
		writer.WriteAttributeString("slot", XmlConvert.ToString(Slot));
		writer.WriteAttributeString("id", ID);
		writer.WriteAttributeString("ReturnTime", (!ReturnDate.HasValue) ? "NONE" : ReturnDate.Value.ToString());
		writer.WriteAttributeString("excluded", Excluded.ToString());
		writer.WriteAttributeString("emptyStuckCount", EmptyStuckCount.ToString());
		writer.WriteAttributeString("merchFlags", MerchFlags.ToString());
	}

	internal string UpdateCommand(IDataStore store)
	{
		string text = store.FormatNullableTime(ReturnDate);
		return string.Format("UPDATE {0} SET ID = '{1}', LastReturnTime = {2}, Excluded = '{3}', EmptyStuckCount = '{4}', MerchFlags = '{5}' WHERE  Deck = '{6}' AND Slot = '{7}'", "Inventory_v1", ID, text, Excluded ? 1 : 0, EmptyStuckCount, (int)MerchFlags, Deck, Slot);
	}

	internal string InsertCommand(IDataStore store)
	{
		string text = store.FormatNullableTime(ReturnDate);
		return string.Format("INSERT INTO [{0}] (Deck, Slot, ID, LastReturnTime, Excluded, EmptyStuckCount, MerchFlags) VALUES ('{1}', '{2}', '{3}', {4}, false, '0', '0')", "Inventory_v1", Deck, Slot, ID, text);
	}

	internal void Reset()
	{
		ReturnDate = null;
		ID = "EMPTY";
		EmptyStuckCount = 0;
		MerchFlags = MerchFlags.None;
	}

	internal DeckData(int deck, int slot)
	{
		Deck = deck;
		Slot = slot;
		ID = "EMPTY";
		MerchFlags = MerchFlags.None;
		ReturnDate = null;
		EmptyStuckCount = 0;
		Excluded = false;
	}

	private DeckData()
	{
	}
}
