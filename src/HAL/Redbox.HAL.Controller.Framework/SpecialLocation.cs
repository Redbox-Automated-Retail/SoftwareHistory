using System.Xml;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public class SpecialLocation
{
	public bool IsSecureLocation { get; protected set; }

	public bool CanMoveTo { get; protected set; }

	public string Name { get; private set; }

	public int? Deck { get; private set; }

	public Range Slots { get; private set; }

	public int? Offset { get; protected set; }

	public virtual string EncryptedPassword => string.Empty;

	public SpecialLocation(string name, int deck, Range slotRange)
	{
		Name = name;
		Deck = deck;
		Slots = slotRange;
	}

	public SpecialLocation(string name, int offset)
	{
		Name = name;
		Offset = offset;
	}

	public static SpecialLocation FromXmlNode(XmlNode node)
	{
		string attributeValue = XmlNodeExtensions.GetAttributeValue<string>(node, "Name", null);
		int attributeValue2 = XmlNodeExtensions.GetAttributeValue(node, "Deck", -1);
		if (attributeValue2 != -1)
		{
			int attributeValue3 = XmlNodeExtensions.GetAttributeValue(node, "StartSlot", -1);
			int attributeValue4 = XmlNodeExtensions.GetAttributeValue(node, "EndSlot", -1);
			return new SpecialLocation(attributeValue, attributeValue2, new Range(attributeValue3, attributeValue4));
		}
		return new SpecialLocation(attributeValue, XmlNodeExtensions.GetAttributeValue(node, "YOffset", (int?)0).Value);
	}

	public void ToXmlWriter(XmlWriter writer)
	{
		writer.WriteStartElement("SpecialLocation");
		writer.WriteAttributeString("Name", Name);
		if (Deck.HasValue)
		{
			writer.WriteAttributeString("Deck", XmlConvert.ToString(Deck.Value));
			writer.WriteAttributeString("StartSlot", XmlConvert.ToString(Slots.Start));
			writer.WriteAttributeString("EndSlot", XmlConvert.ToString(Slots.End));
		}
		else
		{
			writer.WriteAttributeString("YOffset", XmlConvert.ToString(Offset.Value));
		}
		writer.WriteEndElement();
	}

	public virtual Location MoveableLocation()
	{
		if (IsSecureLocation || !Deck.HasValue)
		{
			return null;
		}
		return new Location(Deck.Value, Slots.Start);
	}

	internal virtual bool ExcludeFromESP(Deck deck, int slot)
	{
		return false;
	}

	private SpecialLocation()
	{
		IsSecureLocation = true;
	}
}
