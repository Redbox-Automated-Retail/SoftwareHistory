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

	private bool _loyaltySignupEnabled;

	private bool _loyaltySignupAfterSwipeEnabled;

	private TimeSpan _loyaltySignupStartTime;

	private TimeSpan _loyaltySignupEndTime;

	private TimeSpan _loyaltySignupAfterSwipeStartTime;

	private TimeSpan _loyaltySignupAfterSwipeEndTime;

	private List<TitleType> _sendReturnImageTitleTypes;

	private List<long> _sendReturnImageProductIds;

	private string _welcomeScreenABTestVersion;

	private string _startViewAdABTestVersion;

	private string _startViewRentAndBuyABTestVersion;

	private string _priceFiltersOnBrowseABTestVersion;

	private string _oosTitleDetailsABTestVersion;

	private bool? _cachedIsDefaultCulture;

	private TimeSpan _thinJobStartTime;

	private TimeSpan _thinJobEndTime;

	public static IConfiguration Instance => Singleton<Configuration>.Instance;

	public Dictionary<TitleType, bool> EnabledGameTypes
	{
		get
		{
			if (_enabledGameTypes == null)
			{
				IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
				_enabledGameTypes = new Dictionary<TitleType, bool>
				{
					{
						TitleType.Xbox360,
						GetGameConfig(service, "GameEnableXbox360")
					},
					{
						TitleType.XboxOne,
						GetGameConfig(service, "GameEnableXboxOne")
					},
					{
						TitleType.PS3,
						GetGameConfig(service, "GameEnablePS3")
					},
					{
						TitleType.PS4,
						GetGameConfig(service, "GameEnablePS4")
					},
					{
						TitleType.Wii,
						GetGameConfig(service, "GameEnableWii")
					},
					{
						TitleType.WiiU,
						GetGameConfig(service, "GameEnableWiiU")
					},
					{
						TitleType.NintendoSwitch,
						GetGameConfig(service, "GameEnableNintendoSwitch")
					}
				};
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
				_gameHasContentTypes = new Dictionary<TitleType, bool>
				{
					{
						TitleType.Xbox360,
						GetMachineSetting(service, "GameHasContentXbox360")
					},
					{
						TitleType.XboxOne,
						GetMachineSetting(service, "GameHasContentXboxOne")
					},
					{
						TitleType.PS3,
						GetMachineSetting(service, "GameHasContentPS3")
					},
					{
						TitleType.PS4,
						GetMachineSetting(service, "GameHasContentPS4")
					},
					{
						TitleType.Wii,
						GetMachineSetting(service, "GameHasContentWii")
					},
					{
						TitleType.WiiU,
						GetMachineSetting(service, "GameHasContentWiiU")
					},
					{
						TitleType.NintendoSwitch,
						GetMachineSetting(service, "GameHasContentNintendoSwitch")
					}
				};
			}
			return _gameHasContentTypes;
		}
	}

	public bool UseTimerToInvokeCallbackEntries { get; private set; }

	public bool GiftCardsEnabled { get; private set; }

	public bool ShowAuthChangeMessage { get; private set; }

	public bool AnimatedDiscReturnEnabled { get; private set; }

	public int BurnInTimeout { get; private set; }

	public int BurnInViewDuration { get; private set; }

	public bool EarlyIdEnabled { get; private set; }

	public bool ComingSoonOptInEnabled { get; private set; }

	public int TextToSpeechVolume { get; private set; }

	public bool MarketingOptInEnabled { get; private set; }

	public bool RecommendationOnPickup { get; private set; }

	public bool ShowOptinForNormalCheckout { get; private set; }

	public bool LoyaltyEnabled { get; private set; }

	public int SignInTimeout { get; private set; }

	public int SignInADATimeout { get; private set; }

	public bool EnablePricingEngine { get; private set; }

	public bool UseLowestPriceSortOrder { get; private set; }

	public bool ServiceFeeEnabled { get; private set; }

	public bool BlurayUpsellEnabled { get; private set; }

	public bool _4KUpsellEnabled { get; private set; }

	public bool EndGameRentals { get; private set; }

	public bool EndGames { get; private set; }

	public int OfflineItemsLimit { get; private set; }

	public int OfflineGameRentalLimit { get; private set; }

	public int OfflineGamePurchaseLimit { get; private set; }

	public bool OfflineBlockListEnabled { get; private set; }

	public int UploadDiscImageMessagePriority { get; private set; }

	public int KioskAnalyticsMessagePriority { get; private set; }

	public bool EnableVistarAds { get; private set; }

	public int VistarGetAdTimeout { get; private set; }

	public bool EnableFlashDeals { get; private set; }

	public List<decimal> FlashDealPrices { get; private set; }

	public bool ShowBrowse10Titles { get; private set; }

	public bool Show99CentsBrowseFilterButton { get; private set; }

	public bool EnableBrowseMessagePopup { get; private set; }

	public int BrowseMessagePopupDuration { get; private set; }

	public bool EnableSonySweepstakes { get; private set; }

	public bool EnableRedboxSweepstakes { get; private set; }

	public bool EnableAvodLeadGenerationOfferWithoutBoxArt { get; private set; }

	public bool EnableAvodLeadGenerationOfferWithBoxArt { get; private set; }

	public bool EnableRedboxPlusLeadGenerationOffer { get; private set; }

	public bool DisplayUpdatedPerksPoints { get; private set; }

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

	public bool EnableIRSecurity { get; private set; }

	public bool EnableImageSecurity { get; private set; }

	public bool MonitorBarcodeTracking { get; private set; }

	public int BarcodeTrackingMaxBatchSize { get; private set; }

	public int BarcodeTrackingTimerDuration { get; private set; }

	public int BarcodeTrackingMessagePriority { get; private set; }

	public string FraudServicesUrl { get; private set; }

	public string FraudServicesKey { get; private set; }

	public int KeepHalImagesForNumberOfDays { get; private set; }

	public bool AwsBarcodeTrackingEnabled { get; private set; }

	public bool AwsUploadDiscImageEnabled { get; private set; }

	public string AwsFraudServicesUrl { get; private set; }

	public string AwsFraudServicesKey { get; private set; }

	public string DeviceServiceClientUrl { get; private set; }

	public bool ChipEnabled { get; private set; }

	public bool ContactlessEnabled { get; private set; }

	public int CardReadTechnicalFallbackCountLimit { get; private set; }

	public int DeviceServicePleaseWaitDuration { get; private set; }

	public bool CardReadTechnicalFallbackEnabled { get; private set; }

	public bool ContactlessVisaEnabled { get; private set; }

	public bool ContactlessAmexEnabled { get; private set; }

	public bool ContactlessMasterCardEnabled { get; private set; }

	public bool ContactlessDiscoverEnabled { get; private set; }

	public bool VisaInsertChipEnabled { get; private set; }

	public bool AmexInsertChipEnabled { get; private set; }

	public bool MasterCardInsertChipEnabled { get; private set; }

	public bool DiscoverInsertChipEnabled { get; private set; }

	public bool HideMobilePayOption { get; private set; }

	public bool HideApplePayImage { get; private set; }

	public bool HideGooglePayImage { get; private set; }

	public bool PerksPassEnabled { get; private set; }

	public string WelcomeScreenABTestVersion => GetCultureBasedValue(_welcomeScreenABTestVersion, "A");

	public string WelcomeScreenTimingABTestVersion { get; private set; }

	public string StartViewAdABTestVersion => GetCultureBasedValue(_startViewAdABTestVersion, "A");

	public string StartViewRentAndBuyABTestVersion => GetCultureBasedValue(_startViewRentAndBuyABTestVersion, "A");

	public string PriceFiltersOnBrowseABTestVersion => GetCultureBasedValue(_priceFiltersOnBrowseABTestVersion, "A");

	public int ScreenSaverTimeout { get; private set; }

	public string OutOfStockTitleDetailsABTestVersion => GetCultureBasedValue(_oosTitleDetailsABTestVersion, "A");

	public bool StartViewDarkModeEnabled { get; private set; }

	public string StartViewDarkModeStartTime { get; private set; }

	public string StartViewDarkModeEndTime { get; private set; }

	public bool ShowAfterSwipePersonalizedOffers { get; private set; }

	public string KioskClientServiceBaseUrl { get; private set; }

	public string UpdateClientServiceBaseUrl { get; private set; }

	public string KioskEngineApiServiceBaseUrl { get; private set; }

	public bool UseLegacyBrokerService { get; private set; }

	public bool UseAWSBrokerService { get; private set; }

	public bool AuthorizeAtPickup { get; private set; }

	public string BluefinServiceUrl { get; private set; }

	public string BluefinServiceApiKey { get; private set; }

	public int BluefinServiceTimeout { get; private set; }

	public bool BluefinAutoActivate { get; private set; }

	public bool ShowKMTInCartOffers { get; private set; }

	public bool UseDiscountsForPmnpOffers { get; private set; }

	public bool EnableStarzOffer { get; private set; }

	public bool EnableRedboxPlusOffer { get; private set; }

	public bool EnableRedboxPlusRedemption { get; private set; }

	public bool ShowKioskClosingVSM { get; private set; }

	public bool ReturnVisitPromoOfferEnabled { get; private set; }

	public string ReturnVisitPromoOfferPromotionName { get; private set; }

	public decimal ReturnVisitPromoOfferPromotionPrice { get; private set; }

	public string ReturnVisitPromoOfferPromotionExpirationDate { get; private set; }

	public bool CallReturnVisitPromoCheck { get; private set; }

	public int ReturnVisitPromoCheckTimeout { get; private set; }

	public int StartViewAdTimeout { get; private set; }

	public string KioskDataServicesUrl { get; private set; }

	public string KioskDataServicesKey { get; private set; }

	public bool SendJsonKioskAnalyticsToKioskDataService { get; private set; }

	public bool SendKioskAlertsToKioskDataService { get; private set; }

	public bool SendAuditEventsToKioskDataService { get; private set; }

	public bool SendKioskFunctionChecksToKioskDataService { get; private set; }

	public bool SendKioskVersionsToKioskDataService { get; private set; }

	public bool SendKioskStatisticsToKioskDataService { get; private set; }

	public bool SendKioskPropertiesToKioskDataService { get; private set; }

	public bool SendKioskEventsToKioskDataService { get; private set; }

	public bool ForcedAirExchangerConfigured { get; private set; }

	public string FAQDocument { get; private set; }

	public string TermsDocument { get; private set; }

	public string RedboxCardDocument { get; private set; }

	public string PrivacyDocument { get; private set; }

	public string PerksDocument { get; private set; }

	public string MobileFaqDocument { get; private set; }

	public string CAPrivacyNoticeDocument { get; private set; }

	public string DoNotSellDocument { get; private set; }

	public string SubscriptionsDocument { get; private set; }

	public string RedboxPlusDocument { get; private set; }

	public bool ZeroTouchEnabled { get; private set; }

	public bool ZeroTouchPickupEnabled { get; private set; }

	public bool ZeroTouchReturnEnabled { get; private set; }

	public bool ZeroTouchContactlessEnabled { get; private set; }

	public int ZeroTouchReadTimeout { get; private set; }

	public bool ZeroTouchVasEnabled { get; private set; }

	public bool ZeroTouchGoogleTwoTap { get; private set; }

	public bool UseMerchOrdersForThinning { get; private set; }

	public int KioskClientServiceGetMerchandizingOrdersTimeout { get; private set; }

	public TimeSpan ThinJobRangeStart => _thinJobStartTime;

	public TimeSpan ThinJobRangeEnd => _thinJobEndTime;

	public int MerchOrderExpirationDays { get; private set; }

	public int AuthorizeTimeout { get; private set; }

	public bool SendAuthorizeToKioskClientService { get; private set; }

	public bool SendReturnToKioskClientService { get; set; }

	public bool SendAuthenticationApisToKioskClientService { get; private set; }

	public bool SendInstallerApisToKioskClientService { get; private set; }

	public bool SendGiftCardAuthorizeToKioskClientService { get; private set; }

	public bool SendPickupAuthorizeToKioskClientSerivce { get; private set; }

	public bool SendReservationAuthorizeToKioskClientService { get; private set; }

	public bool SendMarketingApisToKioskClientService { get; private set; }

	public bool SendExpireAndCancelReservationToKioskClientService { get; private set; }

	public bool SendReconcileToKioskClientService { get; private set; }

	public bool SendInventorySnapshotToKioskClientService { get; private set; }

	public int KioskClientServiceInventorySnapshotTimeout { get; private set; }

	public bool SendReservationPickupToKioskClientService { get; private set; }

	public bool SendCustomerOffersToKioskClientService { get; private set; }

	public bool SendDiscActionToKioskClientService { get; private set; }

	public int KioskClientServiceDiscActionTimeout { get; private set; }

	public bool SendDiscFraudToKioskClientService { get; private set; }

	public int KioskClientServiceDiscFraudTimeout { get; private set; }

	public bool SendEstimateAccrualToKioskClientService { get; private set; }

	public int KioskClientServiceEstimateAccrualTimeout { get; private set; }

	public bool SendEstimateRedemptionToKioskClientService { get; private set; }

	public int KioskClientServiceEstimateRedemptionTimeout { get; private set; }

	public bool SendPromoCodeValidationToKioskClientService { get; private set; }

	public int KioskClientServicePromoCodeValidationTimeout { get; private set; }

	public bool SendUpdateAccountToKioskClientService { get; private set; }

	public int KioskClientServiceUpdateAccountTimeout { get; private set; }

	public bool SendPickupValidationToKioskClientService { get; private set; }

	public bool ButtonAnimationEnabled { get; private set; }

	public int LoginLockoutMinutes { get; private set; }

	public bool UseKCSPricing { get; private set; }

	public bool UseKCSInCart { get; private set; }

	private string GetCultureBasedValue(string defaultCultureValue, string alternateCultureValue)
	{
		if (!_cachedIsDefaultCulture.HasValue)
		{
			_cachedIsDefaultCulture = OrchestrationService.GetCulture() == OrchestrationService.GetDefaultCulture();
		}
		if (_cachedIsDefaultCulture.HasValue && !_cachedIsDefaultCulture.Value)
		{
			return alternateCultureValue;
		}
		return defaultCultureValue;
	}

	public void ResetCachedIsDefaultCulture()
	{
		_cachedIsDefaultCulture = null;
	}

	private Configuration()
	{
		Initialize();
	}

	private void Initialize()
	{
		ResetCachedIsDefaultCulture();
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		BurnInTimeout = ConfigGetValue(service, "system", "General", "BurnInTimeout", 1800000);
		BurnInViewDuration = ConfigGetValue(service, "system", "General", "BurnInViewDuration", 30000);
		SignInTimeout = ConfigGetValue(service, "system", "Personalization", "SignInTimeout", 20000);
		SignInADATimeout = ConfigGetValue(service, "system", "Personalization", "SignInADATimeout", 90000);
		EnablePricingEngine = ConfigGetValue(service, "system", "General", "EnablePricingEngine", defaultValue: true);
		DeviceServiceClientUrl = ConfigGetValue(service, "system", "DeviceService", "DeviceServiceClientUrl", "http://localhost:9000/CardReaderEvents");
		KioskClientServiceBaseUrl = ConfigGetValue(service, "system", "KioskClientService", "KioskClientServiceBaseUrl", "http://localhost:9001");
		UpdateClientServiceBaseUrl = ConfigGetValue(service, "system", "UpdateClientService", "UpdateClientServiceBaseUrl", "http://localhost:6001");
		KioskEngineApiServiceBaseUrl = ConfigGetValue(service, "system", "KioskEngineApiService", "KioskEngineApiServiceBaseUrl", "http://localhost:9002");
		SessionReload();
	}

	public void SessionReload()
	{
		ResetCachedIsDefaultCulture();
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		AnimatedDiscReturnEnabled = ConfigGetValue(service, "system", "General", "AnimatedDiscReturnEnabled", defaultValue: false);
		ComingSoonOptInEnabled = ConfigGetValue(service, "system", "General", "ComingSoonOptIn", defaultValue: true);
		MarketingOptInEnabled = ConfigGetValue(service, "system", "General", "MarketingOptInEnabled", defaultValue: true);
		RecommendationOnPickup = ConfigGetValue(service, "system", "General", "RecommendationOnPickup", defaultValue: true);
		ShowOptinForNormalCheckout = ConfigGetValue(service, "system", "General", "ShowOptinForNormalCheckout", defaultValue: true);
		ServiceFeeEnabled = ConfigGetValue(service, "system", "General", "ServiceFeeEnabled", defaultValue: false);
		BlurayUpsellEnabled = ConfigGetValue(service, "system", "General", "BluRayUpsellEnabled", defaultValue: false);
		_4KUpsellEnabled = ConfigGetValue(service, "system", "General", "4KUpsellEnabled", defaultValue: false);
		OfflineItemsLimit = ConfigGetValue(service, "system", "General", "OfflineItemsLimit", 3);
		OfflineGameRentalLimit = ConfigGetValue(service, "system", "General", "OfflineGameRentalLimit", 1);
		OfflineGamePurchaseLimit = ConfigGetValue(service, "system", "General", "OfflineGamePurchaseLimit", 2);
		OfflineBlockListEnabled = ConfigGetValue(service, "system", "General", "OfflineBlockListEnabled", defaultValue: true);
		UploadDiscImageMessagePriority = ConfigGetValue(service, "system", "QueueService", "UploadDiscImageMessagePriority", 60);
		KioskAnalyticsMessagePriority = ConfigGetValue(service, "system", "QueueService", "KioskAnalyticsMessagePriority", 0);
		UseTimerToInvokeCallbackEntries = ConfigGetValue(service, "system", "General", "UseTimerToInvokeCallbackEntries", defaultValue: true);
		GiftCardsEnabled = ConfigGetValue(service, "system", "General", "GiftCardsEnabled", defaultValue: true);
		ShowAuthChangeMessage = false;
		TextToSpeechVolume = ConfigGetValue(service, "system", "General", "TextToSpeechVolume", 100);
		ButtonAnimationEnabled = ConfigGetValue(service, "system", "General", "ButtonAnimationEnabled", defaultValue: true);
		UseLowestPriceSortOrder = ConfigGetValue(service, "system", "General", "UserLowestPriceSortOrder", defaultValue: false);
		EndGameRentals = ConfigGetValue(service, "marketing", "MarketingGeneral", "EndGameRentals", defaultValue: true);
		EndGames = ConfigGetValue(service, "marketing", "MarketingGeneral", "EndGames", defaultValue: true);
		EnableVistarAds = ConfigGetValue(service, "marketing", "MarketingGeneral", "EnableVistarAds", defaultValue: false);
		VistarGetAdTimeout = ConfigGetValue(service, "marketing", "MarketingGeneral", "VistarGetAdTimeout", 500);
		EnableFlashDeals = ConfigGetValue(service, "marketing", "MarketingGeneral", "EnableFlashDeals", defaultValue: false);
		FlashDealPrices = ParseFlashDealPrices(ConfigGetValue<string>(service, "marketing", "MarketingGeneral", "FlashDealPrices", null));
		ShowBrowse10Titles = ConfigGetValue(service, "marketing", "MarketingGeneral", "ShowBrowse10Titles", defaultValue: false);
		Show99CentsBrowseFilterButton = ConfigGetValue(service, "marketing", "MarketingGeneral", "Show99CentsBrowseFilterButton", defaultValue: false);
		ShowAfterSwipePersonalizedOffers = ConfigGetValue(service, "marketing", "MarketingGeneral", "ShowAfterSwipePersonalizedOffers", defaultValue: false);
		ShowKMTInCartOffers = ConfigGetValue(service, "marketing", "MarketingGeneral", "ShowKMTInCartOffers", defaultValue: true);
		UseDiscountsForPmnpOffers = ConfigGetValue(service, "marketing", "MarketingGeneral", "UseDiscountsForPmnpOffers", defaultValue: false);
		EnableBrowseMessagePopup = ConfigGetValue(service, "marketing", "MarketingGeneral", "EnableBrowseMessagePopup", defaultValue: false);
		BrowseMessagePopupDuration = ConfigGetValue(service, "marketing", "MarketingGeneral", "BrowseMessagePopupDuration", 15000);
		EnableSonySweepstakes = ConfigGetValue(service, "marketing", "MarketingGeneral", "EnableSonySweepstakes", defaultValue: false);
		EnableRedboxSweepstakes = ConfigGetValue(service, "marketing", "MarketingGeneral", "EnableRedboxSweepstakes", defaultValue: false);
		EnableAvodLeadGenerationOfferWithoutBoxArt = ConfigGetValue(service, "marketing", "MarketingGeneral", "EnableAvodLeadGenerationOfferWithoutBoxArt", defaultValue: false);
		EnableAvodLeadGenerationOfferWithBoxArt = ConfigGetValue(service, "marketing", "MarketingGeneral", "EnableAvodLeadGenerationOfferWithBoxArt", defaultValue: false);
		EnableRedboxPlusLeadGenerationOffer = ConfigGetValue(service, "marketing", "MarketingGeneral", "EnableRedboxPlusLeadGenerationOffer", defaultValue: false);
		EnableStarzOffer = ConfigGetValue(service, "marketing", "MarketingGeneral", "EnableStarzOffer", defaultValue: false);
		EnableRedboxPlusOffer = ConfigGetValue(service, "marketing", "MarketingGeneral", "EnableRedboxPlusOffer", defaultValue: false);
		EnableRedboxPlusRedemption = ConfigGetValue(service, "marketing", "MarketingGeneral", "EnableRedboxPlusRedemption", defaultValue: false);
		ShowKioskClosingVSM = ConfigGetValue(service, "marketing", "MarketingGeneral", "ShowKioskClosingVSM", defaultValue: false);
		DisplayUpdatedPerksPoints = ConfigGetValue(service, "marketing", "MarketingGeneral", "DisplayUpdatedPerksPoints", defaultValue: false);
		ReturnVisitPromoOfferEnabled = ConfigGetValue(service, "marketing", "MarketingReturn", "ReturnVisitPromoOfferEnabled", defaultValue: false);
		ReturnVisitPromoOfferPromotionName = ConfigGetValue<string>(service, "marketing", "MarketingReturn", "ReturnVisitPromoOfferPromotionName", null);
		ReturnVisitPromoOfferPromotionPrice = ConfigGetValue(service, "marketing", "MarketingReturn", "ReturnVisitPromoOfferPromotionPrice", 0m);
		ReturnVisitPromoOfferPromotionExpirationDate = ConfigGetValue<string>(service, "marketing", "MarketingReturn", "ReturnVisitPromoOfferPromotionExpirationDate", null);
		CallReturnVisitPromoCheck = ConfigGetValue(service, "marketing", "MarketingReturn", "CallReturnVisitPromoCheck", defaultValue: false);
		ReturnVisitPromoCheckTimeout = ConfigGetValue(service, "marketing", "MarketingReturn", "ReturnVisitPromoCheckTimeout", 10000);
		StartViewAdTimeout = ConfigGetValue(service, "marketing", "MarketingAd", "StartViewAdTimeout", 120000);
		EarlyIdEnabled = ConfigGetValue(service, "system", "Personalization", "EarlyIDEnabled", defaultValue: false);
		LoyaltyEnabled = ConfigGetValue(service, "system", "Personalization", "LoyaltyEnabled", defaultValue: false);
		PerksPassEnabled = ConfigGetValue(service, "system", "Personalization", "PerksPassEnabled", defaultValue: false);
		_loyaltySignupEnabled = ConfigGetValue(service, "system", "Personalization", "LoyaltySignUpEnabled", defaultValue: false);
		_loyaltySignupAfterSwipeEnabled = ConfigGetValue(service, "system", "Personalization", "LoyaltySignUpAfterSwipeEnabled", defaultValue: false);
		string timeSpan = ConfigGetValue<string>(service, "system", "Personalization", "SignUpStartEndTime", null);
		ParseTimeRange(timeSpan, out _loyaltySignupStartTime, out _loyaltySignupEndTime, "SignUpStartEndTime");
		string timeSpan2 = ConfigGetValue<string>(service, "system", "Personalization", "SignUpAfterSwipeStartEndTime", null);
		ParseTimeRange(timeSpan2, out _loyaltySignupAfterSwipeStartTime, out _loyaltySignupAfterSwipeEndTime, "SignUpAfterSwipeStartEndTime");
		FraudServicesUrl = ConfigGetValue<string>(service, "system", "Fraud", "FraudServicesUrl", null);
		FraudServicesKey = ConfigGetValue<string>(service, "system", "Fraud", "FraudServicesKey", null);
		BarcodeTrackingMaxBatchSize = ConfigGetValue(service, "system", "Fraud", "BarcodeTrackingMaxBatchSize", 10);
		BarcodeTrackingTimerDuration = ConfigGetValue(service, "system", "Fraud", "BarcodeTrackingTimerDuration", 3600000);
		BarcodeTrackingMessagePriority = ConfigGetValue(service, "system", "Fraud", "BarcodeTrackingMessagePriority", 110);
		MonitorBarcodeTracking = ConfigGetValue(service, "system", "Fraud", "MonitorBarcodeTracking", defaultValue: false);
		EnableIRSecurity = ConfigGetValue(service, "system", "Fraud", "EnableIRSecurity", defaultValue: false);
		EnableImageSecurity = ConfigGetValue(service, "system", "Fraud", "EnableImageSecurity", defaultValue: false);
		string sendImageTypes = ConfigGetValue(service, "system", "Fraud", "SendReturnImageTypes", TitleFamily.Games.ToString());
		ParseImageTypes(sendImageTypes);
		KeepHalImagesForNumberOfDays = ConfigGetValue(service, "system", "Fraud", "KeepHalImagesForNumberOfDays", 30);
		AwsBarcodeTrackingEnabled = ConfigGetValue(service, "system", "Fraud", "AwsBarcodeTrackingEnabled", defaultValue: false);
		AwsUploadDiscImageEnabled = ConfigGetValue(service, "system", "Fraud", "AwsUploadDiscImageEnabled", defaultValue: false);
		AwsFraudServicesUrl = ConfigGetValue<string>(service, "system", "Fraud", "AwsFraudServicesUrl", null);
		AwsFraudServicesKey = ConfigGetValue<string>(service, "system", "Fraud", "AwsFraudServicesKey", null);
		_welcomeScreenABTestVersion = GetABTestValue(service, "WelcomeScreen", "AB Testing");
		WelcomeScreenTimingABTestVersion = ConfigGetValue(service, "marketing", "AB Testing", "WelcomeScreenTiming", "D");
		_startViewAdABTestVersion = GetABTestValue(service, "StartViewAd", "AB Testing");
		_startViewRentAndBuyABTestVersion = GetABTestValue(service, "StartViewRentAndBuy", "AB Testing");
		_priceFiltersOnBrowseABTestVersion = GetABTestValue(service, "PriceFiltersOnBrowse", "MarketingGeneral");
		_oosTitleDetailsABTestVersion = GetABTestValue(service, "OutOfStockTitleDetails", "AB Testing");
		ScreenSaverTimeout = ConfigGetValue(service, "marketing", "AB Test Parameters", "ScreenSaverTimer", 25000);
		AuthorizeAtPickup = ConfigGetValue(service, "system", "General", "AuthorizeAtPickup", defaultValue: false);
		StartViewDarkModeEnabled = ConfigGetValue(service, "system", "General", "StartViewDarkModeEnabled", defaultValue: false);
		StartViewDarkModeStartTime = ConfigGetValue(service, "system", "General", "StartViewDarkModeStartTime", "19:0:0");
		StartViewDarkModeEndTime = ConfigGetValue(service, "system", "General", "StartViewDarkModeEndTime", "7:0:0");
		ChipEnabled = ConfigGetValue(service, "system", "DeviceService", "ChipEnabled", defaultValue: false);
		ContactlessEnabled = ConfigGetValue(service, "system", "DeviceService", "ContactlessEnabled", defaultValue: false);
		CardReadTechnicalFallbackEnabled = ConfigGetValue(service, "system", "DeviceService", "CardReadTechnicalFallbackEnabled", defaultValue: true);
		CardReadTechnicalFallbackCountLimit = ConfigGetValue(service, "system", "DeviceService", "CardReadTechnicalFallbackCountLimit", 3);
		DeviceServicePleaseWaitDuration = ConfigGetValue(service, "system", "DeviceService", "DeviceServicePleaseWaitDuration", 90000);
		ContactlessVisaEnabled = ConfigGetValue(service, "system", "DeviceService", "ContactlessVisaEnabled", defaultValue: true);
		ContactlessAmexEnabled = ConfigGetValue(service, "system", "DeviceService", "ContactlessAmexEnabled", defaultValue: true);
		ContactlessMasterCardEnabled = ConfigGetValue(service, "system", "DeviceService", "ContactlessMasterCardEnabled", defaultValue: true);
		ContactlessDiscoverEnabled = ConfigGetValue(service, "system", "DeviceService", "ContactlessDiscoverEnabled", defaultValue: true);
		VisaInsertChipEnabled = ConfigGetValue(service, "system", "DeviceService", "VisaInsertChipEnabled", defaultValue: true);
		AmexInsertChipEnabled = ConfigGetValue(service, "system", "DeviceService", "AmexInsertChipEnabled", defaultValue: true);
		MasterCardInsertChipEnabled = ConfigGetValue(service, "system", "DeviceService", "MasterCardInsertChipEnabled", defaultValue: true);
		DiscoverInsertChipEnabled = ConfigGetValue(service, "system", "DeviceService", "DiscoverInsertChipEnabled", defaultValue: true);
		HideMobilePayOption = ConfigGetValue(service, "system", "DeviceService", "HideMobilePayOption", defaultValue: false);
		HideApplePayImage = ConfigGetValue(service, "system", "DeviceService", "HideApplePayImage", defaultValue: false);
		HideGooglePayImage = ConfigGetValue(service, "system", "DeviceService", "HideGooglePayImage", defaultValue: false);
		BluefinAutoActivate = ConfigGetValue(service, "system", "DeviceService", "BluefinAutoActivate", defaultValue: false);
		BluefinServiceUrl = ConfigGetValue<string>(service, "system", "DeviceService", "BluefinServiceUrl", null);
		BluefinServiceApiKey = ConfigGetValue<string>(service, "system", "DeviceService", "BluefinServiceApiKey", null);
		BluefinServiceTimeout = ConfigGetValue(service, "system", "DeviceService", "BluefinServiceTimeout", 5000);
		KioskDataServicesUrl = ConfigGetValue<string>(service, "system", "General", "KioskDataServicesUrl", null);
		KioskDataServicesKey = ConfigGetValue<string>(service, "system", "General", "KioskDataServicesKey", null);
		SendJsonKioskAnalyticsToKioskDataService = ConfigGetValue(service, "system", "General", "SendJsonKioskAnalyticsToKioskDataService", defaultValue: true);
		SendKioskAlertsToKioskDataService = ConfigGetValue(service, "system", "General", "SendKioskAlertsToKioskDataService", defaultValue: true);
		SendAuditEventsToKioskDataService = ConfigGetValue(service, "system", "General", "SendAuditEventsToKioskDataService", defaultValue: true);
		SendKioskFunctionChecksToKioskDataService = ConfigGetValue(service, "system", "General", "SendKioskFunctionChecksToKioskDataService", defaultValue: true);
		SendKioskVersionsToKioskDataService = ConfigGetValue(service, "system", "General", "SendKioskVersionsToKioskDataService", defaultValue: true);
		SendKioskStatisticsToKioskDataService = ConfigGetValue(service, "system", "General", "SendKioskStatisticsToKioskDataService", defaultValue: true);
		SendKioskPropertiesToKioskDataService = ConfigGetValue(service, "system", "General", "SendKioskPropertiesToKioskDataService", defaultValue: true);
		SendKioskEventsToKioskDataService = ConfigGetValue(service, "system", "General", "SendKioskEventsToKioskDataService", defaultValue: true);
		ForcedAirExchangerConfigured = ConfigGetValue(service, "system", "General", "ForcedAirExchangerConfigured", defaultValue: true);
		FAQDocument = ConfigGetValue(service, "system", "Documents", "FAQDocument", "document-faqs");
		TermsDocument = ConfigGetValue(service, "system", "Documents", "TermsDocument", "document-terms");
		RedboxCardDocument = ConfigGetValue(service, "system", "Documents", "RedboxCardDocument", "document-terms-redbox-card");
		PrivacyDocument = ConfigGetValue(service, "system", "Documents", "PrivacyDocument", "document-privacy-policy");
		PerksDocument = ConfigGetValue(service, "system", "Documents", "PerksDocument", "document-perks");
		MobileFaqDocument = ConfigGetValue(service, "system", "Documents", "MobileFAQDocument", "document-mobile-faqs");
		CAPrivacyNoticeDocument = ConfigGetValue(service, "system", "Documents", "CAPrivacyNoticeDocument", string.Empty);
		DoNotSellDocument = ConfigGetValue(service, "system", "Documents", "DoNotSellDocument", string.Empty);
		SubscriptionsDocument = ConfigGetValue(service, "system", "Documents", "SubscriptionsDocument", string.Empty);
		RedboxPlusDocument = ConfigGetValue(service, "system", "Documents", "RedboxPlusDocument", string.Empty);
		UseMerchOrdersForThinning = ConfigGetValue(service, "system", "General", "UseMerchOrdersForThinning", defaultValue: false);
		KioskClientServiceGetMerchandizingOrdersTimeout = ConfigGetValue(service, "system", "General", "KioskClientServiceGetMerchandizingOrdersTimeout", 60000);
		string timeSpan3 = ConfigGetValue(service, "system", "General", "ThinJobTimeRange", "03:00:00-04:30:00");
		ParseTimeRange(timeSpan3, out _thinJobStartTime, out _thinJobEndTime, "ThinJobTimeRange");
		MerchOrderExpirationDays = ConfigGetValue(service, "system", "General", "MerchOrderExpirationDays", 7);
		ZeroTouchReadTimeout = ConfigGetValue(service, "system", "ZeroTouch", "ZeroTouchReadTimeout", 600000);
		ZeroTouchPickupEnabled = ConfigGetValue(service, "system", "ZeroTouch", "ZeroTouchPickupEnabled", defaultValue: true);
		ZeroTouchReturnEnabled = ConfigGetValue(service, "system", "ZeroTouch", "ZeroTouchReturnEnabled", defaultValue: true);
		ZeroTouchVasEnabled = ConfigGetValue(service, "system", "ZeroTouch", "ZeroTouchVasEnabled", defaultValue: false);
		ZeroTouchContactlessEnabled = ConfigGetValue(service, "system", "ZeroTouch", "ZeroTouchContactlessEnabled", defaultValue: true) && ContactlessEnabled;
		ZeroTouchEnabled = ConfigGetValue(service, "system", "ZeroTouch", "ZeroTouchEnabled", defaultValue: false) && (ZeroTouchPickupEnabled || ZeroTouchReturnEnabled);
		ZeroTouchGoogleTwoTap = ConfigGetValue(service, "system", "ZeroTouch", "ZeroTouchGoogleTwoTap", defaultValue: true);
		AuthorizeTimeout = ConfigGetValue(service, "system", "General", "AuthorizeTimeout", 60000);
		SendAuthorizeToKioskClientService = ConfigGetValue(service, "kioskengine", "Jarvis", "SendAuthorizeToKioskClientService", defaultValue: true);
		SendGiftCardAuthorizeToKioskClientService = ConfigGetValue(service, "kioskengine", "Jarvis", "SendGiftCardAuthorizeToKioskClientService", defaultValue: true);
		SendPickupAuthorizeToKioskClientSerivce = ConfigGetValue(service, "kioskengine", "Jarvis", "SendPickupAuthorizeToKioskClientService", defaultValue: true);
		SendReservationAuthorizeToKioskClientService = ConfigGetValue(service, "kioskengine", "Jarvis", "SendReservationAuthorizeToKioskClientService", defaultValue: true);
		SendReturnToKioskClientService = ConfigGetValue(service, "kioskengine", "Jarvis", "SendReturnToKioskClientService", defaultValue: true);
		SendAuthenticationApisToKioskClientService = ConfigGetValue(service, "kioskengine", "Jarvis", "SendAuthenticationApisToKioskClientService", defaultValue: true);
		SendInstallerApisToKioskClientService = ConfigGetValue(service, "kioskengine", "Jarvis", "SendInstallerApisToKioskClientService", defaultValue: true);
		SendMarketingApisToKioskClientService = ConfigGetValue(service, "kioskengine", "Jarvis", "SendMarketingApisToKioskClientService", defaultValue: true);
		SendExpireAndCancelReservationToKioskClientService = ConfigGetValue(service, "kioskengine", "Jarvis", "SendExpireAndCancelReservationToKioskClientService", defaultValue: true);
		SendReconcileToKioskClientService = ConfigGetValue(service, "kioskengine", "Jarvis", "SendReconcileToKioskClientService", defaultValue: true);
		SendInventorySnapshotToKioskClientService = ConfigGetValue(service, "kioskengine", "Jarvis", "SendInventorySnapshotToKioskClientService", defaultValue: true);
		KioskClientServiceInventorySnapshotTimeout = ConfigGetValue(service, "kioskengine", "Jarvis", "KioskClientServiceInventorySnapshotTimeout", 60000);
		UseLegacyBrokerService = ConfigGetValue(service, "kioskengine", "Jarvis", "UseLegacyBrokerService", defaultValue: true);
		UseAWSBrokerService = ConfigGetValue(service, "kioskengine", "Jarvis", "UseAWSBrokerService", defaultValue: true);
		SendReservationPickupToKioskClientService = ConfigGetValue(service, "kioskengine", "Jarvis", "SendReservationPickupToKioskClientService", defaultValue: true);
		SendCustomerOffersToKioskClientService = ConfigGetValue(service, "kioskengine", "Jarvis", "SendCustomerOffersToKioskClientService", defaultValue: true);
		SendDiscActionToKioskClientService = ConfigGetValue(service, "kioskengine", "Jarvis", "SendDiscActionToKioskClientService", defaultValue: true);
		KioskClientServiceDiscActionTimeout = ConfigGetValue(service, "kioskengine", "Jarvis", "KioskClientServiceDiscActionTimeout", 5000);
		SendPickupValidationToKioskClientService = ConfigGetValue(service, "kioskengine", "Jarvis", "SendPickupValidationToKioskClientService", defaultValue: true);
		SendDiscFraudToKioskClientService = ConfigGetValue(service, "kioskengine", "Jarvis", "SendDiscFraudToKioskClientService", defaultValue: true);
		KioskClientServiceDiscFraudTimeout = ConfigGetValue(service, "kioskengine", "Jarvis", "KioskClientServiceDiscFraudTimeout", 5000);
		SendEstimateAccrualToKioskClientService = ConfigGetValue(service, "kioskengine", "Jarvis", "SendEstimateAccrualToKioskClientService", defaultValue: true);
		KioskClientServiceEstimateAccrualTimeout = ConfigGetValue(service, "kioskengine", "Jarvis", "KioskClientServiceEstimateAccrualTimeout", 5000);
		SendEstimateRedemptionToKioskClientService = ConfigGetValue(service, "kioskengine", "Jarvis", "SendEstimateRedemptionToKioskClientService", defaultValue: true);
		KioskClientServiceEstimateRedemptionTimeout = ConfigGetValue(service, "kioskengine", "Jarvis", "KioskClientServiceEstimateRedemptionTimeout", 5000);
		UseKCSPricing = ConfigGetValue(service, "kioskengine", "Jarvis", "UseKCSPricing", defaultValue: true);
		UseKCSInCart = ConfigGetValue(service, "kioskengine", "Jarvis", "UseKCSInCart", defaultValue: false);
		SendPromoCodeValidationToKioskClientService = ConfigGetValue(service, "kioskengine", "Jarvis", "SendPromoCodeValidationToKioskClientService", defaultValue: true);
		KioskClientServicePromoCodeValidationTimeout = ConfigGetValue(service, "kioskengine", "Jarvis", "KioskClientServicePromoCodeValidationTimeout", 5000);
		SendUpdateAccountToKioskClientService = ConfigGetValue(service, "kioskengine", "Jarvis", "SendUpdateAccountToKioskClientService", defaultValue: true);
		KioskClientServiceUpdateAccountTimeout = ConfigGetValue(service, "kioskengine", "Jarvis", "KioskClientServiceUpdateAccountTimeout", 3000);
		LoginLockoutMinutes = ConfigGetValue(service, "kioskengine", "FMACP", "LoginLockoutMinutes", 5);
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
		string text = ConfigGetValue(configService, "marketing", categoryName, testName, "A");
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
