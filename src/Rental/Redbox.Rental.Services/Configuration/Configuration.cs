using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Services.Configuration;

public class Configuration : IConfiguration
{
	private Dictionary<TitleType, bool> _enabledGameTypes;

	private Dictionary<TitleType, bool> _gameHasContentTypes;

	private int _burnInTimeout;

	private int _burnInViewDuration;

	private bool _comingSoonOptInEnabled;

	private bool _earlyIdEnabled;

	private bool _loyaltyEnabled;

	private int _signInTimeout;

	private int _signInADATimeout;

	private bool _enablePricingEngine;

	private bool _useLowestPriceSortOrder;

	private bool _serviceFeeEnabled;

	private bool __4KUpsellEnabled;

	private bool _blurayUpsellEnabled;

	private int _offlineItemsLimit;

	private int _offlineGameRentalLimit;

	private int _offlineGamePurchaseLimit;

	private bool _offlineBlockListEnabled;

	private bool _useTimerToInvokeCallbackEntries;

	private bool _giftCardsEnabled;

	private bool _showAuthChangeMessage;

	private bool _enableFlashDeals;

	private List<decimal> _flashDealPrices;

	private bool _show99CentsBrowseFilterButton;

	private bool _loyaltySignupEnabled;

	private bool _loyaltySignupAfterSwipeEnabled;

	private TimeSpan _loyaltySignupStartTime;

	private TimeSpan _loyaltySignupEndTime;

	private TimeSpan _loyaltySignupAfterSwipeStartTime;

	private TimeSpan _loyaltySignupAfterSwipeEndTime;

	private int _uploadDiscImageMessagePriority;

	private int _kioskAnalyticsMessagePriority;

	private string _fraudServicesUrl;

	private string _fraudServicesKey;

	private bool _enableIRSecurity;

	private bool _enableImageSecurity;

	private bool _monitorBarcodeTracking;

	private int _barcodeTrackingMaxBatchSize;

	private int _barcodeTrackingTimerDuration;

	private int _barcodeTrackingMessagePriority;

	private List<TitleType> _sendReturnImageTitleTypes;

	private List<long> _sendReturnImageProductIds;

	private string _welcomeScreenABTestVersion;

	private string _welcomeScreenTimingABTestVersion;

	private string _startViewAdABTestVersion;

	private string _priceFiltersOnBrowseABTestVersion;

	private string _oosTitleDetailsABTestVersion;

	private int _screenSaverTimeout;

	private string _starRatingTitleDetailsABTestVersion;

	private string _tvEnabledABTestVersion;

	private List<long> _tvTitleIds;

	private int _keepHalImagesForNumberOfDays;

	private bool _awsBarcodeTrackingEnabled;

	private bool _awsUploadDiscImageEnabled;

	private string _awsFraudServicesUrl;

	private string _awsFraudServicesKey;

	private bool _authorizeAtPickup;

	private bool _kioskAnalyticsEnabled;

	private bool _sendKioskAnalyticsToKioskDataService;

	private bool _kioskAnalyticsJsonEnabled;

	private string _kioskDataServicesUrl;

	private string _kioskDataServicesKey;

	private string _deviceServiceClientUrl;

	private bool _chipEnabled;

	private bool _contactlessEnabled;

	private int _cardReadTechnicalFallbackCountLimit;

	private bool _cardReadTechnicalFallbackEnabled;

	private int _deviceServicePleaseWaitDuration;

	private bool _contactlessVisaEnabled;

	private bool _contactlessAmexEnabled;

	private bool _contactlessMasterCardEnabled;

	private bool _contactlessDiscoverEnabled;

	private bool _visaInsertChipEnabled;

	private bool _amexInsertChipEnabled;

	private bool _masterCardInsertChipEnabled;

	private bool _discoverInsertChipEnabled;

	private string _bluefinServiceUrl;

	private string _bluefinServiceApiKey;

