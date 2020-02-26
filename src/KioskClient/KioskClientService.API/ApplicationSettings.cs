using KioskClientService.API.Models;

namespace KioskClientService.API;

public class ApplicationSettings : IApplicationSettings
{
	public string DataFilePath { get; set; }

	public string ServiceUrl { get; set; }

	public string UpdateManagerAddress { get; set; }

	public int UpdateManagerPort { get; set; }
}
