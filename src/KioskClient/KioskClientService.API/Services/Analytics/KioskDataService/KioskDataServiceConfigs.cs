using KioskClientService.API.Services.Configuration;

namespace KioskClientService.API.Services.Analytics.KioskDataService;

public class KioskDataServiceConfigs : KioskCloudClientConfiguration
{
	public bool Enabled { get; set; }

	public string Url { get; set; }

	public int? Timeout { get; set; }
}
