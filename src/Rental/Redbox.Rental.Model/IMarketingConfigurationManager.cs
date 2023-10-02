namespace Redbox.Rental.Model;

public interface IMarketingConfigurationManager
{
	bool ShowBlurayBrowse { get; }

	bool ShowCollections { get; }

	bool ShowCollectionsPopUp { get; }

	int CollectionsPopUpTimer { get; }

	int CollectionsPopUpPaddleClicks { get; }
}
