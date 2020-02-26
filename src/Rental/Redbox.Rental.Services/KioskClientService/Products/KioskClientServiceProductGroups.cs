using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.Rental.Model.KioskClientService.Products;

namespace Redbox.Rental.Services.KioskClientService.Products;

public class KioskClientServiceProductGroups : BaseKioskClientService, IKioskClientServiceProductGroups
{
	private const string _controllerPrefix = "api/productgroups";

	public List<long> GetMoreLikeThis(long productGroupId)
	{
		List<long> result = new List<long>();
		if (productGroupId > 0)
		{
			LogHelper.Instance.Log($"Getting more like this for productGroupId: {productGroupId}");
			WebResult<GetMoreLikeThisResponse> webResult = ExecuteWebRequest<GetMoreLikeThisResponse>(new Uri(string.Format("{0}/{1}/{2}/morelikethis", base.KioskClientServiceBaseUrl, "api/productgroups", productGroupId)), null, "GET", 500);
			LogKioskClientServiceResponse("KioskClientServiceProductGroups", webResult);
			result = webResult?.Response?.MoreLikeThis;
		}
		else
		{
			LogHelper.Instance.Log($"Not making KCS call to get more like this because given productGroupId was less than 1. Given ProductGroupId: {productGroupId}");
		}
		return result;
	}

	public void RefreshAllMLTData(List<long> productGroupIds)
	{
		Task.Run(() => ExecuteWebRequest<BaseResponse>(new Uri(string.Format("{0}/{1}/morelikethis/refresh", base.KioskClientServiceBaseUrl, "api/productgroups")), productGroupIds, "POST", 180000));
	}
}
