using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Configuration;
using Microsoft.Extensions.Logging;

namespace KioskClientService.API.Services.Analytics.KioskDataService;

public class KioskDataServiceClient : IKioskDataServiceClient
{
	private readonly IHttpService _httpService;

	private ILogger<KioskDataServiceClient> _logger;

	private readonly KioskDataServiceConfigs _kdsConfigs;

	private readonly List<Header> _requestHeaders;

	public KioskDataServiceClient(IStoreService store, IHttpService httpService, ILogger<KioskDataServiceClient> logger, IApplicationConfiguration applicationConfiguration)
	{
		_httpService = httpService;
		_logger = logger;
		_kdsConfigs = applicationConfiguration.KioskDataService;
		if (_kdsConfigs.Timeout.HasValue)
		{
			_httpService.Timeout = _kdsConfigs.Timeout.Value;
		}
		_requestHeaders = _kdsConfigs.GetKioskCloudHeaders(store?.KioskId);
	}

	public async Task<APIResponse<AnalyticsResponse>> PostKioskClientEvent(KioskClientEventRequest request)
	{
		return await _httpService.SendRequest<AnalyticsResponse>(_kdsConfigs.Url, "api/KioskData/KioskClientServiceEvent", request, HttpMethod.Post, _requestHeaders);
	}
}
