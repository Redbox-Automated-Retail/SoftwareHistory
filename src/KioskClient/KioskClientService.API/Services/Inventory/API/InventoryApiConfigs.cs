using KioskClientService.API.Services.Configuration;

namespace KioskClientService.API.Services.Inventory.API;

public class InventoryApiConfigs : KioskCloudClientConfiguration
{
	public string Url { get; set; }

	public int? Timeout { get; set; }

	public InventoryApiConfigs()
	{
		base.AuthHeader = "x-api-key";
	}
}
