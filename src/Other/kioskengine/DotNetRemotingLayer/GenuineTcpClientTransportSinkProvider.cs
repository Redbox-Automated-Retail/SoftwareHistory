using System;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.DotNetRemotingLayer;

[ComVisible(true)]
public class GenuineTcpClientTransportSinkProvider : IClientChannelSinkProvider
{
	public ITransportContext ITransportContext;

	IClientChannelSinkProvider IClientChannelSinkProvider.Next
	{
		get
		{
			return null;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public GenuineTcpClientTransportSinkProvider(ITransportContext iTransportContext)
	{
		ITransportContext = iTransportContext;
	}

	IClientChannelSink IClientChannelSinkProvider.CreateSink(IChannelSender channel, string url, object remoteChannelData)
	{
		return new GenuineTcpClientTransportSink(url, ITransportContext);
	}
}
