using KioskClientService.API.App;

namespace KioskClientService.API.Services.Campaigns;

public abstract class CampaignBaseResponse<T> : ApiBaseResponse where T : CampaignBase
{
	public T Campaign { get; set; }
}
