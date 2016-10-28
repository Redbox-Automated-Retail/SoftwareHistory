using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.BroadcastEngine;

[ComVisible(true)]
public interface IBroadcastSenderProvider
{
	GeneralBroadcastSender GetBroadcastSender(string court);
}
