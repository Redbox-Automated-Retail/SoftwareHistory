using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;

namespace Redbox.Rental.Kernel;

public static class InventoryFunctions
{
	[KernelFunction(Name = "kernel.rental_inventory_get_rapidstatus_all")]
	internal static LuaTable GetRapidStatus()
	{
		try
		{
			return GetAllRapidStatusAsLuaFormat(null)?.ToLuaTable();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("InventoryFunctions.GetRapidStatus - an unhanded exception occurred.", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.rental_inventory_get_rapidstatus_by_status")]
	internal static LuaTable GetRapidStatusByStatus(string status)
	{
		try
		{
			return GetAllRapidStatusAsLuaFormat(status)?.ToLuaTable();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("InventoryFunctions.GetRapidStatusByStatus - an unhanded exception occurred.", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.rental_inventory_get_barcodes_by_status")]
	internal static LuaTable GetBarcodesByStatus(string status, bool orderByTitle)
	{
		try
		{
			List<Inventory> allInventory = GetAllInventory(status);
			if (allInventory == null)
			{
				return null;
			}
			if (orderByTitle)
			{
				return (from a in allInventory
					orderby a.TitleId
					select a.Barcode).ToList().ToLuaTable();
			}
			return (from a in allInventory
				orderby a.Barcode
				select a.Barcode).ToList().ToLuaTable();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("InventoryFunctions.GetBarcodesByStatus - an unhanded exception occurred.", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.rental_inventory_get_rapidstatus")]
	internal static LuaTable GetRapidStatus(string barcode)
	{
		try
		{
			return GetRapidStatusAsLuaFormat(barcode)?.ToLuaTable();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("InventoryFunctions.GetRapidStatus - an unhanded exception occurred.", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.rental_inventory_get_rebalancesin")]
	internal static LuaTable GetRebalancesIn()
	{
		LuaTable result = new List<Dictionary<string, object>>().ToLuaTable();
		try
		{
			List<Dictionary<string, object>> rebalancesInAsLuaFormat = GetRebalancesInAsLuaFormat();
			if (rebalancesInAsLuaFormat == null)
			{
				return result;
			}
			return rebalancesInAsLuaFormat.ToLuaTable();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("InventoryFunctions.GetRebalancesIn - an unhanded exception occurred.", e);
			return result;
		}
	}

	[KernelFunction(Name = "kernel.rental_inventory_get_fill_quantity")]
	internal static int GetFillQuantity()
	{
		try
		{
			return ServiceLocator.Instance.GetService<IInventory>()?.GetFillQuantity() ?? 0;
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("InventoryFunctions.GetFillQuantity - an unhanded exception occurred.", e);
			return 0;
		}
	}

	[KernelFunction(Name = "kernel.rental_inventory_check_for_new_files")]
	internal static void CheckForNewFiles(Action inventoryCompletion, Action newRapidStatus)
	{
		try
		{
			ServiceLocator.Instance.GetService<IInventory>()?.CheckForNewFiles(inventoryCompletion, newRapidStatus);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("InventoryFunctions.CheckForNewFiles - an unhanded exception occurred.", e);
		}
	}

	[KernelFunction(Name = "kernel.rental_inventory_get_inventory_action_modification_time")]
	internal static string GetInventoryActionModificationTime(string format)
	{
		try
		{
			IInventory service = ServiceLocator.Instance.GetService<IInventory>();
			if (service == null)
			{
				return null;
			}
			if (!service.InventoryActionModificationTime.HasValue)
			{
				return null;
			}
			return service.InventoryActionModificationTime.Value.ToLocalTime().ToString(format);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("InventoryFunctions.GetInventoryActionModificationTime - an unhanded exception occurred.", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.rental_inventory_get_rapid_status_modification_time")]
	internal static string GetRadpidStatusModificationTime(string format)
	{
		try
		{
			IInventory service = ServiceLocator.Instance.GetService<IInventory>();
			if (service == null)
			{
				return null;
			}
			if (!service.RapidStatusModificationTime.HasValue)
			{
				return null;
			}
			return service.RapidStatusModificationTime.Value.ToLocalTime().ToString(format);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("InventoryFunctions.GetRapidStatusModificationTime - an unhanded exception occurred.", e);
			return null;
		}
	}

	private static Dictionary<string, object> GetAllRapidStatusAsLuaFormat(string filter)
	{
		IInventory service = ServiceLocator.Instance.GetService<IInventory>();
		if (service == null)
		{
			return null;
		}
		ProductLookupCatalogModel rapidStatus = service.GetRapidStatus();
		if (rapidStatus == null)
		{
			return null;
		}
		Dictionary<string, object> dict = new Dictionary<string, object>();
		rapidStatus.Barcodes.ForEach(delegate(KeyValuePair<string, Inventory> item)
		{
			string text = item.Value.Code.ToString();
			if (item.Value.Code == InventoryStatusCode.WrongTitle)
			{
				text = "Wrong Title";
			}
			if (string.IsNullOrEmpty(filter) || text.Equals(filter, StringComparison.CurrentCultureIgnoreCase))
			{
				Dictionary<string, object> value = new Dictionary<string, object>
				{
					["barcode"] = item.Value.Barcode,
					["product_id"] = item.Value.TitleId,
					["status"] = text,
					["total_rental_count"] = item.Value.TotalRentalCount
				};
				dict[item.Value.Barcode] = value;
			}
		});
		return dict;
	}

	private static List<Inventory> GetAllInventory(string filter)
	{
		IInventory service = ServiceLocator.Instance.GetService<IInventory>();
		if (service == null)
		{
			return null;
		}
		ProductLookupCatalogModel rapidStatus = service.GetRapidStatus();
		if (rapidStatus == null)
		{
			return null;
		}
		List<Inventory> list = new List<Inventory>();
		rapidStatus.Barcodes.ForEach(delegate(KeyValuePair<string, Inventory> item)
		{
			string text = item.Value.Code.ToString();
			if (item.Value.Code == InventoryStatusCode.WrongTitle)
			{
				text = "Wrong Title";
			}
			if (string.IsNullOrEmpty(filter) || text.Equals(filter, StringComparison.CurrentCultureIgnoreCase))
			{
				list.Add(item.Value);
			}
		});
		return list;
	}

	private static Dictionary<string, object> GetRapidStatusAsLuaFormat(string barcode)
	{
		IInventory service = ServiceLocator.Instance.GetService<IInventory>();
		if (service == null)
		{
			return null;
		}
		Inventory rapidStatus = service.GetRapidStatus(barcode);
		if (rapidStatus == null)
		{
			return null;
		}
		string value = rapidStatus.Code.ToString();
		if (rapidStatus.Code == InventoryStatusCode.WrongTitle)
		{
			value = "Wrong Title";
		}
		return new Dictionary<string, object>
		{
			["barcode"] = rapidStatus.Barcode,
			["product_id"] = rapidStatus.TitleId,
			["status"] = value,
			["total_rental_count"] = rapidStatus.TotalRentalCount
		};
	}

	private static List<Dictionary<string, object>> GetRebalancesInAsLuaFormat()
	{
		IInventory service = ServiceLocator.Instance.GetService<IInventory>();
		if (service == null)
		{
			return null;
		}
		List<RebalancesInModel> rebalancesIn = service.GetRebalancesIn();
		if (rebalancesIn == null)
		{
			return null;
		}
		List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
		rebalancesIn.ForEach(delegate(RebalancesInModel item)
		{
			Dictionary<string, object> item2 = new Dictionary<string, object>
			{
				["TitleId"] = item.TitleId,
				["Quantity"] = item.Quantity
			};
			result.Add(item2);
		});
		return result;
	}
}
