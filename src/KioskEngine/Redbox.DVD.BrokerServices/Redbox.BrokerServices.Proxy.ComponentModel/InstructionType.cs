namespace Redbox.BrokerServices.Proxy.ComponentModel;

internal enum InstructionType : byte
{
	Start = 1,
	Shutdown,
	WaitAbsolute,
	SetRequestsPerSecond,
	WaitForSuccessfulPing
}
