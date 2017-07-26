namespace Redbox.Rental.Model.Marketing;

public interface IMarketingAdService
{
	void StartApplication();

	void StartSession();

	MarketingAd GetScreenAd(MarketingAdLocation marketingAdLocation, bool createImpression);

	bool CheckForNewActiveCampaign();
}
