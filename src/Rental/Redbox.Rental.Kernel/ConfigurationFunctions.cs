using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Kernel;

public class ConfigurationFunctions
{
	[KernelFunction(Name = "kernel.get_rental_config_value")]
	internal static object GetRentalConfigValue(string configPropertyName)
	{
		IConfiguration instance = Configuration.Instance;
		return configPropertyName switch
		{
			"OfflineItemsLimit" => instance.OfflineItemsLimit, 
			"OfflineGameRentalLimit" => instance.OfflineGameRentalLimit, 
			"OfflineGamePurchaseLimit" => instance.OfflineGamePurchaseLimit, 
			_ => null, 
		};
	}
}
