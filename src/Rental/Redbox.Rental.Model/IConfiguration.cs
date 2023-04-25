using System;
using System.Collections.Generic;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model;

public interface IConfiguration
{
	Dictionary<TitleType, bool> EnabledGameTypes { get; }

	Dictionary<TitleType, bool> GameHasContentTypes { get; }

	bool AnimatedDiscReturnEnabled { get; }

	int BurnInTimeout { get; }

	int BurnInViewDuration { get; }

	bool EarlyIdEnabled { get; }

	bool LoyaltyEnabled { get; }

	bool PerksPassEnabled { get; }

	int SignInTimeout { get; }

	int SignInADATimeout { get; }

	bool EnablePricingEngine { get; }

	bool UseLowestPriceSortOrder { get; }

	bool ComingSoonOptInEnabled { get; }

	bool MarketingOptInEnabled { get; }

	bool RecommendationOnPickup { get; }

	bool ServiceFeeEnabled { get; }

	bool BlurayUpsellEnabled { get; }

	bool _4KUpsellEnabled { get; }

	bool EndGameRentals { get; }

	bool EndGames { get; }

	bool EnableFlashDeals { get; }

	List<decimal> FlashDealPrices { get; }

	bool ShowBrowse10Titles { get; }

	bool Show99CentsBrowseFilterButton { get; }

	bool EnableBrowseMessagePopup { get; }

	int BrowseMessagePopupDuration { get; }

	bool EnableSonySweepstakes { get; }

	bool EnableRedboxSweepstakes { get; }

	bool EnableAvodLeadGenerationOfferWithoutBoxArt { get; }

	bool EnableAvodLeadGenerationOfferWithBoxArt { get; }

	bool EnableRedboxPlusLeadGenerationOffer { get; }

	bool DisplayUpdatedPerksPoints { get; }

	int OfflineItemsLimit { get; }

	int OfflineGameRentalLimit { get; }

	int OfflineGamePurchaseLimit { get; }

	bool LoyaltySignUpAfterSwipeEnabled { get; }

	bool EnableIRSecurity { get; }

	bool EnableImageSecurity { get; }

	int UploadDiscImageMessagePriority { get; }

	int KioskAnalyticsMessagePriority { get; }

	string PriceFiltersOnBrowseABTestVersion { get; }

	int ScreenSaverTimeout { get; }

	string OutOfStockTitleDetailsABTestVersion { get; }

	bool StartViewDarkModeEnabled { get; }

	string StartViewDarkModeStartTime { get; }

	string StartViewDarkModeEndTime { get; }

	bool MonitorBarcodeTracking { get; }

	int BarcodeTrackingMaxBatchSize { get; }

	int BarcodeTrackingTimerDuration { get; }

	int BarcodeTrackingMessagePriority { get; }

	int KeepHalImagesForNumberOfDays { get; }

	string FraudServicesUrl { get; }

	bool AuthorizeAtPickup { get; }

	string FraudServicesKey { get; }

	bool UseTimerToInvokeCallbackEntries { get; }

	bool AwsBarcodeTrackingEnabled { get; }

	bool AwsUploadDiscImageEnabled { get; }

	string AwsFraudServicesUrl { get; }

	string AwsFraudServicesKey { get; }

	string DeviceServiceClientUrl { get; }

	bool ChipEnabled { get; }

	bool ContactlessEnabled { get; }

	int CardReadTechnicalFallbackCountLimit { get; }

	bool CardReadTechnicalFallbackEnabled { get; }

	int DeviceServicePleaseWaitDuration { get; }

	bool ContactlessVisaEnabled { get; }

	bool ContactlessAmexEnabled { get; }

	bool ContactlessMasterCardEnabled { get; }

	bool ContactlessDiscoverEnabled { get; }

	bool HideMobilePayOption { get; }

	bool HideApplePayImage { get; }

	bool HideGooglePayImage { get; }

	bool VisaInsertChipEnabled { get; }

	bool AmexInsertChipEnabled { get; }

	bool MasterCardInsertChipEnabled { get; }

	bool DiscoverInsertChipEnabled { get; }

	string BluefinServiceUrl { get; }

	string BluefinServiceApiKey { get; }

	int BluefinServiceTimeout { get; }

	bool BluefinAutoActivate { get; }

	bool GiftCardsEnabled { get; }

	int TextToSpeechVolume { get; }

	bool ShowAfterSwipePersonalizedOffers { get; }

	bool UseDiscountsForPmnpOffers { get; }

	bool EnableStarzOffer { get; }

	bool EnableRedboxPlusOffer { get; }

	bool EnableRedboxPlusRedemption { get; }

	bool ShowKioskClosingVSM { get; }

	bool ShowOptinForNormalCheckout { get; }

	bool ReturnVisitPromoOfferEnabled { get; }

	int ReturnVisitPromoCheckTimeout { get; }

	string KioskDataServicesUrl { get; }

	string KioskDataServicesKey { get; }

	bool SendJsonKioskAnalyticsToKioskDataService { get; }

	bool EnableVistarAds { get; }

	int VistarGetAdTimeout { get; }

	string KioskClientServiceBaseUrl { get; }

	string UpdateClientServiceBaseUrl { get; }

	string KioskEngineApiServiceBaseUrl { get; }

	bool ForcedAirExchangerConfigured { get; }

	string FAQDocument { get; }

	string TermsDocument { get; }

	string RedboxCardDocument { get; }

	string PrivacyDocument { get; }

	string PerksDocument { get; }

	string MobileFaqDocument { get; }

	string CAPrivacyNoticeDocument { get; }

	string DoNotSellDocument { get; }

	string SubscriptionsDocument { get; }

	string RedboxPlusDocument { get; }

	bool ZeroTouchEnabled { get; }

	bool ZeroTouchPickupEnabled { get; }

	bool ZeroTouchReturnEnabled { get; }

	bool ZeroTouchContactlessEnabled { get; }

	int ZeroTouchReadTimeout { get; }

	bool ZeroTouchVasEnabled { get; }

	bool ZeroTouchGoogleTwoTap { get; }

	bool UseMerchOrdersForThinning { get; }

	int KioskClientServiceGetMerchandizingOrdersTimeout { get; }

	TimeSpan ThinJobRangeStart { get; }

	TimeSpan ThinJobRangeEnd { get; }

	int MerchOrderExpirationDays { get; }

	int AuthorizeTimeout { get; }

	int LoginLockoutMinutes { get; }

	int KioskClientServiceInventorySnapshotTimeout { get; }

	bool UseLegacyBrokerService { get; }

	bool UseAWSBrokerService { get; }

	int KioskClientServiceDiscActionTimeout { get; }

	int KioskClientServiceDiscFraudTimeout { get; }

	int KioskClientServiceEstimateAccrualTimeout { get; }

	int KioskClientServiceEstimateRedemptionTimeout { get; }

	int KioskClientServicePromoCodeValidationTimeout { get; }

	int KioskClientServiceUpdateAccountTimeout { get; }

	bool UseKCSPricing { get; }

	bool UseKCSInCart { get; }

	bool UseKCSStartScreen { get; }

	int KCSLoadCampaignTimerDuration { get; }

	bool ButtonAnimationEnabled { get; }

	int StartViewAdTimeout { get; }

	void SessionReload();

	void ResetCachedIsDefaultCulture();
}
