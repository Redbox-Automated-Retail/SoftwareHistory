namespace KioskClientService.API.Services.Campaigns;

public class StartScreenData : ICampaignData
{
	public ViewControlList Controls { get; set; } = new ViewControlList();
}
