namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "reset-motioncontrol", Operand = "RESET-MOTIONCONTROL")]
internal sealed class ResetArcusJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		if (!base.MotionService.Reset(base.AppLog, quick: false))
		{
			base.Context.CreateInfoResult("ResetFailure", "RESET of the controller failed.");
			AddError("Reset failure.");
		}
		else
		{
			base.Context.CreateInfoResult("ResetSuccess", "RESET OK.");
		}
	}

	internal ResetArcusJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
