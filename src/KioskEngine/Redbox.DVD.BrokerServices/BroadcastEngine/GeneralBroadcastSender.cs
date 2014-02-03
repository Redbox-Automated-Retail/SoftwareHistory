using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.BroadcastEngine;

internal abstract class GeneralBroadcastSender
{
	private string _court;

	private ITransportContext _iTransportContext;

	public string Court => _court;

	public ITransportContext ITransportContext => _iTransportContext;

	public GeneralBroadcastSender(string court, ITransportContext iTransportContext)
	{
		_court = court;
		_iTransportContext = iTransportContext;
	}

	public abstract void SendMessage(Message message, ResultCollector resultCollector);
}
