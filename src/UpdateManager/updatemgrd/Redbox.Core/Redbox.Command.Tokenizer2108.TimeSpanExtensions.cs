using System;

namespace Redbox.Core;

internal static class Redbox_002ECommand_002ETokenizer2108_002ETimeSpanExtensions
{
	public static void SpinSleep(this TimeSpan timeSpan)
	{
		using Redbox_002ECommand_002ETokenizer2108_002EExecutionTimer obj = new Redbox_002ECommand_002ETokenizer2108_002EExecutionTimer();
		while ((double)obj.ElapsedMilliseconds < timeSpan.TotalMilliseconds)
		{
		}
	}
}
