using System;
using System.Xml;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal class DeckData
{
	internal int Deck { get; set; }

	internal int Slot { get; set; }

	internal string ID { get; set; }

	internal DateTime? ReturnDate { get; set; }

	internal bool Excluded { get; set; }

	internal int EmptyStuckCount { get; set; }

	internal MerchFlags MerchFlags { get; set; }

	public static implicit operator Location(DeckData loc)
	{
		return new Location(loc.Deck, loc.Slot);
	}

	internal static DeckData FromXML(XmlNode node, ErrorList errors)
	{
		int? deck = XmlNodeExtensions.GetAttributeValue<int?>(node, "deck", null);
		int? attributeValue = XmlNodeExtensions.GetAttributeValue<int?>(node, "slot", null);
		string attributeValue2 = XmlNodeExtensions.GetAttributeValue<string>(node, "id", null);
		string attributeValue3 = XmlNodeExtensions.GetAttributeValue<string>(node, "ReturnTime", null);
		DateTime? returnDate = null;
		if (!string.IsNullOrEmpty(attributeValue3) && !attributeValue3.Equals("NONE"))
		{
			try
			{
				returnDate = DateTime.Parse(attributeValue3);
			}
			catch (Exception)
			{
				LogHelper.Instance.Log($"Unable to parse date time {attributeValue3}", LogEntryType.Error);
				returnDate = null;
			}
		}
		bool attributeValue4 = XmlNodeExtensions.GetAttributeValue(node, "excluded", defaultValue: false);
		int attributeValue5 = XmlNodeExtensions.GetAttributeValue(node, "emptyStuckCount", 0);
		string attributeValue6 = XmlNodeExtensions.GetAttributeValue(node, "merchFlags", "None");
		MerchFlags merchFlags = MerchFlags.None;
		try
		{
			merchFlags = (MerchFlags)Enum.Parse(typeof(MerchFlags), attributeValue6, ignoreCase: true);
		}
		catch
		{
			merchFlags = MerchFlags.None;
		}
		if (!deck.HasValue || !attributeValue.HasValue || string.IsNullOrEmpty(attributeValue2))
		{
			errors.Add(Error.NewError("P002", "A valid deck, slot, and id must be specified for each item element.", "Add a valid item element with a deck, slot, and id attributes."));
			LogHelper.Instance.Log($"Attempt to import invalid item. XmlNode: {node.OuterXml}", LogEntryType.Error);
			return null;
		}
		if (attributeValue2.Length > 100)
		{
			errors.Add(Error.NewError("P002", $"Barcode {attributeValue2} exceeds the maximum length of {100} characters.", "Ensure that the length of the barcode is valid."));
			LogHelper.Instance.Log($"Barcode {attributeValue2} exceeds the maximum length of {100} characters.", LogEntryType.Error);
			return null;
		}
		Deck deck2 = DecksManager.Instance.Decks.Find((Deck x) => x.Number == deck);
		if (deck2 == null)
		{
			errors.Add(Error.NewError("P003", $"The deck value must be between 1 and {DecksManager.Instance.Decks.Count}.", "Ensure the deck attribute value is within the valid range."));
			LogHelper.Instance.Log($"Deck {deck} doesn't match any decks in the configuration.", LogEntryType.Error);
			return null;
		}
		if (attributeValue < 1 || deck2.NumberOfSlots < attributeValue)
		{
			errors.Add(Error.NewError("P003", $"The slot must be between 1 and {deck2.NumberOfSlots}.", "Ensure the slot number is within the valid range."));
			LogHelper.Instance.Log($"Deck {deck2.Number} expects a value between 1 and {deck2.NumberOfSlots}, received {attributeValue}", LogEntryType.Error);
			return null;
		}
		DeckData deckData = new DeckData();
		deckData.Deck = deck.Value;
		deckData.Slot = attributeValue.Value;
		deckData.ReturnDate = returnDate;
		deckData.Excluded = attributeValue4;
		deckData.EmptyStuckCount = attributeValue5;
		deckData.MerchFlags = merchFlags;
		deckData.ID = attributeValue2;
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

	internal DeckData()
	{
		ID = "EMPTY";
		MerchFlags = MerchFlags.None;
	}
}
