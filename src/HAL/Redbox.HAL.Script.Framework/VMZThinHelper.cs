using System.Collections.Generic;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class VMZThinHelper : ThinHelper
{
	private VMZ VMZ => VMZ.Instance;

	protected override Location PrepTarget(MerchFlags flags, out MerchandizeResult result)
	{
		ExecutionContext activeContext = ExecutionEngine.Instance.ActiveContext;
		if (ControllerService.Instance.MachineIsFull())
		{
			if (MerchFlags.Rebalance == flags)
			{
				LogHelper.Instance.Log("There are insufficient slots in the machine.");
				result = MerchandizeResult.MachineFull;
				return null;
			}
			if (DumpBin.Instance.IsFull())
			{
				ResultHelper.Instance.CreateInfoResult("DumpBinIsFull", "The dump bin is filled to capacity.");
				activeContext.AppLog.Write("The dump bin is full.");
				result = MerchandizeResult.MachineFull;
				return null;
			}
			result = MerchandizeResult.Success;
			return DumpBin.Instance.MoveableLocation();
		}
		if (MerchFlags.Rebalance != flags)
		{
			return PrepThin(out result);
		}
		return PrepRebalance(out result);
	}

	protected override MerchandizeResult ProcessSuccessfulThin(Location loc, string id, MerchFlags flags)
	{
		_ = ExecutionEngine.Instance.ActiveContext;
		if (loc == DumpBin.Instance.PutLocation)
		{
			ResultHelper.Instance.CreateResult("ThinTransferSuccessful", "The item was transferred from storage to the dump bin.", null, null, id, null, null);
		}
		else
		{
			ResultHelper.Instance.CreateResult((ControllerConfiguration.Instance.MerchandizingMode == MerchandizingMode.VMZ_Gen2) ? "DiskTransferSuccessful" : "ThinTransferSuccessful", "The item was transferred from storage to the VMZ.", loc.Deck, loc.Slot, id, null, null);
			VMZ.MarkLocationAs(loc, flags);
		}
		return MerchandizeResult.Success;
	}

	private bool MakeNewHighThin(Location thinToMove, ref VMZSearchResult highRebalance)
	{
		VMZSearchResult result = default(VMZSearchResult);
		VMZ.FindLastLocation(MerchFlags.Thin, ref result);
		Location location = (highRebalance.Found ? VMZ.FindEmptyInteriorToHighThinType(ref result, ref highRebalance) : VMZ.FindEmptyInteriorToHighThinType(ref result));
		if (location != null)
		{
			TransferResult transferResult = ControllerService.Instance.Transfer(thinToMove, location, base.AppLog);
			LogHelper.Instance.Log("MakeNewHigh: found interior empty at {0}; transfer {1} successful.", location.ToString(), transferResult.Transferred ? "was" : "was not");
			if (!transferResult.Transferred)
			{
				return false;
			}
			HandleTransferResult(transferResult);
			return true;
		}
		VMZIterator iter = default(VMZIterator);
		iter.ResetTo(ref result);
		if (!iter.Up())
		{
			if (ThinHelper.Debug)
			{
				LogHelper.Instance.Log("MakeNewHighThin: no more slots left in VMZ.");
			}
			if (DumpBin.Instance.IsFull())
			{
				return false;
			}
			if (ThinHelper.Debug)
			{
				LogHelper.Instance.Log("The dumpbin has room; move thin to bin.");
			}
			TransferResult xferResult = ControllerService.Instance.Transfer(thinToMove, DumpBin.Instance.MoveableLocation(), base.AppLog);
			HandleTransferResult(xferResult);
			return true;
		}
		Location location2 = FindAndClearNextLocation(MerchFlags.Thin, ref iter, out var _);
		if (location2 == null)
		{
			if (DumpBin.Instance.IsFull())
			{
				return false;
			}
			if (ThinHelper.Debug)
			{
				LogHelper.Instance.Log("The dumpbin has room; move thin to bin.");
			}
			TransferResult transferResult2 = ControllerService.Instance.Transfer(thinToMove, DumpBin.Instance.MoveableLocation(), base.AppLog);
			HandleTransferResult(transferResult2);
			if (!transferResult2.Transferred)
			{
				return transferResult2.SourceEmpty;
			}
			return true;
		}
		iter.ResetTo(location2);
		TransferResult transferResult3 = ControllerService.Instance.Transfer(thinToMove, iter.Location, base.AppLog);
		HandleTransferResult(transferResult3);
		if (!transferResult3.Transferred)
		{
			return transferResult3.SourceEmpty;
		}
		return true;
	}

	private Location PrepRebalance(out MerchandizeResult result)
	{
		if (ThinHelper.Debug)
		{
			LogHelper.Instance.Log(">> Prep rebalance.");
		}
		_ = ExecutionEngine.Instance.ActiveContext;
		VMZSearchResult result2 = default(VMZSearchResult);
		VMZ.FindLastLocation(MerchFlags.Rebalance, ref result2);
		VMZIterator iter = default(VMZIterator);
		iter.ResetTo(ref result2);
		if (!result2.Found)
		{
			iter.ResetToFirst();
			return FindAndClearNextLocation(MerchFlags.Rebalance, ref iter, out result);
		}
		Location location = VMZ.FindEmptyInteriorToHighThinType(ref result2);
		if (location != null)
		{
			if (ThinHelper.Debug)
			{
				LogHelper.Instance.Log(" Empty found at {0}", location.ToString());
			}
			result = MerchandizeResult.Success;
			return location;
		}
		if (ThinHelper.Debug)
		{
			LogHelper.Instance.Log(" There are no empty slots interior to start.");
		}
		List<Location> list = VMZ.FindNonThinsInteriorTo(ref result2);
		if (ThinHelper.Debug)
		{
			LogHelper.Instance.Log(" There are {0} non-thins to attempt to move.", list.Count);
		}
		if (list.Count > 0)
		{
			try
			{
				Location result3 = MoveInteriorNonThin(list, out result);
				if (result == MerchandizeResult.Success)
				{
					return result3;
				}
				if (MerchandizeResult.EmptyStuck != result)
				{
					return null;
				}
			}
			finally
			{
				list.Clear();
			}
		}
		List<Location> list2 = VMZ.FindThinTypeInteriorTo(MerchFlags.Thin, ref result2);
		if (ThinHelper.Debug)
		{
			LogHelper.Instance.Log(" There are {0} disks of thin type.", list2.Count);
		}
		try
		{
			foreach (Location item in list2)
			{
				if (!item.IsStuck && MakeNewHighThin(item, ref result2))
				{
					result = MerchandizeResult.Success;
					return iter.Location;
				}
			}
		}
		finally
		{
			list2.Clear();
		}
		if (!iter.Up())
		{
			result = MerchandizeResult.VMZFull;
			return null;
		}
		return FindAndClearNextLocation(MerchFlags.Rebalance, ref iter, out result);
	}

	private Location PrepThin(out MerchandizeResult result)
	{
		if (ThinHelper.Debug)
		{
			LogHelper.Instance.Log(">> Prep thin.");
		}
		_ = ExecutionEngine.Instance.ActiveContext;
		VMZSearchResult result2 = default(VMZSearchResult);
		VMZSearchResult result3 = default(VMZSearchResult);
		VMZ.FindLastLocation(MerchFlags.Thin, ref result2);
		VMZIterator iter = default(VMZIterator);
		iter.ResetTo(ref result2);
		if (!result2.Found)
		{
			VMZ.FindLastLocation(MerchFlags.Rebalance, ref result3);
			if (result3.Found)
			{
				if (BackfillEmpty(MerchFlags.Rebalance, ref result3))
				{
					result = MerchandizeResult.Success;
					return result3.Location;
				}
				iter.ResetTo(ref result3);
				if (!iter.Up())
				{
					if (!DumpBin.Instance.IsFull())
					{
						result = MerchandizeResult.Success;
						return DumpBin.Instance.MoveableLocation();
					}
					result = MerchandizeResult.VMZFull;
					return null;
				}
			}
			else
			{
				iter.ResetToFirst();
			}
			return FindAndClearNextLocation(MerchFlags.Thin, ref iter, out result);
		}
		VMZ.FindLastLocation(MerchFlags.Rebalance, ref result3);
		if (result3.Found && BackfillEmpty(MerchFlags.Rebalance, ref result3))
		{
			result = MerchandizeResult.Success;
			return result3.Location;
		}
		Location location = (result3.Found ? VMZ.FindEmptyInteriorToHighThinType(ref result2, ref result3) : VMZ.FindEmptyInteriorToHighThinType(ref result2));
		if (location != null)
		{
			result = MerchandizeResult.Success;
			return location;
		}
		if (ThinHelper.Debug)
		{
			LogHelper.Instance.Log("There are no empty slots interior to start.");
		}
		List<Location> list = (result3.Found ? VMZ.FindNonThinsInteriorTo(ref result2, result3.Location) : VMZ.FindNonThinsInteriorTo(ref result2));
		if (ThinHelper.Debug)
		{
			LogHelper.Instance.Log("There are {0} non-thins to attempt to move.", list.Count);
		}
		if (list.Count > 0)
		{
			try
			{
				Location result4 = MoveInteriorNonThin(list, out result);
				if (result == MerchandizeResult.Success)
				{
					return result4;
				}
				if (MerchandizeResult.EmptyStuck != result)
				{
					return null;
				}
			}
			finally
			{
				list.Clear();
			}
		}
		iter.ResetTo(ref result2);
		if (!iter.Up())
		{
			result = MerchandizeResult.VMZFull;
			return null;
		}
		return FindAndClearNextLocation(MerchFlags.Thin, ref iter, out result);
	}

	private bool BackfillEmpty(MerchFlags flags, ref VMZSearchResult high)
	{
		if (high.Location.IsStuck)
		{
			return false;
		}
		Location location = VMZ.FindEmptyInteriorToHighThinType(ref high);
		if (location == null)
		{
			return false;
		}
		if (ThinHelper.Debug)
		{
			LogHelper.Instance.Log(" Interior empty in {0) found at {1}", flags.ToString(), location.ToString());
		}
		return ControllerService.Instance.Transfer(high.Location, location, ExecutionEngine.Instance.ActiveContext.AppLog).Transferred;
	}

	private void HandleTransferResult(TransferResult xferResult)
	{
		if (xferResult.Transferred)
		{
			ResultHelper.Instance.CreateResult("DiskTransferSuccessful", xferResult.TransferredItem.SourceIsVMZ ? "The item was moved internally in the VMZ." : "Transferred disk from VMZ to drum.", xferResult.TransferredItem.Source.Deck, xferResult.TransferredItem.Source.Slot, "EMPTY", null, xferResult.TransferredItem.Matrix);
			if (DumpBin.Instance.IsBin(xferResult.TransferredItem.Destination))
			{
				ResultHelper.Instance.CreateResult("ThinTransferSuccessful", "The item was transferred from storage to the dump bin.", null, null, xferResult.TransferredItem.Matrix, null, null);
			}
			else
			{
				ResultHelper.Instance.CreateResult("DiskTransferSuccessful", xferResult.TransferredItem.TargetIsVMZ ? "The item was moved internally in the VMZ." : "Transferred disk from VMZ to drum.", xferResult.TransferredItem.Destination.Deck, xferResult.TransferredItem.Destination.Slot, xferResult.TransferredItem.Matrix, null, "EMPTY");
			}
		}
	}

	private Location FindAndClearNextLocation(MerchFlags context, ref VMZIterator iter, out MerchandizeResult result)
	{
		while (true)
		{
			if (iter.IsEmpty)
			{
				result = MerchandizeResult.Success;
				return iter.Location;
			}
			if (!iter.IsStuck)
			{
				break;
			}
			if (!iter.Up())
			{
				result = MerchandizeResult.VMZFull;
				return null;
			}
		}
		if (MerchFlags.Rebalance == context && iter.Flags == MerchFlags.Thin)
		{
			VMZSearchResult result2 = default(VMZSearchResult);
			VMZ.FindLastLocation(MerchFlags.Rebalance, ref result2);
			if (MakeNewHighThin(iter.Location, ref result2))
			{
				result = MerchandizeResult.Success;
				return iter.Location;
			}
			result = MerchandizeResult.TransferFailure;
			return null;
		}
		result = TransferNonThinOutOfZone(iter.Location);
		if (result != MerchandizeResult.Success)
		{
			return null;
		}
		return iter.Location;
	}

	internal MerchandizeResult TransferNonThinOutOfZone(Location toMove)
	{
		List<Location> list = VMZ.Instance.FindEmptyOutsideOfCompressedZone();
		if (list.Count == 0)
		{
			return MerchandizeResult.MachineFull;
		}
		TransferResult transferResult = ControllerService.Instance.Transfer(toMove, list, ExecutionEngine.Instance.ActiveContext.AppLog);
		if (transferResult.Transferred)
		{
			return MerchandizeResult.Success;
		}
		if (transferResult.NoEmptySlots)
		{
			LogHelper.Instance.Log("The transfer failed because there were no slots available.");
			return MerchandizeResult.VMZFull;
		}
		if (!string.IsNullOrEmpty(transferResult.ErrorCode))
		{
			LogHelper.Instance.Log("The transfer result had a hardware error: {0}", transferResult.ErrorCode.ToUpper());
			return MerchandizeResult.HardwareError;
		}
		if (transferResult.ReturnedToSource)
		{
			LogHelper.Instance.Log("The transfer failed to put the disk in a target, but it was returned to source.");
			return MerchandizeResult.TransferFailure;
		}
		if (transferResult.GetOk || transferResult.SourceEmpty)
		{
			return MerchandizeResult.Success;
		}
		if (transferResult.SourceStuck)
		{
			return MerchandizeResult.EmptyStuck;
		}
		return MerchandizeResult.TransferFailure;
	}

	private Location MoveInteriorNonThin(List<Location> nonThins, out MerchandizeResult result)
	{
		try
		{
			foreach (Location nonThin in nonThins)
			{
				if (nonThin.IsStuck)
				{
					continue;
				}
				result = TransferNonThinOutOfZone(nonThin);
				if (result == MerchandizeResult.Success)
				{
					if (ThinHelper.Debug)
					{
						LogHelper.Instance.Log("Successfully moved non-thin at {0}.", nonThin.ToString());
					}
					return nonThin;
				}
				if (MerchandizeResult.EmptyStuck != result)
				{
					result = MerchandizeResult.TransferFailure;
					return null;
				}
			}
			result = MerchandizeResult.EmptyStuck;
			return null;
		}
		finally
		{
			nonThins.Clear();
		}
	}
}
