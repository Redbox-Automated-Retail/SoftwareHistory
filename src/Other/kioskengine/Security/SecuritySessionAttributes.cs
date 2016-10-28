using System;
using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Security;

[Flags]
[ComVisible(true)]
public enum SecuritySessionAttributes
{
	None = 0,
	EnableCompression = 1,
	ForceAsync = 0x20,
	ForceSync = 0x40,
	UseExistentConnection = 0x100
}
