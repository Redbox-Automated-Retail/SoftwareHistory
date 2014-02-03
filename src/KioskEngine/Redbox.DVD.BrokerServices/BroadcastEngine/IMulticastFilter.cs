using System.Runtime.Remoting.Messaging;

namespace Belikov.GenuineChannels.BroadcastEngine;

internal interface IMulticastFilter
{
	object[] GetReceivers(object[] cachedReceiverList, IMessage iMessage);
}
