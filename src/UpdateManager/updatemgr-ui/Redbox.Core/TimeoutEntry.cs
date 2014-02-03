using System;

namespace Redbox.Core;

internal class TimeoutEntry
{
	public DateTime Timeout { get; set; }

	public ITimeoutSink TimeoutSink { get; set; }
}
