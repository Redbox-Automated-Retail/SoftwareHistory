using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "soft-sync", Operand = "SOFT-SYNC")]
internal sealed class SoftSyncJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		base.Context.ServiceLog("Begin soft sync request.");
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			int numberOfSlots = deck.NumberOfSlots;
			for (int i = 0; i < numberOfSlots; i++)
			{
				int num = i + 1;
				string item = InventoryService.Instance.Inspect(deck.Number, num);
				base.Context.CreateResult("SoftSync", "The inventory location has been successfully inspected.", deck.Number, num, item, null, null);
			}
			return true;
		});
		base.Context.CreateInfoResult("SoftSyncEnd", "The soft sync request succeeded.");
	}

	internal SoftSyncJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
