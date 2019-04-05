using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "clear-hardware-statistics")]
internal sealed class ClearHardwareStatisticsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		string value = base.Context.PopTop<string>();
		HardwareCorrectionStatistic hardwareCorrectionStatistic = Enum<HardwareCorrectionStatistic>.ParseIgnoringCase(value, HardwareCorrectionStatistic.None);
		if (hardwareCorrectionStatistic != HardwareCorrectionStatistic.None)
		{
			IHardwareCorrectionStatisticService service = ServiceLocator.Instance.GetService<IHardwareCorrectionStatisticService>();
			service.RemoveAll(hardwareCorrectionStatistic);
		}
	}

	internal ClearHardwareStatisticsJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
