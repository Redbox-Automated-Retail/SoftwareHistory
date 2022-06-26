using Microsoft.Extensions.DependencyInjection;

namespace KioskClientService.API.Services.WelcomeScreen;

public static class WelcomeScreenAssetsExtensions
{
	public static IServiceCollection AddWelcomeScreenAssets(this IServiceCollection services)
	{
		return services.AddScoped<IWelcomeScreenAssetsService, WelcomeScreenAssetsService>().AddScoped<WelcomeScreenAssetsJob>();
	}
}
