using KioskClientService.API.Services.Campaign;
using Microsoft.Extensions.DependencyInjection;

namespace KioskClientService.API.Services.Campaigns;

public static class CampaignExtensions
{
	public static IServiceCollection AddCampaignService(this IServiceCollection services)
	{
		return services.AddScoped<ICampaignService, CampaignService>().AddScoped<CampaignJob>();
	}
}
