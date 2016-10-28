using System.Runtime.InteropServices;
using Belikov.GenuineChannels.DotNetRemotingLayer;

namespace Belikov.GenuineChannels.TransportContext;

[ComVisible(true)]
public interface IGenuineEventHandler
{
	void OnGenuineEvent(GenuineEventArgs genuineEventArgs);
}
