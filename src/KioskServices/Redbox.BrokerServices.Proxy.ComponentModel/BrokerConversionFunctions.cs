using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.BrokerServices.Proxy.ComponentModel;

public static class BrokerConversionFunctions
{
	public static Dictionary<string, ReadOnlyCollection<IProductItem>> ConvertPickupResults(LuaTable pickupResults, IReservationServices reservationServices)
	{
		Dictionary<string, ReadOnlyCollection<IProductItem>> dictionary = new Dictionary<string, ReadOnlyCollection<IProductItem>>();
		foreach (object key in pickupResults.Keys)
		{
			if (!(pickupResults[key] is LuaTable luaTable))
			{
				continue;
			}
			List<IProductItem> list = new List<IProductItem>();
			foreach (object key2 in luaTable.Keys)
			{
				if (luaTable[key2] is LuaTable luaTable2)
				{
					IProductItem productItem = reservationServices.NewProductItem();
					productItem.Barcode = (string)luaTable2["barcode"];
					productItem.MultiDisc = (bool)luaTable2["multi_disc"];
					productItem.Status = Enum<LineItemStatus>.ParseIgnoringCase((string)luaTable2["status"], LineItemStatus.InventoryError);
					list.Add(productItem);
				}
			}
			dictionary[key.ToString()] = list.AsReadOnly();
		}
		return dictionary;
	}
}
