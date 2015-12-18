using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Script.Framework;

internal abstract class ThinHelper
{
	private static bool? m_debug = null;

	protected ApplicationLog AppLog => Context.AppLog;

	protected ExecutionResult Result { get; set; }

	protected ExecutionContext Context { get; private set; }

	internal static bool Debug
	{
		get
		{
			if (m_debug.HasValue)
			{
				return m_debug.Value;
			}
			IPersistentMap dataStore = StoreManager.Instance.GetDataStore();
			m_debug = dataStore.GetValue("DebugVMZ", def: false);
			return m_debug.Value;
		}
	}

	internal static ThinHelper MakeInstance(ExecutionResult result, ExecutionContext context)
	{
		if (!ControllerConfiguration.Instance.IsVMZMachine)
		{
			return new QlmThinHelper(result, context);
		}
		return new VMZThinHelper(result, context);
	}

	internal MerchandizeResult ProcessSuccessfulThin(Location loc, string id, MerchFlags flags)
	{
		if (!ControllerConfiguration.Instance.IsVMZMachine)
		{
			Context.CreateResult("ThinTransferSuccessful", "The item was transferred from storage to the QLM.", loc.Deck, loc.Slot, id, null, null);
			return MerchandizeResult.Success;
		}
		if (loc == DumpBin.Instance.PutLocation)
		{
			Context.CreateResult("ThinTransferSuccessful", "The item was transferred from storage to the dump bin.", null, null, id, null, null);
		}
		else
		{
			Context.CreateResult(ControllerConfiguration.Instance.IsVMZMachine ? "DiskTransferSuccessful" : "ThinTransferSuccessful", "The item was transferred from storage to the VMZ.", loc.Deck, loc.Slot, id, null, null);
			loc.ChangeMerchFlags(flags);
		}
		return MerchandizeResult.Success;
	}

	internal virtual MerchandizeResult ThinDisk(string id, MerchFlags flags)
	{
		Context.AppLog.Write($"Attempt thin of item {id} (flags = {flags.ToString()})");
		Location location = InventoryService.Instance.Lookup(id);
		if (location == null)
		{
			Context.AppLog.LogLookupFailure(id);
			Context.CreateInfoResult("ThinLookupFailed", "The LOOKUP request failed because the item was not found.", id);
			return MerchandizeResult.LookupFailure;
		}
		if (ControllerConfiguration.Instance.IsVMZMachine)
		{
			MerchSegment merchSegment = MerchSegmentFactory.Get(flags);
			if (merchSegment.Contains(location))
			{
				LogHelper.Instance.Log("The disk {0} {1} is already in its segment; do not move.", id, location.ToString());
				location.ChangeMerchFlags(flags);
				Context.CreateResult("VMZThinNotMoved", "The thin item is already in the VMZ; do not move.", location.Deck, location.Slot, id, null, null);
				return MerchandizeResult.DiskInVMZ;
			}
		}
		Location location2 = FindEmptyTarget(flags, out var result);
		if (Debug)
		{
			LogHelper.Instance.Log("FindEmptyTarget returned {0}; location = {1}", result.ToString(), (location2 == null) ? "None" : location2.ToString());
		}
		if (location2 == null)
		{
			if (!ControllerConfiguration.Instance.IsVMZMachine)
			{
				return result;
			}
			if (result != MerchandizeResult.VMZFull)
			{
				return result;
			}
			MerchSegment merchSegment2 = MerchSegmentFactory.Get(flags);
			if (!merchSegment2.CanDump)
			{
				return MerchandizeResult.VMZFull;
			}
			if (DumpBin.Instance.IsFull())
			{
				return result;
			}
			location2 = DumpBin.Instance.MoveableLocation();
		}
		location = InventoryService.Instance.Lookup(id);
		if (location == null)
		{
			Context.AppLog.LogLookupFailure(id);
			Context.CreateInfoResult("ThinLookupFailed", "The LOOKUP request failed because the item was not found.", id);
			return MerchandizeResult.LookupFailure;
		}
		MerchandizeResult merchandizeResult = FetchThin(location, id);
		if (merchandizeResult != MerchandizeResult.Success)
		{
			return merchandizeResult;
		}
		MerchandizeResult merchandizeResult2 = ThinDiskInner(id, location, location2, flags);
		if (merchandizeResult2 == MerchandizeResult.Success)
		{
			return merchandizeResult2;
		}
		ReturnToSource(location, id);
		return merchandizeResult2;
	}

	protected virtual MerchandizeResult ThinDiskInner(string id, Location source, Location target, MerchFlags flags)
	{
		return ThinDiskToLocation(source, target, id, flags);
	}

