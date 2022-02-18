using System;
using Redbox.Core;
using Redbox.Rental.Model.KioskClientService.Subscriptions;

namespace Redbox.Rental.Services.KioskClientService.Subscriptions;

public class KioskClientServiceSubscriptions : BaseKioskClientService, IKioskClientServiceSubscriptions
{
	private const string _apiPath = "api/subscription";

	public ISubscriptionPlans GetSubscriptionPlans()
	{
		ISubscriptionPlans result = null;
		WebResult<GetSubscriptionPlanResponse> webResult = ExecuteWebRequest<GetSubscriptionPlanResponse>(new Uri(string.Format("{0}/{1}/plans", base.KioskClientServiceBaseUrl, "api/subscription")), null, "GET", 20000);
		LogResponse("KioskClientServiceSubscriptions.GetSubscriptionPlans", webResult);
		if (webResult?.Response?.Success ?? false)
		{
			result = new SubscriptionPlans(webResult.Response);
			LogHelper.Instance.Log($"GetSubscriptionPlanResponse: {webResult.Response.ToJson()}");
		}
		return result;
	}
}
