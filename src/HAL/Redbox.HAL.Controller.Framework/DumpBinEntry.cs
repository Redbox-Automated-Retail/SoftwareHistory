using System;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal sealed class DumpBinEntry : IDumpBinInventoryItem, IXmlConfigurationElement
{
	public DateTime PutTime { get; private set; }

	public string ID { get; private set; }

	public void WriteElement(XmlWriter writer)
	{
		writer.WriteStartElement("bin-item");
		writer.WriteAttributeString("PutTime", PutTime.ToString());
		writer.WriteAttributeString("id", ID);
		writer.WriteEndElement();
	}

	internal static DumpBinEntry FromXml(XmlNode node, ErrorList errors)
	{
		string attributeValue = XmlNodeExtensions.GetAttributeValue(node, "id", "UNKNOWN");
		string attributeValue2 = XmlNodeExtensions.GetAttributeValue<string>(node, "PutTime", null);
		if (attributeValue2 == null)
		{
			return new DumpBinEntry(attributeValue);
		}
		DateTime? dateTime = null;
		try
		{
			return new DumpBinEntry(attributeValue, new DateTime?(DateTime.Parse(attributeValue2)).Value);
		}
		catch (Exception)
		{
			LogHelper.Instance.Log($"Unable to parse date time {attributeValue2}", LogEntryType.Error);
			dateTime = null;
			return new DumpBinEntry(attributeValue);
		}
	}

	internal DumpBinEntry(string id)
		: this(id, DateTime.Now)
	{
	}

	internal DumpBinEntry(string id, DateTime dt)
	{
		ID = id;
		PutTime = dt;
	}
}
