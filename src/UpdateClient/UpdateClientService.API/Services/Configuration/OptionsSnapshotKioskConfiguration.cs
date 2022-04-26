using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace UpdateClientService.API.Services.Configuration;

public class OptionsSnapshotKioskConfiguration : BaseKioskConfiguration, IOptionsSnapshotKioskConfiguration, IKioskConfiguration
{
	public OptionsSnapshotKioskConfiguration(ILogger<OptionsSnapshotKioskConfiguration> logger, IOptionsSnapshot<KioskConfigurationSettings> optionsSnapshotKioskConfigurationSettings)
		: base(logger, optionsSnapshotKioskConfigurationSettings)
	{
	}
}
