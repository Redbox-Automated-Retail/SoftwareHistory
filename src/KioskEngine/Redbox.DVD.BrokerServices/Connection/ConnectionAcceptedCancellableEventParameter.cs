using System.Net;
using System.Net.Sockets;

namespace Belikov.GenuineChannels.Connection;

internal class ConnectionAcceptedCancellableEventParameter
{
	public bool Cancel = false;

	public Socket Socket;

	public IPEndPoint IPEndPoint;
}
