using Belikov.GenuineChannels.DotNetRemotingLayer;

namespace Belikov.GenuineChannels.TransportContext;

internal interface IGenuineEventProvider
{
	void Register(IGenuineEventHandler iGenuineEventHandler);

	void Unregister(IGenuineEventHandler iGenuineEventHandler);

	void Fire(GenuineEventArgs genuineEventArgs);
}
