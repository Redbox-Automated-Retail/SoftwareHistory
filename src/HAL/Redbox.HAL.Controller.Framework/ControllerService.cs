using System.Collections.Generic;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Controller.Framework;

public sealed class ControllerService
{
	public readonly ControllerLocation Location;

	public static ControllerService Instance => Singleton<ControllerService>.Instance;

	public bool IsAtVendLocation { get; private set; }

	public void ParkHardware()
	{
		MotionController.GetController().ClosePort();
		ControlSystem.Instance.ClosePort(null);
	}

	public bool CenterItem(CenterDiskMethod method)
	{
		if (method == CenterDiskMethod.None)
		{
			return false;
		}
		int milliseconds = 250;
		int opTimeout = 8000;
		try
		{
			RollerPosition rollerPosition = ((CenterDiskMethod.DrumAndBack == method) ? RollerPosition.Position1 : RollerPosition.Position6);
			bool flag = PickerBoard.RollerToPosition(rollerPosition, opTimeout);
			if (flag)
			{
				string message = $"CenterItem: roll to position {rollerPosition.ToString()} timed out.";
				LogHelper.Instance.Log(message, LogEntryType.Error);
				PickerBoard.LogPickerSensorState(LogEntryType.Error);
			}
			PerfFunctions.SpinWait(milliseconds);
			rollerPosition = ((CenterDiskMethod.DrumAndBack == method) ? RollerPosition.Position5 : RollerPosition.Position3);
			bool flag2 = PickerBoard.RollerToPosition(rollerPosition, opTimeout);
			if (flag2)
			{
				string message2 = $"CenterItem: roll to position {rollerPosition.ToString()} timed out.";
				LogHelper.Instance.Log(message2, LogEntryType.Error);
				PickerBoard.LogPickerSensorState(LogEntryType.Error);
			}
			PerfFunctions.SpinWait(milliseconds);
			return flag2 || flag;
		}
		finally
		{
			Track.Cycle();
		}
	}

	public PickerStatus GetPickerStatus()
	{
		return GetPickerStatusInner();
	}

	public TransferResult Transfer(Location source, Location destination, IFormattedLog log)
	{
		if (ControllerMode.Emulated == ControllerConfiguration.Instance.Mode)
		{
			return TransferResult.EmulatedSuccessResult(source, destination);
		}
		TransferResult transferResult = new TransferResult();
		MotionResult motionResult = MoveTo(source, MoveMode.Get, log);
		if (motionResult != MotionResult.Success)
		{
			transferResult.FailedMoveResult = motionResult;
			return transferResult;
		}
		GetResult getResult = (transferResult.GetResult = Get(log));
		GetResult getResult3 = getResult;
		if (!getResult3.Success)
		{
			return transferResult;
		}
		MotionResult motionResult2 = MoveTo(destination, MoveMode.Put, log);
		if (motionResult2 != MotionResult.Success)
		{
			transferResult.FailedMoveResult = motionResult2;
			return transferResult;
		}
		PutResult putResult = (transferResult.PutResult = Put(getResult3.Previous, log));
		PutResult putResult3 = putResult;
		if (putResult3.Success)
		{
			transferResult.TransferDone(source, destination);
			if (getResult3.ReturnTime.HasValue)
			{
				destination.UpdateReturnTime(getResult3.ReturnTime);
			}
		}
		else if (MoveTo(source, MoveMode.Put, log) == MotionResult.Success && Put(getResult3.Previous, log).Success)
		{
			transferResult.ReturnedToSource = true;
			Deck deck = DecksManager.Instance.GetDeck(source.Deck);
			deck.UpdateOnTransfer(source.Slot, getResult3.Flags, getResult3.ReturnTime);
		}
		return transferResult;
	}

