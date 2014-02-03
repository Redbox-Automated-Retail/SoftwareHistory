using System;
using System.Text.RegularExpressions;
using Belikov.GenuineChannels.Connection;
using Belikov.GenuineChannels.Parameters;

namespace Belikov.GenuineChannels.GenuineXHttp;

internal class XHttpConnection : GeneralConnection
{
	public string UserAgent;

	public XHttpConnectionManager XHttpConnectionManager;

	public byte[] HostId;

	public bool IsClient;

	public string ConnectionName;

	public int DbgConnectionId = ConnectionManager.GetUniqueConnectionId();

	public XHttpPhysicalConnection Listener;

	public XHttpPhysicalConnection Sender;

	public XHttpConnection(XHttpConnectionManager xHttpConnectionManager, bool isClient, string connectionName)
		: base(xHttpConnectionManager.ITransportContext)
	{
		XHttpConnectionManager = xHttpConnectionManager;
		IsClient = isClient;
		ConnectionName = connectionName;
		HostId = xHttpConnectionManager.ITransportContext.BinaryHostIdentifier;
		HostIdAsString = xHttpConnectionManager.ITransportContext.HostIdentifier;
		UserAgent = xHttpConnectionManager.ITransportContext.IParameterProvider[GenuineParameter.HttpWebUserAgent] as string;
		if (UserAgent == null)
		{
			UserAgent = string.Concat("Mozilla/4.0+ (compatible; MSIE 6.0; Windows ", Environment.OSVersion.Version, "; GXHTTP; MS .NET CLR ", Environment.Version.ToString(), ")");
		}
		UserAgent = Regex.Replace(UserAgent, "\r|\n", " ", RegexOptions.None);
	}

	public override void InternalDispose(Exception reason)
	{
		if (Listener != null)
		{
			Listener.Dispose(reason);
		}
		if (Sender != null)
		{
			Sender.Dispose(reason);
		}
	}
}
