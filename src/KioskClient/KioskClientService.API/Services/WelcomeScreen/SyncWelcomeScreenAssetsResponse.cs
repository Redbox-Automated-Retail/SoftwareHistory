using System.Net;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.WelcomeScreen;

public class SyncWelcomeScreenAssetsResponse : ApiBaseResponse
{
	public SyncWelcomeScreenAssetsResponse()
		: base(HttpStatusCode.OK)
	{
	}
}
