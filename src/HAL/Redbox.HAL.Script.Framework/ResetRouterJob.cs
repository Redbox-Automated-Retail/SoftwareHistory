namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "power-cycle-router")]
internal sealed class ResetRouterJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
	}

	internal ResetRouterJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
