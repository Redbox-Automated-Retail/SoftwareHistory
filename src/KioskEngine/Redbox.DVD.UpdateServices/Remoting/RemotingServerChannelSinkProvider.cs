using System.Runtime.Remoting.Channels;

namespace Redbox.IPC.Framework.Remoting;

internal class RemotingServerChannelSinkProvider : IServerChannelSinkProvider
{
	public IServerChannelSinkProvider Next { get; set; }

	private Redbox.IPC.Framework.Remoting.RemotingServiceHost RemotingHost { get; set; }

	public RemotingServerChannelSinkProvider(Redbox.IPC.Framework.Remoting.RemotingServiceHost host, IServerFormatterSinkProvider next)
	{
		Next = next;
		RemotingHost = host;
	}

	public void GetChannelData(IChannelDataStore channelData)
	{
	}

	public IServerChannelSink CreateSink(IChannelReceiver channel)
	{
		IServerChannelSink nextSink = Next.CreateSink(channel);
		return new Redbox.IPC.Framework.Remoting.RemotingServerSink(RemotingHost, nextSink);
	}
}
