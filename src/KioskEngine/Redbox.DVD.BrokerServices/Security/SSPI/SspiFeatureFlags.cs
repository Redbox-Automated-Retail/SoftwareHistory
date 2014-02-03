using System;

namespace Belikov.GenuineChannels.Security.SSPI;

[Flags]
internal enum SspiFeatureFlags
{
	None = 0,
	Encryption = 1,
	Signing = 2,
	Impersonation = 4,
	Delegation = 8
}
