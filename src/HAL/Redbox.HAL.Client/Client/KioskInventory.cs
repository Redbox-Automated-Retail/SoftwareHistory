using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Redbox.HAL.Client;

public static class KioskInventory
{
	public static List<InventoryLocation> Load(HardwareService service)
	{
		List<InventoryLocation> list = new List<InventoryLocation>();
		HardwareCommandResult inventoryState = service.GetInventoryState();
		if (!inventoryState.Success)
		{
			return list;
		}
		XmlDocument xmlDocument = new XmlDocument();
		using StringReader txtReader = new StringReader(inventoryState.CommandMessages[0]);
		xmlDocument.Load(txtReader);
		if (xmlDocument.DocumentElement == null)
		{
			return list;
		}
		XmlNodeList elementsByTagName = xmlDocument.GetElementsByTagName("item");
		for (int i = 0; i < elementsByTagName.Count; i++)
		{
			string value = elementsByTagName[i].Attributes["id"].Value;
			string value2 = elementsByTagName[i].Attributes["ReturnTime"].Value;
			DateTime? returnTime = null;
			if (value2 != "NONE")
			{
				returnTime = DateTime.Parse(value2);
			}
			string value3 = elementsByTagName[i].Attributes["excluded"].Value;
			string value4 = elementsByTagName[i].Attributes["emptyStuckCount"].Value;
			string value5 = elementsByTagName[i].Attributes["merchFlags"].Value;
			InventoryLocation item = new InventoryLocation(Convert.ToInt32(elementsByTagName[i].Attributes["deck"].Value), Convert.ToInt32(elementsByTagName[i].Attributes["slot"].Value), value, Convert.ToInt32(value4), returnTime, value5, Convert.ToBoolean(value3));
			list.Add(item);
		}
		return list;
	}
}
