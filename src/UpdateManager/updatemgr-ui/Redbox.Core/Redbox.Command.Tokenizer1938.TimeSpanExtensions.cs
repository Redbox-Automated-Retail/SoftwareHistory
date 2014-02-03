using System;

namespace Redbox.Core;

internal static class Redbox_002ECommand_002ETokenizer1938_002ETimeSpanExtensions
{
	public static void SpinSleep(this TimeSpan timeSpan)
	{
		using Redbox_002ECommand_002ETokenizer1938_002EExecutionTimer obj = new Redbox_002ECommand_002ETokenizer1938_002EExecutionTimer();
		while ((double)obj.ElapsedMilliseconds < timeSpan.TotalMilliseconds)
		{
		}
	}
}
