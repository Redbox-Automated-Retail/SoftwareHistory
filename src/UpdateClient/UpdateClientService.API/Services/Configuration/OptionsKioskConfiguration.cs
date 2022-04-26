using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace UpdateClientService.API.Services.Configuration;

public class OptionsKioskConfiguration : BaseKioskConfiguration, IOptionsKioskConfiguration, IKioskConfiguration
{
	public OptionsKioskConfiguration(ILogger<OptionsKioskConfiguration> logger, IOptions<KioskConfigurationSettings> optionsKioskConfigurationSettings)
		: base(logger, optionsKioskConfigurationSettings)
	{
	}
}
