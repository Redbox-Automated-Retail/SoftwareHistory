namespace KioskClientService.API.Models;

public interface IApplicationSettings
{
	string ServiceUrl { get; set; }

	string DataFilePath { get; set; }

	string UpdateManagerAddress { get; set; }

	int UpdateManagerPort { get; set; }
}
