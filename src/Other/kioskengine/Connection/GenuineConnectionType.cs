using System;
using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Connection;

[Flags]
[ComVisible(true)]
public enum GenuineConnectionType
{
	None = 0,
	Persistent = 1,
	Named = 2,
	Invocation = 4,
	All = 0xFF
}
