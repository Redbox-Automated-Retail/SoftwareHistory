using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "fraud-sensor-post-test", Operand = "FRAUD-SENSOR-POST-TEST")]
internal sealed class FraudSensorPostJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog log = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Fraud", flushOnWrite: true, "fraud-sensor-post");
		base.Context.Send("POST Start");
		IDiskValidatorService service = ServiceLocator.Instance.GetService<IDiskValidatorService>();
		if (service.IsConfigured)
		{
			ErrorCodes errorCodes = service.FraudSensor.Reset(log);
			if (errorCodes != ErrorCodes.Success)
			{
				base.Context.Send("POST failed.");
				base.Context.CreateInfoResult("POSTFailure", $"POST failed with error = {errorCodes}.");
				AddError("POST failure.");
			}
			else
			{
				base.Context.Send("POST end.");
				base.Context.CreateInfoResult("POSTSuccess", "The POST succeeded.");
			}
		}
		else
		{
			base.Context.Send("POST failed.");
			base.Context.CreateInfoResult("POSTFailure", $"POST failed with error = {ErrorCodes.DeviceNotConfigured}.");
			AddError("POST failure.");
		}
	}

	internal FraudSensorPostJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
