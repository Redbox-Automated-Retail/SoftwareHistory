using System.Net.Http;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Services.App;

namespace Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Fraud;

public class KioskEngineApiFraudServiceClient : KioskEngineApiBaseClient
{
	private static string _apiPath = "api/fraud";

	public static async Task<FraudServiceInfo> GetFraudServiceInfoResponse()
	{
		FraudServiceInfo response = null;
		IHttpService service = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = service.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, $"{_apiPath}/info", null, HttpMethod.Get);
		APIResponse<FraudServiceInfoResponse> aPIResponse = await service.SendRequestAsync<FraudServiceInfoResponse>(request, null, "GetFraudServiceInfoResponse", "D:\\work\\53\\SC\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\Fraud\\KioskEngineApiFraudServiceClient.cs");
		if (aPIResponse != null && aPIResponse.IsSuccessStatusCode && aPIResponse.Response != null)
		{
			response = aPIResponse.Response.Content;
		}
		return response;
	}
}
