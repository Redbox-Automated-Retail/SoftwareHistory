namespace Redbox.Rental.Model;

public interface IMarketingConfigurationManager
{
	bool ShowGameBanner { get; }

	bool ShowBlurayBrowse { get; }

	bool ShowCollections { get; }

	bool ShowCollectionsBanner { get; }

	bool ShowCollectionsPopUp { get; }

	int CollectionsPopUpTimer { get; }

	int CollectionsPopUpPaddleClicks { get; }

	bool EnableMarketingAds { get; }
}
