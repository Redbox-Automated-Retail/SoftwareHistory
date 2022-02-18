using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.ProxyApi;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Promotion;

public class PromotionService : IPromotionService
{
	private readonly ILogger<IPromotionService> _logger;

	private readonly IProxyApi _proxyApi;

	public PromotionService(ILogger<IPromotionService> logger, IProxyApi proxyApi)
	{
		_logger = logger;
		_proxyApi = proxyApi;
	}

	public async Task<APIResponse<PromotionCodeValidationResponse>> ValidatePromotionCodeAsync(PromotionCodeValidationPayload payload)
	{
		_logger.LogInfoWithSource("Making Promotion Code Validation call to Proxy Service api", "ValidatePromotionCodeAsync");
		return await _proxyApi.PromoCodeValidationAsync(payload);
	}
}
