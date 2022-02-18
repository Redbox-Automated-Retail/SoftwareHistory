using Microsoft.Extensions.DependencyInjection;

namespace UpdateClientService.API.Services.Configuration;

public static class ConfigurationServiceExtensions
{
	public static IServiceCollection AddConfigurationService(this IServiceCollection serviceCollection)
	{
		serviceCollection.AddScoped<IConfigurationService, ConfigurationService>();
		serviceCollection.AddScoped<IConfigurationServiceJob, ConfigurationServiceJob>();
		serviceCollection.AddScoped<IConfigurationServiceUpdateStatusJob, ConfigurationServiceUpdateStatusJob>();
		return serviceCollection;
	}
}
