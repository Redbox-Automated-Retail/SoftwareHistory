using System.Collections.Generic;
using Redbox.Core;
using Redbox.Rental.Model.KioskClientService.WelcomeScreenAssets;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.KioskClientService.WelcomeScreenAssets;

public class GetWelcomeScreenAssetsResponse : ApiBaseResponse, IMessageScrub
{
	public bool EnableAssetsSync { get; set; }

	public List<WelcomeScreenAsset> WelcomeScreenAssets { get; } = new List<WelcomeScreenAsset>();

	public object Scrub()
	{
		return this.ToJson();
	}
}
