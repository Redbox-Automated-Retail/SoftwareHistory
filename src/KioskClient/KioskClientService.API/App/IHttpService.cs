using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace KioskClientService.API.App;

public interface IHttpService
{
	int Timeout { get; set; }

	Task<APIResponse<T>> SendRequest<T>(string baseUrl, string endpoint, object requestObject, HttpMethod method, List<Header> headers = null);

	Task<APIResponse<T>> SendGZipRequest<T>(string baseUrl, string endpoint, object requestObject, HttpMethod method, List<Header> headers = null);

	Task<APIResponse<T>> SendDeflateRequest<T>(string baseUrl, string endpoint, object requestObject, HttpMethod method, List<Header> headers = null);

	Task<APIResponse<T>> SendRequest<T>(HttpRequestMessage request);

	Task<APIResponse<byte[]>> DownloadFile(string url);
}
