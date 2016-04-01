using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-last-fraud-post-result", Operand = "GET-LAST-FRAUD-POST-RESULT")]
internal sealed class GetLastFraudPostJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IDiskValidatorService service = ServiceLocator.Instance.GetService<IDiskValidatorService>();
		if (service.IsConfigured)
		{
			ErrorCodes lastPostResult = service.FraudSensor.LastPostResult;
			if (lastPostResult != ErrorCodes.Success)
			{
				base.Context.CreateInfoResult("LastFraudPostFailure", $"POST failed with error = {lastPostResult}.");
				AddError("POST failure.");
				return;
			}
		}
		base.Context.CreateInfoResult("LastFraudPostOK", "The last fraud sensor POST was successful.");
	}

	internal GetLastFraudPostJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
