namespace Belikov.GenuineChannels.BroadcastEngine;

internal interface IBroadcastSenderProvider
{
	GeneralBroadcastSender GetBroadcastSender(string court);
}
