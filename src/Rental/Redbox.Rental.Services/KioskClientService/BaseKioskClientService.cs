using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Redbox.Core;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;

namespace Redbox.Rental.Services.KioskClientService;

public class BaseKioskClientService
{
	protected class WebResult<T>
	{
		public T Response { get; set; }

		public long Duration { get; set; }

		public WebExceptionStatus ExceptionStatus { get; set; }
	}

	protected class Methods
	{
		public const string Get = "GET";

		public const string Post = "POST";
	}

	protected string KioskClientServiceBaseUrl => ServiceLocator.Instance.GetService<IConfiguration>()?.KioskClientServiceBaseUrl;

	protected void LogKioskClientServiceResponse<T>(string description, WebResult<T> webResult) where T : BaseResponse
	{
		LogHelper.Instance.Log(string.Format("{0} {1}. Service Call Duration: {2}", description, (webResult?.Response?.Success ?? false) ? "successful" : "failed", webResult?.Duration));
		if (webResult?.Response?.Errors == null || webResult.Response.Errors.Count <= 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (IError error in webResult.Response.Errors)
		{
			stringBuilder.AppendLine($"Error code {error.Code}, Message {error.Message}");
		}
		LogHelper.Instance.Log(stringBuilder.ToString());
	}

	protected WebResult<T> ExecuteWebRequest<T>(Uri uri, object requestPayload, string method, int timeout = 3000)
	{
		WebResult<T> webResult = new WebResult<T>();
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		try
		{
			WebRequest webRequest = WebRequest.Create(uri);
			webRequest.Method = method;
			if (requestPayload == null)
			{
				webRequest.ContentLength = 0L;
			}
			else
			{
				webRequest.ContentType = "application/json";
				string text = JsonConvert.SerializeObject(requestPayload);
				Stream requestStream = webRequest.GetRequestStream();
				requestStream.Write(Encoding.UTF8.GetBytes(text), 0, text.Length);
				requestStream.Close();
			}
			webRequest.Timeout = timeout;
			using WebResponse webResponse = webRequest.GetResponse();
			using StreamReader streamReader = new StreamReader(webResponse.GetResponseStream());
			string str = streamReader.ReadToEnd();
			str = HttpUtility.UrlDecode(str);
			webResult.Response = JsonConvert.DeserializeObject<T>(str);
		}
		catch (Exception ex)
		{
			if (ex is WebException ex2)
			{
				webResult.ExceptionStatus = ex2.Status;
			}
			LogHelper.Instance.Log($"Error in ExecuteGetWebRequest. Uri: {uri}  {ex}");
		}
		stopwatch.Stop();
		webResult.Duration = stopwatch.ElapsedMilliseconds;
		return webResult;
	}

	protected async Task<WebResult<T>> ExecuteAsyncWebRequest<T>(Uri uri, object requestPayload, string method, int timeout = 3000)
	{
		WebResult<T> result = new WebResult<T>();
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();
		try
		{
			WebRequest request = WebRequest.Create(uri);
			request.Method = method;
			if (requestPayload == null)
			{
				request.ContentLength = 0L;
			}
			else
			{
				request.ContentType = "application/json";
				string text = JsonConvert.SerializeObject(requestPayload);
				Stream requestStream = request.GetRequestStream();
				requestStream.Write(Encoding.UTF8.GetBytes(text), 0, text.Length);
				requestStream.Close();
			}
			request.Timeout = timeout;
			Task<WebResponse> webRequestTask = request.GetResponseAsync();
			using CancellationTokenSource timeoutCancellationTokenSource = new CancellationTokenSource();
			if (await Task.WhenAny(webRequestTask, Task.Delay(timeout, timeoutCancellationTokenSource.Token)).ConfigureAwait(continueOnCapturedContext: false) == webRequestTask)
			{
				timeoutCancellationTokenSource.Cancel();
				using WebResponse response = await request.GetResponseAsync();
				HttpWebResponse httpWebResponse = response as HttpWebResponse;
				if (httpWebResponse != null && httpWebResponse.StatusCode == HttpStatusCode.OK)
				{
					using StreamReader reader = new StreamReader(response.GetResponseStream());
					string value = HttpUtility.UrlDecode(await reader.ReadToEndAsync().ConfigureAwait(continueOnCapturedContext: false));
					result.Response = JsonConvert.DeserializeObject<T>(value);
				}
				else
				{
					LogHelper.Instance.Log($"Error in ExecuteGetWebRequest. Uri: {uri}  HttpWebResponse StatusCode: {httpWebResponse?.StatusCode}");
				}
			}
			else
			{
				LogHelper.Instance.Log($"Error in ExecuteGetWebRequest. Uri: {uri}  Request timed out with timeout of {timeout}");
			}
		}
		catch (Exception arg)
		{
			LogHelper.Instance.Log($"Error in ExecuteGetWebRequest. Uri: {uri}  {arg}");
		}
		stopwatch.Stop();
		result.Duration = stopwatch.ElapsedMilliseconds;
		return result;
	}
}
