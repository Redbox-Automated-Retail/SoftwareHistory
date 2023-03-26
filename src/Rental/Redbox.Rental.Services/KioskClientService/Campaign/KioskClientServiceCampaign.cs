using System;
using System.Net;
using Redbox.Rental.Model.KioskClientService.Campaign;

namespace Redbox.Rental.Services.KioskClientService.Campaign;

public class KioskClientServiceCampaign : BaseKioskClientService, IKioskClientServiceCampaign
{
	private const string _controllerPrefix = "api/campaign";

	public KioskInCartDetails GetInCart()
	{
		WebResult<GetInCartsResponse> webResult = ExecuteWebRequest<GetInCartsResponse>(new Uri(string.Format("{0}/{1}/incart", base.KioskClientServiceBaseUrl, "api/campaign")), null, "GET", 500);
		if (webResult == null || webResult.StatusCode != HttpStatusCode.OK)
		{
			return null;
		}
		return webResult?.Response?.InCart;
	}

	public KioskStartScreenDetails GetStartScreen()
	{
		WebResult<GetStartScreenResponse> webResult = ExecuteWebRequest<GetStartScreenResponse>(new Uri(string.Format("{0}/{1}/startscreen", base.KioskClientServiceBaseUrl, "api/campaign")), null, "GET", 500);
		if (webResult == null || webResult.StatusCode != HttpStatusCode.OK)
		{
			return null;
		}
		return webResult?.Response?.StartScreen;
	}
}
