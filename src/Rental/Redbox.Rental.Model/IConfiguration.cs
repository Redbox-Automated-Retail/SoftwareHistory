using System.Collections.Generic;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model;

public interface IConfiguration
{
	Dictionary<TitleType, bool> EnabledGameTypes { get; }

	Dictionary<TitleType, bool> GameHasContentTypes { get; }

	int BurnInTimeout { get; }

	int BurnInViewDuration { get; }

	bool EarlyIdEnabled { get; }

	bool LoyaltyEnabled { get; }

	int SignInTimeout { get; }

	int SignInADATimeout { get; }

	bool EnablePricingEngine { get; }

	bool UseLowestPriceSortOrder { get; }

	bool ComingSoonOptInEnabled { get; }

	bool ServiceFeeEnabled { get; }

	bool BlurayUpsellEnabled { get; }

	bool _4KUpsellEnabled { get; }

	bool EnableFlashDeals { get; }

	int OfflineItemsLimit { get; }

	int OfflineGameRentalLimit { get; }

	int OfflineGamePurchaseLimit { get; }

	bool LoyaltySignUpEnabled { get; }

	bool LoyaltySignUpAfterSwipeEnabled { get; }

	bool OfflineBlockListEnabled { get; }

	bool EnableIRSecurity { get; }

	bool EnableImageSecurity { get; }

	int UploadDiscImageMessagePriority { get; }

	int KioskAnalyticsMessagePriority { get; }

	string WelcomeScreenABTestVersion { get; }

	string PriceFiltersOnBrowseABTestVersion { get; }

	string PrmUpsellOnMnpABTestVersion { get; }

	int ScreenSaverTimeout { get; }

	string OutOfStockTitleDetailsABTestVersion { get; }

	string StarRatingTitleDetailsABTestVersion { get; }

	string TVEnabledABTestVersion { get; }

	List<long> TVTitleIds { get; }

	bool MonitorBarcodeTracking { get; }

	int BarcodeTrackingMaxBatchSize { get; }

	int BarcodeTrackingTimerDuration { get; }

	int BarcodeTrackingMessagePriority { get; }

	int KeepHalImagesForNumberOfDays { get; }

	string FraudServicesUrl { get; }

	bool AuthorizeAtPickup { get; }

	string FraudServicesKey { get; }

	bool KioskAnalyticsEnabled { get; }

	bool SendKioskAnalyticsToKioskDataService { get; }

	string KioskDataServicesUrl { get; }

	string KioskDataServicesKey { get; }

	bool UseTimerToInvokeCallbackEntries { get; }

	bool FlashDealFlagForGamesEnabled { get; }

	void SessionReload();
}
