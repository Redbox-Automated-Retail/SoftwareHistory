using KioskClientService.API.App;

namespace KioskClientService.API.Services.Ads.AdServer;

public class AdResponse : ApiBaseResponse
{
	public Ad Data { get; set; }
}
