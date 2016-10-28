using System.Runtime.InteropServices;
using Belikov.GenuineChannels.Messaging;

namespace Belikov.GenuineChannels.Receiving;

[ComVisible(true)]
public interface ITransportUser
{
	void HandleMessage(Message message);
}
