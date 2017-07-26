using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "reset-fraud-sensor", Operand = "RESET-FRAUD-SENSOR")]
internal sealed class ResetFraudSensorJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog log = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Fraud", flushOnWrite: true, "reset-fraud-sensor");
		bool flag = true;
		try
		{
			IDiskValidatorService service = ServiceLocator.Instance.GetService<IDiskValidatorService>();
			if (!service.IsConfigured)
			{
				base.Context.CreateInfoResult("FraudSensorNotConfigured", "The fraud sensor is not configured.");
				return;
			}
			if (service.FraudSensor.Reset(log) != ErrorCodes.Success)
			{
				base.Context.CreateInfoResult("FraudSensorResetError", "Unable to reset the fraud sensor.");
				return;
			}
			flag = false;
			base.Context.CreateInfoResult("FraudSensorResetSuccess", "Fraud sensor is initialized.");
		}
		finally
		{
			if (flag)
			{
				AddError("Failed to reset fraud sensor.");
			}
		}
	}

	internal ResetFraudSensorJob(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}
}
