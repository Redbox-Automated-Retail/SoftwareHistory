namespace Redbox.Rental.Model.Marketing;

public interface IMarketingAdImpressionService
{
	void AddImpression(MarketingAdCampaign marketingAdCampaign, MarketingAdLocation marketingAdLocation);

	void Start();

	void Stop();
}
