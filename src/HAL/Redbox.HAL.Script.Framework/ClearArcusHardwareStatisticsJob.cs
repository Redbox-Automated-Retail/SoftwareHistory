using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "clear-arcus-hardware-statistics", Operand = "CLEAR-ARCUS-HARDWARE-STATISTICS")]
internal sealed class ClearArcusHardwareStatisticsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IPersistentCounterService service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		service.RemoveAll(HardwareCorrectionStatistic.Arcus);
	}

	internal ClearArcusHardwareStatisticsJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
