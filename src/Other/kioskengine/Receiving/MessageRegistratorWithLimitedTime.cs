using System.Runtime.InteropServices;
using Belikov.GenuineChannels.BroadcastEngine;

namespace Belikov.GenuineChannels.Receiving;

[ComVisible(true)]
public class MessageRegistratorWithLimitedTime : IMessageRegistrator
{
	public bool WasRegistered(string uri, int messageId, int replyId)
	{
		return UniqueCallTracer.Instance.WasGuidRegistered("_GRH_/" + uri + "/" + messageId + "/" + replyId);
	}
}
