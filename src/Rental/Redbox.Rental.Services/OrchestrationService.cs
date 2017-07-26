using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Services.KioskProduct;

namespace Redbox.Rental.Services;

public static class OrchestrationService
{
	public static bool DataServiceRegisterImage(string name, string cacheName)
	{
		IBitmapCacheService service = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		if (service.GetImage(name) != null)
		{
			return true;
		}
		byte[] image = ServiceLocator.Instance.GetService<IDataService>().GetImage(cacheName);
		if (image == null)
		{
			LogHelper.Instance.Log("OrchestrationService.RegisterImageFromCache was not able to read any data for name: {0} cacheName {1}", name, cacheName);
			return false;
		}
		return service.RegisterImage(name, image) != null;
	}

	public static List<IKioskProduct> RentalGetCarouselProducts()
	{
		LogHelper.Instance.Log("RentalGetCarouselProducts");
		try
		{
			ErrorList carouselProducts = ServiceLocator.Instance.GetService<IDataService>().GetCarouselProducts(RentalIsCollectionsEnabled(), out var products);
			if (carouselProducts.Count > 0)
			{
				carouselProducts.ToLogHelper();
				return null;
			}
			if (products == null)
			{
				LogHelper.Instance.Log("Error: OrchestrationService.RentalGetCarouselProducts return null");
				return null;
			}
			return products;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in OrchestrationService.RentalGetCarouselProducts", e);
			return null;
		}
	}

	public static bool RentalRefreshIfNeeded()
	{
		LogHelper.Instance.Log("RentalRefreshIfNeeded");
		try
		{
			return ServiceLocator.Instance.GetService<IDataService>().RefreshIfNeeded();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in OrchestrationService.RentalRefreshIfNeeded", e);
			return false;
		}
	}

	public static LuaTable RentalGetCollection(long collectionId, string familyName, string familyType, LuaTable includeTitles, LuaTable excludeTitles)
	{
		LogHelper.Instance.Log("RentalGetCollection");
		try
		{
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			ICollectionProduct collection = null;
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
			ErrorList collection2 = service.GetCollection(collectionId, out collection);
			if (collection2.Count > 0)
			{
				collection2.ToLogHelper();
				return null;
			}
			if (collection == null)
			{
				LogHelper.Instance.Log("Error: OrchestrationService.RentalGetCollection return null");
				return null;
			}
			return collection.ToDictionary(titleFamily, titleType, list, list2).ToLuaTable();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was raised in OrchestrationService.RentalGetCollection", e);
			return null;
		}
	}

	public static bool RentalIsCollectionsEnabled()
	{
		IMarketingConfigurationManager service = ServiceLocator.Instance.GetService<IMarketingConfigurationManager>();
		IResourceBundleService service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service == null || service2 == null)
		{
			return false;
		}
		ApplicationState instance = ApplicationState.Instance;
		if (instance == null || !instance.IsADAAccessible)
		{
			return service.ShowCollections;
		}
		return false;
	}

	public static void CreateStartViewAnalytics(string sessionType)
	{
		CreateStartViewAnalytics(Enum<AnalyticsSessionType>.ParseIgnoringCase(sessionType, AnalyticsSessionType.BrowseVend));
	}

	public static string GetCulture()
	{
		return ServiceLocator.Instance.GetService<IResourceBundleService>()?.Filter["culture"] ?? null;
	}

	public static void SetCulture(string culture)
	{
		ServiceLocator.Instance.GetService<IResourceBundleService>().Filter["culture"] = culture;
		ResetCurrentView();
	}

	public static void ViewShow(string viewName)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		service.Push(viewName);
		service.Show();
	}

	private static void ResetCurrentView()
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		service.ClearCache();
		service.Show();
	}

	public static bool IsCultureSpanish()
	{
		return (ServiceLocator.Instance.GetService<IResourceBundleService>()?.Filter["culture"] ?? null) == "es-MX";
	}

	public static string GetDefaultCulture()
	{
		return ServiceLocator.Instance.GetService<IStoreManager>()?.GetDefaultCulture();
	}

	public static string GetOtherCulture()
	{
		return ServiceLocator.Instance.GetService<IStoreManager>()?.GetAlternateCulture();
	}

	public static void CreateStartViewAnalytics(AnalyticsSessionType sessionType)
	{
		IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
		if (service == null)
		{
			return;
		}
		IShoppingSession currentSession = service.GetCurrentSession();
		if (currentSession == null)
		{
			return;
		}
		_ = currentSession.Id;
		ISessionAnalyticsService service2 = ServiceLocator.Instance.GetService<ISessionAnalyticsService>();
		if (service2 != null && !service2.SessionExists(currentSession.Id.ToString()))
		{
			service2.Start(currentSession.Id.ToString(), sessionType);
			IViewService service3 = ServiceLocator.Instance.GetService<IViewService>();
			if (service3 != null)
			{
				service2.AddView(currentSession.Id.ToString(), service3?.GetStartOverViewName());
			}
		}
	}

	public static void AnalyticsAddViewEvent(string subItemType, string subItemValue)
	{
		ServiceLocator.Instance.GetService<ISessionAnalyticsService>()?.AddViewEvent(subItemType, subItemValue);
	}

	public static void RemoveTimer(string timerName)
	{
		ServiceLocator.Instance.GetService<ITimerService>()?.RemoveTimer(timerName);
	}

	public static void SetMacro(string name, string value)
	{
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		if (service != null)
		{
			service[name] = value;
		}
	}

	public static object[] ExecuteScript(string name)
	{
		List<object> list = new List<object>();
		IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
		if (service != null)
		{
			ReadOnlyCollection<object> readOnlyCollection = service.ExecuteScript(name);
			if (readOnlyCollection != null)
			{
				list.AddRange(readOnlyCollection);
			}
		}
		return list.ToArray();
	}
}
