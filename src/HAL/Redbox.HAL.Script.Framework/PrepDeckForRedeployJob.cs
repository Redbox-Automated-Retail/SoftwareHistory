using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "prep-deck-for-redeploy", Operand = "PREP-DECK-FOR-REDEPLOY")]
internal sealed class PrepDeckForRedeployJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int top = GetTop<int>();
		int num = 0;
		bool isVMZMachine = ControllerConfiguration.Instance.IsVMZMachine;
		for (int i = 0; i < top; i++)
		{
			int top2 = GetTop<int>();
			IDeck byNumber = base.DecksService.GetByNumber(top2);
			if (byNumber == null)
			{
				LogHelper.Instance.Log("Invalid deck '{0}' specified to prep redeploy deck job.", top2);
				continue;
			}
			if (!isVMZMachine && byNumber.IsQlm)
			{
				LogHelper.Instance.Log("Deck specified '{0}' is QLM; will not alter.", top2);
				continue;
			}
			int start = ((!isVMZMachine) ? 1 : (VMZ.Instance.Bounds.End + 1));
			Range slotRange = new Range(start, byNumber.NumberOfSlots);
			IList<int> list = base.InventoryService.SwapEmptyWith(byNumber, "UNKNOWN", MerchFlags.None, slotRange);
			foreach (int item in list)
			{
				base.Context.CreateResult("LocationChanged", "The location was set to UNKNOWN", top2, item, null, null, null);
			}
			num += list.Count;
			LogHelper.Instance.Log("Inventory: changed {0} empty slots on deck {1} to UNKNOWN", list.Count, top2);
			list.Clear();
		}
		base.Context.CreateInfoResult("LocationChangedCount", num.ToString());
	}

	internal PrepDeckForRedeployJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
