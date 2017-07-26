using Redbox.HAL.Client;

namespace HALUtilities;

internal sealed class QuickReturnResetCounters : JobExecutor
{
	protected override string JobName => "reset-quick-return-counters";

	internal QuickReturnResetCounters(HardwareService service)
		: base(service)
	{
	}
}
