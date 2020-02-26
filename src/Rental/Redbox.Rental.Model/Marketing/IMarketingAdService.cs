using Redbox.Rental.Model.Ads;

namespace Redbox.Rental.Model.Marketing;

public interface IMarketingAdService : IAdService
{
	bool CheckForNewActiveCampaign();

	void ActivateCampaigns();

	void LoadCampaigns();
}
