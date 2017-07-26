using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Services;
using Redbox.Rental.Services.KioskProduct;

namespace Redbox.Rental.Kernel;

public static class RentalDataFunctions
{
	[KernelFunction(Name = "kernel.rental_check_new_rebalance")]
	internal static void CheckNewRebalance(string kioskIdFilter)
	{
		new RebalanceService(kioskIdFilter).CheckNewRebalance();
	}

	[KernelFunction(Name = "kernel.rental_is_name_in_blocklist")]
	internal static bool IsNameInBlockList(string firstName, string lastName, int market_id, string zipCode)
	{
		IBlockList service = ServiceLocator.Instance.GetService<IBlockList>();
		if (service == null)
		{
			LogHelper.Instance.Log("IBlockList interface has not been added to the service locater.");
			return false;
		}
		LogHelper.Instance.Log("IsNameInBlockList(FirstName={0},LastName={1},Market_id={2},ZipCode={3})", "pci", "pci", market_id, zipCode);
		return service.IsNameInBlockList(firstName, lastName, market_id, zipCode);
	}

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

	[KernelFunction(Name = "kernel.rental_get_products")]
	internal static LuaTable RentalGetProducts(string browseViewFilter, string familyName, string familyType, string selectedSort, string selectedGenre, LuaTable includeTitles, LuaTable excludeTitles, bool isADAView)
	{
		LogHelper.Instance.Log("RentalGetProducts by browseViewfilter: {0} family: {1} familyType: {2} selectedSort: {3} selectedGenere: {4}, isADAView: {5} ", browseViewFilter, familyName, familyType, selectedSort, selectedGenre, isADAView);
		try
		{
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			if (string.IsNullOrEmpty(browseViewFilter))
			{
				browseViewFilter = string.Empty;
			}
			if (string.IsNullOrEmpty(familyName))
			{
				familyName = string.Empty;
			}
			if (string.IsNullOrEmpty(familyType))
			{
				familyType = string.Empty;
			}
			if (string.IsNullOrEmpty(selectedSort))
			{
				selectedSort = string.Empty;
			}
			if (string.IsNullOrEmpty(selectedGenre))
			{
				selectedGenre = string.Empty;
			}
			BrowseViewFilter browseViewFilter2 = browseViewFilter.ConvertToBrowseViewFilter();
			TitleFamily titleFamily = familyName.ConvertToTitleFamily();
			TitleType titleType = familyType.ConvertToTitleType();
			BrowseSort browseSort = selectedSort.ConvertToBrowseSort(browseViewFilter2);
			long? genreFilter = selectedGenre.ConvertToGenre();
			List<long> list = new List<long>();
			List<long> list2 = new List<long>();
			if (includeTitles != null)
			{
				foreach (object key in includeTitles.Keys)
				{
					list.Add(Convert.ToInt64(key));
				}
			}
			if (excludeTitles != null)
			{
				foreach (object key2 in excludeTitles.Keys)
				{
					list2.Add(Convert.ToInt64(key2));
				}
			}
			LogHelper.Instance.Log("RentalGetProducts by View Filter: {0} Title Family: {1} Title Type: {2}", browseViewFilter2, titleFamily, titleType);
			ErrorList products = service.GetProducts(browseViewFilter2, titleFamily, titleType, browseSort, genreFilter, list, list2, isADAView, out var products2);
			if (products.Count > 0)
			{
				products.ToLogHelper();
				return null;
			}
			if (products2 == null)
			{
				LogHelper.Instance.Log("Error: RentalDataFunctions.RentalGetProducts return null");
				return null;
			}
			LuaTable result = products2.ToDictionary().ToLuaTable();
			LogHelper.Instance.Log("RentalGetProducts by browseViewfilter count: {0}", products2.Count);
			return result;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in RentalDataFunctions.RentalGetProducts", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.rental_get_carousel_products")]
	internal static LuaTable RentalGetCarouselProducts(bool showCollections)
	{
		LogHelper.Instance.Log("RentalGetCarouselProducts");
		try
		{
			ErrorList carouselProducts = ServiceLocator.Instance.GetService<IDataService>().GetCarouselProducts(showCollections, out var products);
			if (carouselProducts.Count > 0)
			{
				carouselProducts.ToLogHelper();
				return null;
			}
			if (products == null)
			{
				LogHelper.Instance.Log("Error: RentalDataFunctions.RentalGetCarouselProducts return null");
				return null;
			}
			return products.ToDictionary().ToLuaTable();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in RentalDataFunctions.RentalGetCarouselProducts", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.rental_get_title_rollup")]
	internal static LuaTable RentalGetTitleRollup(long productGroupId)
	{
		LogHelper.Instance.Log("RentalDataFunctions.RentalGetTitleRollup by ProductGroupId: {0}", productGroupId);
		try
		{
			ErrorList titleRollupProduct = ServiceLocator.Instance.GetService<IDataService>().GetTitleRollupProduct(productGroupId, out var kioskProduct);
			if (titleRollupProduct.Count > 0)
			{
				titleRollupProduct.ToLogHelper();
				return null;
			}
			if (kioskProduct == null)
			{
				LogHelper.Instance.Log("Error: RentalDataFunctions.RentalGetTitleRollup returned null");
				return null;
			}
			return kioskProduct.ToDictionary().ToLuaTable();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in RentalDataFunctions.RentalGetTitleRollup", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.rental_get_collection")]
	internal static LuaTable RentalGetCollection(long collectionId, string familyName, string familyType, LuaTable includeTitles, LuaTable excludeTitles)
	{
		return OrchestrationService.RentalGetCollection(collectionId, familyName, familyType, includeTitles, excludeTitles);
	}

