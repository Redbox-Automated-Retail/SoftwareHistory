using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.FraudServices.Proxy;

public class FraudServicesProxy
{
	private delegate string PerformRemoteMessage(string apiUri, string body);

	public static FraudServicesProxy Instance => Singleton<FraudServicesProxy>.Instance;

	private Uri OldFraudServicesBaseUri => MakeUri(ServiceLocator.Instance.GetService<IConfiguration>()?.FraudServicesUrl);

	private Uri AwsFraudServicesBaseUri => MakeUri(ServiceLocator.Instance.GetService<IConfiguration>()?.AwsFraudServicesUrl);

	private string OldAuthorizationKey => ServiceLocator.Instance.GetService<IConfiguration>()?.FraudServicesKey;

	private string AwsAuthorizationKey => ServiceLocator.Instance.GetService<IConfiguration>()?.AwsFraudServicesKey;

	public IDictionary<string, object> OldBarcodeTracking(byte[] payload)
	{
		return ExecuteMessage("BarcodeTracking", "api/Fraud/addbarcodetracking", payload, PerformOldRemoteMessage);
	}

	public IDictionary<string, object> AwsBarcodeTracking(byte[] payload)
	{
		return ExecuteMessage("BarcodeTracking", "api/frauddata/BarcodeTracking", payload, PerformAwsRemoteMessage);
	}

	public IDictionary<string, object> OldUploadDiscImage(byte[] payload)
	{
		return ExecuteMessage("UploadDiscImage", "api/Fraud/discimage", payload, PerformOldRemoteMessage);
	}

	public IDictionary<string, object> AwsUploadDiscImage(byte[] payload)
	{
		return ExecuteMessage("UploadDiscImage", "api/frauddata/DiscImage", payload, PerformAwsRemoteMessage);
	}

	private IDictionary<string, object> ExecuteMessage(string messageType, string apiUri, byte[] payload, PerformRemoteMessage caller)
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		Dictionary<string, object> dictionary = new Dictionary<string, object> { { "success", false } };
		try
		{
			string text = Encoding.ASCII.GetString(payload);
			Dictionary<string, object> dictionary2 = text.ToObject<Dictionary<string, object>>();
			if (dictionary2.ContainsKey("Image"))
			{
				string text2 = dictionary2["Image"] as string;
				string arg = (string.IsNullOrEmpty(text2) ? string.Empty : text2.Substring(0, Math.Min(10, text2.Length)));
				dictionary2["Image"] = $"{arg} ... {text2?.Length ?? 0}";
			}
			string text3 = dictionary2.ToJson();
			string text4 = text3.ToPrettyJson();
			LogHelper.Instance.Log("FraudServicesProxy JSON request: type " + messageType + "; body: " + ((text4.Length <= 10000) ? text4 : text3));
			string text5 = caller(apiUri, text);
			LogHelper.Instance.Log("FraudServicesProxy JSON response: type " + messageType + "; response: " + text5.ToPrettyJson());
			LogHelper.Instance.Log($"FraudServicesProxy call execution time: {executionTimer.Elapsed}");
			if (!string.IsNullOrEmpty(text5))
			{
				dictionary = text5.ToDictionary();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in FraudServicesProxy.ExecuteMessage.", e);
			dictionary["ExceptionRaised"] = true;
		}
		return dictionary;
	}

	private string PerformOldRemoteMessage(string apiUri, string body)
	{
		if (Uri.TryCreate(OldFraudServicesBaseUri, apiUri, out var result))
		{
			LogHelper.Instance.Log("FraudServicesProxy remote URL: {0}", result);
			WebRequest webRequest = WebRequest.Create(result);
			webRequest.Method = "POST";
			webRequest.ContentType = "application/json";
			webRequest.ContentLength = body.Length;
			webRequest.Headers.Add("Authorization: Bearer " + OldAuthorizationKey);
			return MakeCall(webRequest, body);
		}
		LogHelper.Instance.Log($"Unable to create Uri from {OldFraudServicesBaseUri} and {apiUri}");
		return null;
	}

	private string PerformAwsRemoteMessage(string apiUri, string body)
	{
		if (Uri.TryCreate(AwsFraudServicesBaseUri, apiUri, out var result))
		{
			LogHelper.Instance.Log("FraudServicesProxy remote URL: {0}", result);
			WebRequest webRequest = WebRequest.Create(result);
			webRequest.Method = "POST";
			webRequest.ContentType = "application/json";
			webRequest.ContentLength = body.Length;
			webRequest.Headers.Add("x-api-key", AwsAuthorizationKey);
			webRequest.Headers.Add($"x-redbox-kioskid: {ServiceLocator.Instance.GetService<IStoreManager>()?.KioskId}");
			return MakeCall(webRequest, body);
		}
		LogHelper.Instance.Log($"Unable to create Uri from {AwsFraudServicesBaseUri} and {apiUri}");
		return null;
	}

	private FraudServicesProxy()
	{
	}

	private Uri MakeUri(string url)
	{
		if (url != null)
		{
			return new Uri(url?.TrimEnd('/') + "/");
		}
		return null;
	}

	private string MakeCall(WebRequest request, string payload)
	{
		LogHelper.Instance.Log("...Making remote call using HTTPS POST.");
		Stream requestStream = request.GetRequestStream();
		requestStream.Write(Encoding.UTF8.GetBytes(payload), 0, payload.Length);
		requestStream.Close();
		using WebResponse webResponse = request.GetResponse();
		using StreamReader streamReader = new StreamReader(webResponse.GetResponseStream());
		return HttpUtility.UrlDecode(streamReader.ReadToEnd());
	}
}
