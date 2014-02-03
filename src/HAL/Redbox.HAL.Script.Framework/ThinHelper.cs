using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal abstract class ThinHelper
{
	private static bool? m_debug = null;

	protected ExecutionResult Result { get; set; }

	protected ApplicationLog AppLog => ExecutionEngine.Instance.ActiveContext.AppLog;

	internal static bool Debug
	{
		get
		{
			if (m_debug.HasValue)
			{
				return m_debug.Value;
			}
			m_debug = RegistryOptions.Instance.GetSecretValue<bool?>("SOFTWARE\\Redbox\\HAL", "DebugVMZ");
			if (!m_debug.HasValue)
			{
				m_debug = false;
			}
			return m_debug.Value;
		}
	}

	internal static ThinHelper MakeInstance(ExecutionResult result)
	{
		if (ControllerConfiguration.Instance.MerchandizingMode == MerchandizingMode.QLM)
		{
			QlmThinHelper qlmThinHelper = new QlmThinHelper();
			qlmThinHelper.Result = result;
			return qlmThinHelper;
		}
		VMZThinHelper vMZThinHelper = new VMZThinHelper();
		vMZThinHelper.Result = result;
		return vMZThinHelper;
	}

	internal virtual MerchandizeResult ThinDisk(string id, MerchFlags flags)
	{
		ExecutionContext activeContext = ExecutionEngine.Instance.ActiveContext;
		activeContext.AppLog.Write($"Attempt thin of item {id}");
		Location location = ControllerService.Instance.Lookup(id);
		if (location == null)
		{
			activeContext.AppLog.LogLookupFailure(id);
			ResultHelper.Instance.CreateInfoResult("ThinLookupFailed", "The LOOKUP request failed because the item was not found.", id);
			return MerchandizeResult.LookupFailure;
		}
		if (MerchandizingMode.VMZ_Gen2 == ControllerConfiguration.Instance.MerchandizingMode && VMZ.Instance.DiskInteriorToTop(flags, location))
		{
			LogHelper.Instance.Log("The disk {0} {1} is already in the thin zone; do not move.", id, location.ToString());
			ResultHelper.Instance.CreateResult("VMZThinNotMoved", "The thin item is already in the VMZ; do not move.", location.Deck, location.Slot, id, null, null);
			return MerchandizeResult.DiskInVMZ;
		}
		Location location2 = PrepTarget(flags, out var result);
		if (Debug)
		{
			LogHelper.Instance.Log("Prep target returned {0}; location = {1}", result.ToString(), (location2 == null) ? "None" : location2.ToString());
		}
		if (location2 == null)
		{
			if (ControllerConfiguration.Instance.MerchandizingMode == MerchandizingMode.QLM)
			{
				return result;
			}
			if (flags == MerchFlags.Rebalance)
			{
				return result;
			}
			if (result != MerchandizeResult.VMZFull)
			{
				return result;
			}
			if (DumpBin.Instance.IsFull())
			{
				return result;
			}
			location2 = DumpBin.Instance.MoveableLocation();
		}
		location = ControllerService.Instance.Lookup(id);
		if (location == null)
		{
			activeContext.AppLog.LogLookupFailure(id);
			ResultHelper.Instance.CreateInfoResult("ThinLookupFailed", "The LOOKUP request failed because the item was not found.", id);
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

	protected abstract MerchandizeResult ProcessSuccessfulThin(Location loc, string id, MerchFlags flags);

	protected virtual MerchandizeResult ThinDiskInner(string id, Location source, Location target, MerchFlags flags)
	{
		return ThinDiskToLocation(source, target, id, flags);
	}

	protected abstract Location PrepTarget(MerchFlags flags, out MerchandizeResult result);

	protected MerchandizeResult ThinDiskToLocation(Location source, Location target, string id, MerchFlags flags)
	{
		ExecutionContext activeContext = ExecutionEngine.Instance.ActiveContext;
		if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
		{
			ServiceLogHelper.Instance.WriteFormattedMessage(LogEntryType.Info, "Transfer from {0},{1} to {2},{3}.", source.Deck, source.Slot, target.Deck, target.Slot);
		}
		MotionResult motionResult = ControllerService.Instance.MoveTo(target, MoveMode.Put, activeContext.AppLog);
		if (motionResult != MotionResult.Success)
		{
			MerchandizingHelper.Instance.CleanupJob(Result, motionResult);
			MerchandizingHelper.Instance.ExitOnMoveError(Result, leaveResult: true);
			return MerchandizeResult.HardwareError;
		}
		PutResult putResult = ControllerService.Instance.Put(id, activeContext.AppLog);
		if (putResult.Success)
		{
			return ProcessSuccessfulThin(target, id, flags);
		}
		if (putResult.IsSlotInUse)
		{
			ResultHelper.Instance.CreateResult(putResult.ToString().ToUpper(), "The slot is in use.", target.Deck, target.Slot, id, null, null);
			if (ControllerConfiguration.Instance.MerchandizingMode != MerchandizingMode.QLM)
			{
				VMZHelperFunctions.Instance.ProcessFailedPut(target, markUnknown: true);
				return MerchandizeResult.TransferFailure;
			}
			return MerchandizeResult.SlotInUse;
		}
		if (!ControllerService.Instance.IsGripperClear())
		{
			return MerchandizeResult.HardwareError;
		}
		if (!PickerBoard.Instance.IsFull())
		{
			ControllerService.Instance.UpdateInventoryOnPut(putResult, id, out var _);
			return ProcessSuccessfulThin(target, id, flags);
		}
		if (ControllerConfiguration.Instance.MerchandizingMode != MerchandizingMode.QLM)
		{
			VMZHelperFunctions.Instance.ProcessFailedPut(target, markUnknown: false);
		}
		return MerchandizeResult.TransferFailure;
	}

	protected MerchandizeResult ReturnToSource(Location source, string id)
	{
		ExecutionContext activeContext = ExecutionEngine.Instance.ActiveContext;
		activeContext.AppLog.Write("Unable to find a slot for the thin; put it back and bail.", LogEntryType.Info);
		if (ControllerService.Instance.MoveTo(source, MoveMode.Put, activeContext.AppLog) != MotionResult.Success)
		{
			MerchandizingHelper.Instance.ExitOnMoveError(Result, leaveResult: true);
			return MerchandizeResult.HardwareError;
		}
		PutResult putResult = ControllerService.Instance.Put(id, activeContext.AppLog);
		if (putResult.Success)
		{
			return MerchandizeResult.HardwareError;
		}
		return MerchandizeResult.MachineFull;
	}

	protected MerchandizeResult FetchThin(Location source, string id)
	{
		ExecutionContext activeContext = ExecutionEngine.Instance.ActiveContext;
		int deck = source.Deck;
		int slot = source.Slot;
		ResultHelper.Instance.CreateResult("ThinDiskFromDrum", "Removing disk from drum for thinning.", deck, slot, id, null, null);
		MotionResult motionResult = ControllerService.Instance.MoveTo(source, MoveMode.Get, activeContext.AppLog);
		if (motionResult != MotionResult.Success)
		{
			MerchandizingHelper.Instance.CleanupJob(Result, motionResult);
			MerchandizingHelper.Instance.ExitOnMoveError(Result, leaveResult: true);
			return MerchandizeResult.HardwareError;
		}
		GetResult getResult = ControllerService.Instance.Get(testOnEmpty: false, activeContext.AppLog);
		if (getResult.IsSlotEmpty)
		{
			return MerchandizeResult.EmptyStuck;
		}
		if (getResult.Success)
		{
			return MerchandizeResult.Success;
		}
		if (!ControllerService.Instance.IsGripperClear())
		{
			ResultHelper.Instance.CreateResult("ThinItemStuck", "Could not retrieve the item.", deck, slot, null, null, null);
			LogHelper.Instance.Log("Thin: the gripper is obstructed - bailing thin.", LogEntryType.Info);
			return MerchandizeResult.HardwareError;
		}
		if (!PickerBoard.Instance.IsFull())
		{
			ResultHelper.Instance.CreateResult("ThinItemStuck", "Could not retrieve the item.", deck, slot, null, null, null);
			return MerchandizeResult.EmptyStuck;
		}
		return MerchandizeResult.HardwareError;
	}
}
