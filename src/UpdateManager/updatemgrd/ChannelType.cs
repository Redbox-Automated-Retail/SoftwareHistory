namespace Redbox.IPC.Framework;

internal enum ChannelType
{
	Unknown,
	Socket,
	NamedPipe,
	ActiveMQ,
	MSMQ,
	OracleMQ,
	TibcoEMS,
	WebSphereMQ,
	Remoting
}
