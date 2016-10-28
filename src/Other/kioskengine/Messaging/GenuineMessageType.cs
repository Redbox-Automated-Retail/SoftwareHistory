using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Messaging;

[ComVisible(true)]
public enum GenuineMessageType
{
	Unknown,
	Ordinary,
	TrueBroadcast,
	BroadcastEngine,
	RedirectLogicalChannel,
	ExternalStreamConsumer,
	Custom
}
