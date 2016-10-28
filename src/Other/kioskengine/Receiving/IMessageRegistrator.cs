using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Receiving;

[ComVisible(true)]
public interface IMessageRegistrator
{
	bool WasRegistered(string uri, int messageId, int replyId);
}
