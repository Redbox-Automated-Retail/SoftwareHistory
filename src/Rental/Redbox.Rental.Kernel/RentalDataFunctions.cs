using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;

namespace Redbox.Rental.Kernel;

public static class RentalDataFunctions
{
	[KernelFunction(Name = "kernel.rental_check_for_new_blocklist")]
	internal static void CheckForNewBlockList()
	{
		IBlockList service = ServiceLocator.Instance.GetService<IBlockList>();
		if (service == null)
		{
			LogHelper.Instance.Log("IBlockList interface has not been added to the service locater.");
			return;
		}
		LogHelper.Instance.Log("BlockList.CheckForNewFile");
		service.CheckForNewFile();
	}

	[KernelFunction(Name = "kernel.rental_get_default_culture")]
	internal static string GetDefaultCulture()
	{
		IStoreManager service = ServiceLocator.Instance.GetService<IStoreManager>();
		if (service == null)
		{
			LogHelper.Instance.Log("IStoreManager interface has not been added to the service locater.");
			return null;
		}
		string defaultCulture = service.GetDefaultCulture();
		LogHelper.Instance.Log("StoreManager.GetDefaultCulture({0})", defaultCulture);
		return defaultCulture;
	}

	[KernelFunction(Name = "kernel.rental_get_alternate_culture")]
	internal static string GetAlternateCulture()
	{
		IStoreManager service = ServiceLocator.Instance.GetService<IStoreManager>();
		if (service == null)
		{
			LogHelper.Instance.Log("IStoreManager interface has not been added to the service locater.");
			return null;
		}
		string alternateCulture = service.GetAlternateCulture();
		LogHelper.Instance.Log("StoreManager.GetAlternateCulture({0})", alternateCulture);
		return alternateCulture;
	}

	[KernelFunction(Name = "kernel.get_can_sell_used")]
	internal static bool GetCanSellUsed()
	{
		try
		{
			return ServiceLocator.Instance.GetService<IStoreManager>().CanSellUsed;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in RentalDataFunctions.GetCanSellUsed", e);
			return false;
		}
	}

	[KernelFunction(Name = "kernel.is_multi_disc")]
	internal static bool IsMultiDisc(string productId)
	{
		try
		{
			bool result = false;
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			if (service != null)
			{
				long result2 = 0L;
				if (long.TryParse(productId, out result2))
				{
					result = service.IsMultiDisc(result2);
				}
			}
			return result;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in RentalDataFunctions.IsMultiDisc", e);
			return false;
		}
	}

	[KernelFunction(Name = "kernel.get_all_unmatched_multi_disc_barcodes")]
	internal static LuaTable GetAllUnmatchedMultiDiscBarcodes()
	{
		try
		{
			LuaTable result = null;
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			if (service != null)
			{
				result = service.GetAllUnmatchedMultiDiscBarcodes().ToLuaTable();
			}
			return result;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in RentalDataFunctions.GetAllUnmatchedMultiDiscBarcodes", e);
			return null;
		}
	}
}
