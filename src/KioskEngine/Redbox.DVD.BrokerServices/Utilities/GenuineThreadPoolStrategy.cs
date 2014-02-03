namespace Belikov.GenuineChannels.Utilities;

internal enum GenuineThreadPoolStrategy
{
	AlwaysNative,
	OnlyLongDuration,
	SwitchAfterExhaustion,
	AlwaysThreads
}
