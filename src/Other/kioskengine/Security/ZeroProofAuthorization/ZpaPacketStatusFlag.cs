using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Security.ZeroProofAuthorization;

[ComVisible(true)]
public enum ZpaPacketStatusFlag
{
	ForceInitialization = 15,
	Salt = 20,
	HashedPassword = 25,
	SessionEstablished = 30,
	ExceptionThrown = 35
}
