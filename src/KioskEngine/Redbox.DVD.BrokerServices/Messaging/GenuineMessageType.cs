namespace Belikov.GenuineChannels.Messaging;

internal enum GenuineMessageType
{
	Unknown,
	Ordinary,
	TrueBroadcast,
	BroadcastEngine,
	RedirectLogicalChannel,
	ExternalStreamConsumer,
	Custom
}
