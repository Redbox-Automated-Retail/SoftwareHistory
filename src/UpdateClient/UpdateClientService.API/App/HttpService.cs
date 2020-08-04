using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace UpdateClientService.API.App;

public class HttpService : IHttpService
{
	private readonly ILogger<HttpService> _logger;

	private readonly HttpClient _httpClient;

	public int Timeout { get; set; } = 20000;

	public HttpService(ILogger<HttpService> logger, HttpClient httpClient)
	{
		_logger = logger;
		_httpClient = httpClient;
		_httpClient.Timeout = TimeSpan.FromMilliseconds(-1.0);
	}

	public HttpRequestMessage GenerateRequest(string baseUrl, string endpoint, object requestObject, HttpMethod method, List<Header> headers = null)
	{
		Uri requestUri;
		if (string.IsNullOrEmpty(baseUrl))
		{
			requestUri = new Uri(endpoint);
		}
		else
		{
			baseUrl = baseUrl.TrimEnd('/');
			requestUri = new Uri(baseUrl + "/" + endpoint);
		}
		HttpRequestMessage httpRequest = new HttpRequestMessage(method, requestUri);
		headers?.ForEach(delegate(Header header)
		{
			httpRequest.Headers.Add(header.Key, header.Value);
		});
		if (requestObject != null)
		{
			httpRequest.Content = new StringContent(JsonConvert.SerializeObject(requestObject, Formatting.None, new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore
			}), Encoding.UTF8, "application/json");
		}
		return httpRequest;
	}

	public async Task<APIResponse<T>> SendRequest<T>(string baseUrl, string endpoint, object requestObject, HttpMethod method, List<Header> headers = null)
	{
		using HttpRequestMessage genRequest = GenerateRequest(baseUrl, endpoint, requestObject, method, headers);
		return await SendRequest<T>(genRequest, requestObject);
	}

	public async Task<APIResponse<T>> SendRequest<T>(HttpRequestMessage request, object requestObject)
	{
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.CancelAfter(Timeout);
		APIResponse<T> apiResponse = new APIResponse<T>(request, _logger);
		Stopwatch sw = new Stopwatch();
		sw.Start();
		try
		{
			apiResponse.RequestContent = JsonConvert.SerializeObject(requestObject, Formatting.None);
			HttpResponseMessage result = await _httpClient.SendAsync(request, cancellationTokenSource.Token);
			sw.Stop();
			apiResponse.SetAPIResponse(requestTime: (int)sw.ElapsedMilliseconds, responseMessage: result, content: await (result.Content?.ReadAsStringAsync()));
		}
		catch (Exception exception)
		{
			sw.Stop();
			apiResponse.SetException(exception, (int)sw.ElapsedMilliseconds);
		}
		apiResponse.Log();
		return apiResponse;
	}

	public async Task<APIResponse<byte[]>> DownloadFile(string url, int? timeout = null)
	{
		CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		cancellationTokenSource.CancelAfter(timeout ?? Timeout);
		using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
		APIResponse<byte[]> apiResponse = new APIResponse<byte[]>(request, _logger);
		Stopwatch sw = new Stopwatch();
		sw.Start();
		try
		{
			_logger.LogInfoWithSource("Attempting to download file from " + (url ?? "null"), "DownloadFile");
			using HttpResponseMessage result = await _httpClient.GetAsync(url, cancellationTokenSource.Token);
			sw.Stop();
			if (result.IsSuccessStatusCode)
			{
				apiResponse.SetAPIResponse(result, await result.Content.ReadAsByteArrayAsync(), (int)sw.ElapsedMilliseconds);
			}
			else
			{
				apiResponse.SetAPIResponse(result, new byte[0], (int)sw.ElapsedMilliseconds);
			}
		}
		catch (Exception exception)
		{
			sw.Stop();
			apiResponse.SetException(exception, (int)sw.ElapsedMilliseconds);
		}
		apiResponse.Log(responseContentLoggingEnabled: false);
		return apiResponse;
	}
}