	protected abstract Location FindEmptyTarget(MerchFlags flags, out MerchandizeResult result);

	protected MerchandizeResult ThinDiskToLocation(Location source, Location target, string id, MerchFlags flags)
	{
		if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
		{
			Context.ServiceLog("Transfer from {0},{1} to {2},{3}.", source.Deck, source.Slot, target.Deck, target.Slot);
		}
		if (target == DumpBin.Instance.MoveableLocation() && DumpBin.Instance.IsFull())
		{
			LogHelper.Instance.Log("The dumpbin is full.");
			return MerchandizeResult.DumpBinFull;
		}
		MotionResult motionResult = ControllerService.Instance.MoveTo(target, MoveMode.Put, Context.AppLog);
		if (motionResult != MotionResult.Success)
		{
			MerchandizingHelper.Instance.CleanupJob(Result, Context, motionResult);
			MerchandizingHelper.Instance.ExitOnMoveError(Result, Context, leaveResult: true);
			return MerchandizeResult.HardwareError;
		}
		PutResult putResult = ControllerService.Instance.Put(id, Context.AppLog);
		if (putResult.Success)
		{
			return ProcessSuccessfulThin(target, id, flags);
		}
		if (putResult.IsSlotInUse)
		{
			Context.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", target.Deck, target.Slot, id, null, null);
			if (ControllerConfiguration.Instance.IsVMZMachine)
			{
				VMZHelperFunctions.Instance.ProcessFailedPut(target, markUnknown: true);
				return MerchandizeResult.TransferFailure;
			}
			return MerchandizeResult.SlotInUse;
		}
		if (!Gripper.IsClear())
		{
			return MerchandizeResult.HardwareError;
		}
		if (!PickerBoard.IsFull())
		{
			InventoryService.Instance.UpdateInventoryOnPut(putResult, target, id, out var _);
			return ProcessSuccessfulThin(target, id, flags);
		}
		if (ControllerConfiguration.Instance.IsVMZMachine)
		{
			VMZHelperFunctions.Instance.ProcessFailedPut(target, markUnknown: false);
		}
		return MerchandizeResult.TransferFailure;
	}

	protected MerchandizeResult ReturnToSource(Location source, string id)
	{
		Context.AppLog.Write("Unable to find a slot for the thin; put it back and bail.", LogEntryType.Info);
		if (ControllerService.Instance.MoveTo(source, MoveMode.Put, Context.AppLog) != MotionResult.Success)
		{
			MerchandizingHelper.Instance.ExitOnMoveError(Result, Context, leaveResult: true);
			return MerchandizeResult.HardwareError;
		}
		PutResult putResult = ControllerService.Instance.Put(id, Context.AppLog);
		if (putResult.Success)
		{
			return MerchandizeResult.HardwareError;
		}
		return MerchandizeResult.MachineFull;
	}

	protected MerchandizeResult FetchThin(Location source, string id)
	{
		int deck = source.Deck;
		int slot = source.Slot;
		Context.CreateResult("ThinDiskFromDrum", "Removing disk from drum for thinning.", deck, slot, id, null, null);
		MotionResult motionResult = ControllerService.Instance.MoveTo(source, MoveMode.Get, Context.AppLog);
		if (motionResult != MotionResult.Success)
		{
			MerchandizingHelper.Instance.CleanupJob(Result, Context, motionResult);
			MerchandizingHelper.Instance.ExitOnMoveError(Result, Context, leaveResult: true);
			return MerchandizeResult.HardwareError;
		}
		GetResult getResult = (ControllerConfiguration.Instance.IsVMZMachine ? ControllerService.Instance.Get(new FetchThinGetObserver(), Context.AppLog) : ControllerService.Instance.Get(Context.AppLog));
		if (getResult.IsSlotEmpty)
		{
			return MerchandizeResult.EmptyStuck;
		}
		if (getResult.Success)
		{
			return MerchandizeResult.Success;
		}
		if (!Gripper.IsClear())
		{
			Context.CreateResult("ThinItemStuck", "Could not retrieve the item.", deck, slot, null, null, null);
			LogHelper.Instance.Log("Thin: the gripper is obstructed - bailing thin.", LogEntryType.Info);
			return MerchandizeResult.HardwareError;
		}
		if (!PickerBoard.IsFull())
		{
			Context.CreateResult("ThinItemStuck", "Could not retrieve the item.", deck, slot, null, null, null);
			return MerchandizeResult.EmptyStuck;
		}
		return MerchandizeResult.HardwareError;
	}

	protected ThinHelper(ExecutionResult result, ExecutionContext context)
	{
		Result = result;
		Context = context;
	}
}
