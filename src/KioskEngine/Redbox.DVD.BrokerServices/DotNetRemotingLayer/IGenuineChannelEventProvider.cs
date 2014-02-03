namespace Belikov.GenuineChannels.DotNetRemotingLayer;

internal interface IGenuineChannelEventProvider
{
	event GenuineChannelsGlobalEventHandler GenuineChannelsEvent;

	void FireGenuineEvent(GenuineEventArgs e);
}
