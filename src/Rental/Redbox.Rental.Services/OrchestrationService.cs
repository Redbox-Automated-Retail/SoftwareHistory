using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Xml;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services;

public static class OrchestrationService
{
	private static Action _idleTimerExpireSessionCallback;

	private static Action<long> _getEmailAddressAndRegisterEmailReminderCallback;

	public static bool IsCollectionsPopupEnabled()
	{
		ISession instance = Redbox.Rental.Services.Session.Session.Instance;
		if (instance != null && !instance.BrowseState.CollectionsPopupShown && RentalIsCollectionsEnabled())
		{
			return RentalShowCollectionsPopup();
		}
		return false;
	}

	public static bool IsCollectionsPopupTimerEnabled()
	{
		IMarketingConfigurationManager service = ServiceLocator.Instance.GetService<IMarketingConfigurationManager>();
		if (IsCollectionsPopupEnabled())
		{
			return (service?.CollectionsPopUpTimer ?? 0) > 0;
		}
		return false;
	}

	public static void RegisterIdleTimerExpireSessionCallback(Action idleTimerExpireSessionCallback)
	{
		_idleTimerExpireSessionCallback = idleTimerExpireSessionCallback;
	}

	public static void RegisterGetEmailAddressAndRegisterEmailReminderCallback(Action<long> getEmailAddressAndRegisterEmailReminderCallback)
	{
		_getEmailAddressAndRegisterEmailReminderCallback = getEmailAddressAndRegisterEmailReminderCallback;
	}

	public static void InvokeGetEmailAddressAndRegisterEmailReminderCallback(long productId)
	{
		if (_getEmailAddressAndRegisterEmailReminderCallback != null)
		{
			_getEmailAddressAndRegisterEmailReminderCallback(productId);
		}
	}

	public static void InvokeIdleTimerExpireSessionCallback()
	{
		if (_idleTimerExpireSessionCallback != null)
		{
			_idleTimerExpireSessionCallback();
		}
	}

