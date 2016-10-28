using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Connection;

[ComVisible(true)]
public enum GenuinePersistentConnectionState
{
	NotEstablished,
	Opened,
	Accepted
}
