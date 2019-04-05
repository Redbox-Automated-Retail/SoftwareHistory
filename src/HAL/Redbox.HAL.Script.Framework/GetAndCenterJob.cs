namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-and-center")]
internal sealed class GetAndCenterJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		GetAndCenterOperation getAndCenterOperation = new GetAndCenterOperation(base.Context);
		if (!getAndCenterOperation.Run())
		{
			AddError("get-and-center failed.");
		}
	}

	internal GetAndCenterJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
