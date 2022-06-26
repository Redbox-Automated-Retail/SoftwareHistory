using System;
using System.Collections.Generic;
using Redbox.Rental.Model.KioskClientService.WelcomeScreenAssets;

namespace Redbox.Rental.Services.KioskClientService.WelcomeScreenAssets;

public class KioskClientServiceWelcomeScreenAssets : BaseKioskClientService, IKioskClientServiceWelcomeScreenAssets
{
	private const string _controllerPrefix = "api/welcomescreenassets";

	public List<WelcomeScreenAsset> GetWelcomeScreenAssets()
	{
		WebResult<GetWelcomeScreenAssetsResponse> webResult = ExecuteWebRequest<GetWelcomeScreenAssetsResponse>(new Uri(string.Format("{0}/{1}", base.KioskClientServiceBaseUrl, "api/welcomescreenassets")), null, "GET", 500);
		if (webResult?.Response == null)
		{
			return null;
		}
		if (!webResult.Response.EnableAssetsSync)
		{
			return null;
		}
		return webResult.Response.WelcomeScreenAssets;
	}
}
