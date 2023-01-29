using System;
using System.Collections.Generic;

namespace KioskClientService.API.Services.Campaigns;

public class CampaignList<T> : List<Campaign<T>> where T : ICampaignData
{
	public Campaign<T> GetActiveCampaign()
	{
		Campaign<T> active = null;
		DateTime now = DateTime.Now;
		ForEach(delegate(Campaign<T> campaign)
		{
			if (campaign.StartDate <= now && (!campaign.EndDate.HasValue || campaign.EndDate > now) && (active == null || active.StartDate < campaign.StartDate))
			{
				active = campaign;
			}
		});
		return active;
	}
}
