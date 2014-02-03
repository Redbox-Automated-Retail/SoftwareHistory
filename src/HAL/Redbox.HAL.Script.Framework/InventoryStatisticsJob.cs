using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class InventoryStatisticsJob : NativeJobAdapter
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
		ResultHelper.Instance.CreateInfoResult("EmptyCount", ControllerConfiguration.Instance.GetEmptyCount().ToString());
		ResultHelper.Instance.CreateInfoResult("UnknownCount", ControllerConfiguration.Instance.GetUnknowns().Count.ToString());
		ResultHelper.Instance.CreateInfoResult("ExcludedCount", ControllerConfiguration.Instance.ComputeExcludedSlots().Count.ToString());
		ResultHelper.Instance.CreateInfoResult("TotalMachineSlots", totalMachineSlots.ToString());
		ResultHelper.Instance.CreateInfoResult("DumpBinCapacity", DumpBin.Instance.Capacity.ToString());
		ResultHelper.Instance.CreateInfoResult("DumpBinCount", DumpBin.Instance.CurrentCount().ToString());
		if (ControllerService.Instance.MachineIsFull())
		{
			ResultHelper.Instance.CreateInfoResult("MachineFull", "The machine is full.");
		}
	}
}
