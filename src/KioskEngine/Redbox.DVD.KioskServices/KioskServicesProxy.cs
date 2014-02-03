using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Http;
using System.Text;
using System.Web;
using Redbox.Core;
using Redbox.JSONPrettyPrinter;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.ComponentModel;
using Redbox.REDS.Framework;

namespace Redbox.KioskServices.Proxy;

internal class KioskServicesProxy
{
	internal const int DefaultKioskServicesTimeout = 30000;

	internal const string DefaultKioskServicesUrl = "https://bl2.accessredbox.net/KioskServices/MessageService.rem";

	private HttpChannel m_channel;

	public static KioskServicesProxy Instance => Singleton<KioskServicesProxy>.Instance;

	public KioskServicesProtocol Protocol
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Store Services", "KioskServicesProtocol", KioskServicesProtocol.Remoting);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Store Services", "KioskServicesProtocol", value);
			RegisterChannel();
		}
	}

	public int Timeout
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Store Services", "KioskServicesTimeout", 30000);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Store Services", "KioskServicesTimeout", value);
			RegisterChannel();
		}
	}

	public string Url
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Store Services", "KioskServicesURL", "https://bl2.accessredbox.net/KioskServices/MessageService.rem");
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Store Services", "KioskServicesURL", value);
		}
	}

	public IDictionary<string, object> ExecuteMessage(object message)
	{
		return ExecuteMessage(message.ToJson());
	}

	private string ObfuscateMessageData(string msgText, char newChar, int leading, int trailing)
	{
		if (leading + trailing > msgText.Length)
		{
			leading = 0;
			trailing = 0;
		}
		string text = ((leading > 0) ? msgText.Substring(0, leading) : string.Empty);
		string text2 = ((trailing > 0) ? msgText.Substring(msgText.Length - trailing, trailing) : string.Empty);
		string text3 = new string(newChar, msgText.Length - (leading + trailing));
		return text + text3 + text2;
	}

	public IDictionary<string, object> ExecuteMessage(string jsonRequest)
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		if (string.IsNullOrEmpty(Url) || string.IsNullOrEmpty(jsonRequest))
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["Success"] = false;
			return dictionary;
		}
		try
		{
			Dictionary<string, object> dictionary2 = jsonRequest.ToObject<Dictionary<string, object>>();
			if (dictionary2.ContainsKey("Password") && dictionary2["Password"] != null)
			{
				dictionary2.Remove("Password");
			}
			if (dictionary2.ContainsKey("Email") && dictionary2["Email"] != null)
			{
				dictionary2["Email"] = ObfuscateMessageData(dictionary2["Email"].ToString(), 'x', 1, 0);
			}
			if (dictionary2.ContainsKey("CardHash") && dictionary2["CardHash"] != null)
			{
				dictionary2.Remove("CardHash");
			}
			if (dictionary2.ContainsKey("OptInItems") && dictionary2["OptInItems"] != null)
			{
				ArrayList arrayList = dictionary2["OptInItems"] as ArrayList;
				foreach (object item in arrayList)
				{
					Dictionary<string, object> dictionary3 = item as Dictionary<string, object>;
					if (dictionary3.ContainsKey("OptInMethodData") && dictionary3["OptInMethodData"] != null)
					{
						dictionary3["OptInMethodData"] = ObfuscateMessageData(dictionary3["OptInMethodData"].ToString(), 'x', 1, 0);
					}
				}
			}
			if (dictionary2.ContainsKey("CreditCard"))
			{
				Dictionary<string, object> dictionary4 = (Dictionary<string, object>)dictionary2["CreditCard"];
				dictionary4.Remove("CardId");
				dictionary4.Remove("Track2");
				dictionary4.Remove("Number");
				if (dictionary4.ContainsKey("LastFour") && dictionary4["LastFour"] != null)
				{
					dictionary4["LastFour"] = ObfuscateMessageData(dictionary4["LastFour"].ToString(), '#', 0, 1);
				}
				if (dictionary4.ContainsKey("LastName") && dictionary4["LastName"] != null)
				{
					dictionary4["LastName"] = ObfuscateMessageData(dictionary4["LastName"].ToString(), 'x', 2, 0);
				}
				if (dictionary4.ContainsKey("FirstName") && dictionary4["FirstName"] != null)
				{
					dictionary4["FirstName"] = ObfuscateMessageData(dictionary4["FirstName"].ToString(), 'x', 1, 0);
				}
				if (dictionary4.ContainsKey("PostalCode") && dictionary4["PostalCode"] != null)
				{
					dictionary4["PostalCode"] = ObfuscateMessageData(dictionary4["PostalCode"].ToString(), '#', 0, 1);
				}
				if (dictionary4.ContainsKey("ExpirationYear") && dictionary4["ExpirationYear"] != null)
				{
					dictionary4["ExpirationYear"] = ObfuscateMessageData(dictionary4["ExpirationYear"].ToString(), '#', 0, 0);
				}
				if (dictionary4.ContainsKey("ExpirationMonth") && dictionary4["ExpirationMonth"] != null)
				{
					dictionary4["ExpirationMonth"] = ObfuscateMessageData(dictionary4["ExpirationMonth"].ToString(), '#', 0, 0);
				}
			}
			string text = dictionary2.ToJson();
			string prettyString = PrettyPrinter.GetPrettyString(text);
			LogHelper.Instance.Log("KioskServicesProxy JSON request: {0}", (prettyString.Length <= 10000) ? prettyString : text);
			string text2 = PerformRemoteMessage(jsonRequest);
			dictionary2 = text2.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>();
			if (dictionary2.ContainsKey("CustomerProfile"))
			{
				Dictionary<string, object> dictionary5 = (Dictionary<string, object>)dictionary2["CustomerProfile"];
				if (dictionary5.ContainsKey("FirstName") && dictionary5["FirstName"] != null)
				{
					dictionary5["FirstName"] = ObfuscateMessageData(dictionary5["FirstName"].ToString(), 'x', 1, 0);
				}
			}
			if (dictionary2.ContainsKey("Email") && dictionary2["Email"] != null)
			{
				dictionary2["Email"] = ObfuscateMessageData(dictionary2["Email"].ToString(), 'x', 1, 0);
			}
			if (dictionary2.ContainsKey("email") && dictionary2["email"] != null)
			{
				dictionary2["email"] = ObfuscateMessageData(dictionary2["email"].ToString(), 'x', 1, 0);
			}
			if (dictionary2.ContainsKey("CustomerProfileLastName") && dictionary2["CustomerProfileLastName"] != null)
			{
				dictionary2["CustomerProfileLastName"] = ObfuscateMessageData(dictionary2["CustomerProfileLastName"].ToString(), 'x', 2, 0);
			}
			if (dictionary2.ContainsKey("CustomerProfileFirstName") && dictionary2["CustomerProfileFirstName"] != null)
			{
				dictionary2["CustomerProfileFirstName"] = ObfuscateMessageData(dictionary2["CustomerProfileFirstName"].ToString(), 'x', 1, 0);
			}
			string input = dictionary2.ToJson();
			LogHelper.Instance.Log("KioskServicesProxy JSON response: {0}", PrettyPrinter.GetPrettyString(input));
			LogHelper.Instance.Log("KioskServicesProxy call execution time: {0}", executionTimer.Elapsed);
			if (!string.IsNullOrEmpty(text2))
			{
				return text2.ToDictionary();
			}
			Dictionary<string, object> dictionary6 = new Dictionary<string, object>();
			dictionary6["Success"] = false;
			return dictionary6;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in KioskServicesProxy.ExecuteMessage.", e);
			Dictionary<string, object> dictionary7 = new Dictionary<string, object>();
			dictionary7["Success"] = false;
			dictionary7["ExceptionRaised"] = true;
			return dictionary7;
		}
	}

	public Dictionary<string, string> GetCurrentVersions()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		if (service != null)
		{
			dictionary["Kiosk Engine"] = service["EngineVersion"];
			dictionary["Hal Service"] = service["HALServiceVersion"] ?? "Not Found";
			dictionary["Hal Client"] = service["HALVersion"] ?? "Not Found";
		}
		IKernelExtensionService service2 = ServiceLocator.Instance.GetService<IKernelExtensionService>();
		if (service2 != null)
		{
			foreach (IKernelExtension extension in service2.Extensions)
			{
				dictionary[extension.Name] = extension.Version;
			}
		}
		IResourceBundleService service3 = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service3 != null)
		{
			foreach (IBundleSpecifier bundle in service3.Bundles)
			{
				dictionary[bundle.Name] = bundle.Version.ToString();
			}
		}
		return dictionary;
	}

	internal void RegisterChannel()
	{
		if (m_channel != null)
		{
			UnregisterChannel();
			LogHelper.Instance.Log("Unregistering remoting channel");
		}
		if (Protocol == KioskServicesProtocol.Remoting)
		{
			ListDictionary listDictionary = new ListDictionary();
			listDictionary.Add("port", 0);
			listDictionary.Add("timeout", Timeout);
			ListDictionary properties = listDictionary;
			LogHelper.Instance.Log("Registering remoting channel with timeout {0}", Timeout);
			m_channel = new HttpChannel(properties, new BinaryClientFormatterSinkProvider(), new BinaryServerFormatterSinkProvider());
			ChannelServices.RegisterChannel(m_channel, ensureSecurity: false);
		}
	}

	private KioskServicesProxy()
	{
		RegisterChannel();
	}

	private string PerformRemoteMessage(string jsonRequest)
	{
		LogHelper.Instance.Log("KioskServicesProxy remote URL: {0}", Url);
		if (Protocol == KioskServicesProtocol.REST)
		{
			string arg = HttpUtility.UrlEncode(jsonRequest);
			string text = $"payload={arg}";
			WebRequest webRequest = WebRequest.Create(Url);
			webRequest.Method = "POST";
			webRequest.ContentType = "application/x-www-form-urlencoded";
			webRequest.ContentLength = text.Length;
			LogHelper.Instance.Log("...Making remote call using HTTPS POST.");
			Stream requestStream = webRequest.GetRequestStream();
			requestStream.Write(Encoding.UTF8.GetBytes(text), 0, text.Length);
			requestStream.Close();
			using WebResponse webResponse = webRequest.GetResponse();
			using StreamReader streamReader = new StreamReader(webResponse.GetResponseStream());
			string str = streamReader.ReadToEnd();
			return HttpUtility.UrlDecode(str);
		}
		IMessageService messageService = (IMessageService)Activator.GetObject(typeof(IMessageService), Url);
		if (messageService == null)
		{
			LogHelper.Instance.Log("...KioskServicesProxy.PerformRemoteMessage unable to connect.", LogEntryType.Error);
			return null;
		}
		LogHelper.Instance.Log("...Making remote call using .NET Remoting channel.");
		return messageService.Process(jsonRequest);
	}

	private void UnregisterChannel()
	{
		if (m_channel != null || Protocol == KioskServicesProtocol.Remoting)
		{
			ChannelServices.UnregisterChannel(m_channel);
			m_channel = null;
		}
	}
}
