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
		return new RemotingClientSink(nextSink, remoteChannelData);
	}
}
