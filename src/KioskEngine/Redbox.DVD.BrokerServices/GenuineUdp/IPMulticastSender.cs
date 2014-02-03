using Belikov.GenuineChannels.BroadcastEngine;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.GenuineUdp;

internal class IPMulticastSender : GeneralBroadcastSender
{
	public IPMulticastSender(string court, ITransportContext iTransportContext)
		: base(court, iTransportContext)
	{
	}

	public override void SendMessage(Message message, ResultCollector resultCollector)
	{
		message.Recipient = base.ITransportContext.KnownHosts["_gbudp://" + base.Court];
		message.DestinationMarshalByRef = base.Court;
		base.ITransportContext.ConnectionManager.Send(message);
	}
}
