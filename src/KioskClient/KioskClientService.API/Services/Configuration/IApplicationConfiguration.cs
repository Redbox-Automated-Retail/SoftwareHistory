using System.Collections.Generic;
using KioskClientService.API.Ads;
using KioskClientService.API.Services.Analytics.KioskDataService;
using KioskClientService.API.Services.Analytics.Vistar;
using KioskClientService.API.Services.Products.ProductReelsApi;

namespace KioskClientService.API.Services.Configuration;

public interface IApplicationConfiguration
{
	bool AdsActive { get; }

	List<AdLocation> AdsToPreload { get; }

	Dictionary<string, List<AdLocation>> AdsToPreloadByView { get; }

	string VistarUrl { get; }

	int VistarTimeout { get; }

	string VistarNetworkId { get; }

	string VistarAPIKey { get; }

	int MoreLikeThisDataRefreshFrequency { get; }

	bool TestDataEnabled { get; }

	AdServerConfigs AdServer { get; }

	KioskDataServiceConfigs KioskDataService { get; }

	ProductReelsApiConfigs ProductReelsApi { get; }
}