	public TransferResult Transfer(Location source, List<Location> destinations, IFormattedLog log)
	{
		TransferResult transferResult = new TransferResult();
		MotionResult motionResult = MoveTo(source, MoveMode.Get, log);
		if (motionResult != MotionResult.Success)
		{
			transferResult.FailedMoveResult = motionResult;
			return transferResult;
		}
		GetResult getResult = (transferResult.GetResult = Get(log));
		GetResult getResult3 = getResult;
		if (!getResult3.Success)
		{
			return transferResult;
		}
		bool flag = false;
		foreach (Location destination in destinations)
		{
			MotionResult motionResult2 = MoveTo(destination, MoveMode.Put, log);
			if (motionResult2 != MotionResult.Success)
			{
				transferResult.FailedMoveResult = motionResult2;
				return transferResult;
			}
			PutResult putResult = (transferResult.PutResult = Put(getResult3.Previous, log));
			PutResult putResult3 = putResult;
			if (putResult3.Success)
			{
				transferResult.TransferDone(source, destination);
				if (getResult3.ReturnTime.HasValue)
				{
					destination.UpdateReturnTime(getResult3.ReturnTime);
				}
				flag = true;
				break;
			}
		}
		if (!flag && MoveTo(source, MoveMode.Put, log) == MotionResult.Success && Put(getResult3.Previous, log).Success)
		{
			transferResult.ReturnedToSource = true;
			Deck deck = DecksManager.Instance.GetDeck(source.Deck);
			deck.UpdateOnTransfer(source.Slot, getResult3.Flags, getResult3.ReturnTime);
		}
		return transferResult;
	}

	public VendItemResult VendItem(int pollCount)
	{
		VendItemResult vendItemResult = new VendItemResult();
		vendItemResult.Presented = true;
		vendItemResult.PickerStatus = PickerStatus.Empty;
		VendItemResult vendItemResult2 = vendItemResult;
		PickerBoard.RollerToPosition(RollerPosition.Position5, 3000);
		if (VendDoor.Rent())
		{
			vendItemResult2.PickerStatus = PickerStatus.HardwareError;
			return vendItemResult2;
		}
		LogHelper.Instance.Log(">> VendDiskAtDoor...", LogEntryType.Info);
		int ms = 1000;
		if (PickerBoard.PushOut() == PushOutStatus.HardwareError)
		{
			vendItemResult2.PickerStatus = PickerStatus.HardwareError;
			return vendItemResult2;
		}
		Track.Cycle();
		int num = 0;
		for (int i = 0; i <= pollCount; i++)
		{
			PickerSensorReadResult pickerSensorReadResult = PickerBoard.ReadSensors();
			if (pickerSensorReadResult.ReadError)
			{
				vendItemResult2.PickerStatus = PickerStatus.HardwareError;
				return vendItemResult2;
			}
			bool[] sensors = pickerSensorReadResult.Sensors;
			if (sensors[0] || sensors[1] || sensors[2] || sensors[3])
			{
				pickerSensorReadResult.LogSensorState(LogEntryType.Error);
				if (num == 1 && ControllerConfiguration.Instance.TrackPushOutFailures)
				{
					vendItemResult2.Presented = false;
					LogHelper.Instance.Log("After 2 pushes, the disk has not cleared sensor 4.", LogEntryType.Info);
					vendItemResult2.PickerStatus = PickerStatus.DiscInPicker;
					return vendItemResult2;
				}
				num++;
				LogHelper.Instance.Log("Disc was pushed to the drum - push it back out.", LogEntryType.Error);
				Track.Cycle();
				PickerBoard.PushOut();
				PerfFunctions.Wait(ms);
				continue;
			}
			num = 0;
			if (sensors[4])
			{
				if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
				{
					LogHelper.Instance.Log("The item is still blocking sensor 5.", LogEntryType.Info);
				}
				PerfFunctions.Wait(ms);
				continue;
			}
			if (!sensors[5])
			{
				vendItemResult2.PickerStatus = PickerStatus.Empty;
				return vendItemResult2;
			}
			PerfFunctions.Wait(ms);
		}
		vendItemResult2.PickerStatus = ((PickerBoard.PickerBlockedStatus() == PickerDiskCount.Empty) ? PickerStatus.Empty : PickerStatus.DiscInPicker);
		return vendItemResult2;
	}

