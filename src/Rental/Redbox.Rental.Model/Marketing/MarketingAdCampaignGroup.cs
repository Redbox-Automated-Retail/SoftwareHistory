using System.Collections.Generic;

namespace Redbox.Rental.Model.Marketing;

public class MarketingAdCampaignGroup
{
	private List<MarketingAdCampaign> _marketingAdCampaigns = new List<MarketingAdCampaign>();

	public List<MarketingAdCampaign> MarketingAdCampaigns => _marketingAdCampaigns;

	public List<MarketingAdCampaign> GetCampaigns(long kioskId, string banner, string market)
	{
		List<MarketingAdCampaign> list = new List<MarketingAdCampaign>();
		_marketingAdCampaigns.ForEach(delegate(MarketingAdCampaign item)
		{
			if (item.IsValidIn(kioskId, banner, market))
			{
				list.Add(item);
			}
		});
		return list;
	}
}
