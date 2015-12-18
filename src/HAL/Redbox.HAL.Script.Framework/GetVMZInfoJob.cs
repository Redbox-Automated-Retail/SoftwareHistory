using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "vmz-merch-summary", Operand = "VMZ-MERCH-SUMMARY")]
internal sealed class GetVMZInfoJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		VMZSearchResult result = default(VMZSearchResult);
		foreach (MerchFlags item in SegmentManager.Instance.DecreasingPriorityOrder)
		{
			MerchSegment merchSegment = MerchSegmentFactory.Get(item);
			merchSegment.FindHigh(ref result);
			if (!result.Found)
			{
				base.Context.CreateInfoResult($"No{item.ToString()}s", $"There are no {item.ToString()} in the VMZ.");
				continue;
			}
			string code = $"VMZ{item.ToString()}Count";
			int num = merchSegment.ItemCount();
			base.Context.CreateInfoResult(code, num.ToString());
			string code2 = $"Last{item.ToString()}Info";
			base.Context.CreateResult(code2, $"The high {item.ToString()} was found.", result.Deck, result.Slot, result.Location.Matrix, null, null);
		}
		base.Context.CreateInfoResult("DumpBinInventoryCount", DumpBin.Instance.CurrentCount().ToString());
	}

	internal GetVMZInfoJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
