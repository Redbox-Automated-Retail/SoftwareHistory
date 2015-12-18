using System;

namespace Redbox.Core;

internal static class Redbox_002EMacros1955_002ETimeSpanExtensions
{
	public static void SpinSleep(this TimeSpan timeSpan)
	{
		using Redbox_002EMacros1955_002EExecutionTimer obj = new Redbox_002EMacros1955_002EExecutionTimer();
		while ((double)obj.ElapsedMilliseconds < timeSpan.TotalMilliseconds)
		{
		}
	}
}
