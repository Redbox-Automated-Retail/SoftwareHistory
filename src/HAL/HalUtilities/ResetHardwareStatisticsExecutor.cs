using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace HALUtilities;

internal sealed class ResetHardwareStatisticsExecutor : JobExecutor
{
	private readonly HardwareCorrectionStatistic Stat;

	protected override string JobName => "clear-hardware-statistics";

	protected override void SetupJob(HardwareJob job)
	{
		job.Push(Stat.ToString());
	}

	internal ResetHardwareStatisticsExecutor(HardwareService service, HardwareCorrectionStatistic s)
		: base(service)
	{
		Stat = s;
	}
}
