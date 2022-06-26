using System.Collections.Generic;
using System.Net;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.WelcomeScreen;

public class GetWelcomeScreenAssetsResponse : ApiBaseResponse, IMessageScrub
{
	public bool EnableAssetsSync { get; set; }

	public List<GetWelcomeScreenAsset> WelcomeScreenAssets { get; } = new List<GetWelcomeScreenAsset>();

	public GetWelcomeScreenAssetsResponse()
		: base(HttpStatusCode.OK)
	{
	}

	public override object Scrub()
	{
		return $"EnabledAssetsSync={EnableAssetsSync}, WelcomeScreenAssets Count={WelcomeScreenAssets.Count}";
	}
}
