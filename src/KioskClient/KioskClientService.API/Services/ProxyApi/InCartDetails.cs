using System;
using System.Collections.Generic;

namespace KioskClientService.API.Services.ProxyApi;

public class InCartDetails
{
	public long CampaignId { get; set; }

	public CampaignType CampaignType { get; set; }

	public DateTime StartDate { get; set; }

	public DateTime? EndDate { get; set; }

	public long CampaignInCartId { get; set; }

	public InCartType InCartType { get; set; }

	public decimal? Amount { get; set; }

	public string PromoCode { get; set; }

	public List<long> ExcludeTitles { get; set; }

	public int Rank { get; set; }
}