	private int _bluefinServiceTimeout;

	private bool _bluefinAutoActivate;

	private bool _afterSwipePersonalizedOffers;

	private bool _showKmtInCartOffers;

	public static IConfiguration Instance => Singleton<Configuration>.Instance;

	public Dictionary<TitleType, bool> EnabledGameTypes
	{
		get
		{
			if (_enabledGameTypes == null)
			{
				IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
				_enabledGameTypes = new Dictionary<TitleType, bool>();
				_enabledGameTypes.Add(TitleType.Xbox360, GetGameConfig(service, ConfigSetting.GameEnableXbox360));
				_enabledGameTypes.Add(TitleType.XboxOne, GetGameConfig(service, ConfigSetting.GameEnableXboxOne));
				_enabledGameTypes.Add(TitleType.PS3, GetGameConfig(service, ConfigSetting.GameEnablePS3));
				_enabledGameTypes.Add(TitleType.PS4, GetGameConfig(service, ConfigSetting.GameEnablePS4));
				_enabledGameTypes.Add(TitleType.Wii, GetGameConfig(service, ConfigSetting.GameEnableWii));
				_enabledGameTypes.Add(TitleType.WiiU, GetGameConfig(service, ConfigSetting.GameEnableWiiU));
				_enabledGameTypes.Add(TitleType.NintendoSwitch, GetGameConfig(service, ConfigSetting.GameEnableNintendoSwitch));
			}
			return _enabledGameTypes;
		}
	}

	public Dictionary<TitleType, bool> GameHasContentTypes
	{
		get
		{
			if (_gameHasContentTypes == null)
			{
				IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
				_gameHasContentTypes = new Dictionary<TitleType, bool>();
				_gameHasContentTypes.Add(TitleType.Xbox360, GetMachineSetting(service, MachineSettings.GameHasContentXbox360));
				_gameHasContentTypes.Add(TitleType.XboxOne, GetMachineSetting(service, MachineSettings.GameHasContentXboxOne));
				_gameHasContentTypes.Add(TitleType.PS3, GetMachineSetting(service, MachineSettings.GameHasContentPS3));
				_gameHasContentTypes.Add(TitleType.PS4, GetMachineSetting(service, MachineSettings.GameHasContentPS4));
				_gameHasContentTypes.Add(TitleType.Wii, GetMachineSetting(service, MachineSettings.GameHasContentWii));
				_gameHasContentTypes.Add(TitleType.WiiU, GetMachineSetting(service, MachineSettings.GameHasContentWiiU));
				_gameHasContentTypes.Add(TitleType.NintendoSwitch, GetMachineSetting(service, MachineSettings.GameHasContentNintendoSwitch));
			}
			return _gameHasContentTypes;
		}
	}

	public bool UseTimerToInvokeCallbackEntries => _useTimerToInvokeCallbackEntries;

	public bool GiftCardsEnabled => _giftCardsEnabled;

	public bool ShowAuthChangeMessage => _showAuthChangeMessage;

	public int BurnInTimeout => _burnInTimeout;

	public int BurnInViewDuration => _burnInViewDuration;

	public bool EarlyIdEnabled => _earlyIdEnabled;

	public bool ComingSoonOptInEnabled => _comingSoonOptInEnabled;

	public bool LoyaltyEnabled => _loyaltyEnabled;

	public int SignInTimeout => _signInTimeout;

	public int SignInADATimeout => _signInADATimeout;

	public bool EnablePricingEngine => _enablePricingEngine;

	public bool UseLowestPriceSortOrder => _useLowestPriceSortOrder;

	public bool ServiceFeeEnabled => _serviceFeeEnabled;

	public bool BlurayUpsellEnabled => _blurayUpsellEnabled;

	public bool _4KUpsellEnabled => __4KUpsellEnabled;

	public int OfflineItemsLimit => _offlineItemsLimit;

	public int OfflineGameRentalLimit => _offlineGameRentalLimit;

