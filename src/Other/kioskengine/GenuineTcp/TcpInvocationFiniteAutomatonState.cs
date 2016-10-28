using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.GenuineTcp;

[ComVisible(true)]
public enum TcpInvocationFiniteAutomatonState
{
	Teardown,
	ClientAvailable,
	ClientSending,
	ClientReceiving,
	ServerAwaiting,
	ServerExecution,
	ServerSending
}
