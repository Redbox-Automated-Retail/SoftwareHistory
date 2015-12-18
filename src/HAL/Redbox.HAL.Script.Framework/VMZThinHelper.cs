using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal sealed class VMZThinHelper : ThinHelper
{
	protected override Location FindEmptyTarget(MerchFlags flags, out MerchandizeResult result)
	{
		LogHelper.Instance.Log(LogEntryType.Debug, "[FindEmptyTarget] Context = {0}", flags.ToString());
		if (InventoryService.Instance.MachineIsFull())
		{
			LogHelper.Instance.Log("[FindEmptyTarget] The machine is full.", LogEntryType.Debug);
			result = MerchandizeResult.MachineFull;
			return null;
		}
		result = MerchandizeResult.Success;
		using MerchTransferManager manager = new MerchTransferManager(base.Context, ThinHelper.Debug);
		Location location = SegmentManager.Instance.SelectLocation(flags, manager, out result);
		if (location != null)
		{
			LogHelper.Instance.Log("[FindEmptyTarget] Select location found target at = {0}", location.ToString());
			return location;
		}
		if (ThinHelper.Debug)
		{
			LogHelper.Instance.Log("[FindEmptyTarget] Select location didn't find a target; return code = {0}", result.ToString());
		}
		if (result != MerchandizeResult.VMZFull && result != MerchandizeResult.Success)
		{
			return null;
		}
		if (DumpBin.Instance.IsFull())
		{
			LogHelper.Instance.Log("MoveLowPriorityToBin: the dump bin is full.");
			result = MerchandizeResult.VMZFull;
			return null;
		}
		Location location2 = null;
		foreach (MerchFlags item in SegmentManager.Instance.IncreasingPriorityOrder)
		{
			if (item == flags)
			{
				break;
			}
			VMZSearchResult low = default(VMZSearchResult);
			VMZSearchResult high = default(VMZSearchResult);
			MerchSegment merchSegment = MerchSegmentFactory.Get(item);
			if (!merchSegment.CanDump)
			{
				continue;
			}
			merchSegment.ComputeBounds(ref low, ref high);
			if (!low.Found)
			{
				continue;
			}
			string matrix = low.Location.Matrix;
			MerchFlags flags2 = low.Location.Flags;
			if (FetchThin(low.Location, matrix) != MerchandizeResult.Success)
			{
				LogHelper.Instance.Log("Thin: cannot move low at {0}; bail.", low.ToString());
				continue;
			}
			Location location3 = ((location2 == null) ? DumpBin.Instance.MoveableLocation() : location2);
			MerchandizeResult merchandizeResult = ThinDiskToLocation(low.Location, location3, matrix, flags2);
			if (ThinHelper.Debug)
			{
				LogHelper.Instance.Log("MoveLowPriorityToBin: transfer of {0} [Flags = {1}] to {2} returned {3}", low.ToString(), item.ToString(), location3.ToString(), merchandizeResult.ToString());
			}
			if (merchandizeResult != MerchandizeResult.Success)
			{
				result = merchandizeResult;
				return null;
			}
			location2 = low.Location;
		}
		if (location2 != null)
		{
			result = MerchandizeResult.Success;
			return location2;
		}
		result = MerchandizeResult.VMZFull;
		return null;
	}

	internal VMZThinHelper(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}

	private Location TryDumpBin(MerchFlags flags, out MerchandizeResult result)
	{
		return TryDumpBin(MerchSegmentFactory.Get(flags), out result);
	}

	private Location TryDumpBin(MerchSegment segment, out MerchandizeResult result)
	{
		if (!segment.CanDump)
		{
			LogHelper.Instance.Log("There are insufficient slots in the machine.");
			result = MerchandizeResult.MachineFull;
			return null;
		}
		if (DumpBin.Instance.IsFull())
		{
			base.Context.CreateInfoResult("DumpBinIsFull", "The dump bin is filled to capacity.");
			base.Context.AppLog.Write("The dump bin is full.");
			result = MerchandizeResult.MachineFull;
			return null;
		}
		result = MerchandizeResult.Success;
		return DumpBin.Instance.MoveableLocation();
	}
}