	public int OfflineGamePurchaseLimit => _offlineGamePurchaseLimit;

	public bool OfflineBlockListEnabled => _offlineBlockListEnabled;

	public int UploadDiscImageMessagePriority => _uploadDiscImageMessagePriority;

	public int KioskAnalyticsMessagePriority => _kioskAnalyticsMessagePriority;

	public bool EnableFlashDeals => _enableFlashDeals;

	public List<decimal> FlashDealPrices => _flashDealPrices;

	public bool Show99CentsBrowseFilterButton => _show99CentsBrowseFilterButton;

	public bool LoyaltySignUpEnabled
	{
		get
		{
			if (_loyaltySignupEnabled)
			{
				return IsCurrentTimeInRange(_loyaltySignupStartTime, _loyaltySignupEndTime);
			}
			return false;
		}
	}

	public bool LoyaltySignUpAfterSwipeEnabled
	{
		get
		{
			if (_loyaltySignupAfterSwipeEnabled)
			{
				return IsCurrentTimeInRange(_loyaltySignupAfterSwipeStartTime, _loyaltySignupAfterSwipeEndTime);
			}
			return false;
		}
	}

	public bool EnableIRSecurity => _enableIRSecurity;

	public bool EnableImageSecurity => _enableImageSecurity;

	public bool MonitorBarcodeTracking => _monitorBarcodeTracking;

	public int BarcodeTrackingMaxBatchSize => _barcodeTrackingMaxBatchSize;

	public int BarcodeTrackingTimerDuration => _barcodeTrackingTimerDuration;

	public int BarcodeTrackingMessagePriority => _barcodeTrackingMessagePriority;

	public string FraudServicesUrl => _fraudServicesUrl;

	public string FraudServicesKey => _fraudServicesKey;

	public int KeepHalImagesForNumberOfDays => _keepHalImagesForNumberOfDays;

	public bool AwsBarcodeTrackingEnabled => _awsBarcodeTrackingEnabled;

	public bool AwsUploadDiscImageEnabled => _awsUploadDiscImageEnabled;

	public string AwsFraudServicesUrl => _awsFraudServicesUrl;

	public string AwsFraudServicesKey => _awsFraudServicesKey;

	public string DeviceServiceClientUrl => _deviceServiceClientUrl;

	public bool ChipEnabled => _chipEnabled;

	public bool ContactlessEnabled => _contactlessEnabled;

	public int CardReadTechnicalFallbackCountLimit => _cardReadTechnicalFallbackCountLimit;

	public int DeviceServicePleaseWaitDuration => _deviceServicePleaseWaitDuration;

	public bool CardReadTechnicalFallbackEnabled => _cardReadTechnicalFallbackEnabled;

	public bool ContactlessVisaEnabled => _contactlessVisaEnabled;

	public bool ContactlessAmexEnabled => _contactlessAmexEnabled;

	public bool ContactlessMasterCardEnabled => _contactlessMasterCardEnabled;

	public bool ContactlessDiscoverEnabled => _contactlessDiscoverEnabled;

	public bool VisaInsertChipEnabled => _visaInsertChipEnabled;

	public bool AmexInsertChipEnabled => _amexInsertChipEnabled;

	public bool MasterCardInsertChipEnabled => _masterCardInsertChipEnabled;

	public bool DiscoverInsertChipEnabled => _discoverInsertChipEnabled;

	public string WelcomeScreenABTestVersion => DefaultIfAlternateCulture(_welcomeScreenABTestVersion, "A");

	public string WelcomeScreenTimingABTestVersion => DefaultIfAlternateCulture(_welcomeScreenTimingABTestVersion, "A");

	public string StartViewAdABTestVersion => DefaultIfAlternateCulture(_startViewAdABTestVersion, "A");

	public string PriceFiltersOnBrowseABTestVersion => DefaultIfAlternateCulture(_priceFiltersOnBrowseABTestVersion, "A");