	public VendItemResult VendItem()
	{
		return VendItem(ControllerConfiguration.Instance.VendDiskPollCount);
	}

	public PickerStatus TakeDiskAtDoor()
	{
		LogHelper.Instance.Log(">TakeDiskAtDoor.", LogEntryType.Info);
		PickerBoard.StartRollerIn();
		if (PickerBoard.WaitToBlockSensor(PickerSensor.Sensor5, 15000))
		{
			PickerBoard.StopRoller();
			PickerDiskCount pickerDiskCount = PickerBoard.PickerBlockedStatus();
			if (pickerDiskCount == PickerDiskCount.HardwareError)
			{
				return PickerStatus.HardwareError;
			}
			if (PickerDiskCount.Empty == pickerDiskCount)
			{
				LogHelper.Instance.Log("After 15 seconds, didn't get anything in the picker; exiting.", LogEntryType.Info);
				if (VendDoor.Close())
				{
					ControlSystem.Instance.LogInputs(ControlBoards.Picker, LogEntryType.Info);
					return PickerStatus.Obstructed;
				}
				return PickerStatus.Empty;
			}
		}
		LogHelper.Instance.Log("Sensor 5 or 6 was tripped; pull into the picker.", LogEntryType.Info);
		if (PickerBoard.RollerToPosition(RollerPosition.Position3, 5000))
		{
			LogHelper.Instance.Log("Unable to roll the disk to sensor 3.", LogEntryType.Error);
			Track.CloseIfOpen();
			PickerSensorReadResult pickerSensorReadResult = PickerBoard.ReadSensors();
			pickerSensorReadResult.LogSensorState();
			if (pickerSensorReadResult.IsFull)
			{
				return PickerStatus.Obstructed;
			}
			return PickerStatus.Empty;
		}
		PickerDiskCount pickerDiskCount2 = PickerBoard.PickerBlockedStatus();
		if (pickerDiskCount2 == PickerDiskCount.HardwareError)
		{
			return PickerStatus.HardwareError;
		}
		if (PickerDiskCount.MulitpleDisk == pickerDiskCount2)
		{
			LogHelper.Instance.Log("TakeDiskAtDoor: the gripper is obstructed.", LogEntryType.Info);
			PickerBoard.LogPickerSensorState(LogEntryType.Info);
			return PickerStatus.Obstructed;
		}
		if (!VendDoor.Close())
		{
			return PickerStatus.DiscInPicker;
		}
		return PickerStatus.Obstructed;
	}

	public PickerStatus RejectDisk(int tries)
	{
		LogHelper.Instance.Log($"RejectDisk: called with {tries} tries", LogEntryType.Info);
		int ms = 1000;
		if (VendDoor.Rent())
		{
			LogHelper.Instance.Log("RejectDisc: unable to close the vend door.", LogEntryType.Error);
			return PickerStatus.HardwareError;
		}
		PickerSensorReadResult pickerSensorReadResult = null;
		bool[] array = null;
		while (tries-- >= 0)
		{
			Track.Cycle();
			pickerSensorReadResult = PickerBoard.ReadSensors();
			if (pickerSensorReadResult.ReadError)
			{
				return PickerStatus.HardwareError;
			}
			if (pickerSensorReadResult.BlockedCount == 0)
			{
				if (!VendDoor.Close())
				{
					return PickerStatus.Empty;
				}
				return PickerStatus.Obstructed;
			}
			LogHelper.Instance.Log("[RejectDisk] The picker is not clear.");
			pickerSensorReadResult.LogSensorState();
			array = pickerSensorReadResult.Sensors;
			bool flag = array[3] || array[2] || array[1] || array[0];
			if (array[5])
			{
				if (flag)
				{
					PickerBoard.PushOut();
					PerfFunctions.Wait(ms);
					continue;
				}
				if (!PickerBoard.RollerToPosition(RollerPosition.Position3, 8000))
				{
					PickerStatus pickerStatusInner = GetPickerStatusInner();
					if (pickerStatusInner != PickerStatus.Obstructed)
					{
						return pickerStatusInner;
					}
				}
				PerfFunctions.Wait(ms);
			}
			else if (flag)
			{
				PickerBoard.PushOut();
				PerfFunctions.Wait(1500);
			}
		}
		return GetPickerStatusInner();
	}

