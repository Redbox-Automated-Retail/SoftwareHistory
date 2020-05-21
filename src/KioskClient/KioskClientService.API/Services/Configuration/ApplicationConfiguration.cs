using System;
using System.Collections.Generic;
using System.Linq;
using KioskClientService.API.Ads;
using KioskClientService.API.Services.Analytics.KioskDataService;
using KioskClientService.API.Services.Analytics.Vistar;
using KioskClientService.API.Services.Inventory.API;
using KioskClientService.API.Services.Products.ProductReelsApi;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Configuration;

public class ApplicationConfiguration : IApplicationConfiguration
{
	private IConfigurationService _configurationService;

	public bool AdsActive => _configurationService.GetSettingValue(ConfigFiles.KioskClient, KioskClientCategories.KioskClientAds, "AdsActive", defaultValue: false);

	public List<AdLocation> AdsToPreload
	{
		get
		{
			List<AdLocation> defaultValue = new List<AdLocation>
			{
				AdLocation.TitleDetail,
				AdLocation.Welcome
			};
			string settingValue = _configurationService.GetSettingValue<string>(ConfigFiles.KioskClient, KioskClientCategories.KioskClientAds, "AdsToPreload", null);
			if (string.IsNullOrWhiteSpace(settingValue))
			{
				return new List<AdLocation>();
			}
			return GetEnumList(settingValue, defaultValue);
		}
	}

	public Dictionary<string, List<AdLocation>> AdsToPreloadByView
	{
		get
		{
			Dictionary<string, List<AdLocation>> result = new Dictionary<string, List<AdLocation>>
			{
				{
					"zip_code_view",
					new List<AdLocation> { AdLocation.VSM }
				},
				{
					"vsm_view",
					new List<AdLocation> { AdLocation.Vending }
				}
			};
			string settingValue = _configurationService.GetSettingValue<string>(ConfigFiles.KioskClient, KioskClientCategories.KioskClientAds, "AdsToPreloadByView", null);
			if (string.IsNullOrWhiteSpace(settingValue))
			{
				return result;
			}
			try
			{
				return JsonConvert.DeserializeObject<Dictionary<string, List<AdLocation>>>(settingValue);
			}
			catch
			{
				return result;
			}
		}
	}

	public string VistarUrl => _configurationService.GetSettingValue<string>(ConfigFiles.KioskClient, KioskClientCategories.KioskClientAds, "VistarUrl", null);

	public int VistarTimeout => _configurationService.GetSettingValue(ConfigFiles.KioskClient, KioskClientCategories.KioskClientAds, "VistarTimeout", 5000);

	public string VistarNetworkId => _configurationService.GetSettingValue<string>(ConfigFiles.KioskClient, KioskClientCategories.KioskClientAds, "VistarNetworkId", null);

	public string VistarAPIKey => _configurationService.GetSettingValue<string>(ConfigFiles.KioskClient, KioskClientCategories.KioskClientAds, "VistarAPIKey", null);

	public int MoreLikeThisDataRefreshFrequency => _configurationService.GetSettingValue(ConfigFiles.KioskClient, KioskClientCategories.KioskClientDefault, "MoreLikeThisDataRefreshFrequency", 7);

	public bool TestDataEnabled => _configurationService.GetSettingValue(ConfigFiles.KioskClient, KioskClientCategories.TestData, "TestDataEnabled", defaultValue: false);

	public AdServerConfigs AdServer => new AdServerConfigs
	{
		Enabled = _configurationService.GetSettingValue(ConfigFiles.KioskClient, KioskClientCategories.KioskClientAds, "AdServerEnabled", defaultValue: true),
		Url = _configurationService.GetSettingValue<string>(ConfigFiles.KioskClient, KioskClientCategories.KioskClientAds, "AdServerUrl", null),
		Timeout = _configurationService.GetSettingValue(ConfigFiles.KioskClient, KioskClientCategories.KioskClientAds, "AdServerTimeout", 20000),
		APIKey = _configurationService.GetSettingValue<string>(ConfigFiles.KioskClient, KioskClientCategories.KioskClientAds, "AdServerAPIKey", null),
		AuthHeader = _configurationService.GetSettingValue(ConfigFiles.KioskClient, KioskClientCategories.KioskClientAds, "AdServerAuthHeader", "x-api-key")
	};

	public KioskDataServiceConfigs KioskDataService => new KioskDataServiceConfigs
	{
		Enabled = _configurationService.GetSettingValue(ConfigFiles.KioskClient, KioskClientCategories.KioskClientDefault, "KioskDataServiceEnabled", defaultValue: true),
		Url = _configurationService.GetSettingValue<string>(ConfigFiles.KioskClient, KioskClientCategories.KioskClientDefault, "KioskDataServiceUrl", null),
		Timeout = _configurationService.GetSettingValue(ConfigFiles.KioskClient, KioskClientCategories.KioskClientDefault, "KioskDataServiceTimeout", 20000),
		APIKey = _configurationService.GetSettingValue<string>(ConfigFiles.KioskClient, KioskClientCategories.KioskClientDefault, "KioskDataServiceAPIKey", null),
		AuthHeader = _configurationService.GetSettingValue(ConfigFiles.KioskClient, KioskClientCategories.KioskClientDefault, "KioskDataServiceAuthHeader", "Authorization")
	};

	public ProductReelsApiConfigs ProductReelsApi => new ProductReelsApiConfigs
	{
		Url = _configurationService.GetSettingValue<string>(ConfigFiles.KioskClient, KioskClientCategories.KioskClientDefault, "ProductReelsApiUrl", null),
		Timeout = _configurationService.GetSettingValue(ConfigFiles.KioskClient, KioskClientCategories.KioskClientDefault, "ProductReelsApiTimeout", 60000)
	};

	public InventoryApiConfigs InventoryApi => new InventoryApiConfigs
	{
		Url = _configurationService.GetSettingValue<string>(ConfigFiles.KioskClient, KioskClientCategories.KioskClientDefault, "InventoryApiUrl", null),
		Timeout = _configurationService.GetSettingValue(ConfigFiles.KioskClient, KioskClientCategories.KioskClientDefault, "InventoryApiTimeout", 60000),
		APIKey = _configurationService.GetSettingValue<string>(ConfigFiles.KioskClient, KioskClientCategories.KioskClientDefault, "InventoryApiKey", null)
	};

	public int ProductGroupSortOrderType => _configurationService.GetSettingValue(ConfigFiles.KioskClient, KioskClientCategories.KioskClientDefault, "ProductGroupSortOrderType", 0);

	public ApplicationConfiguration(IConfigurationService configurationService)
	{
		_configurationService = configurationService;
	}

	private List<T> GetEnumList<T>(string enumListString, List<T> defaultValue = null) where T : struct, Enum
	{
		try
		{
			string[] source = enumListString.Split(',');
			return (from enumString in source
				select enumString.Trim() into enumString
				where Enum.TryParse<T>(enumString, out var _)
				select enumString).Select(Enum.Parse<T>).ToList();
		}
		catch
		{
			return defaultValue ?? new List<T>();
		}
	}
}
