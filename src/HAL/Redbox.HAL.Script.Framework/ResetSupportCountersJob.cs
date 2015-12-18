using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "reset-support-counters", Operand = "RESET-SUPPORT-COUNTERS")]
internal class ResetSupportCountersJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		string[] supportCounters = HardwareSupport.SupportCounters;
		foreach (string name in supportCounters)
		{
			CounterManager.Instance.Reset(name);
		}
	}

	internal ResetSupportCountersJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
