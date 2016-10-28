using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace Belikov.GenuineChannels.BroadcastEngine;

[ComVisible(true)]
public interface IMulticastFilter
{
	object[] GetReceivers(object[] cachedReceiverList, IMessage iMessage);
}
