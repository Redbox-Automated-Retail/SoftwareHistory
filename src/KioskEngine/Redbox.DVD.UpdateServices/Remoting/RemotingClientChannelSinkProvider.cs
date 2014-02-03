using System.Runtime.Remoting.Channels;

namespace Redbox.IPC.Framework.Remoting;

internal class RemotingClientChannelSinkProvider : IClientChannelSinkProvider
{
	public IClientChannelSinkProvider Next { get; set; }

	public RemotingClientChannelSinkProvider(IClientChannelSinkProvider nextProvider)
	{
		Next = nextProvider;
	}

	public IClientChannelSink CreateSink(IChannelSender channel, string url, object remoteChannelData)
	{
		IClientChannelSink nextSink = Next.CreateSink(channel, url, remoteChannelData);
		return new Redbox.IPC.Framework.Remoting.RemotingClientSink(nextSink, remoteChannelData);
	}
}
