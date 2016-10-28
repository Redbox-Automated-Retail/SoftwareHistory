namespace Redbox.BrokerServices.Proxy.ComponentModel;

public enum InstructionType : byte
{
	Start = 1,
	Shutdown,
	WaitAbsolute,
	SetRequestsPerSecond,
	WaitForSuccessfulPing
}
