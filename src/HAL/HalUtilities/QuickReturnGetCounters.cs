using Redbox.HAL.Client;

namespace HALUtilities;

internal sealed class QuickReturnGetCounters : JobExecutor
{
	protected override string JobName => "get-quick-return-counters";

	internal QuickReturnGetCounters(HardwareService service)
		: base(service)
	{
	}
}