	public GetResult Get(IFormattedLog log)
	{
		return Get(DefaultGetObserver.Instance, log);
	}

	public GetResult Get(IGetObserver observer, IFormattedLog log)
	{
		if (!Location.IsValid)
		{
			GetResult getResult = new GetResult();
			getResult.UpdateCode(GetResultCode.LocationOutOfRange);
			return getResult;
		}
		using GetOperation getOperation = new GetOperation(observer, log);
		GetResult getResult2 = getOperation.Execute();
		int deck = Location.Deck;
		int slot = Location.Slot;
		if (getResult2.IsSlotEmpty)
		{
			string msg = $"GET Deck={deck} Slot={slot} returned SLOTEMPTY";
			log.WriteFormatted(msg);
			return getResult2;
		}
		if (!getResult2.Success)
		{
			string msg2 = $"GET Deck={deck} Slot={slot} returned error status {getResult2.ToString().ToUpper()}";
			log.WriteFormatted(msg2, LogEntryType.Error);
			return getResult2;
		}
		string msg3 = $"GET Deck={deck} Slot={slot} ID={getResult2.Previous}";
		log.WriteFormatted(msg3);
		return getResult2;
	}

	public PutResult Put(string id, IFormattedLog log)
	{
		if (!Location.IsValid)
		{
			PutResult putResult = new PutResult();
			putResult.Code = PutResultCode.LocationOutOfRange;
			return putResult;
		}
		if (!PickerBoard.IsFull())
		{
			LogHelper.Instance.Log("PUT: picker is empty - returning PickerEmpty.", LogEntryType.Error);
			PickerBoard.LogPickerSensorState();
			ControlSystem.Instance.LogInputs(ControlBoards.Picker, LogEntryType.Info);
			PutResult putResult2 = new PutResult();
			putResult2.Code = PutResultCode.PickerEmpty;
			return putResult2;
		}
		Deck deck = DecksManager.Instance.GetDeck(Location.Deck);
		int slot = Location.Slot;
		PutResult putResult3 = PutInner(deck, slot, id);
		if (DumpBin.Instance.IsBin(Location))
		{
			string msg = (putResult3.Success ? $"PUT DUMPBIN ID={id}" : $"PUT DUMPBIN ID={id} returned error status {putResult3.ToString().ToUpper()}");
			log.WriteFormatted(msg);
			if (putResult3.Success)
			{
				DumpBin.Instance.AddToInventory(id);
				MoveTo(DumpBin.Instance.RotationLocation, MoveMode.None, log);
				if (deck.GetIdForSlot(slot) != "EMPTY")
				{
					deck.ResetLocation(slot);
				}
				LogHelper.Instance.Log("An item was just placed to the bin: current count = {0}", DumpBin.Instance.CurrentCount());
			}
			return putResult3;
		}
		if (putResult3.Success)
		{
			InventoryService.Instance.UpdateInventoryOnPut(putResult3, Location, id, out var _);
			string msg2 = $"PUT Deck={deck.Number} Slot={slot} ID={id}";
			log.WriteFormatted(msg2);
			return putResult3;
		}
		string format = $"PUT Deck={deck.Number} Slot={slot} ID={id} returned error status {putResult3.ToString().ToUpper()}";
		log.WriteFormatted(LogEntryType.Error, format);
		return putResult3;
	}

