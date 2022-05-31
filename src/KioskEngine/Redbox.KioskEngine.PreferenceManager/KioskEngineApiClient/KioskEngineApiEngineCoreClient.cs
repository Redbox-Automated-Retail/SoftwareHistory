using System.Net.Http;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.ApiService.EngineCore;
using Redbox.Rental.Services.App;

namespace Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient;

public class KioskEngineApiEngineCoreClient : KioskEngineApiBaseClient
{
	private static string _apiPath = "api/enginecore";

	public static async Task<APIResponse<EngineCoreKioskEngineVersionResponse>> GetKioskEngineVersions()
	{
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, $"{_apiPath}/getkioskengineversions", null, HttpMethod.Get);
		return await service.SendRequestAsync<EngineCoreKioskEngineVersionResponse>(request, null, "GetKioskEngineVersions", "D:\\work\\45\\SC\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\KioskEngineApiEngineCoreClient.cs");
	}

	public static async Task<APIResponse<GetStoreInfoResponse>> GetStoreInfo()
	{
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, $"{_apiPath}/getstoreinfo", null, HttpMethod.Get);
		return await service.SendRequestAsync<GetStoreInfoResponse>(request, null, "GetStoreInfo", "D:\\work\\45\\SC\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\KioskEngineApiEngineCoreClient.cs");
	}

	public static async Task<APIResponse<GetPreferenceValueResponse>> GetPreferenceValue(string pageName, string settingName)
	{
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, $"{_apiPath}/getpreferencevalue?pageName={pageName}&settingName={settingName}", null, HttpMethod.Get);
		return await service.SendRequestAsync<GetPreferenceValueResponse>(request, null, "GetPreferenceValue", "D:\\work\\45\\SC\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\KioskEngineApiEngineCoreClient.cs");
	}
}
