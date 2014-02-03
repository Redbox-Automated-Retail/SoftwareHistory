using System.Net.Sockets;

namespace Belikov.GenuineChannels.GenuineTcp;

internal interface IAcceptConnectionConsumer
{
	bool IsDisposed();

	void AcceptConnection(Socket socket);
}
