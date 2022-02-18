using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.Ads;
using KioskClientService.API.App;
using KioskClientService.API.Services.Analytics.Vistar;
using KioskClientService.API.Services.Configuration;

namespace KioskClientService.API.Services.Ads.AdServer;

public class AdServerClient : IAdServerClient
{
	private readonly IHttpService _httpService;

	private readonly AdServerConfigs _config;

	private readonly List<Header> _requestHeaders;

	public AdServerClient(IHttpService httpService, IApplicationConfiguration config, IStoreService store)
	{
		_httpService = httpService;
		_config = config?.AdServer;
		_requestHeaders = _config.GetKioskCloudHeaders(null, store?.KioskId);
	}

	public async Task<APIResponse<AdResponse>> GetAd(AdLocation location, AdProviderName providerName)
	{
		return await _httpService.SendRequest<AdResponse>(_config.Url, $"api/ads/{location}?providerName={providerName}", null, HttpMethod.Get, _requestHeaders);
	}

	public async Task<APIResponse<AdsResponse>> GetAds(List<AdLocation> locations, AdProviderName providerName)
	{
		string text = locations.ToQueryString("locations", $"?providerName={providerName}");
		return await _httpService.SendRequest<AdsResponse>(_config.Url, "api/ads" + text, null, HttpMethod.Get, _requestHeaders);
	}

	public async Task<APIResponse<AdsResponse>> CacheAds(List<AdLocation> locations, AdProviderName providerName)
	{
		string text = locations.ToQueryString("locations", $"?providerName={providerName}");
		return await _httpService.SendRequest<AdsResponse>(_config.Url, "api/ads/cache" + text, null, HttpMethod.Get, _requestHeaders);
	}

	public async Task<APIResponse<dynamic>> PostProofOfPlay(ProofOfPlayRequest proofOfPlay)
	{
		return await _httpService.SendRequest<object>(_config.Url, "api/ads/proofOfPlay", proofOfPlay, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<dynamic>> PostExpire(string id)
	{
		return await _httpService.SendRequest<object>(_config.Url, "api/ads/" + id + "/expire", null, HttpMethod.Post, _requestHeaders);
	}
}
