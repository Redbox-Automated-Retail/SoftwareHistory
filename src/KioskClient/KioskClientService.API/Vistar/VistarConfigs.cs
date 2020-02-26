using KioskClientService.API.Services.Configuration;

namespace KioskClientService.API.Vistar;

public class VistarConfigs
{
	public string Url { get; set; }

	public int? Timeout { get; set; }

	public string NetworkId { get; set; }

	public string APIKey { get; set; }

	public static VistarConfigs GetConfigs(IApplicationConfiguration applicationConfiguration)
	{
		return new VistarConfigs
		{
			Url = applicationConfiguration.VistarUrl,
			Timeout = applicationConfiguration.VistarTimeout,
			NetworkId = applicationConfiguration.VistarNetworkId,
			APIKey = applicationConfiguration.VistarAPIKey
		};
	}
}
