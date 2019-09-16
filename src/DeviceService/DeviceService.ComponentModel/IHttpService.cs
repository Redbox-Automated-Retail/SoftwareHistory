using System.Net.Http;
using System.Threading.Tasks;

namespace DeviceService.ComponentModel;

public interface IHttpService
{
	HttpRequestMessage GenerateRequest(string endpoint, object requestObject, HttpMethod method, string authHeader, string apiKey, string baseUrl);

	Task<HttpResponseMessage> SendRequest(HttpRequestMessage request, int timeout);
}
