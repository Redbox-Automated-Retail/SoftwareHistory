namespace KioskClientService.API.Services.Campaigns;

public class Campaign<T> : CampaignBase where T : ICampaignData
{
	public T Data { get; set; }
}
