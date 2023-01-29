namespace KioskClientService.API.Services.Campaigns;

public class ConcreteBaseResponse<T> : CampaignBaseResponse<Campaign<T>> where T : ICampaignData
{
}
