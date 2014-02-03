using Belikov.GenuineChannels.DotNetRemotingLayer;

namespace Belikov.GenuineChannels.TransportContext;

internal interface IGenuineEventHandler
{
	void OnGenuineEvent(GenuineEventArgs genuineEventArgs);
}
