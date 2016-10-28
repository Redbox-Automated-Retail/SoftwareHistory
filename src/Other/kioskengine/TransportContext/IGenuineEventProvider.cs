using System.Runtime.InteropServices;
using Belikov.GenuineChannels.DotNetRemotingLayer;

namespace Belikov.GenuineChannels.TransportContext;

[ComVisible(true)]
public interface IGenuineEventProvider
{
	void Register(IGenuineEventHandler iGenuineEventHandler);

	void Unregister(IGenuineEventHandler iGenuineEventHandler);

	void Fire(GenuineEventArgs genuineEventArgs);
}