	public bool Reset()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedControllerResetResult;
		}
		CoreResponse coreResponse = ControlSystem.Instance.Initialize();
		MotionController controller = MotionController.GetController();
		controller.InitializeCommunication();
		return coreResponse.TimedOut;
	}

	public MotionResult HomeAxis(Axis axis, IFormattedLog log)
	{
		MotionController controller = MotionController.GetController();
		MotionResult motionResult = controller.HomeAxis(axis);
		if (motionResult != MotionResult.Success)
		{
			string msg = $"HOME{axis.ToString().ToUpper()} returned an error status {motionResult.ToString().ToUpper()}";
			log.WriteFormatted(msg, LogEntryType.Error);
		}
		return motionResult;
	}

	public string GetMotionControllerPosition(Axis axis)
	{
		string result = "MOTIONCONTROL ERROR";
		MotionController controller = MotionController.GetController();
		MoveTarget target = new MoveTarget
		{
			Axis = axis
		};
		if (!controller.GetCurrentLocation(axis, ref target))
		{
			return result;
		}
		if (axis == Axis.X)
		{
			if (target.XCoordinate.HasValue)
			{
				return target.XCoordinate.Value.ToString();
			}
			return result;
		}
		if (target.YCoordinate.HasValue)
		{
			return target.YCoordinate.Value.ToString();
		}
		return result;
	}

	public MotionResult MoveToVend(MoveMode mode, IFormattedLog writer)
	{
		ResetLocation();
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			IsAtVendLocation = ControllerEmulationState.Instance.ExpectedMoveToVendResult == MotionResult.Success;
			if (!IsAtVendLocation)
			{
				return MotionResult.Error;
			}
			return MotionResult.Success;
		}
		MotionController controller = MotionController.GetController();
		MotionResult motionResult = controller.MoveToVend(mode);
		if (writer != null && motionResult != MotionResult.Success)
		{
			string text = $"MOVEVEND returned an error status {motionResult.ToString().ToUpper()}";
			writer.WriteFormatted(text);
			LogHelper.Instance.Log(text, LogEntryType.Error);
		}
		IsAtVendLocation = MotionResult.Success == motionResult;
		return motionResult;
	}

	public MotionResult MoveAbsolute(MoveTarget target, bool checkSensors)
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedMoveAbsoluteResult;
		}
		MotionController controller = MotionController.GetController();
		if (!checkSensors)
		{
			return controller.MoveToTargetWithoutSensorCheck(target);
		}
		return controller.MoveToTarget(target);
	}

	public MotionResult MoveTo(Location location, MoveMode mode, IFormattedLog log)
	{
		return MoveTo(location.Deck, location.Slot, mode, log);
	}

	public MotionResult MoveTo(int deck, int slot, MoveMode mode, IFormattedLog log)
	{
		Deck deck2 = DecksManager.Instance.GetDeck(deck);
		if (deck2 == null)
		{
			return MotionResult.DeckOutOfRange;
		}
		if (!deck2.IsSlotValid(slot))
		{
			return MotionResult.SlotOutOfRange;
		}
		int num = deck2.YOffset;
		int num2 = (deck2.IsQlmDeck ? ControllerConfiguration.Instance.QlmYOffset : 50);
		switch (mode)
		{
		case MoveMode.Put:
			num += num2 * (int)ControllerConfiguration.Instance.GearY.GetStepRatio();
			break;
		case MoveMode.Get:
			num -= num2 * (int)ControllerConfiguration.Instance.GearY.GetStepRatio();
			break;
		}
		int slotOffset = deck2.GetSlotOffset(slot);
		MotionController controller = MotionController.GetController();
		if (deck2.IsQlmDeck)
		{
			MoveTarget target = new MoveTarget
			{
				Axis = Axis.X
			};
			if (controller.GetCurrentLocation(Axis.X, ref target))
			{
				int value = slotOffset - ControllerConfiguration.Instance.QlmApproachOffset;
				MoveTarget target2 = new MoveTarget
				{
					Axis = Axis.XY,
					XCoordinate = value,
					YCoordinate = num
				};
				switch (mode)
				{
				case MoveMode.None:
				case MoveMode.Get:
					controller.MoveToTarget(target2);
					break;
				case MoveMode.Put:
					if (slotOffset < target.XCoordinate.Value)
					{
						controller.MoveToTarget(target2);
					}
					break;
				}
			}
		}
		MoveTarget target3 = new MoveTarget
		{
			Axis = Axis.XY,
			XCoordinate = slotOffset,
			YCoordinate = num
		};
		MotionResult motionResult = controller.MoveToTarget(target3);
		if (motionResult == MotionResult.Success)
		{
			IsAtVendLocation = false;
			Location.Deck = deck;
			Location.Slot = slot;
			return MotionResult.Success;
		}
		if (log != null)
		{
			string text = $"MOVE Deck ={deck} Slot = {slot} returned an error status {motionResult.ToString().ToUpper()}";
			log.WriteFormatted(text);
			LogHelper.Instance.Log(text, LogEntryType.Error);
		}
		return motionResult;
	}

	internal bool CanMove(out MotionResult result)
	{
		result = MotionResult.Success;
		if (VendDoor.Close())
		{
			LogHelper.Instance.Log("MOVE: could not close the vend door.", LogEntryType.Error);
			result = MotionResult.VendDoorNotClosed;
			return false;
		}
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return true;
		}
		if (!Gripper.IsClear())
		{
			result = MotionResult.ObstructionDetected;
			LogHelper.Instance.Log("The picker is blocked - cannot move.", LogEntryType.Error);
			PickerBoard.LogPickerSensorState(LogEntryType.Error);
			return false;
		}
		return Gripper.CheckForMove(out result);
	}

	internal void ResetLocation()
	{
		Location.Invalidate();
		IsAtVendLocation = false;
	}

	private ControllerService()
	{
		Location = new ControllerLocation();
	}

	private void UpdateAggressivePutCounter()
	{
		LogHelper.Instance.Log("Aggressive PUT was able to clear disk.", LogEntryType.Info);
		CounterManager.Instance.Increment("AggressivePutSuccesses");
	}

	private PutResult PutInner(Deck deck, int slot, string id)
	{
		PutResult putResult = new PutResult();
		if (deck.IsSlotSellThru(slot) && ControllerConfiguration.Instance.SettleDiscInWideSlot)
		{
			Gripper.SettleDiskInSlot();
		}
		if (Track.CloseIfOpen())
		{
			putResult.HardwareError = PickerHardwareError.TrackCloseTimeout;
			return putResult;
		}
		if (Track.Open())
		{
			putResult.HardwareError = PickerHardwareError.TrackOpenTimeout;
			return putResult;
		}
		if (Track.Close())
		{
			putResult.HardwareError = PickerHardwareError.TrackCloseTimeout;
			return putResult;
		}
		if (Gripper.RetractArm())
		{
			putResult.HardwareError = PickerHardwareError.GripperRetractTimeout;
			return putResult;
		}
		if (Gripper.SetFinger(GripperFingerState.Rent))
		{
			LogHelper.Instance.Log("PUT: SetGripperFinger to Rent timed out.", LogEntryType.Error);
			putResult.HardwareError = PickerHardwareError.GripperRentTimeout;
			return putResult;
		}
		PickerBoard.StartRollerIn();
		bool flag = PickerBoard.ClearDiskFromPicker();
		PerfFunctions.SpinWait(100);
		if (flag)
		{
			LogHelper.Instance.Log($"PUT: Disk cannot be placed into Deck = {deck.Number} Slot = {slot} because it cannot clear sensor 2.", LogEntryType.Error);
			PickerBoard.LogPickerSensorState(LogEntryType.Error);
			PickerBoard.StopRoller();
			PerfFunctions.SpinWait(100);
			if (Track.Open())
			{
				putResult.HardwareError = PickerHardwareError.TrackOpenTimeout;
				return putResult;
			}
			Track.Close();
			PerfFunctions.SpinWait(100);
			bool flag2 = PickerBoard.RollerToPosition(RollerPosition.Position5, 8000);
			if (flag2)
			{
				LogHelper.Instance.Log(string.Format("Put: roll item back to sensor 5 returned {0}", flag2 ? "FAILURE" : "SUCCESS"), LogEntryType.Info);
				PickerBoard.StopRoller();
			}
			PerfFunctions.SpinWait(100);
			if (flag2)
			{
				LogHelper.Instance.Log("PUT: unable to roll item into slot, and cannot move it back into the picker.", LogEntryType.Error);
				putResult.Code = PutResultCode.PickerObstructed;
				return putResult;
			}
			Deck deck2 = DecksManager.Instance.GetDeck(Location.Deck);
			deck2.MarkSlotUnknown(Location.Slot);
			putResult.Code = PutResultCode.SlotInUse;
			return putResult;
		}
		if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
		{
			LogHelper.Instance.Log("Sensor wait for 2, 5 didn't time out.", LogEntryType.Info);
		}
		GripperErrorModes gripperErrorModes = Gripper.PushIntoSlot();
		if (gripperErrorModes != GripperErrorModes.Success)
		{
			LogHelper.Instance.Log($"PUT: item_cleared_gripper: PushIntoSlot returned {gripperErrorModes.ToString().ToUpper()}", LogEntryType.Error);
		}
		PickerBoard.StopRoller();
		PickerEdgeCheckResult pickerEdgeCheckResult = Gripper.CheckPickerEdges();
		if (PickerEdgeCheckResult.Clear == pickerEdgeCheckResult)
		{
			return putResult;
		}
		LogHelper.Instance.Log("PUT: can't put disk in slot.", LogEntryType.Error);
		PickerBoard.LogPickerSensorState(LogEntryType.Error);
		if (PickerEdgeCheckResult.JustOutOfSlot == pickerEdgeCheckResult && ControllerConfiguration.Instance.AggressiveClearPickerOnPut)
		{
			Track.Cycle();
			PickerBoard.StartRollerIn();
			Gripper.PushIntoSlot();
			PickerBoard.StopRoller();
			PickerEdgeCheckResult pickerEdgeCheckResult2 = Gripper.CheckPickerEdges();
			if (PickerEdgeCheckResult.Clear == pickerEdgeCheckResult2)
			{
				UpdateAggressivePutCounter();
				return putResult;
			}
			Track.Open();
			Gripper.PushIntoSlot();
			Track.Close();
			pickerEdgeCheckResult2 = Gripper.CheckPickerEdges();
			if (PickerEdgeCheckResult.Clear == pickerEdgeCheckResult2)
			{
				UpdateAggressivePutCounter();
				return putResult;
			}
			LogHelper.Instance.Log("AggressivePut not able to clear the disk.", LogEntryType.Error);
			PickerBoard.LogPickerSensorState(LogEntryType.Error);
			CounterManager.Instance.Increment("AggressivePutFailures");
		}
		PickerBoard.StartRollerOut();
		Gripper.PullFromLocation(ControllerConfiguration.Instance.NumberOfPulls);
		bool flag3 = PickerBoard.RollerToPosition(RollerPosition.Position5, 5000);
		LogHelper.Instance.Log($"Put: gripperClear: false, rollerTo 5 returned {flag3}", LogEntryType.Error);
		PickerBoard.LogPickerSensorState(LogEntryType.Error);
		PerfFunctions.SpinWait(100);
		PickerBoard.StopRoller();
		bool flag4 = PickerBoard.IsFull();
		LogHelper.Instance.Log($"PUT: move to sensor 5 returns {flag4}", LogEntryType.Info);
		putResult.Code = PutResultCode.PickerObstructed;
		return putResult;
	}

	private PickerStatus GetPickerStatusInner()
	{
		PickerDiskCount pickerDiskCount = PickerBoard.PickerBlockedStatus();
		if (pickerDiskCount == PickerDiskCount.HardwareError)
		{
			return PickerStatus.HardwareError;
		}
		if (PickerDiskCount.Empty == pickerDiskCount)
		{
			if (!VendDoor.Close())
			{
				return PickerStatus.Empty;
			}
			return PickerStatus.Obstructed;
		}
		if (PickerDiskCount.MulitpleDisk == pickerDiskCount)
		{
			return PickerStatus.Obstructed;
		}
		if (!VendDoor.Close())
		{
			return PickerStatus.DiscInPicker;
		}
		return PickerStatus.Obstructed;
	}
}
