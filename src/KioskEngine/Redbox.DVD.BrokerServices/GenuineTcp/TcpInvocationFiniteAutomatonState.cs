namespace Belikov.GenuineChannels.GenuineTcp;

internal enum TcpInvocationFiniteAutomatonState
{
	Teardown,
	ClientAvailable,
	ClientSending,
	ClientReceiving,
	ServerAwaiting,
	ServerExecution,
	ServerSending
}
