using System;

namespace Belikov.GenuineChannels.Connection;

[Flags]
internal enum GenuineConnectionType
{
	None = 0,
	Persistent = 1,
	Named = 2,
	Invocation = 4,
	All = 0xFF
}
