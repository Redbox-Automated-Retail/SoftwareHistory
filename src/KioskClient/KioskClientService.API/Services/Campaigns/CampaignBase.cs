using System;
using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.Campaigns;

public class CampaignBase
{
	public long CampaignId { get; set; }

	public DateTime StartDate { get; set; }

	public DateTime? EndDate { get; set; }

	public CampaignType CampaignType { get; set; }
}
