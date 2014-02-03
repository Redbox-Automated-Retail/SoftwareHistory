using System.Threading;

namespace Belikov.GenuineChannels.Utilities;

internal class GenuineThreadPoolWorkItem
{
	public WaitCallback WorkCallback;

	public object CallbackState;

	public GenuineThreadPoolWorkItem(WaitCallback workCallback, object callbackState)
	{
		WorkCallback = workCallback;
		CallbackState = callbackState;
	}
}
