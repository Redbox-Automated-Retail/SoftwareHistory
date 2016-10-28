using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.DotNetRemotingLayer;

[ComVisible(true)]
public interface IGenuineChannelEventProvider
{
	event GenuineChannelsGlobalEventHandler GenuineChannelsEvent;

	void FireGenuineEvent(GenuineEventArgs e);
}
