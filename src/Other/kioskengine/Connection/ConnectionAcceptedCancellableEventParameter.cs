using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Connection;

[ComVisible(true)]
public class ConnectionAcceptedCancellableEventParameter
{
	public bool Cancel = false;

	public Socket Socket;

	public IPEndPoint IPEndPoint;
}
