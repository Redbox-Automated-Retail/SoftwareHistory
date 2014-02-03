using System.IO;
using System.Threading;
using Belikov.GenuineChannels.Messaging;

namespace Belikov.GenuineChannels.GenuineTcp;

internal class SyncMessageSlot : Message
{
	internal TcpSocketInfo TcpSocketInfo;

	public SyncMessageSlot()
	{
		Stream = Stream.Null;
		SerializedContent = Stream.Null;
		ConnectionAvailable = new ManualResetEvent(initialState: false);
	}
}
