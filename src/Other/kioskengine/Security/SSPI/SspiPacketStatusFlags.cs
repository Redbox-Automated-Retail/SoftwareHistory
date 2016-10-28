using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Security.SSPI;

[ComVisible(true)]
public enum SspiPacketStatusFlags
{
	InitializeFromScratch,
	ContinueAuthentication,
	ForceInitialization,
	SessionEstablished,
	ExceptionThrown
}
