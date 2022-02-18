using KioskClientService.API.Services.Configuration;

namespace KioskClientService.API.Services.ProxyApi;

public class ProxyApiConfigs : KioskCloudClientConfiguration
{
	public string Url { get; set; }

	public int? Timeout { get; set; }

	public int? UpdateAccountTimeout { get; set; }

	public ProxyApiConfigs()
	{
		base.AuthHeader = "x-api-key";
	}
}
