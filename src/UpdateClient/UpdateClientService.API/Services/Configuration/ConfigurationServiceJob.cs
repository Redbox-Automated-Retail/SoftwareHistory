using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;

namespace UpdateClientService.API.Services.Configuration;

public class ConfigurationServiceJob : IConfigurationServiceJob, IInvocable
{
	private IConfigurationService _configurationService;

	private ILogger<ConfigurationServiceJob> _logger;

	public ConfigurationServiceJob(ILogger<ConfigurationServiceJob> logger, IConfigurationService configurationService)
	{
		_configurationService = configurationService;
		_logger = logger;
	}

	public async Task Invoke()
	{
		_logger.LogInfoWithSource("Invoking ConfigurationService.GetKioskConfigurationSettingChanges", "Invoke");
		await _configurationService.GetKioskConfigurationSettingChanges(null);
	}
}
