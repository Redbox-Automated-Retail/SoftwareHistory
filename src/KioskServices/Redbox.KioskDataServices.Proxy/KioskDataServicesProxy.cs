using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Redbox.Core;
using Redbox.JSONPrettyPrinter;
using Redbox.Rental.Model;

namespace Redbox.KioskDataServices.Proxy;

public class KioskDataServicesProxy
{
	public static KioskDataServicesProxy Instance => Singleton<KioskDataServicesProxy>.Instance;

	public Uri KioskDataServicesBaseUri
	{
		get
		{
			string text = ServiceLocator.Instance.GetService<IConfiguration>()?.KioskDataServicesUrl;
			if (!text.EndsWith("/"))
			{
				text = $"{text}/";
			}
			return new Uri(text);
		}
	}

	private string AuthorizationKey => ServiceLocator.Instance.GetService<IConfiguration>()?.KioskDataServicesKey;

	public IDictionary<string, object> ExecuteMessage(string messageType, string apiUri, string jsonRequest)
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		Dictionary<string, object> dictionary = new Dictionary<string, object> { { "success", false } };
		try
		{
			string prettyString = PrettyPrinter.GetPrettyString(jsonRequest);
			LogHelper.Instance.Log("KioskDataServicesProxy JSON request: type {0}; body: {1}", messageType, (prettyString.Length <= 10000) ? prettyString : jsonRequest);
			string text = PerformRemoteMessage(apiUri, jsonRequest, AuthorizationKey);
			LogHelper.Instance.Log("KioskDataServicesProxy JSON response: type {0}; response: {1}", messageType, PrettyPrinter.GetPrettyString(text));
			LogHelper.Instance.Log("KioskDataServicesProxy call execution time: {0}", executionTimer.Elapsed);
			if (!string.IsNullOrEmpty(text))
			{
				dictionary = text.ToDictionary();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in KioskDataServicesProxy.ExecuteMessage.", e);
			dictionary["ExceptionRaised"] = true;
		}
		return dictionary;
	}

	private string PerformRemoteMessage(string apiUri, string jsonRequest, string authorizationKey)
	{
		Uri result = null;
		if (Uri.TryCreate(KioskDataServicesBaseUri, apiUri, out result))
		{
			LogHelper.Instance.Log("KioskDataServicesProxy remote URL: {0}", result);
			WebRequest webRequest = WebRequest.Create(result);
			webRequest.Method = "POST";
			webRequest.ContentType = "application/json";
			webRequest.ContentLength = jsonRequest.Length;
			webRequest.Headers.Add($"Authorization: Bearer {authorizationKey}");
			IStoreManager service = ServiceLocator.Instance.GetService<IStoreManager>();
			if (service != null)
			{
				webRequest.Headers.Add($"x-redbox-kioskid: {service.KioskId}");
				LogHelper.Instance.Log($"Added x-redbox-kioskid header where KioskId = '{service.KioskId}'");
			}
			else
			{
				LogHelper.Instance.Log("Could not add x-redbox-kioskid header; Store Manager is null.");
			}
			LogHelper.Instance.Log("...Making remote call using HTTPS POST.");
			Stream requestStream = webRequest.GetRequestStream();
			requestStream.Write(Encoding.UTF8.GetBytes(jsonRequest), 0, jsonRequest.Length);
			requestStream.Close();
			using WebResponse webResponse = webRequest.GetResponse();
			using StreamReader streamReader = new StreamReader(webResponse.GetResponseStream());
			return HttpUtility.UrlDecode(streamReader.ReadToEnd());
		}
		LogHelper.Instance.Log($"Unable to create Uri from {KioskDataServicesBaseUri} and {apiUri}");
		return null;
	}

	private KioskDataServicesProxy()
	{
	}
}
