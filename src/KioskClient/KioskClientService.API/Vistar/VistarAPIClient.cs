using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Configuration;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Vistar;

public class VistarAPIClient : IVistarAPIClient
{
	private readonly IHttpService _httpService;

	private ILogger<VistarAPIClient> _logger;

	private IApplicationConfiguration _applicationConfiguration;

	private VistarConfigs _vistarConfigs;

	public VistarAPIClient(IHttpService httpService, ILogger<VistarAPIClient> logger, IApplicationConfiguration applicationConfiguration)
	{
		_httpService = httpService;
		_applicationConfiguration = applicationConfiguration;
		_logger = logger;
		_vistarConfigs = VistarConfigs.GetConfigs(_applicationConfiguration);
		VistarConfigs vistarConfigs = _vistarConfigs;
		if (vistarConfigs != null && vistarConfigs.Timeout.HasValue)
		{
			_httpService.Timeout = _vistarConfigs.Timeout.Value;
		}
	}

	public async Task<APIResponse<VistarGetAssetsResponse>> GetAssets(long kioskId, List<VistarDisplayArea> displayArea)
	{
		VistarGetAssetRequest requestObject = new VistarGetAssetRequest
		{
			device_id = "kiosk",
			direct_connection = false,
			device_attribute = new List<VistarDeviceAttribute>(),
			display_area = displayArea,
			venue_id = kioskId.ToString(),
			display_time = new DateTimeOffset(DateTime.Now).AddHours(24.0).ToUnixTimeSeconds(),
			api_key = _vistarConfigs.APIKey,
			network_id = _vistarConfigs.NetworkId
		};
		return await _httpService.SendRequest<VistarGetAssetsResponse>(_vistarConfigs.Url, "api/v1/get_asset/json", requestObject, HttpMethod.Get);
	}

	public async Task<APIResponse<VistarGetAdResponse>> GetAd(long kioskId, List<VistarDisplayArea> displayArea)
	{
		VistarGetAdRequest requestObject = new VistarGetAdRequest
		{
			device_id = "kiosk",
			direct_connection = false,
			device_attribute = new List<VistarDeviceAttribute>(),
			display_area = displayArea,
			venue_id = kioskId.ToString(),
			display_time = new DateTimeOffset(DateTime.Now).AddMilliseconds(10.0).ToUnixTimeSeconds(),
			api_key = _vistarConfigs.APIKey,
			network_id = _vistarConfigs.NetworkId
		};
		APIResponse<VistarGetAdResponse> aPIResponse = await _httpService.SendRequest<VistarGetAdResponse>(_vistarConfigs.Url, "api/v1/get_ad/json", requestObject, HttpMethod.Post);
		if (aPIResponse.IsSuccessStatusCode && aPIResponse.Response.advertisement == null)
		{
			aPIResponse.Response.advertisement = new List<VistarAdvertisement>();
		}
		return aPIResponse;
	}

	public async Task<APIResponse<VistarProofOfPlayResponse>> GetProofOfPlay(VistarProofOfPlayRequest request)
	{
		return await _httpService.SendRequest<VistarProofOfPlayResponse>(null, request.ProofOfPlayUrl, null, HttpMethod.Get);
	}

	public async Task<APIResponse<VistarExpirationResponse>> GetExpiration(VistarExpirationRequest request)
	{
		return await _httpService.SendRequest<VistarExpirationResponse>(null, request.ExpirationUrl ?? "", null, HttpMethod.Get);
	}
}
