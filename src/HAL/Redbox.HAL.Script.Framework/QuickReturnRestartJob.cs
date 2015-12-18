namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "quick-return-restart", Operand = "QUICK-RETURN-RESTART")]
internal sealed class QuickReturnRestartJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		if (QuickReturnPoller.Instance.Running)
		{
			base.Context.PushTop("SUCCESS");
		}
		else
		{
			QuickReturnPoller.Instance.Start();
		}
	}

	internal QuickReturnRestartJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
