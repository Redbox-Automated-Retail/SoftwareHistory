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

public class KioskServicesProxy
{
	internal const int DefaultKioskServicesTimeout = 30000;

	internal const string DefaultKioskServicesUrl = "https://bl2.accessredbox.net/KioskServices/MessageService.rem";

	private HttpChannel m_channel;

	public static KioskServicesProxy Instance => Singleton<KioskServicesProxy>.Instance;

	public KioskServicesProtocol Protocol
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Store Services", "KioskServicesProtocol", KioskServicesProtocol.Remoting);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Store Services", "KioskServicesProtocol", value);
			RegisterChannel();
		}
	}

	public int Timeout
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Store Services", "KioskServicesTimeout", 30000);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Store Services", "KioskServicesTimeout", value);
			RegisterChannel();
		}
	}

	public string Url
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Store Services", "KioskServicesURL", "https://bl2.accessredbox.net/KioskServices/MessageService.rem");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Store Services", "KioskServicesURL", value);
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
		string obj = ((leading > 0) ? msgText.Substring(0, leading) : string.Empty);
		string text;
		if (trailing <= 0)
		{
			text = string.Empty;
		}
		else
		{
			text = msgText.Substring(msgText.Length - trailing, trailing);
		}
		string text2 = text;
		string text3 = new string(newChar, msgText.Length - (leading + trailing));
		return obj + text3 + text2;
	}

	public IDictionary<string, object> ExecuteMessage(string jsonRequest)
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		if (string.IsNullOrEmpty(Url) || string.IsNullOrEmpty(jsonRequest))
		{
			return new Dictionary<string, object> { ["Success"] = false };
		}
		try
		{
			Dictionary<string, object> dictionary = jsonRequest.ToObject<Dictionary<string, object>>();
			if (dictionary.ContainsKey("Username") && dictionary["Username"] != null)
			{
				dictionary.Remove("Username");
			}
			if (dictionary.ContainsKey("Password") && dictionary["Password"] != null)
			{
				dictionary.Remove("Password");
			}
			if (dictionary.ContainsKey("Email") && dictionary["Email"] != null)
			{
				dictionary["Email"] = ObfuscateMessageData(dictionary["Email"].ToString(), 'x', 1, 0);
			}
			if (dictionary.ContainsKey("CardHash") && dictionary["CardHash"] != null)
			{
				dictionary.Remove("CardHash");
			}
			if (dictionary.ContainsKey("OptInItems") && dictionary["OptInItems"] != null)
			{
				foreach (object item in dictionary["OptInItems"] as ArrayList)
				{
					Dictionary<string, object> dictionary2 = item as Dictionary<string, object>;
					if (dictionary2.ContainsKey("OptInMethodData") && dictionary2["OptInMethodData"] != null)
					{
						dictionary2["OptInMethodData"] = ObfuscateMessageData(dictionary2["OptInMethodData"].ToString(), 'x', 1, 0);
					}
				}
			}
			if (dictionary.ContainsKey("CreditCard"))
			{
				Dictionary<string, object> dictionary3 = (Dictionary<string, object>)dictionary["CreditCard"];
				dictionary3.Remove("CardId");
				dictionary3.Remove("Track2");
				dictionary3.Remove("Number");
				if (dictionary3.ContainsKey("LastFour") && dictionary3["LastFour"] != null)
				{
					dictionary3["LastFour"] = ObfuscateMessageData(dictionary3["LastFour"].ToString(), '#', 0, 1);
				}
				if (dictionary3.ContainsKey("LastName") && dictionary3["LastName"] != null)
				{
					dictionary3["LastName"] = ObfuscateMessageData(dictionary3["LastName"].ToString(), 'x', 2, 0);
				}
				if (dictionary3.ContainsKey("FirstName") && dictionary3["FirstName"] != null)
				{
					dictionary3["FirstName"] = ObfuscateMessageData(dictionary3["FirstName"].ToString(), 'x', 1, 0);
				}
				if (dictionary3.ContainsKey("PostalCode") && dictionary3["PostalCode"] != null)
				{
					dictionary3["PostalCode"] = ObfuscateMessageData(dictionary3["PostalCode"].ToString(), '#', 0, 1);
				}
				if (dictionary3.ContainsKey("ExpirationYear") && dictionary3["ExpirationYear"] != null)
				{
					dictionary3["ExpirationYear"] = ObfuscateMessageData(dictionary3["ExpirationYear"].ToString(), '#', 0, 0);
				}
				if (dictionary3.ContainsKey("ExpirationMonth") && dictionary3["ExpirationMonth"] != null)
				{
					dictionary3["ExpirationMonth"] = ObfuscateMessageData(dictionary3["ExpirationMonth"].ToString(), '#', 0, 0);
				}
			}
			string text = dictionary.ToJson();
			string prettyString = PrettyPrinter.GetPrettyString(text);
			LogHelper.Instance.Log("KioskServicesProxy JSON request: {0}", (prettyString.Length <= 10000) ? prettyString : text);
			string text2 = PerformRemoteMessage(jsonRequest);
			dictionary = text2.ToObject<Dictionary<string, object>>() ?? new Dictionary<string, object>();
			if (dictionary.ContainsKey("CustomerProfile"))
			{
				Dictionary<string, object> dictionary4 = (Dictionary<string, object>)dictionary["CustomerProfile"];
				if (dictionary4.ContainsKey("FirstName") && dictionary4["FirstName"] != null)
				{
					dictionary4["FirstName"] = ObfuscateMessageData(dictionary4["FirstName"].ToString(), 'x', 1, 0);
				}
			}
			if (dictionary.ContainsKey("Email") && dictionary["Email"] != null)
			{
				dictionary["Email"] = ObfuscateMessageData(dictionary["Email"].ToString(), 'x', 1, 0);
			}
			if (dictionary.ContainsKey("email") && dictionary["email"] != null)
			{
				dictionary["email"] = ObfuscateMessageData(dictionary["email"].ToString(), 'x', 1, 0);
			}
			if (dictionary.ContainsKey("CustomerProfileLastName") && dictionary["CustomerProfileLastName"] != null)
			{
				dictionary["CustomerProfileLastName"] = ObfuscateMessageData(dictionary["CustomerProfileLastName"].ToString(), 'x', 2, 0);
			}
			if (dictionary.ContainsKey("CustomerProfileFirstName") && dictionary["CustomerProfileFirstName"] != null)
			{
				dictionary["CustomerProfileFirstName"] = ObfuscateMessageData(dictionary["CustomerProfileFirstName"].ToString(), 'x', 1, 0);
			}
			string input = dictionary.ToJson();
			LogHelper.Instance.Log("KioskServicesProxy JSON response: {0}", PrettyPrinter.GetPrettyString(input));
			LogHelper.Instance.Log("KioskServicesProxy call execution time: {0}", executionTimer.Elapsed);
			if (!string.IsNullOrEmpty(text2))
			{
				return text2.ToDictionary();
			}
			return new Dictionary<string, object> { ["Success"] = false };
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in KioskServicesProxy.ExecuteMessage.", e);
			return new Dictionary<string, object>
			{
				["Success"] = false,
				["ExceptionRaised"] = true
			};
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
			ListDictionary properties = new ListDictionary
			{
				{ "port", 0 },
				{ "timeout", Timeout }
			};
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
			return HttpUtility.UrlDecode(streamReader.ReadToEnd());
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
