using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class PutOperation : AbstractOperation<IPutResult>
{
	private readonly string ID;

	private readonly IPutObserver Observer;

	private readonly ILocation PutLocation;

	private readonly PutResult Result;

	private readonly IFormattedLog Log;

	private const string AggressivePutFailuresCounter = "AggressivePutFailures";

	private const string AggressivePutSuccessCounter = "AggressivePutSuccesses";

	public override IPutResult Execute()
	{
		try
		{
			if (PutLocation.IsWide)
			{
				SettleDiskInSlot();
			}
			if (Controller.TrackState != TrackState.Closed && !Controller.TrackClose().Success)
			{
				Result.Code = ErrorCodes.TrackCloseTimeout;
				return Result;
			}
			if (!Controller.RetractArm().Success)
			{
				Result.Code = ErrorCodes.GripperRetractTimeout;
				return Result;
			}
			if (!Controller.SetFinger(GripperFingerState.Rent).Success)
			{
				LogHelper.Instance.Log("PUT: SetGripperFinger to Rent timed out.", LogEntryType.Error);
				Result.Code = ErrorCodes.GripperRentTimeout;
				return Result;
			}
			Controller.StartRollerIn();
			bool num = ClearDiskFromPicker();
			RuntimeService.SpinWait(100);
			if (num)
			{
				LogHelper.Instance.Log($"PUT: Disk cannot be placed into {PutLocation.ToString()} because it cannot clear sensor 2.", LogEntryType.Error);
				Controller.LogPickerSensorState(LogEntryType.Error);
				Controller.StopRoller();
				RuntimeService.SpinWait(100);
				if (!Controller.TrackOpen().Success)
				{
					Result.Code = ErrorCodes.TrackOpenTimeout;
					return Result;
				}
				Controller.TrackClose();
				RuntimeService.SpinWait(100);
				if (!Controller.RollerToPosition(RollerPosition.Position5, 8000).Success)
				{
					LogHelper.Instance.Log("Put: roll item back to sensor 5 returned TIMEOUT");
					LogHelper.Instance.Log("PUT: unable to roll item into slot, and cannot move it back into the picker.", LogEntryType.Error);
					Controller.LogPickerSensorState(LogEntryType.Error);
					Result.Code = ErrorCodes.PickerObstructed;
					return Result;
				}
				OnSlotInUse();
				return Result;
			}
			if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
			{
				LogHelper.Instance.Log("Sensor wait for 2, 5 didn't time out.", LogEntryType.Info);
			}
			ErrorCodes errorCodes = PushIntoSlot();
			if (errorCodes != ErrorCodes.Success)
			{
				LogHelper.Instance.Log($"PUT: item_cleared_gripper: PushIntoSlot returned {errorCodes.ToString().ToUpper()}", LogEntryType.Error);
			}
			Controller.StopRoller();
			IPickerSensorReadResult pickerSensorReadResult = Controller.ReadSensors();
			if (pickerSensorReadResult.Error != ErrorCodes.Success)
			{
				Result.Code = ErrorCodes.SensorReadError;
				return Result;
			}
			if (!pickerSensorReadResult.Sensors[0])
			{
				UpdateInventory();
				return Result;
			}
			LogHelper.Instance.Log("[PUT] can't put disk in slot.", LogEntryType.Error);
			Controller.LogPickerSensorState(LogEntryType.Error);
			if (ControllerConfiguration.Instance.AggressiveClearPickerOnPut)
			{
				OnAggressivePut();
			}
			else
			{
				if (!Controller.TrackOpen().Success)
				{
					Result.Code = ErrorCodes.TrackOpenTimeout;
					return Result;
				}
				PullFrom(PutLocation);
				if (!Controller.TrackClose().Success)
				{
					Result.Code = ErrorCodes.TrackCloseTimeout;
					return Result;
				}
				if (!Controller.RollerToPosition(RollerPosition.Position5, 5000).Success)
				{
					Controller.LogPickerSensorState(LogEntryType.Error);
					RuntimeService.SpinWait(100);
					Controller.StopRoller();
					Result.Code = ErrorCodes.PickerObstructed;
				}
				else
				{
					IPickerSensorReadResult pickerSensorReadResult2 = Controller.ReadSensors();
					if (!pickerSensorReadResult2.Sensors[0] && !pickerSensorReadResult2.Sensors[5])
					{
						OnSlotInUse();
					}
					else
					{
						Result.Code = ErrorCodes.PickerObstructed;
					}
				}
			}
			return Result;
		}
		finally
		{
			if (Result.Code != ErrorCodes.Success)
			{
				Observer.OnFailedPut(Result, Log);
			}
			else
			{
				Observer.OnSuccessfulPut(Result, Log);
			}
		}
	}

	internal PutOperation(string id, IPutObserver observer, ILocation putLocation, IFormattedLog log)
	{
		Result = new PutResult(id, putLocation);
		Log = log;
		PutLocation = putLocation;
		ID = id;
		Observer = observer;
	}

	private void OnAggressivePut()
	{
		Controller.TrackCycle();
		Controller.StartRollerIn();
		PushIntoSlot();
		Controller.StopRoller();
		IPickerSensorReadResult pickerSensorReadResult = Controller.ReadSensors();
		if (pickerSensorReadResult.Error != ErrorCodes.Success)
		{
			Result.Code = ErrorCodes.SensorReadError;
			return;
		}
		if (!pickerSensorReadResult.Sensors[0])
		{
			UpdateAggressivePutCounter();
			UpdateInventory();
			return;
		}
		Controller.TrackOpen();
		PushIntoSlot();
		Controller.TrackClose();
		pickerSensorReadResult = Controller.ReadSensors();
		if (pickerSensorReadResult.Error != ErrorCodes.Success)
		{
			Result.Code = ErrorCodes.SensorReadError;
			return;
		}
		if (!pickerSensorReadResult.Sensors[0])
		{
			UpdateInventory();
			UpdateAggressivePutCounter();
			return;
		}
		LogHelper.Instance.Log("AggressivePut not able to clear the disk.", LogEntryType.Error);
		Controller.LogPickerSensorState(LogEntryType.Error);
		PersistentCounterService.Increment("AggressivePutFailures");
		Controller.StartRollerOut();
		PullFrom(PutLocation);
		IControlResponse controlResponse = Controller.RollerToPosition(RollerPosition.Position5, 5000);
		LogHelper.Instance.Log(LogEntryType.Error, "[PUT] gripperClear: false, rollerTo 5 returned {0}", controlResponse.ToString());
		Controller.LogPickerSensorState(LogEntryType.Error);
		RuntimeService.SpinWait(100);
		Controller.StopRoller();
		LogHelper.Instance.Log("[PUT]: picker full = {0}", PickerBoard.IsFull());
		Result.Code = ErrorCodes.PickerObstructed;
	}

	private void OnSlotInUse()
	{
		PutLocation.ID = "UNKNOWN";
		InventoryService.Save(PutLocation);
		Result.Code = ErrorCodes.SlotInUse;
	}

	private void UpdateAggressivePutCounter()
	{
		LogHelper.Instance.Log("Aggressive PUT was able to clear disk.", LogEntryType.Info);
		PersistentCounterService.Increment("AggressivePutSuccesses");
	}

	private void UpdateInventory()
	{
		ILocation original = null;
		Result.IsDuplicate = InventoryService.IsBarcodeDuplicate(ID, out original);
		Result.StoredMatrix = (IsSuspiciousId(ID) ? "UNKNOWN" : ID);
		if (PutLocation.Deck == 7 && PutLocation.IsWide)
		{
			Result.StoredMatrix = "UNKNOWN";
		}
		if (Result.IsDuplicate)
		{
			Result.OriginalMatrixLocation = original;
			Result.StoredMatrix = (ControllerConfiguration.Instance.MarkDuplicatesUnknown ? "UNKNOWN" : string.Format("{0}{1}", ID, "(DUPLICATE)"));
			LogHelper.Instance.Log("The ID {0} is a duplicate ( original at Deck = {1} Slot = {2} ); marking as {3}", ID, original.Deck, original.Slot, Result.StoredMatrix);
			if (ControllerConfiguration.Instance.MarkOriginalMatrixUnknown)
			{
				LogHelper.Instance.Log("Mark the original matrix ID = {0} at Deck = {1} Slot = {2} ); marking as {3}", ID, original.Deck, original.Slot, "UNKNOWN");
				original.ID = "UNKNOWN";
				InventoryService.Save(original);
			}
			PersistentCounterService.Increment("DUPLICATE-COUNT");
		}
		PutLocation.ID = Result.StoredMatrix;
		InventoryService.Save(PutLocation);
	}

	private bool IsSuspiciousId(string id)
	{
		if (InventoryConstants.IsKnownInventoryToken(id))
		{
			return false;
		}
		try
		{
			return Enum.IsDefined(typeof(ErrorCodes), id);
		}
		catch (Exception)
		{
			return false;
		}
	}
}
