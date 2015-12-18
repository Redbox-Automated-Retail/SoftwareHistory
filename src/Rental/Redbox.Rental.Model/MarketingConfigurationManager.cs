using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Model;

public class MarketingConfigurationManager : IMarketingConfigurationManager
{
	private IConfigurationService _configService;

	public bool ShowGameBanner
	{
		get
		{
			if (_configService.TryGetValue<bool>(ConfigFile.Marketing, ConfigCategory.Banner, ConfigSetting.ShowGameBanner, out var value))
			{
				return value;
			}
			return false;
		}
	}

	public bool ShowBlurayBrowse
	{
		get
		{
			if (_configService.TryGetValue<bool>(ConfigFile.Marketing, ConfigCategory.Banner, ConfigSetting.ShowBlurayBrowse, out var value))
			{
				return value;
			}
			return false;
		}
	}

	public bool ShowCollections
	{
		get
		{
			if (_configService.TryGetValue<bool>(ConfigFile.Marketing, ConfigCategory.Collections, ConfigSetting.ShowCollections, out var value))
			{
				return value;
			}
			return false;
		}
	}

	public bool ShowCollectionsBanner
	{
		get
		{
			if (!ShowCollections)
			{
				return false;
			}
			if (_configService.TryGetValue<bool>(ConfigFile.Marketing, ConfigCategory.Collections, ConfigSetting.ShowCollectionsBanner, out var value))
			{
				return value;
			}
			return false;
		}
	}

	public bool ShowCollectionsPopUp
	{
		get
		{
			if (!ShowCollections)
			{
				return false;
			}
			if (_configService.TryGetValue<bool>(ConfigFile.Marketing, ConfigCategory.Collections, ConfigSetting.ShowCollectionsPopUp, out var value))
			{
				return value;
			}
			return false;
		}
	}

	public bool CollectionsPopUpOnComingSoon
	{
		get
		{
			if (!ShowCollectionsPopUp)
			{
				return false;
			}
			if (_configService.TryGetValue<bool>(ConfigFile.Marketing, ConfigCategory.Collections, ConfigSetting.CollectionsPopUpOnComingSoon, out var value))
			{
				return value;
			}
			return false;
		}
	}

	public int CollectionsPopUpTimer
	{
		get
		{
			if (!ShowCollections)
			{
				return 0;
			}
			if (_configService.TryGetValue<int>(ConfigFile.Marketing, ConfigCategory.Collections, ConfigSetting.CollectionsPopUpTimer, out var value))
			{
				return value;
			}
			return 0;
		}
	}

	public int CollectionsPopUpPaddleClicks
	{
		get
		{
			if (!ShowCollections)
			{
				return 0;
			}
			if (_configService.TryGetValue<int>(ConfigFile.Marketing, ConfigCategory.Collections, ConfigSetting.CollectionsPopUpPaddleClicks, out var value))
			{
				return value;
			}
			return 0;
		}
	}

	public MarketingConfigurationManager(IConfigurationService configService)
	{
		_configService = configService;
	}
}
