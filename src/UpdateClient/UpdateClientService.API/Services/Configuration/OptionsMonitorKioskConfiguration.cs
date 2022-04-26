using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace UpdateClientService.API.Services.Configuration;

public class OptionsMonitorKioskConfiguration : BaseKioskConfiguration, IOptionsMonitorKioskConfiguration, IKioskConfiguration
{
	public OptionsMonitorKioskConfiguration(ILogger<OptionsMonitorKioskConfiguration> logger, IOptionsMonitor<KioskConfigurationSettings> optionsMonitorKioskConfigurationSettings)
		: base(logger, optionsMonitorKioskConfigurationSettings)
	{
	}
}
