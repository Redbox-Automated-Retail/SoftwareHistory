using KioskClientService.API.Services.Configuration;

namespace KioskClientService.API.Services.Transaction.API;

public class TransactionApiConfigs : KioskCloudClientConfiguration
{
	public string Url { get; set; }

	public int? Timeout { get; set; }

	public int? ReturnPromoCheckTimeout { get; set; }

	public TransactionApiConfigs()
	{
		base.AuthHeader = "x-api-key";
	}
}
