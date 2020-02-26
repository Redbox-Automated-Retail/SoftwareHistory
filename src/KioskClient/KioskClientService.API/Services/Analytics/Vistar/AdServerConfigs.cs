using KioskClientService.API.Services.Configuration;

namespace KioskClientService.API.Services.Analytics.Vistar;

public class AdServerConfigs : KioskCloudClientConfiguration
{
	public bool Enabled { get; set; }

	public string Url { get; set; }

	public int? Timeout { get; set; }
}