	public static int GetIdleTimerTimeout()
	{
		int num = 40000;
		if (IsCollectionsPopupEnabled())
		{
			num = ServiceLocator.Instance.GetService<IMarketingConfigurationManager>()?.CollectionsPopUpTimer ?? 0;
			num = Math.Min(Math.Max(num, 16000), 60000);
		}
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		IViewService service2 = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null && service2 != null)
		{
			string timeoutType = ((service2.Peek() == "help_document_view") ? "help" : "long");
			num = service.GetTimeout(timeoutType, num);
		}
		return num;
	}

	public static void ResetApplicationIdleTimerTimeout()
	{
		IIdleTimerService service = ServiceLocator.Instance.GetService<IIdleTimerService>();
		if (service != null)
		{
			IIdleTimer idleTimer = service.GetIdleTimer("idle_timer");
			if (idleTimer != null && (idleTimer.Timeout ?? 0) != GetIdleTimerTimeout())
			{
				idleTimer.Timeout = GetIdleTimerTimeout();
			}
		}
	}

	public static BitmapImage GetProductBitmapImage(IKioskProduct kioskProduct)
	{
		BitmapImage result = null;
		if (kioskProduct != null)
		{
			switch (kioskProduct.KioskProductType)
			{
			case KioskProductType.None:
				result = Constants.Images.MovieMissingBoxArt;
				break;
			case KioskProductType.Title:
			case KioskProductType.MDV:
			case KioskProductType.TitleRollup:
				if (kioskProduct is ITitleProduct titleProduct)
				{
					if (DataServiceRegisterBitmapImage(titleProduct.ImageFile, titleProduct.ImageFile))
					{
						result = ServiceLocator.Instance.GetService<IBitmapCacheService>().GetBitmapImage(titleProduct.ImageFile);
					}
					else if (titleProduct.TitleFamily == TitleFamily.Movies)
					{
						result = Constants.Images.MovieMissingBoxArt;
					}
					else if (titleProduct.TitleFamily == TitleFamily.Games)
					{
						result = Constants.Images.GameMissingBoxArt;
					}
				}
				break;
			case KioskProductType.Collection:
				if (kioskProduct is ICollectionProduct)
				{
					result = Constants.Images.CollectionsCarousel;
				}
				break;
			default:
				result = Constants.Images.MovieMissingBoxArt;
				break;
			}
		}
		return result;
	}

	public static BitmapImage GetSubPlatformBitmapImage(ITitleProduct kioskProduct)
	{
		BitmapImage result = null;
		long? num = kioskProduct?.SubPlatformId;
		if (num.HasValue && num > 0)
		{
			result = Constants.SubPlatformImages.GetSubPlatformImage((Constants.SubPlatforms)num.Value);
		}
		return result;
	}

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

	public static bool DataServiceRegisterBitmapImage(string name, string cacheName)
	{
		IBitmapCacheService service = ServiceLocator.Instance.GetService<IBitmapCacheService>();
		if (service.GetBitmapImage(name) != null)
		{
			return true;
		}
		byte[] image = ServiceLocator.Instance.GetService<IDataService>().GetImage(cacheName);
		if (image == null)
		{
			LogHelper.Instance.Log("OrchestrationService.DataServiceRegisterBitmapImage was not able to read any data for name: {0} cacheName {1}", name, cacheName);
			return false;
		}
		return service.RegisterBitmapImage(name, image) != null;
	}

	public static void RegisterPricingService(Action pricingSetChangeCallback)
	{
		IPricingService service = ServiceLocator.Instance.GetService<IPricingService>();
		if (service == null)
		{
			return;
		}
		service.ClearOnPricingSetChange();
		service.OnPricingSetChange += delegate
		{
			LogHelper.Instance.Log(">  Price change detected; force web broker to re-register");
			if (pricingSetChangeCallback != null)
			{
				pricingSetChangeCallback();
			}
		};
	}

	public static void ReloadPricingService()
	{
		ServiceLocator.Instance.GetService<IPricingService>()?.Reload();
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

	public static bool RentalShowCollectionsPopup()
	{
		IMarketingConfigurationManager service = ServiceLocator.Instance.GetService<IMarketingConfigurationManager>();
		IResourceBundleService service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service == null || service2 == null)
		{
			return false;
		}
		return service.ShowCollectionsPopUp;
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

	public static bool GetCanSellUsedGames()
	{
		return ServiceLocator.Instance.GetService<IStoreManager>()?.CanSellUsedGames ?? false;
	}

	public static bool GetCanSellUsedMovies()
	{
		return ServiceLocator.Instance.GetService<IStoreManager>()?.CanSellUsedMovies ?? false;
	}

	public static object GetResourceProperty(string resourceName, string propertyName)
	{
		return ServiceLocator.Instance.GetService<IResourceBundleService>()?.GetResource(resourceName)?[propertyName];
	}

	public static List<Dictionary<string, object>> GetResourceTable(string name)
	{
		XmlNode xml = ServiceLocator.Instance.GetService<IResourceBundleService>().GetXml(name);
		if (xml == null)
		{
			return null;
		}
		XmlNodeList xmlNodeList = xml.SelectNodes("row");
		if (xmlNodeList == null)
		{
			return null;
		}
		List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();
		foreach (XmlNode item in xmlNodeList)
		{
			XmlNodeList xmlNodeList2 = item.SelectNodes("column");
			if (xmlNodeList2 == null)
			{
				continue;
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			foreach (XmlNode item2 in xmlNodeList2)
			{
				string attributeValue = item2.GetAttributeValue<string>("name");
				object value = item2.GetNodeValue(string.Empty);
				string attributeValue2 = item2.GetAttributeValue<string>("type");
				if (!string.IsNullOrEmpty(attributeValue2))
				{
					Type type = attributeValue2.ToType();
					if (type != null)
					{
						value = ConversionHelper.ChangeType(value, type);
					}
				}
				dictionary[attributeValue] = value;
			}
			list.Add(dictionary);
		}
		return list;
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