	public int ScreenSaverTimeout => _screenSaverTimeout;

	public string OutOfStockTitleDetailsABTestVersion => DefaultIfAlternateCulture(_oosTitleDetailsABTestVersion, "A");

	public string StarRatingTitleDetailsABTestVersion => DefaultIfAlternateCulture(_starRatingTitleDetailsABTestVersion, "A");

	public string TVEnabledABTestVersion => _tvEnabledABTestVersion;

	public List<long> TVTitleIds => _tvTitleIds;

	public bool ShowAfterSwipePersonalizedOffers => _afterSwipePersonalizedOffers;

	public bool AuthorizeAtPickup => _authorizeAtPickup;

	public bool KioskAnalyticsEnabled => _kioskAnalyticsEnabled;

	public bool SendKioskAnalyticsToKioskDataService => _sendKioskAnalyticsToKioskDataService;

	public string KioskDataServicesUrl => _kioskDataServicesUrl;

	public string KioskDataServicesKey => _kioskDataServicesKey;

	public string BluefinServiceUrl => _bluefinServiceUrl;

	public string BluefinServiceApiKey => _bluefinServiceApiKey;

	public int BluefinServiceTimeout => _bluefinServiceTimeout;

	public bool BluefinAutoActivate => _bluefinAutoActivate;

	public bool KioskAnalyticsJsonEnabled => _kioskAnalyticsJsonEnabled;

	public bool ShowKMTInCartOffers => _showKmtInCartOffers;

	public bool SendReturnImageForTitle(ITitleProduct title)
	{
		if (!_sendReturnImageTitleTypes.Contains(title.TitleType))
		{
			return _sendReturnImageProductIds.Contains(title.ProductId);
		}
		return true;
	}

	private string DefaultIfAlternateCulture(string setting, string defaultValue)
	{
		if (!(OrchestrationService.GetCulture() == OrchestrationService.GetDefaultCulture()))
		{
			return defaultValue;
		}
		return setting;
	}

	private Configuration()
	{
		Initialize();
	}

	private void Initialize()
	{
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		_burnInTimeout = ConfigGetValue(service, ConfigFile.System, ConfigCategory.General, "BurnInTimeout", 1800000);
		_burnInViewDuration = ConfigGetValue(service, ConfigFile.System, ConfigCategory.General, "BurnInViewDuration", 30000);
		_signInTimeout = ConfigGetValue(service, ConfigFile.System, ConfigCategory.Personalization, "SignInTimeout", 20000);
		_signInADATimeout = ConfigGetValue(service, ConfigFile.System, ConfigCategory.Personalization, "SignInADATimeout", 90000);
		_enablePricingEngine = ConfigGetValue(service, ConfigFile.System, ConfigCategory.General, "EnablePricingEngine", defaultValue: true);
		_deviceServiceClientUrl = ConfigGetValue(service, ConfigFile.System, ConfigCategory.DeviceService, ConfigSetting.DeviceServiceClientUrl, "http://localhost:9000/CardReaderEvents");
		SessionReload();
	}

