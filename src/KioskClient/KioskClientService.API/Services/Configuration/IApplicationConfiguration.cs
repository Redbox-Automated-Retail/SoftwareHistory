using System.Collections.Generic;
using KioskClientService.API.Ads;
using KioskClientService.API.Services.Analytics.KioskDataService;
using KioskClientService.API.Services.Analytics.Vistar;
using KioskClientService.API.Services.Inventory.API;
using KioskClientService.API.Services.Personalization;
using KioskClientService.API.Services.Products.ProductReelsApi;
using KioskClientService.API.Services.ProxyApi;
using KioskClientService.API.Services.Subscription;
using KioskClientService.API.Services.Transaction.API;

namespace KioskClientService.API.Services.Configuration;

public interface IApplicationConfiguration
{
	bool AdsActive { get; }

	List<AdLocation> AdsToPreload { get; }

	Dictionary<string, List<AdLocation>> AdsToPreloadByView { get; }

	bool UseAdServer { get; }

	string VistarUrl { get; }

	int VistarTimeout { get; }

	string VistarNetworkId { get; }

	string VistarAPIKey { get; }

	int MoreLikeThisDataRefreshFrequency { get; }

	bool TestDataEnabled { get; }

	AdServerConfigs AdServer { get; }

	KioskDataServiceConfigs KioskDataService { get; }

	ProductReelsApiConfigs ProductReelsApi { get; }

	InventoryApiConfigs InventoryApi { get; }

	int ProductGroupSortOrderType { get; }

	ProxyApiConfigs ProxyApi { get; }

	SubscriptionConfigs SubscriptionConfigs { get; }

	TransactionApiConfigs TransactionApi { get; }

	PersonalizationConfigs PersonalizationService { get; }

	bool PricingActive { get; }

	bool PlanogramSyncEnabled { get; }
}
