using System.Collections.Generic;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-inventory-stats", Operand = "GET-INVENTORY-STATS")]
internal sealed class InventoryStatisticsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int totalMachineSlots = 0;
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			if (!deck.IsQlmDeck)
			{
				totalMachineSlots += deck.NumberOfSlots;
			}
			return true;
		});
		InventoryService instance = InventoryService.Instance;
		int emptyCount = instance.GetEmptyCount();
		List<Location> unknowns = instance.GetUnknowns();
		List<Location> list = instance.ComputeExcludedSlots();
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Service", flushOnWrite: false, "InventoryStats");
		int num = emptyCount - ControllerConfiguration.Instance.ReturnSlotBuffer;
		applicationLog.Write("Inventory snapshot.");
		applicationLog.WriteFormatted(" Total Empty: {0}", emptyCount);
		applicationLog.WriteFormatted(" Usable empty: {0}", num);
		applicationLog.WriteFormatted(" Unknown: {0}", unknowns.Count);
		applicationLog.WriteFormatted(" Excluded: {0}", list.Count);
		applicationLog.WriteFormatted(" Total Slots: {0}", totalMachineSlots);
		applicationLog.WriteFormatted(" Dumpbin capacity: {0}", DumpBin.Instance.Capacity);
		applicationLog.WriteFormatted(" Dumpbin current count: {0}", DumpBin.Instance.CurrentCount());
		base.Context.CreateInfoResult("TotalEmptyCount", emptyCount.ToString());
		base.Context.CreateInfoResult("UsableEmptyCount", num.ToString());
		base.Context.CreateInfoResult("UnknownCount", unknowns.Count.ToString());
		base.Context.CreateInfoResult("ExcludedCount", list.Count.ToString());
		base.Context.CreateInfoResult("TotalMachineSlots", totalMachineSlots.ToString());
		base.Context.CreateInfoResult("DumpBinCapacity", DumpBin.Instance.Capacity.ToString());
		base.Context.CreateInfoResult("DumpBinCount", DumpBin.Instance.CurrentCount().ToString());
		if (instance.MachineIsFull())
		{
			applicationLog.Write(" The machine is full.");
			base.Context.CreateInfoResult("MachineFull", "The machine is full.");
		}
	}

	internal InventoryStatisticsJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