	public void SessionReload()
	{
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		_comingSoonOptInEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.General, "ComingSoonOptIn", defaultValue: true);
		_earlyIdEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.Personalization, "EarlyIDEnabled", defaultValue: false);
		_loyaltyEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.Personalization, "LoyaltyEnabled", defaultValue: false);
		_serviceFeeEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.General, "ServiceFeeEnabled", defaultValue: false);
		_blurayUpsellEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.General, "BluRayUpsellEnabled", defaultValue: false);
		__4KUpsellEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.General, "4KUpsellEnabled", defaultValue: false);
		_offlineItemsLimit = ConfigGetValue(service, ConfigFile.System, ConfigCategory.General, "OfflineItemsLimit", 3);
		_offlineGameRentalLimit = ConfigGetValue(service, ConfigFile.System, ConfigCategory.General, "OfflineGameRentalLimit", 1);
		_offlineGamePurchaseLimit = ConfigGetValue(service, ConfigFile.System, ConfigCategory.General, "OfflineGamePurchaseLimit", 2);
		_offlineBlockListEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.General, "OfflineBlockListEnabled", defaultValue: true);
		_uploadDiscImageMessagePriority = ConfigGetValue(service, ConfigFile.System, ConfigCategory.QueueService, "UploadDiscImageMessagePriority", 60);
		_kioskAnalyticsMessagePriority = ConfigGetValue(service, ConfigFile.System, ConfigCategory.QueueService, "KioskAnalyticsMessagePriority", 0);
		_useTimerToInvokeCallbackEntries = ConfigGetValue(service, ConfigFile.System, ConfigCategory.General, "UseTimerToInvokeCallbackEntries", defaultValue: true);
		_giftCardsEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.General, ConfigSetting.GiftCardsEnabled, defaultValue: true);
		_showAuthChangeMessage = ConfigGetValue(service, ConfigFile.System, ConfigCategory.General, ConfigSetting.ShowAuthChangeMessage, defaultValue: true);
		_useLowestPriceSortOrder = ConfigGetValue(service, ConfigFile.System, ConfigCategory.General, "UserLowestPriceSortOrder", defaultValue: false);
		_enableFlashDeals = ConfigGetValue(service, ConfigFile.Marketing, ConfigCategory.MarketingGeneral, ConfigSetting.EnableFlashDeals, defaultValue: false);
		_flashDealPrices = ParseFlashDealPrices(ConfigGetValue<string>(service, ConfigFile.Marketing, ConfigCategory.MarketingGeneral, ConfigSetting.FlashDealPrices, null));
		_show99CentsBrowseFilterButton = ConfigGetValue(service, ConfigFile.Marketing, ConfigCategory.MarketingGeneral, ConfigSetting.Show99CentsBrowseFilterButton, defaultValue: false);
		_afterSwipePersonalizedOffers = ConfigGetValue(service, ConfigFile.Marketing, ConfigCategory.MarketingGeneral, ConfigSetting.ShowAfterSwipePersonalizedOffers, defaultValue: false);
		_showKmtInCartOffers = ConfigGetValue(service, ConfigFile.Marketing, ConfigCategory.MarketingGeneral, ConfigSetting.ShowKMTInCartOffers, defaultValue: true);
		_loyaltySignupEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.Personalization, "LoyaltySignUpEnabled", defaultValue: false);
		_loyaltySignupAfterSwipeEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.Personalization, "LoyaltySignUpAfterSwipeEnabled", defaultValue: false);
		string timeSpan = ConfigGetValue<string>(service, ConfigFile.System, ConfigCategory.Personalization, "SignUpStartEndTime", null);
		ParseTimeRange(timeSpan, out _loyaltySignupStartTime, out _loyaltySignupEndTime, "SignUpStartEndTime");
		string timeSpan2 = ConfigGetValue<string>(service, ConfigFile.System, ConfigCategory.Personalization, "SignUpAfterSwipeStartEndTime", null);
		ParseTimeRange(timeSpan2, out _loyaltySignupAfterSwipeStartTime, out _loyaltySignupAfterSwipeEndTime, "SignUpAfterSwipeStartEndTime");
		_fraudServicesUrl = ConfigGetValue<string>(service, ConfigFile.System, ConfigCategory.Fraud, "FraudServicesUrl", null);
		_fraudServicesKey = ConfigGetValue<string>(service, ConfigFile.System, ConfigCategory.Fraud, "FraudServicesKey", null);
		_barcodeTrackingMaxBatchSize = ConfigGetValue(service, ConfigFile.System, ConfigCategory.Fraud, "BarcodeTrackingMaxBatchSize", 10);
		_barcodeTrackingTimerDuration = ConfigGetValue(service, ConfigFile.System, ConfigCategory.Fraud, "BarcodeTrackingTimerDuration", 3600000);
		_barcodeTrackingMessagePriority = ConfigGetValue(service, ConfigFile.System, ConfigCategory.Fraud, "BarcodeTrackingMessagePriority", 110);
		_monitorBarcodeTracking = ConfigGetValue(service, ConfigFile.System, ConfigCategory.Fraud, "MonitorBarcodeTracking", defaultValue: false);
		_enableIRSecurity = ConfigGetValue(service, ConfigFile.System, ConfigCategory.Fraud, "EnableIRSecurity", defaultValue: false);
		_enableImageSecurity = ConfigGetValue(service, ConfigFile.System, ConfigCategory.Fraud, "EnableImageSecurity", defaultValue: false);
		string sendImageTypes = ConfigGetValue(service, ConfigFile.System, ConfigCategory.Fraud, "SendReturnImageTypes", TitleFamily.Games.ToString());
		ParseImageTypes(sendImageTypes);
		_keepHalImagesForNumberOfDays = ConfigGetValue(service, ConfigFile.System, ConfigCategory.Fraud, "KeepHalImagesForNumberOfDays", 30);
		_awsBarcodeTrackingEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.Fraud, "AwsBarcodeTrackingEnabled", defaultValue: false);
		_awsUploadDiscImageEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.Fraud, "AwsUploadDiscImageEnabled", defaultValue: false);
		_awsFraudServicesUrl = ConfigGetValue<string>(service, ConfigFile.System, ConfigCategory.Fraud, "AwsFraudServicesUrl", null);
		_awsFraudServicesKey = ConfigGetValue<string>(service, ConfigFile.System, ConfigCategory.Fraud, "AwsFraudServicesKey", null);
		_welcomeScreenABTestVersion = GetABTestValue(service, ConfigSetting.WelcomeScreen, ConfigCategory.ABTesting);
		_welcomeScreenTimingABTestVersion = GetABTestValue(service, ConfigSetting.WelcomeScreenTiming, ConfigCategory.ABTesting);
		_startViewAdABTestVersion = GetABTestValue(service, ConfigSetting.StartViewAd, ConfigCategory.ABTesting);
		_priceFiltersOnBrowseABTestVersion = GetABTestValue(service, ConfigSetting.PriceFiltersOnBrowse, ConfigCategory.MarketingGeneral);
		_oosTitleDetailsABTestVersion = GetABTestValue(service, ConfigSetting.OutOfStockTitleDetails, ConfigCategory.ABTesting);
		_screenSaverTimeout = ConfigGetValue(service, ConfigFile.Marketing, ConfigCategory.ABTestParameters, ConfigSetting.ScreenSaverTimer, 25000);
		_starRatingTitleDetailsABTestVersion = GetABTestValue(service, ConfigSetting.StarRatingTitleDetails, ConfigCategory.ABTesting);
		_tvEnabledABTestVersion = GetABTestValue(service, ConfigSetting.TVEnabled, ConfigCategory.ABTesting);
		_tvTitleIds = ParseTVTitleIds(ConfigGetValue<string>(service, ConfigFile.Marketing, ConfigCategory.ABTestParameters, ConfigSetting.TVTitleIds, null));
		_authorizeAtPickup = ConfigGetValue(service, ConfigFile.System, ConfigCategory.General, "AuthorizeAtPickup", defaultValue: false);
		_kioskAnalyticsEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.General, "KioskAnalytics", defaultValue: false);
		_sendKioskAnalyticsToKioskDataService = ConfigGetValue(service, ConfigFile.System, ConfigCategory.General, "SendKioskAnalyticsToKioskDataService", defaultValue: false);
		_kioskAnalyticsJsonEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.General, "KioskAnalyticsJsonEnabled", defaultValue: false);
		_kioskDataServicesUrl = ConfigGetValue<string>(service, ConfigFile.System, ConfigCategory.General, "KioskDataServicesUrl", null);
		_kioskDataServicesKey = ConfigGetValue<string>(service, ConfigFile.System, ConfigCategory.General, "KioskDataServicesKey", null);
		_chipEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.DeviceService, ConfigSetting.ChipEnabled, defaultValue: false);
		_contactlessEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.DeviceService, ConfigSetting.ContactlessEnabled, defaultValue: false);
		_cardReadTechnicalFallbackEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.DeviceService, ConfigSetting.CardReadTechnicalFallbackEnabled, defaultValue: true);
		_cardReadTechnicalFallbackCountLimit = ConfigGetValue(service, ConfigFile.System, ConfigCategory.DeviceService, ConfigSetting.CardReadTechnicalFallbackCountLimit, 3);
		_deviceServicePleaseWaitDuration = ConfigGetValue(service, ConfigFile.System, ConfigCategory.DeviceService, ConfigSetting.DeviceServicePleaseWaitDuration, 90000);
		_contactlessVisaEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.DeviceService, ConfigSetting.ContactlessVisaEnabled, defaultValue: true);
		_contactlessAmexEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.DeviceService, ConfigSetting.ContactlessAmexEnabled, defaultValue: true);
		_contactlessMasterCardEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.DeviceService, ConfigSetting.ContactlessMasterCardEnabled, defaultValue: true);
		_contactlessDiscoverEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.DeviceService, ConfigSetting.ContactlessDiscoverEnabled, defaultValue: true);
		_visaInsertChipEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.DeviceService, ConfigSetting.VisaInsertChipEnabled, defaultValue: true);
		_amexInsertChipEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.DeviceService, ConfigSetting.AmexInsertChipEnabled, defaultValue: true);
		_masterCardInsertChipEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.DeviceService, ConfigSetting.MasterCardInsertChipEnabled, defaultValue: true);
		_discoverInsertChipEnabled = ConfigGetValue(service, ConfigFile.System, ConfigCategory.DeviceService, ConfigSetting.DiscoverInsertChipEnabled, defaultValue: true);
		_bluefinAutoActivate = ConfigGetValue(service, ConfigFile.System, ConfigCategory.DeviceService, ConfigSetting.BluefinAutoActivate, defaultValue: false);
		_bluefinServiceUrl = ConfigGetValue<string>(service, ConfigFile.System, ConfigCategory.DeviceService, ConfigSetting.BluefinServiceUrl, null);
		_bluefinServiceApiKey = ConfigGetValue<string>(service, ConfigFile.System, ConfigCategory.DeviceService, ConfigSetting.BluefinServiceApiKey, null);
		_bluefinServiceTimeout = ConfigGetValue(service, ConfigFile.System, ConfigCategory.DeviceService, ConfigSetting.BluefinServiceTimeout, 5000);
	}

	private bool GetMachineSetting(IMachineSettingsStore machineSettingStore, string key)
	{
		bool result = false;
		if (machineSettingStore != null)
		{
			bool.TryParse(machineSettingStore.GetValue("Store", key, "false"), out result);
		}
		return result;
	}

	private bool GetGameConfig(IConfigurationService configService, string settingName)
	{
		bool value = false;
		if (configService != null && !configService.TryGetValue<bool>("system", "Games", settingName, out value))
		{
			value = false;
		}
		return value;
	}

	private static T ConfigGetValue<T>(IConfigurationService configService, string configFile, string categoryName, string settingName, T defaultValue)
	{
		T value = defaultValue;
		if (configService != null && !configService.TryGetValue<T>(configFile, categoryName, settingName, out value))
		{
			value = defaultValue;
		}
		return value;
	}

	private string GetABTestValue(IConfigurationService configService, string testName, string categoryName)
	{
		string text = ConfigGetValue(configService, ConfigFile.Marketing, categoryName, testName, "A");
		if (string.IsNullOrEmpty(text) || text == "OFF")
		{
			text = "A";
		}
		return text;
	}

	private void ParseTimeRange(string timeSpan, out TimeSpan startTime, out TimeSpan endTime, string configSetting)
	{
		startTime = TimeSpan.MinValue;
		endTime = TimeSpan.MinValue;
		if (string.IsNullOrEmpty(timeSpan))
		{
			return;
		}
		try
		{
			string[] array = timeSpan.Split('-');
			if (array.Count() == 2)
			{
				startTime = DateTime.ParseExact(array[0], "H:m:s", DateTimeFormatInfo.InvariantInfo).TimeOfDay;
				endTime = DateTime.ParseExact(array[1], "H:m:s", DateTimeFormatInfo.InvariantInfo).TimeOfDay;
			}
			else
			{
				LogHelper.Instance.Log($"Invalid time span format used for {configSetting}. Value read was {timeSpan}");
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Unable to parse time span for {configSetting}. Value read was {timeSpan}", e);
			startTime = TimeSpan.MinValue;
			endTime = TimeSpan.MinValue;
		}
	}

	private static bool IsCurrentTimeInRange(TimeSpan startTime, TimeSpan endTime)
	{
		TimeSpan timeOfDay = DateTime.Now.TimeOfDay;
		if (startTime < endTime)
		{
			if (timeOfDay >= startTime)
			{
				return timeOfDay < endTime;
			}
			return false;
		}
		if (!(timeOfDay >= startTime))
		{
			return timeOfDay < endTime;
		}
		return true;
	}

	private void ParseImageTypes(string sendImageTypes)
	{
		_sendReturnImageTitleTypes = new List<TitleType>();
		_sendReturnImageProductIds = new List<long>();
		if (string.IsNullOrEmpty(sendImageTypes))
		{
			return;
		}
		try
		{
			string[] array = sendImageTypes.Split(',');
			foreach (string text in array)
			{
				List<TitleType> imageTypes = new List<TitleType>();
				long result = 0L;
				if (TryParseImageTitleTypes(text, out imageTypes))
				{
					_sendReturnImageTitleTypes.AddRange(imageTypes);
				}
				else if (long.TryParse(text, out result))
				{
					_sendReturnImageProductIds.Add(result);
				}
				else
				{
					LogHelper.Instance.Log(string.Format("Invalid value specified in {0}: '{1}'", "SendReturnImageTypes", text));
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log(string.Format("Unable to parse {0}. Value read was {1}", "SendReturnImageTypes", sendImageTypes), e);
		}
	}

	private bool TryParseImageTitleTypes(string typeString, out List<TitleType> imageTypes)
	{
		imageTypes = new List<TitleType>();
		if (Enum.TryParse<TitleType>(typeString, out var result) && Enum.IsDefined(typeof(TitleType), result))
		{
			imageTypes.Add(result);
			return true;
		}
		if (Enum.TryParse<TitleFamily>(typeString, out var result2) && Enum.IsDefined(typeof(TitleFamily), result2))
		{
			foreach (TitleType value in Enum.GetValues(typeof(TitleType)))
			{
				if (result2 == TitleFamily.All || value.GetTitleFamily() == result2)
				{
					imageTypes.Add(value);
				}
			}
			return true;
		}
		return false;
	}

	private List<long> ParseTVTitleIds(string tvTitleIds)
	{
		List<long> list = new List<long>();
		if (!string.IsNullOrEmpty(tvTitleIds))
		{
			string[] array = tvTitleIds.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (long.TryParse(array[i], out var result))
				{
					list.Add(result);
				}
			}
		}
		return list;
	}

	private List<decimal> ParseFlashDealPrices(string flashDealPrices)
	{
		List<decimal> list = new List<decimal>();
		if (!string.IsNullOrEmpty(flashDealPrices))
		{
			string[] array = flashDealPrices.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (decimal.TryParse(array[i], out var result))
				{
					list.Add(result);
				}
			}
		}
		return list;
	}
}
