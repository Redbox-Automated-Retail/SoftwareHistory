using System;
using System.Xml;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class DumpBinEntry
{
	public DateTime PutTime { get; private set; }

	public string ID { get; private set; }

	public static DumpBinEntry FromXml(XmlNode node)
	{
		string attributeValue = XmlNodeExtensions.GetAttributeValue<string>(node, "id", null);
		if (attributeValue == null)
		{
			return null;
		}
		string attributeValue2 = XmlNodeExtensions.GetAttributeValue<string>(node, "PutTime", null);
		DateTime putTime = DateTime.Now;
		if (!string.IsNullOrEmpty(attributeValue2))
		{
			try
			{
				putTime = DateTime.Parse(attributeValue2);
			}
			catch (Exception)
			{
				LogHelper.Instance.Log($"Unable to parse date time {attributeValue2}", LogEntryType.Error);
				putTime = DateTime.Now;
			}
		}
		return new DumpBinEntry(attributeValue, putTime);
	}

	internal void WriteElement(XmlWriter writer)
	{
		writer.WriteStartElement("bin-item");
		writer.WriteAttributeString("PutTime", PutTime.ToString());
		writer.WriteAttributeString("id", ID);
		writer.WriteEndElement();
	}

	internal DumpBinEntry(string id, DateTime putTime)
	{
		PutTime = putTime;
		ID = id;
	}
}
