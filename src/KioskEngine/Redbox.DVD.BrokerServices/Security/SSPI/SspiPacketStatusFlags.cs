namespace Belikov.GenuineChannels.Security.SSPI;

internal enum SspiPacketStatusFlags
{
	InitializeFromScratch,
	ContinueAuthentication,
	ForceInitialization,
	SessionEstablished,
	ExceptionThrown
}
