using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProxyApi;

public class GetWelcomeScreenAssetsResponse : ApiBaseResponse
{
	public List<WelcomeScreenAssetInfo> WelcomeScreenAssets { get; set; } = new List<WelcomeScreenAssetInfo>();

	public new object Scrub()
	{
		return $"Returning {WelcomeScreenAssets?.Count ?? 0} WelcomeScreenAssets";
	}
}
