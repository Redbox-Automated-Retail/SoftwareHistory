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

	int SignInTimeout { get; }

	int SignInADATimeout { get; }

	bool EnablePricingEngine { get; }

	bool UseLowestPriceSortOrder { get; }

	bool ComingSoonOptInEnabled { get; }

	bool MarketingOptInEnabled { get; }

	bool ServiceFeeEnabled { get; }

	bool BlurayUpsellEnabled { get; }

	bool _4KUpsellEnabled { get; }

	bool EndGameRentals { get; }

	bool EndGames { get; }

	bool EnableFlashDeals { get; }

	List<decimal> FlashDealPrices { get; }

	bool Show99CentsBrowseFilterButton { get; }

	bool ShowExpandedFormatButtons { get; }

	bool ShowExpandedFormatButtonsNewText { get; }

	int OfflineItemsLimit { get; }

	int OfflineGameRentalLimit { get; }

	int OfflineGamePurchaseLimit { get; }

	bool LoyaltySignUpAfterSwipeEnabled { get; }

	bool OfflineBlockListEnabled { get; }

	bool EnableIRSecurity { get; }

	bool EnableImageSecurity { get; }

	int UploadDiscImageMessagePriority { get; }

	int KioskAnalyticsMessagePriority { get; }

	string WelcomeScreenABTestVersion { get; }

	string WelcomeScreenTimingABTestVersion { get; }

	string StartViewAdABTestVersion { get; }

	string StartViewRentAndBuyABTestVersion { get; }

	string PriceFiltersOnBrowseABTestVersion { get; }

	int ScreenSaverTimeout { get; }

	string OutOfStockTitleDetailsABTestVersion { get; }

	string TitleDetailsLayoutABTestVersion { get; }

	string TitleDetailsLayoutABTestVersionIgnoreCulture { get; }

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

	bool ShowAuthChangeMessage { get; }

	bool ShowKMTInCartOffers { get; }

	bool ShowAfterSwipePersonalizedOffers { get; }

	bool UseDiscountsForPmnpOffers { get; }

	bool ShowOptinForNormalCheckout { get; }

	string KioskDataServicesUrl { get; }

	string KioskDataServicesKey { get; }

	bool SendJsonKioskAnalyticsToKioskDataService { get; }

	bool SendKioskAlertsToKioskServices { get; }

	bool SendKioskAlertsToKioskDataService { get; }

	bool SendKioskVersionsToKioskDataService { get; }

	bool SendKioskStatisticsToKioskDataService { get; }

	bool SendKioskPropertiesToKioskDataService { get; }

	bool EnableVistarAds { get; }

	int VistarGetAdTimeout { get; }

	string KioskClientServiceBaseUrl { get; }

	bool ForcedAirExchangerConfigured { get; }

	bool SendKioskFunctionChecksToKioskServices { get; }

	bool SendKioskFunctionChecksToKioskDataService { get; }

	bool SendKioskEventsToKioskServices { get; }

	bool SendKioskEventsToKioskDataService { get; }

	string FAQDocument { get; }

	string TermsDocument { get; }

	string RedboxCardDocument { get; }

	string PrivacyDocument { get; }

	string PerksDocument { get; }

	string MobileFaqDocument { get; }

	string CAPrivacyNoticeDocument { get; }

	string DoNotSellDocument { get; }

	bool ShowRedboxCardUpdated { get; }

	bool ShowTermsUpdated { get; }

	bool ShowMobileFaqUpdated { get; }

	bool ShowFAQUpdated { get; }

	bool ShowPrivacyUpdated { get; }

	bool ShowPerksUpdated { get; }

	bool ShowCAPrivacyNoticeUpdated { get; }

	bool ShowDoNotSellUpdated { get; }

	bool ZeroTouchEnabled { get; }

	bool ZeroTouchPickupEnabled { get; }

	bool ZeroTouchReturnEnabled { get; }

	bool ZeroTouchContactlessEnabled { get; }

	int ZeroTouchReadTimeout { get; }

	bool SendAuditEventsToKioskServices { get; }

	bool SendAuditEventsToKioskDataService { get; }

	bool UseMerchOrdersForThinning { get; }

	int KioskClientServiceGetMerchandizingOrdersTimeout { get; }

	TimeSpan ThinJobRangeStart { get; }

	TimeSpan ThinJobRangeEnd { get; }

	void SessionReload();

	void ResetCachedIsDefaultCulture();
}