	[KernelFunction(Name = "kernel.rental_get_collection_products")]
	internal static LuaTable RentalGetCollectionProducts(string familyName, string familyType, LuaTable includeTitles, LuaTable excludeTitles)
	{
		LogHelper.Instance.Log("RentalGetCollectionProducts");
		try
		{
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			if (string.IsNullOrEmpty(familyName))
			{
				familyName = string.Empty;
			}
			if (string.IsNullOrEmpty(familyType))
			{
				familyType = string.Empty;
			}
			TitleFamily titleFamily = familyName.ConvertToTitleFamily();
			TitleType titleType = familyType.ConvertToTitleType();
			List<long> list = new List<long>();
			List<long> list2 = new List<long>();
			if (includeTitles != null)
			{
				foreach (object key in includeTitles.Keys)
				{
					list.Add(Convert.ToInt64(key));
				}
			}
			if (excludeTitles != null)
			{
				foreach (object key2 in excludeTitles.Keys)
				{
					list2.Add(Convert.ToInt64(key2));
				}
			}
			LogHelper.Instance.Log("rental_get_collection_products by Title Family: {0} Title Type: {1}", titleFamily, titleType);
			ErrorList collectionProducts = service.GetCollectionProducts(out var products);
			if (collectionProducts.Count > 0)
			{
				collectionProducts.ToLogHelper();
				return null;
			}
			if (products == null)
			{
				LogHelper.Instance.Log("Error: RentalDataFunctions.RentalGetCollectionProducts return null");
				return null;
			}
			return products.ToDictionary(titleFamily, titleType, list, list2).ToLuaTable();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in RentalDataFunctions.RentalGetCollectionProducts", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.rental_is_collections_enabled")]
	internal static bool RentalIsCollectionsEnabled()
	{
		return OrchestrationService.RentalIsCollectionsEnabled();
	}

	[KernelFunction(Name = "kernel.get_marketing_config")]
	internal static LuaTable GetMarkingConfig()
	{
		LuaTable result = null;
		IMarketingConfigurationManager service = ServiceLocator.Instance.GetService<IMarketingConfigurationManager>();
		IKernelService service2 = ServiceLocator.Instance.GetService<IKernelService>();
		if (service != null && service2 != null)
		{
			result = new Dictionary<string, object>
			{
				["show_game_banner"] = service.ShowGameBanner,
				["show_bluray_browse"] = service.ShowBlurayBrowse,
				["show_collections"] = service.ShowCollections,
				["show_collections_banner"] = service.ShowCollectionsBanner,
				["show_collections_popup"] = service.ShowCollectionsPopUp,
				["show_collections_popup_on_coming_soon"] = service.CollectionsPopUpOnComingSoon,
				["collections_popup_timer"] = service.CollectionsPopUpTimer,
				["collections_popup_paddle_clicks"] = service.CollectionsPopUpPaddleClicks
			}.ToLuaTable();
		}
		return result;
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

	[KernelFunction(Name = "kernel.get_can_sell_used_movies")]
	internal static bool GetCanSellUsedMovies()
	{
		try
		{
			return ServiceLocator.Instance.GetService<IStoreManager>().CanSellUsedMovies;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in RentalDataFunctions.GetCanSellUsedMovies", e);
			return false;
		}
	}

	[KernelFunction(Name = "kernel.get_can_sell_used_games")]
	internal static bool GetCanSellUsedGames()
	{
		try
		{
			return ServiceLocator.Instance.GetService<IStoreManager>().CanSellUsedGames;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in RentalDataFunctions.GetCanSellUsedGames", e);
			return false;
		}
	}
}
