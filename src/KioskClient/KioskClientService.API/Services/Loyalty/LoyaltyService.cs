using System;
using System.Net;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.ProxyApi;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Loyalty;

internal class LoyaltyService : ILoyaltyService
{
	private readonly ILogger<ILoyaltyService> _logger;

	private readonly IProxyApi _proxyApi;

	public LoyaltyService(ILogger<ILoyaltyService> logger, IProxyApi proxyApi)
	{
		_logger = logger;
		_proxyApi = proxyApi;
	}

	public async Task<APIResponse<ServerLoyaltyResponse>> EstimateAccrualAsync(CartPayload payload)
	{
		return await _proxyApi.EstimateAccrualAsync(payload);
	}

	public async Task<APIResponse<ServerLoyaltyResponse>> EstimateRedemptionAsync(CartPayload payload)
	{
		return await _proxyApi.EstimateRedemptionAsync(payload);
	}

	public async Task<UpdateAccountResponse> UpdateAccountAsync(UpdateAccountPayload payload)
	{
		UpdateAccountResponse result = new UpdateAccountResponse
		{
			MessageId = payload.MessageId,
			KioskId = payload.KioskId
		};
		try
		{
			UpdateAccountResponse updateAccountResponse = result;
			updateAccountResponse.CopyFromApiResponse(await _proxyApi.UpdateAccountAsync(payload));
		}
		catch (Exception exception)
		{
			_logger.LogCritical(exception, "LoyaltyService.UpdateAccountAsync -> Exception updating account!");
			result.StatusCode = HttpStatusCode.InternalServerError;
			result.Error = HttpStatusCode.InternalServerError.ToString();
		}
		return result;
	}
}
