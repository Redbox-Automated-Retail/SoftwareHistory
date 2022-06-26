using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KioskClientService.API.Services.Configuration;

public class BaseKioskConfiguration : BaseConfiguration, IKioskConfiguration
{
	public ZeroTouch ZeroTouch { get; set; }

	public MarketingGeneral MarketingGeneral { get; set; }

	public MarketingWelcomeScreen MarketingWelcomeScreen { get; set; }

	public BaseKioskConfiguration(ILogger<BaseKioskConfiguration> logger, IOptionsMonitor<KioskConfigurationSettings> optionsMonitorKioskConfigurationSettings)
		: base(logger, optionsMonitorKioskConfigurationSettings)
	{
	}

	public BaseKioskConfiguration(ILogger<BaseKioskConfiguration> logger, IOptions<KioskConfigurationSettings> optionsKioskConfigurationSettings)
		: base(logger, optionsKioskConfigurationSettings)
	{
	}
}
