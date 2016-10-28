using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using Redbox.Core;

namespace Redbox.IPC.Framework;

public class Redbox_002EIPC_002EFramework1802119_002EIPCProtocol
{
	private string m_rawUri;

	private List<string> m_properties;

	private Dictionary<string, string> m_query;

	public List<string> Properties
	{
		get
		{
			if (m_properties == null)
			{
				m_properties = new List<string>();
			}
			return m_properties;
		}
	}

	public bool IsSecure { get; internal set; }

	public Redbox_002EIPC_002EFramework1802119_002EChannelType Channel { get; internal set; }

	public string Host { get; private set; }

	public string Port { get; private set; }

	public string Scheme { get; private set; }

	public string UserName { get; private set; }

	public string Password { get; private set; }

	public Dictionary<string, string> Query
	{
		get
		{
			if (m_query == null)
			{
				m_query = new Dictionary<string, string>();
			}
			return m_query;
		}
		set
		{
			if (value == null)
			{
				m_query = null;
				return;
			}
			Query.Clear();
			value.ForEach(delegate(KeyValuePair<string, string> pair)
			{
				Query.Add(pair.Key, pair.Value);
			});
		}
	}

	public static bool Validate(string uri)
	{
		try
		{
			Parse(uri);
			return true;
		}
		catch (UriFormatException ex)
		{
			Console.WriteLine("Caught URIformatException: {0}", ex.Message);
			return false;
		}
		catch (Exception ex2)
		{
			Console.WriteLine("Caught Exception: {0}", ex2.Message);
			return false;
		}
	}

	public static Redbox_002EIPC_002EFramework1802119_002EIPCProtocol Parse(string protocolURI)
	{
		protocolURI = protocolURI.Trim();
		Redbox_002EIPC_002EFramework1802119_002EIPCProtocol obj = new Redbox_002EIPC_002EFramework1802119_002EIPCProtocol
		{
			m_rawUri = protocolURI
		};
		try
		{
			obj.Read(protocolURI);
		}
		catch (Exception ex)
		{
			throw new UriFormatException(ex.Message);
		}
		if (string.IsNullOrEmpty(obj.Host))
		{
			throw new UriFormatException("Host isn't set.");
		}
		if (obj.Channel == Redbox_002EIPC_002EFramework1802119_002EChannelType.Unknown)
		{
			throw new UriFormatException("The channel type is unknown; please correct your URI.");
		}
		if ((obj.Channel == Redbox_002EIPC_002EFramework1802119_002EChannelType.Socket || obj.Channel == Redbox_002EIPC_002EFramework1802119_002EChannelType.Remoting) && !string.IsNullOrEmpty(obj.Port) && !int.TryParse(obj.Port, out var _))
		{
			throw new UriFormatException($"Protocol is set up for {obj.Channel}, but port isn't a valid number.");
		}
		return obj;
	}

	public override string ToString()
	{
		return m_rawUri;
	}

	private void Read(string protocolURI)
	{
		protocolURI = protocolURI.Replace(" ", "");
		Uri uri = new Uri(protocolURI);
		m_rawUri = protocolURI;
		Host = uri.Host;
		if (uri.Port != -1)
		{
			Port = uri.Port.ToString();
		}
		if (!string.IsNullOrEmpty(uri.Query))
		{
			NameValueCollection queries = HttpUtility.ParseQueryString(uri.Query);
			queries.AllKeys.ForEach(delegate(string key)
			{
				Query.Add(key, queries[key]);
			});
		}
		SetChannel(uri);
		Scheme = uri.Scheme;
		if (Query.ContainsKey("USERNAME"))
		{
			UserName = Query["USERNAME"];
		}
		if (Query.ContainsKey("PASSWORD"))
		{
			Password = Query["PASSWORD"];
		}
		int count = 0;
		if (uri.Segments.Length == 0)
		{
			return;
		}
		uri.Segments.ForEach(delegate(string s)
		{
			s = s.Replace("/", "").Replace(" ", "");
			if (count == 0)
			{
				if (!string.IsNullOrEmpty(s))
				{
					Properties.Add(s);
				}
			}
			else
			{
				Properties.Add(s);
			}
			count++;
		});
	}

	private void SetChannel(Uri uri)
	{
		Channel = Redbox_002EIPC_002EFramework1802119_002EChannelType.Unknown;
		IsSecure = false;
		if (uri.Scheme.Equals("rcp", StringComparison.CurrentCultureIgnoreCase))
		{
			Channel = Redbox_002EIPC_002EFramework1802119_002EChannelType.Socket;
		}
		else if (uri.Scheme.Equals("srcp", StringComparison.CurrentCultureIgnoreCase))
		{
			Channel = Redbox_002EIPC_002EFramework1802119_002EChannelType.Socket;
			IsSecure = true;
		}
		else if (uri.Scheme.Equals("rcp-p", StringComparison.CurrentCultureIgnoreCase))
		{
			Channel = Redbox_002EIPC_002EFramework1802119_002EChannelType.NamedPipe;
		}
		else if (uri.Scheme.Equals("srcp-p", StringComparison.CurrentCultureIgnoreCase))
		{
			Channel = Redbox_002EIPC_002EFramework1802119_002EChannelType.NamedPipe;
			IsSecure = true;
		}
		else if (uri.Scheme.Equals("rcp-amq", StringComparison.CurrentCultureIgnoreCase))
		{
			Channel = Redbox_002EIPC_002EFramework1802119_002EChannelType.ActiveMQ;
		}
		else if (uri.Scheme.Equals("rcp-msmq", StringComparison.CurrentCultureIgnoreCase))
		{
			Channel = Redbox_002EIPC_002EFramework1802119_002EChannelType.MSMQ;
		}
		else if (uri.Scheme.Equals("rcp-tems", StringComparison.CurrentCultureIgnoreCase))
		{
			Channel = Redbox_002EIPC_002EFramework1802119_002EChannelType.TibcoEMS;
		}
		else if (uri.Scheme.Equals("rcp-oaq", StringComparison.CurrentCultureIgnoreCase))
		{
			Channel = Redbox_002EIPC_002EFramework1802119_002EChannelType.OracleMQ;
		}
		else if (uri.Scheme.Equals("rcp-wmq", StringComparison.CurrentCultureIgnoreCase))
		{
			Channel = Redbox_002EIPC_002EFramework1802119_002EChannelType.WebSphereMQ;
		}
		else if (uri.Scheme.Equals("rcp-rem", StringComparison.CurrentCultureIgnoreCase))
		{
			Channel = Redbox_002EIPC_002EFramework1802119_002EChannelType.Remoting;
		}
		else if (uri.Scheme.Equals("srcp-rem", StringComparison.CurrentCultureIgnoreCase))
		{
			Channel = Redbox_002EIPC_002EFramework1802119_002EChannelType.Remoting;
			IsSecure = true;
		}
	}
}
