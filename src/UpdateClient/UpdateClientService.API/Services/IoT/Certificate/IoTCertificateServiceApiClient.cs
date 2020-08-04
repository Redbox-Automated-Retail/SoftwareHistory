using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using UpdateClientService.API.App;

namespace UpdateClientService.API.Services.IoT.Certificate;

public class IoTCertificateServiceApiClient : IIoTCertificateServiceApiClient
{
	private readonly IHttpService _httpService;

	private readonly IStoreService _store;

	private readonly AppSettings _appSettings;

	private const string ThingType = "kiosks";

	private readonly object _lockObj = new object();

	public IoTCertificateServiceApiClient(IHttpService httpService, IStoreService store, IOptions<AppSettings> appSettings)
	{
		_httpService = httpService;
		_store = store;
		_appSettings = appSettings.Value;
		_httpService.Timeout = 90000;
	}

	public async Task<bool> IsCertificateValid(string certificateId)
	{
		Monitor.Enter(_lockObj);
		APIResponse<bool> result;
		try
		{
			result = _httpService.SendRequest<bool>(_appSettings.IoTCertificateServiceUrl, string.Format("api/thing-types/{0}/things/{1}/certificates/{2}/is-valid", "kiosks", _store.KioskId, certificateId), null, HttpMethod.Post, new List<Header>
			{
				new Header("x-api-key", _appSettings.IoTCertificateServiceApiKey)
			}).GetAwaiter().GetResult();
		}
		finally
		{
			Monitor.Exit(_lockObj);
		}
		return result?.Response ?? false;
	}

	public async Task<IoTCertificateServiceResponse> GetCertificates()
	{
		Monitor.Enter(_lockObj);
		APIResponse<IoTCertificateServiceResponse> result;
		try
		{
			result = _httpService.SendRequest<IoTCertificateServiceResponse>(_appSettings.IoTCertificateServiceUrl, string.Format("api/thing-types/{0}/things/{1}/certificates?thingGroupName={2}", "kiosks", _store.KioskId, _store.Market), null, HttpMethod.Get, new List<Header>
			{
				new Header("x-api-key", _appSettings.IoTCertificateServiceApiKey)
			}).GetAwaiter().GetResult();
		}
		finally
		{
			Monitor.Exit(_lockObj);
		}
		return result?.Response;
	}

	public async Task<IoTCertificateServiceResponse> GenerateNewCertificates()
	{
		Monitor.Enter(_lockObj);
		APIResponse<IoTCertificateServiceResponse> result;
		try
		{
			result = _httpService.SendRequest<IoTCertificateServiceResponse>(_appSettings.IoTCertificateServiceUrl, string.Format("api/thing-types/{0}/things/{1}/certificates?thingGroupName={2}", "kiosks", _store.KioskId, _store.Market), null, HttpMethod.Post, new List<Header>
			{
				new Header("x-api-key", _appSettings.IoTCertificateServiceApiKey)
			}).GetAwaiter().GetResult();
		}
		finally
		{
			Monitor.Exit(_lockObj);
		}
		return result?.Response;
	}
}
