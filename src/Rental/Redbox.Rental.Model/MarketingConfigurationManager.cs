using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Model;

public class MarketingConfigurationManager : IMarketingConfigurationManager
{
	private IConfigurationService _configService;

	public bool ShowGameBanner
	{
		get
		{
			if (_configService.TryGetValue<bool>("marketing", "Banner", "ShowGameBanner", out var value))
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
			if (_configService.TryGetValue<bool>("marketing", "Banner", "ShowBlurayBrowse", out var value))
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
			if (_configService.TryGetValue<bool>("marketing", "Collections", "ShowCollections", out var value))
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
			if (_configService.TryGetValue<bool>("marketing", "Collections", "ShowCollectionsBanner", out var value))
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
			if (_configService.TryGetValue<bool>("marketing", "Collections", "ShowCollectionsPopUp", out var value))
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
			if (_configService.TryGetValue<int>("marketing", "Collections", "CollectionsPopUpTimer", out var value))
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
			if (_configService.TryGetValue<int>("marketing", "Collections", "CollectionsPopUpPaddleClicks", out var value))
			{
				return value;
			}
			return 0;
		}
	}

	public bool EnableMarketingAds
	{
		get
		{
			if (_configService.TryGetValue<bool>("marketing", "MarketingGeneral", "EnableMarketingAds", out var value))
			{
				return value;
			}
			return false;
		}
	}

	public MarketingConfigurationManager(IConfigurationService configService)
	{
		_configService = configService;
	}
}
