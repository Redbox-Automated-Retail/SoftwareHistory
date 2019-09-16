using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Redbox.Core;
using Redbox.JSONPrettyPrinter;
using Redbox.Rental.Model;

namespace Redbox.FraudServices.Proxy;

public class FraudServicesProxy
{
	private bool _awsFraudEnabled;

	public static FraudServicesProxy Instance => Singleton<FraudServicesProxy>.Instance;

	public Uri FraudServicesBaseUri
	{
		get
		{
			IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
			string text = (_awsFraudEnabled ? service.AwsFraudServicesUrl : service?.FraudServicesUrl);
			if (!text.EndsWith("/"))
			{
				text = $"{text}/";
			}
			return new Uri(text);
		}
	}

	private string AuthorizationKey
	{
		get
		{
			IConfiguration service = ServiceLocator.Instance.GetService<IConfiguration>();
			if (!_awsFraudEnabled)
			{
				return service?.FraudServicesKey;
			}
			return service.AwsFraudServicesKey;
		}
	}

	public IDictionary<string, object> ExecuteMessage(string messageType, string apiUri, string jsonRequest, bool awsFraudEnabled)
	{
		_awsFraudEnabled = awsFraudEnabled;
		using ExecutionTimer executionTimer = new ExecutionTimer();
		Dictionary<string, object> dictionary = new Dictionary<string, object> { { "success", false } };
		try
		{
			Dictionary<string, object> dictionary2 = jsonRequest.ToObject<Dictionary<string, object>>();
			if (dictionary2.ContainsKey("Image"))
			{
				string text = dictionary2["Image"] as string;
				string arg = (string.IsNullOrEmpty(text) ? string.Empty : text.Substring(0, Math.Min(10, text.Length)));
				dictionary2["Image"] = $"{arg} ... {text?.Length ?? 0}";
			}
			string text2 = dictionary2.ToJson();
			string prettyString = PrettyPrinter.GetPrettyString(text2);
			LogHelper.Instance.Log("FraudServicesProxy JSON request: type {0}; body: {1}", messageType, (prettyString.Length <= 10000) ? prettyString : text2);
			string text3 = PerformRemoteMessage(apiUri, jsonRequest, AuthorizationKey);
			LogHelper.Instance.Log("FraudServicesProxy JSON response: type {0}; response: {1}", messageType, PrettyPrinter.GetPrettyString(text3));
			LogHelper.Instance.Log("FraudServicesProxy call execution time: {0}", executionTimer.Elapsed);
			if (!string.IsNullOrEmpty(text3))
			{
				dictionary = text3.ToDictionary();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in FraudServicesProxy.ExecuteMessage.", e);
			dictionary["ExceptionRaised"] = true;
		}
		return dictionary;
	}

	private string PerformRemoteMessage(string apiUri, string jsonRequest, string authorizationKey)
	{
		Uri result = null;
		if (Uri.TryCreate(FraudServicesBaseUri, apiUri, out result))
		{
			LogHelper.Instance.Log("FraudServicesProxy remote URL: {0}", result);
			WebRequest webRequest = WebRequest.Create(result);
			webRequest.Method = "POST";
			webRequest.ContentType = "application/json";
			webRequest.ContentLength = jsonRequest.Length;
			ServiceLocator.Instance.GetService<IConfiguration>();
			IStoreManager service = ServiceLocator.Instance.GetService<IStoreManager>();
			if (_awsFraudEnabled && service != null)
			{
				webRequest.Headers.Add("x-api-key", authorizationKey);
				webRequest.Headers.Add($"x-redbox-kioskid: {service.KioskId}");
			}
			else
			{
				webRequest.Headers.Add($"Authorization: Bearer {authorizationKey}");
			}
			LogHelper.Instance.Log("...Making remote call using HTTPS POST.");
			Stream requestStream = webRequest.GetRequestStream();
			requestStream.Write(Encoding.UTF8.GetBytes(jsonRequest), 0, jsonRequest.Length);
			requestStream.Close();
			using WebResponse webResponse = webRequest.GetResponse();
			using StreamReader streamReader = new StreamReader(webResponse.GetResponseStream());
			return HttpUtility.UrlDecode(streamReader.ReadToEnd());
		}
		LogHelper.Instance.Log($"Unable to create Uri from {FraudServicesBaseUri} and {apiUri}");
		return null;
	}

	private FraudServicesProxy()
	{
	}
}
