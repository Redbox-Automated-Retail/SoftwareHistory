using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "reset-fraud-post-statistics", Operand = "RESET-FRAUD-POST-STATISTICS")]
internal sealed class ResetFraudPostStatisticsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IDiskValidatorService service = ServiceLocator.Instance.GetService<IDiskValidatorService>();
		if (service.IsConfigured)
		{
			service.FraudSensor.ResetPostStatistics();
		}
	}

	internal ResetFraudPostStatisticsJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
