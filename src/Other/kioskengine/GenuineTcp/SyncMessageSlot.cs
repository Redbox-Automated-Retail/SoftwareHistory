using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Belikov.GenuineChannels.Messaging;

namespace Belikov.GenuineChannels.GenuineTcp;

[ComVisible(true)]
public class SyncMessageSlot : Message
{
	internal TcpSocketInfo TcpSocketInfo;

	public SyncMessageSlot()
	{
		Stream = Stream.Null;
		SerializedContent = Stream.Null;
		ConnectionAvailable = new ManualResetEvent(initialState: false);
	}
}
