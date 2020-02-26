using Redbox.Rental.Model.Ads;

namespace Redbox.Rental.Model.Marketing;

public interface IMarketingAdImpressionService
{
	void AddImpression(MarketingAdCampaign marketingAdCampaign, AdLocation marketingAdLocation);

	void Start();

	void Stop();
}
