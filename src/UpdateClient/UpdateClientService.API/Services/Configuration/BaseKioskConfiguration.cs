using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace UpdateClientService.API.Services.Configuration;

public class BaseKioskConfiguration : BaseConfiguration, IKioskConfiguration
{
	public Operations Operations { get; set; }

	public BaseKioskConfiguration(ILogger<BaseKioskConfiguration> logger, IOptionsMonitor<KioskConfigurationSettings> optionsMonitorKioskConfigurationSettings)
		: base(logger, optionsMonitorKioskConfigurationSettings)
	{
	}

	public BaseKioskConfiguration(ILogger<BaseKioskConfiguration> logger, IOptions<KioskConfigurationSettings> optionsKioskConfigurationSettings)
		: base(logger, optionsKioskConfigurationSettings)
	{
	}
}
