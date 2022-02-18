using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;

namespace UpdateClientService.API.Services.Configuration;

public class ConfigurationServiceUpdateStatusJob : IConfigurationServiceUpdateStatusJob, IInvocable
{
	private IConfigurationService _configurationService;

	private ILogger<ConfigurationServiceUpdateStatusJob> _logger;

	public ConfigurationServiceUpdateStatusJob(ILogger<ConfigurationServiceUpdateStatusJob> logger, IConfigurationService configurationService)
	{
		_configurationService = configurationService;
		_logger = logger;
	}

	public async Task Invoke()
	{
		_logger.LogInfoWithSource("Invoking ConfigurationService.UpdateConfigurationStatusIfNeeded", "Invoke");
		await _configurationService.UpdateConfigurationStatusIfNeeded();
	}
}
