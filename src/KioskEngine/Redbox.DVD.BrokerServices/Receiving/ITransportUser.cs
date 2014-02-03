using Belikov.GenuineChannels.Messaging;

namespace Belikov.GenuineChannels.Receiving;

internal interface ITransportUser
{
	void HandleMessage(Message message);
}
