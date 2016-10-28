using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Utilities;

[ComVisible(true)]
public enum GenuineThreadPoolStrategy
{
	AlwaysNative,
	OnlyLongDuration,
	SwitchAfterExhaustion,
	AlwaysThreads
}
