using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Configuration;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Products.ProductReelsApi;

public class ProductReelsApiClient : IProductReelsApiClient
{
	private readonly IApplicationConfiguration _applicationConfiguration;

	private readonly IHttpService _httpService;

	private readonly ILogger<ProductReelsApiClient> _logger;

	public ProductReelsApiClient(IApplicationConfiguration applicationConfiguration, IHttpService httpService, ILogger<ProductReelsApiClient> logger)
	{
		_applicationConfiguration = applicationConfiguration;
		_httpService = httpService;
		_logger = logger;
		_httpService.Timeout = _applicationConfiguration.ProductReelsApi.Timeout ?? 60000;
	}

	public async Task<ConcurrentDictionary<long, List<long>>> MoreLikeThisBulkAsync(List<long> productGroupIds)
	{
		ConcurrentDictionary<long, List<long>> result = new ConcurrentDictionary<long, List<long>>();
		List<string> list = new List<string>();
		foreach (long productGroupId in productGroupIds)
		{
			list.Add(productGroupId.ToString());
		}
		var requestObject = new
		{
			productIds = list
		};
		APIResponse<List<BulkMoreLikeThisResponse>> aPIResponse = await _httpService.SendRequest<List<BulkMoreLikeThisResponse>>(_applicationConfiguration.ProductReelsApi.Url, "products/reels/bulkmorelikethis", requestObject, HttpMethod.Post);
		if (aPIResponse == null || !aPIResponse.IsSuccessStatusCode)
		{
			_logger.LogErrorWithSource(aPIResponse?.Exception, "Call to get bulkMoreLikeThis was not successful. Errors: " + string.Join(".", aPIResponse?.Errors?.Select((Error x) => x.Message)?.ToList() ?? new List<string>()), "MoreLikeThisBulkAsync");
		}
		foreach (BulkMoreLikeThisResponse item in aPIResponse?.Response ?? new List<BulkMoreLikeThisResponse>())
		{
			if (!(item.MoreLikeThisTitles?.Any() ?? false))
			{
				continue;
			}
			List<long> list2 = new List<long>();
			foreach (string moreLikeThisTitle in item.MoreLikeThisTitles)
			{
				long result2 = 0L;
				if (long.TryParse(moreLikeThisTitle, out result2) && result2 > 0)
				{
					list2.Add(result2);
				}
			}
			long result3 = 0L;
			if (long.TryParse(item.ProductGroupId, out result3))
			{
				result.TryAdd(result3, list2);
			}
		}
		return result;
	}
}
