using System;
using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Security.SSPI;

[Flags]
[ComVisible(true)]
public enum SspiFeatureFlags
{
	None = 0,
	Encryption = 1,
	Signing = 2,
	Impersonation = 4,
	Delegation = 8
}
