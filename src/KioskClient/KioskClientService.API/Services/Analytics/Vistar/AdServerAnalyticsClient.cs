using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Analytics.Vistar.Summaries;
using KioskClientService.API.Services.Configuration;

namespace KioskClientService.API.Services.Analytics.Vistar;

public class AdServerAnalyticsClient : IAdServerAnalyticsClient
{
	private readonly IHttpService _httpService;

	private readonly AdServerConfigs _adServerConfigs;

	private readonly List<Header> _requestHeaders = new List<Header>();

	public AdServerAnalyticsClient(IStoreService store, IHttpService httpService, IApplicationConfiguration applicationConfiguration)
	{
		_httpService = httpService;
		_adServerConfigs = applicationConfiguration.AdServer;
		if (_adServerConfigs.Timeout.HasValue)
		{
			_httpService.Timeout = _adServerConfigs.Timeout.Value;
		}
		_requestHeaders = _adServerConfigs.GetKioskCloudHeaders(null, store?.KioskId);
	}

	public async Task<APIResponse<AnalyticsResponse>> PostGetAdAnalytics(VistarGetAdSummary request)
	{
		return await _httpService.SendRequest<AnalyticsResponse>(_adServerConfigs.Url, "api/analytics/get_ad", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<AnalyticsResponse>> PostProofOfPlayAnalytics(VistarProofOfPlaySummary request)
	{
		return await _httpService.SendRequest<AnalyticsResponse>(_adServerConfigs.Url, "api/analytics/proof_of_play", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<AnalyticsResponse>> PostExpireAnalytics(VistarExpireSummary request)
	{
		return await _httpService.SendRequest<AnalyticsResponse>(_adServerConfigs.Url, "api/analytics/expire", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<AnalyticsResponse>> PostCacheAnalytics(VistarCacheSummary request)
	{
		return await _httpService.SendRequest<AnalyticsResponse>(_adServerConfigs.Url, "api/analytics/cache", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<AnalyticsResponse>> PostGetAssetAnalytics(VistarGetAssetSummary request)
	{
		return await _httpService.SendRequest<AnalyticsResponse>(_adServerConfigs.Url, "api/analytics/get_asset", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<AnalyticsResponse>> PostDownloadAssetAnalytics(VistarDownloadAssetSummary request)
	{
		return await _httpService.SendRequest<AnalyticsResponse>(_adServerConfigs.Url, "api/analytics/download_asset", request, HttpMethod.Post, _requestHeaders);
	}

	public async Task<APIResponse<AnalyticsResponse>> PostGetAdTimeoutEventAnalytics(GetAdTimeoutEventSummary request)
	{
		return await _httpService.SendRequest<AnalyticsResponse>(_adServerConfigs.Url, "api/analytics/get_ad_timeout_event", request, HttpMethod.Post, _requestHeaders);
	}
}
