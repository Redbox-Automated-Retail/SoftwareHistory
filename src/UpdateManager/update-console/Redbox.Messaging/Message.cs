using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Redbox.Core;

namespace Redbox.Messaging;

internal class Message : IMessage
{
	public const string SenderIPAddress = "SenderIPAddress";

	public const string SenderHostName = "SenderHostName";

	private static string m_hostname;

	private static string m_ipaddress;

	private byte[] m_messageData;

	private readonly Dictionary<string, string> m_properties = new Dictionary<string, string>();

	public byte[] MessageData
	{
		get
		{
			return m_messageData;
		}
		set
		{
			m_messageData = value;
		}
	}

	public string Data
	{
		get
		{
			return (m_messageData == null) ? string.Empty : Encoding.UTF8.GetString(m_messageData);
		}
		set
		{
			m_messageData = (string.IsNullOrEmpty(value) ? null : Encoding.UTF8.GetBytes(value));
		}
	}

	public byte[] DataAndPropertiesBytes
	{
		get
		{
			MemoryStream memoryStream = new MemoryStream();
			memoryStream.Write(MessageData, 0, MessageData.Length);
			byte[] propertyBytes = PropertyBytes;
			memoryStream.Write(propertyBytes, 0, propertyBytes.Length);
			return StreamExtensions.GetBytes(memoryStream);
		}
		set
		{
			DataAndPropertiesString = ((value == null) ? string.Empty : Encoding.UTF8.GetString(value));
		}
	}

	public string DataAndPropertiesString
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder(Data);
			stringBuilder.Append(PropertiesAsJson);
			return stringBuilder.ToString();
		}
		set
		{
			Properties.Clear();
			int num = value.IndexOf("{\"INTERNALPROPERTIES\"");
			if (num > -1)
			{
				Data = value.Substring(0, num);
				string json = value.Substring(num);
				if (StringExtensions.ToDictionary(json)["INTERNALPROPERTIES"] is Dictionary<string, object> items)
				{
					LinqExtensions.ForEach(items, delegate(KeyValuePair<string, object> pair)
					{
						Properties.Add(pair.Key, pair.Value.ToString());
					});
				}
			}
			else
			{
				Data = value;
			}
		}
	}

	public string MessageId { get; set; }

	public string CorrelationId { get; set; }

	public MessageType MessageType { get; set; }

	public Dictionary<string, string> Properties
	{
		get
		{
			return m_properties;
		}
		set
		{
			m_properties.Clear();
			if (value != null)
			{
				LinqExtensions.ForEach(value, delegate(KeyValuePair<string, string> pair)
				{
					m_properties.Add(pair.Key, pair.Value);
				});
			}
		}
	}

	private string PropertiesAsJson
	{
		get
		{
			var instance = new
			{
				INTERNALPROPERTIES = Properties
			};
			return ObjectExtensions.ToJson(instance);
		}
	}

	private byte[] PropertyBytes
	{
		get
		{
			return Encoding.UTF8.GetBytes(PropertiesAsJson);
		}
		set
		{
			string json = ((value == null) ? string.Empty : Encoding.UTF8.GetString(value));
			Properties.Clear();
			if (StringExtensions.ToDictionary(json)["INTERNALPROPERTIES"] is Dictionary<string, object> items)
			{
				LinqExtensions.ForEach(items, delegate(KeyValuePair<string, object> pair)
				{
					Properties.Add(pair.Key, pair.Value.ToString());
				});
			}
		}
	}

	private string IPAddress
	{
		get
		{
			if (string.IsNullOrEmpty(m_ipaddress))
			{
				m_ipaddress = IPAddressHelper.GetAddressForHostName(HostName).ToString();
			}
			return m_ipaddress;
		}
	}

	private string HostName
	{
		get
		{
			if (string.IsNullOrEmpty(m_hostname))
			{
				m_hostname = Dns.GetHostName();
			}
			return m_hostname;
		}
	}

	public void AddAdditionalProperties()
	{
		if (!Properties.ContainsKey("SenderHostName"))
		{
			Properties.Add("SenderHostName", HostName);
		}
		if (!Properties.ContainsKey("SenderIPAddress"))
		{
			Properties.Add("SenderIPAddress", IPAddress);
		}
	}
}
