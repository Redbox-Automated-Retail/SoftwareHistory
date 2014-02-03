using System;

namespace Belikov.GenuineChannels.Security;

[Flags]
internal enum SecuritySessionAttributes
{
	None = 0,
	EnableCompression = 1,
	ForceAsync = 0x20,
	ForceSync = 0x40,
	UseExistentConnection = 0x100
}
