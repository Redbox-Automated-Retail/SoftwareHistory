using System;
using System.Collections.Generic;
using System.Xml;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

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

	internal static int ItemsCount()
	{
		return StoreManager.Instance.FindByName("HALCounters.vdb3")?.GetTableRowCount(string.Format("SELECT COUNT(*) FROM {0}", "BarcodesInBin")) ?? 0;
	}

	internal static List<DumpBinEntry> GetBarcodesInBin()
	{
		List<DumpBinEntry> rv = new List<DumpBinEntry>();
		IDataStore dataStore = StoreManager.Instance.FindByName("HALCounters.vdb3");
		if (dataStore == null)
		{
			return rv;
		}
		string query = string.Format("SELECT Barcode, PutTime FROM [{0}]", "BarcodesInBin");
		dataStore.ExectuteSelectQuery(delegate(IDataReader reader)
		{
			string id = reader.ReadString(0);
			DateTime? dateTime = reader.ReadDateTime(1);
			rv.Add(new DumpBinEntry(id, dateTime.Value));
		}, query);
		return rv;
	}

	internal static bool CodesRemoved(int expected)
	{
		IDataStore dataStore = StoreManager.Instance.FindByName("HALCounters.vdb3");
		if (dataStore == null)
		{
			return false;
		}
		try
		{
			string deleteQuery = string.Format("DELETE FROM [{0}]", "BarcodesInBin");
			return dataStore.Delete(deleteQuery) == expected;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Failed to delete inventory from dump bin table.", e);
			return false;
		}
	}

	internal static bool AddEntry(string code)
	{
		IDataStore dataStore = StoreManager.Instance.FindByName("HALCounters.vdb3");
		if (dataStore == null)
		{
			return false;
		}
		string arg = dataStore.FormatNullableTime(DateTime.Now);
		string insert = string.Format("INSERT INTO [{0}] ( Barcode, PutTime) VALUES ('{1}', {2})", "BarcodesInBin", code, arg);
		return dataStore.Insert(insert);
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
