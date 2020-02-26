using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace KioskClientService.API.App;

public interface IHttpService
{
	int Timeout { get; set; }

	HttpRequestMessage GenerateRequest(string baseUrl, string endpoint, object requestObject, HttpMethod method, List<Header> headers = null);

	Task<APIResponse<T>> SendRequest<T>(string baseUrl, string endpoint, object requestObject, HttpMethod method, List<Header> headers = null);

	Task<APIResponse<byte[]>> DownloadFile(string url);
}
