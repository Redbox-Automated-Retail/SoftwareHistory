using System;
using System.Collections.Generic;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class ControllerService
{
	private const int CommandRetries = 1;

	private const int SingleDiskInPickerLimit = 3;

	public static ControllerService Instance => Singleton<ControllerService>.Instance;

	public bool IsAtVendLocation { get; set; }

	public ControllerLocation Location { get; set; }

	private VendDoor VendDoor => VendDoor.Instance;

	private Track Track => Track.Instance;

	private Gripper Gripper => Gripper.Instance;

	private QLM QLM => QLM.Instance;

	private PickerBoard PickerBoard => PickerBoard.Instance;

	private AuxBoard AuxBoard => AuxBoard.Instance;

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
				PickerBoard.Instance.LogPickerSensorState(LogEntryType.Error);
			}
			PerfFunctions.SpinWait(milliseconds);
			rollerPosition = ((CenterDiskMethod.DrumAndBack == method) ? RollerPosition.Position5 : RollerPosition.Position3);
			bool flag2 = PickerBoard.RollerToPosition(rollerPosition, opTimeout);
			if (flag2)
			{
				string message2 = $"CenterItem: roll to position {rollerPosition.ToString()} timed out.";
				LogHelper.Instance.Log(message2, LogEntryType.Error);
				PickerBoard.Instance.LogPickerSensorState(LogEntryType.Error);
			}
			PerfFunctions.SpinWait(milliseconds);
			return flag2 || flag;
		}
		finally
		{
			Track.Cycle();
		}
	}

	public PushOutStatus GripperPushOut()
	{
		return GripperPushOutInner();
	}

	public PickerStatus GetPickerStatus()
	{
		return GetPickerStatusInner();
	}

	public bool ClearDiskFromPicker(RollerDirection direction)
	{
		if (direction == RollerDirection.In)
		{
			PickerBoard.Instance.StartRollerIn();
		}
		else if (RollerDirection.Out == direction)
		{
			PickerBoard.Instance.StartRollerOut();
		}
		return ClearDiskFromPicker();
	}

	public TransferResult Transfer(Location source, Location destination, IFormattedLog log)
	{
		TransferResult transferResult = new TransferResult();
		MotionResult motionResult = MoveTo(source, MoveMode.Get, log);
		if (motionResult != MotionResult.Success)
		{
			transferResult.FailedMoveResult = motionResult;
			return transferResult;
		}
		GetResult getResult = (transferResult.GetResult = Get(testOnEmpty: false, log));
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
			Deck deck = DecksManager.Instance.GetDeck(destination.Deck);
			bool flag = VMZ.Instance.LocIsInVMZ(destination);
			MerchFlags flags = MerchFlags.None;
			if (flag && VMZ.Instance.IsThinType(getResult3.Flags))
			{
				flags = getResult3.Flags;
			}
			deck.UpdateOnTransfer(destination.Slot, flags, getResult3.ReturnTime);
		}
		else if (MoveTo(source, MoveMode.Put, log) == MotionResult.Success && Put(getResult3.Previous, log).Success)
		{
			transferResult.ReturnedToSource = true;
			Deck deck2 = DecksManager.Instance.GetDeck(source.Deck);
			deck2.UpdateOnTransfer(source.Slot, getResult3.Flags, getResult3.ReturnTime);
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
		GetResult getResult = (transferResult.GetResult = Get(testOnEmpty: false, log));
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
				Deck deck = DecksManager.Instance.GetDeck(destination.Deck);
				bool flag2 = VMZ.Instance.LocIsInVMZ(destination);
				MerchFlags flags = MerchFlags.None;
				if (flag2 && VMZ.Instance.IsThinType(getResult3.Flags))
				{
					flags = getResult3.Flags;
				}
				deck.UpdateOnTransfer(destination.Slot, flags, getResult3.ReturnTime);
				flag = true;
				break;
			}
		}
		if (!flag && MoveTo(source, MoveMode.Put, log) == MotionResult.Success && Put(getResult3.Previous, log).Success)
		{
			transferResult.ReturnedToSource = true;
			Deck deck2 = DecksManager.Instance.GetDeck(source.Deck);
			deck2.UpdateOnTransfer(source.Slot, getResult3.Flags, getResult3.ReturnTime);
		}
		return transferResult;
	}

	public VendItemResult VendItem()
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
		if (GripperPushOutInner() == PushOutStatus.HardwareError)
		{
			vendItemResult2.PickerStatus = PickerStatus.HardwareError;
			return vendItemResult2;
		}
		Track.Cycle();
		int num = 0;
		for (int i = 0; i <= ControllerConfiguration.Instance.VendDiskPollCount; i++)
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
				if (num == 1 && ControllerConfiguration.Instance.TrackPushOutFailures)
				{
					vendItemResult2.Presented = false;
					LogHelper.Instance.Log("After 2 pushes, the disk has not cleared sensor 4.", LogEntryType.Info);
					PickerBoard.LogPickerSensorState(pickerSensorReadResult, LogEntryType.Error);
					vendItemResult2.PickerStatus = PickerStatus.DiscInPicker;
					return vendItemResult2;
				}
				num++;
				LogHelper.Instance.Log("Disc was pushed to the drum - push it back out.", LogEntryType.Error);
				PickerBoard.LogPickerSensorState(pickerSensorReadResult, LogEntryType.Error);
				Track.Cycle();
				GripperPushOutInner();
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
		vendItemResult2.PickerStatus = ((PickerBlockedStatus() == PickerDiskCount.Empty) ? PickerStatus.Empty : PickerStatus.DiscInPicker);
		return vendItemResult2;
	}

	public PickerStatus TakeDiskAtDoor()
	{
		LogHelper.Instance.Log(">TakeDiskAtDoor.", LogEntryType.Info);
		PickerBoard.StartRollerIn();
		PickerSensorReadResult pickerSensorReadResult = null;
		if (PickerBoard.WaitToBlockSensor(PickerSensor.Sensor5, 15000))
		{
			PickerBoard.StopRoller();
			pickerSensorReadResult = PickerBoard.ReadSensors();
			if (pickerSensorReadResult.ReadError)
			{
				return PickerStatus.HardwareError;
			}
			if (pickerSensorReadResult.BlockedCount() == 0)
			{
				LogHelper.Instance.Log("After 15 seconds, didn't get anything in the picker; exiting.", LogEntryType.Info);
				if (VendDoor.Close())
				{
					PickerBoard.LogInputs(LogEntryType.Info);
					return PickerStatus.Obstructed;
				}
				return PickerStatus.Empty;
			}
		}
		LogHelper.Instance.Log("Sensor 5 or 6 was tripped; pull into the picker.", LogEntryType.Info);
		if (PickerBoard.RollerToPosition(RollerPosition.Position3, 5000))
		{
			LogHelper.Instance.Log("Unable to roll the disk to sensor 3.", LogEntryType.Error);
			if (!PickerBoard.IsFull())
			{
				PickerBoard.Instance.LogPickerSensorState(LogEntryType.Info);
				return PickerStatus.Empty;
			}
			pickerSensorReadResult = PickerBoard.ReadSensors();
			PickerBoard.Instance.LogPickerSensorState(pickerSensorReadResult, LogEntryType.Info);
			return PickerStatus.Obstructed;
		}
		pickerSensorReadResult = PickerBoard.ReadSensors();
		if (pickerSensorReadResult.ReadError)
		{
			return PickerStatus.HardwareError;
		}
		if (pickerSensorReadResult.BlockedCount() > 3)
		{
			LogHelper.Instance.Log("TakeDiskAtDoor: the gripper is obstructed.", LogEntryType.Info);
			PickerBoard.LogPickerSensorState(pickerSensorReadResult, LogEntryType.Info);
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
			array = pickerSensorReadResult.Sensors;
			int num = pickerSensorReadResult.BlockedCount();
			LogHelper.Instance.Log($"REJECT DISC: Sensor 6 state is {pickerSensorReadResult.SensorStatus(5)} (blocked count = {num})", LogEntryType.Info);
			if (num == 0)
			{
				if (!VendDoor.Close())
				{
					return PickerStatus.Empty;
				}
				return PickerStatus.Obstructed;
			}
			array = pickerSensorReadResult.Sensors;
			bool flag = array[3] || array[2] || array[1] || array[0];
			if (array[5])
			{
				if (flag)
				{
					GripperPushOutInner();
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
				GripperPushOutInner();
				PerfFunctions.Wait(1500);
			}
		}
		return GetPickerStatusInner();
	}

	public GetResult Get(bool testOnEmpty, IFormattedLog log)
	{
		if (!Location.IsValid)
		{
			GetResult getResult = new GetResult();
			getResult.Code = GetResultCode.LocationOutOfRange;
			return getResult;
		}
		Deck deck = DecksManager.Instance.GetDeck(Location.Deck);
		GetResult inner = GetInner(deck, Location.Slot, testOnEmpty);
		if (inner.IsSlotEmpty)
		{
			string msg = $"GET Deck={deck.Number} Slot={Location.Slot} returned SLOTEMPTY";
			log.WriteFormatted(msg);
			return inner;
		}
		if (!inner.Success)
		{
			string msg2 = $"GET Deck={deck.Number} Slot={Location.Slot} returned error status {inner.ToString().ToUpper()}";
			log.WriteFormatted(msg2, LogEntryType.Error);
			return inner;
		}
		string msg3 = $"GET Deck={deck.Number} Slot={Location.Slot} ID={inner.Previous}";
		log.WriteFormatted(msg3);
		return inner;
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
			PickerBoard.Instance.LogPickerSensorState();
			PickerBoard.Instance.LogInputs(LogEntryType.Info);
			PutResult putResult2 = new PutResult();
			putResult2.Code = PutResultCode.PickerEmpty;
			return putResult2;
		}
		Deck deck = DecksManager.Instance.GetDeck(Location.Deck);
		int slot = Location.Slot;
		PutResult putResult3 = PutInner(deck, slot, id);
		if (DumpBin.Instance.IsBin(new SearchLocation(deck.Number, slot)))
		{
			string msg = (putResult3.Success ? $"PUT DUMPBIN ID={id}" : $"PUT DUMPBIN ID={id} returned error status {putResult3.ToString().ToUpper()}");
			log.WriteFormatted(msg);
			if (putResult3.Success)
			{
				DumpBin.Instance.AddToInventory(id);
				MoveTo(DumpBin.Instance.RotationTo(), MoveMode.None, log);
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
			UpdateInventoryOnPutInner(putResult3, id, out var _);
			string msg2 = $"PUT Deck={deck.Number} Slot={slot} ID={id}";
			log.WriteFormatted(msg2);
			return putResult3;
		}
		string format = $"PUT Deck={deck.Number} Slot={slot} ID={id} returned error status {putResult3.ToString().ToUpper()}";
		log.WriteFormatted(LogEntryType.Error, format);
		return putResult3;
	}

	public void UpdateInventoryOnPut(PutResult result, string it, out string prev)
	{
		UpdateInventoryOnPutInner(result, it, out prev);
	}

	public string Inspect(int deck, int slot)
	{
		Deck deck2 = DecksManager.Instance.GetDeck(deck);
		if (deck2 != null)
		{
			return deck2.GetIdForSlot(slot);
		}
		return "DECK OUT OF RANGE";
	}

	public Location Lookup(string id)
	{
		if (ControllerMode.Emulated == ControllerConfiguration.Instance.Mode)
		{
			return ControllerEmulationState.Instance.ExpectedLookupResult;
		}
		Location rv = null;
		DecksManager.Instance.ForAllDecksDo(delegate(Deck each)
		{
			if (each.IsQlmDeck)
			{
				return true;
			}
			int num = each.FindId(id);
			if (num == -1)
			{
				return true;
			}
			rv = new Location(each.Number, num);
			return false;
		});
		return rv;
	}

	public bool EmptyCountExceeds(int numberOfEmpty)
	{
		int emptyCount = 0;
		DecksManager.Instance.ForAllDecksDo(delegate(Deck deck)
		{
			if (!deck.IsQlmDeck)
			{
				emptyCount += deck.GetEmptyCount();
			}
			return true;
		});
		return emptyCount > numberOfEmpty;
	}

	public bool MachineIsFull()
	{
		return !Instance.EmptyCountExceeds(ControllerConfiguration.Instance.ReturnSlotBuffer);
	}

	public void SetAudio(bool on)
	{
		if (ControllerConfiguration.Instance.Mode != ControllerMode.Emulated)
		{
			CoreController.ExecuteSingleControllerCommand(on ? CommandType.AudioOn : CommandType.AudioOff);
		}
	}

	public bool Reset()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedControllerResetResult;
		}
		CoreResponse coreResponse = SerialBoard.Instance.Reset();
		using (MotionController motionController = MotionController.GetController())
		{
			motionController.InitializeCommunication();
		}
		return coreResponse.TimedOut;
	}

	public string GetMachineRevision(out string[] versions)
	{
		try
		{
			versions = SerialBoard.Instance.GetVersionInformation();
			if (versions[0].Equals("TIMEOUT") || versions[1].Equals("TIMEOUT"))
			{
				return "UNKNOWN";
			}
			bool flag = versions[0].IndexOf("REDBOX PICKER") != -1;
			if (flag && versions[1].StartsWith("314"))
			{
				return "C";
			}
			return flag ? "B" : "A";
		}
		catch (Exception arg)
		{
			LogHelper.Instance.Log($"GetmachineRevision got an exception: {arg}", LogEntryType.Info);
			versions = new string[3] { "TIMEOUT", "TIMEOUT", "TIMEOUT" };
			return "A";
		}
	}

	public MotionResult HomeAxis(Axis axis, IFormattedLog log)
	{
		using MotionController motionController = MotionController.GetController();
		MotionResult motionResult = motionController.HomeAxis(axis);
		if (motionResult != MotionResult.Success)
		{
			string msg = $"HOME{axis.ToString().ToUpper()} returned an error status {motionResult.ToString().ToUpper()}";
			log.WriteFormatted(msg, LogEntryType.Error);
		}
		return motionResult;
	}

	public string GetMotionControllerPosition(Axis axis)
	{
		string text = "MOTIONCONTROL ERROR";
		using MotionController motionController = MotionController.GetController();
		MoveTarget target = new MoveTarget
		{
			Axis = axis
		};
		if (!motionController.GetCurrentLocation(axis, ref target))
		{
			return text;
		}
		if (axis == Axis.X)
		{
			return (!target.XCoordinate.HasValue) ? text : target.XCoordinate.Value.ToString();
		}
		return (!target.YCoordinate.HasValue) ? text : target.YCoordinate.Value.ToString();
	}

	public MotionResult MoveToVend(MoveMode mode, IFormattedLog writer)
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			ResetLocation();
			IsAtVendLocation = ControllerEmulationState.Instance.ExpectedMoveToVendResult == MotionResult.Success;
			if (!IsAtVendLocation)
			{
				return MotionResult.Error;
			}
			return MotionResult.Success;
		}
		using MotionController motionController = MotionController.GetController();
		MotionResult motionResult = motionController.MoveToVend(mode);
		if (writer != null && motionResult != MotionResult.Success)
		{
			string text = $"MOVEVEND returned an error status {motionResult.ToString().ToUpper()}";
			writer.WriteFormatted(text);
			LogHelper.Instance.Log(text, LogEntryType.Error);
		}
		return motionResult;
	}

	public MotionResult MoveAbsolute(MoveTarget target, bool checkSensors)
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedMoveAbsoluteResult;
		}
		using MotionController motionController = MotionController.GetController();
		return checkSensors ? motionController.MoveToTarget(target) : motionController.MoveToTargetWithoutSensorCheck(target);
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
		using MotionController motionController = MotionController.GetController();
		if (deck2.IsQlmDeck)
		{
			MoveTarget target = new MoveTarget
			{
				Axis = Axis.X
			};
			if (motionController.GetCurrentLocation(Axis.X, ref target))
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
					motionController.MoveToTarget(target2);
					break;
				case MoveMode.Put:
					if (slotOffset < target.XCoordinate.Value)
					{
						motionController.MoveToTarget(target2);
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
		MotionResult motionResult = motionController.MoveToTarget(target3);
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

	public bool MoveToAbsoluteLocationIsValid(Axis axis, int? xOffset, int? yOffset)
	{
		return true;
	}

	public bool IsLocationSecure(Location loc)
	{
		return SpecialLocationLocatorService.Instance.FindByLocation(loc)?.IsSecureLocation ?? false;
	}

	public MotionResult MoveToLocation(string namedLocation, MoveMode mode, IFormattedLog log)
	{
		SpecialLocation specialLocation = SpecialLocationLocatorService.Instance.FindByName(namedLocation);
		if (specialLocation == null)
		{
			return MotionResult.LocationNotFound;
		}
		if (!specialLocation.CanMoveTo)
		{
			return MotionResult.InvalidMoveToSecureLocation;
		}
		if (specialLocation.Deck.HasValue)
		{
			Location location = specialLocation.MoveableLocation();
			if (location == null)
			{
				return MotionResult.LocationNotFound;
			}
			return MoveTo(location, mode, log);
		}
		MoveTarget target = new MoveTarget
		{
			Axis = Axis.Y,
			YCoordinate = specialLocation.Offset.Value
		};
		return MoveAbsolute(target, checkSensors: true);
	}

	public void ToggleRingLight(RinglightState state, int? sleepAfter)
	{
		if (ControllerConfiguration.Instance.Mode != ControllerMode.Emulated)
		{
			CoreController.ExecuteSingleControllerCommand((state == RinglightState.On) ? CommandType.RinglightOn : CommandType.RinglightOff);
			if (sleepAfter.HasValue)
			{
				PerfFunctions.Wait(sleepAfter.Value);
			}
		}
	}

	public bool IsGripperClear()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedIsGripperClearResult;
		}
		if (Track.CloseIfOpen())
		{
			return false;
		}
		for (int num = 2; num > 0; num--)
		{
			PickerEdgeCheckResult pickerEdgeCheckResult = CheckPickerEdges();
			if (PickerEdgeCheckResult.Clear != pickerEdgeCheckResult)
			{
				LogHelper.Instance.Log($" IsGripperClear: check edge result = {pickerEdgeCheckResult.ToString()}", LogEntryType.Info);
			}
			switch (pickerEdgeCheckResult)
			{
			case PickerEdgeCheckResult.JustOutOfSlot:
				try
				{
					PickerBoard.StartRollerIn();
					Gripper.PushWithArm(ControllerConfiguration.Instance.PushTime);
					Gripper.PushIntoSlot();
				}
				finally
				{
					PickerBoard.StopRoller();
				}
				break;
			case PickerEdgeCheckResult.OverhangEnds:
				return false;
			case PickerEdgeCheckResult.OverhangLeft:
				if (Gripper.GetFingerState() != GripperFingerState.Rent)
				{
					Gripper.SetFinger(GripperFingerState.Rent);
				}
				PickerBoard.RollerToPosition(RollerPosition.Position5, 8000);
				break;
			case PickerEdgeCheckResult.OverhangRight:
				if (Gripper.GetFingerState() != GripperFingerState.Rent)
				{
					Gripper.SetFinger(GripperFingerState.Rent);
				}
				try
				{
					if (Instance.IsAtVendLocation && !VendDoor.IsClosed())
					{
						VendDoor.Rent();
					}
					PickerBoard.RollerToPosition(RollerPosition.Position3, 3000);
				}
				finally
				{
					if (!VendDoor.IsClosed())
					{
						VendDoor.Close();
					}
				}
				break;
			default:
				return true;
			}
		}
		return false;
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
		if (!IsGripperClear())
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

	private static PickerEdgeCheckResult CheckPickerEdges()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedCheckPickerEdgesResult;
		}
		PickerEdgeCheckResult result = PickerEdgeCheckResult.Clear;
		bool[] sensors = PickerBoard.Instance.ReadSensors().Sensors;
		if (sensors[0] && !sensors[1] && !sensors[2] && !sensors[3] && !sensors[4] && !sensors[5])
		{
			result = PickerEdgeCheckResult.JustOutOfSlot;
		}
		else if (sensors[0] && sensors[5])
		{
			result = PickerEdgeCheckResult.OverhangEnds;
		}
		else if (sensors[0] && !sensors[5])
		{
			result = PickerEdgeCheckResult.OverhangLeft;
		}
		else if (sensors[5] && !sensors[0])
		{
			result = PickerEdgeCheckResult.OverhangRight;
		}
		return result;
	}

	private void UpdateAggressivePutCounter()
	{
		LogHelper.Instance.Log("Aggressive PUT was able to clear disk.", LogEntryType.Info);
		CounterManager.Instance.Increment("AggressivePutSuccesses");
	}

	private void UpdateInventoryOnPutInner(PutResult pr, string id, out string previous)
	{
		bool flag = false;
		string text = (Deck.IsSuspiciousId(id) ? "UNKNOWN" : id);
		Deck deck = DecksManager.Instance.GetDeck(Location.Deck);
		if (Location.Deck == 7 && deck.NumberOfSlots == 72 && Location.Slot % 6 == 0)
		{
			text = "UNKNOWN";
			flag = true;
		}
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			previous = ControllerEmulationState.Instance.ExpectedSetSlotIdPreviousIdResult;
			pr.IsDuplicate = ControllerEmulationState.Instance.ExpectedSetSlotIdIsDuplicateInventoryResult;
			return;
		}
		Location original = null;
		if (pr.IsDuplicate = ControllerConfiguration.Instance.IsBarcodeDuplicate(id, out original))
		{
			text = (ControllerConfiguration.Instance.MarkDuplicatesUnknown ? "UNKNOWN" : string.Format("{0}{1}", id, "(DUPLICATE)"));
			LogHelper.Instance.Log($"The ID {id} is a duplicate ( original at Deck = {original.Deck} Slot = {original.Slot} ); marking as {text}", LogEntryType.Info);
			if (ControllerConfiguration.Instance.MarkOriginalMatrixUnknown)
			{
				LogHelper.Instance.Log(string.Format("Mark the original matrix ID = {0} at Deck = {1} Slot = {2} ); marking as {3}", id, original.Deck, original.Slot, "UNKNOWN"), LogEntryType.Info);
				Deck deck2 = DecksManager.Instance.GetDeck(original.Deck);
				deck2.SetSlotId(original.Slot, "UNKNOWN");
			}
			CounterManager.Instance.Increment("DUPLICATE-COUNT");
		}
		Deck deck3 = DecksManager.Instance.GetDeck(Location.Deck);
		previous = deck3.SetSlotId(Location.Slot, text);
		if (flag)
		{
			previous = "UNKNOWN";
		}
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			pr.Code = ControllerEmulationState.Instance.ExpectedPutResult.Code;
		}
	}

	private bool ClearDiskFromPicker()
	{
		int milliseconds = 300;
		bool flag = PickerBoard.WaitToClear(PickerSensor.Sensor2, PickerSensor.Sensor5, 8000);
		if (!flag)
		{
			PerfFunctions.SpinWait(milliseconds);
			bool[] array = null;
			bool flag2 = true;
			for (int i = 0; i < 3; i++)
			{
				if (!flag2)
				{
					break;
				}
				if (flag2)
				{
					if (i > 0)
					{
						LogHelper.Instance.Log("2-5 didn't timeout; sensors show something in picker.", LogEntryType.Info);
					}
					PickerBoard.WaitToClear(PickerSensor.Sensor2, PickerSensor.Sensor5, 3000);
				}
				array = PickerBoard.ReadAsFlags();
				flag2 = array[1] || array[2] || array[3] || array[4];
				PerfFunctions.SpinWait(milliseconds);
			}
			array = PickerBoard.ReadAsFlags();
			flag = array[1] || array[2] || array[3] || array[4];
		}
		return flag;
	}

	private GetResult GetInner(Deck deck, int slot, bool testOnEmpty)
	{
		GetResult getResult = new GetResult();
		if (ControllerMode.Normal == ControllerConfiguration.Instance.Mode)
		{
			getResult.Previous = deck.GetIdForSlot(Location.Slot);
		}
		if (Track.CloseIfOpen())
		{
			getResult.HardwareError = PickerHardwareError.TrackCloseTimeout;
			return getResult;
		}
		if (PickerBoard.IsFull())
		{
			LogHelper.Instance.Log($">> ControllerService.Get: {GetResultCode.PickerFull.ToString()}", LogEntryType.Info);
			getResult.Code = GetResultCode.PickerFull;
			return getResult;
		}
		if (deck.IsSlotSellThru(slot) && ControllerConfiguration.Instance.SettleDiscInWideSlot)
		{
			Gripper.SettleDiskInSlot();
		}
		if (Track.OpenIfClosed())
		{
			getResult.HardwareError = PickerHardwareError.TrackOpenTimeout;
			return getResult;
		}
		PickerBoard.StartRollerOut();
		PickerHardwareError pickerHardwareError = Gripper.PullFromLocation(ControllerConfiguration.Instance.NumberOfPulls);
		if (pickerHardwareError != PickerHardwareError.Success)
		{
			PickerBoard.StopRoller();
			Track.Close();
			getResult.HardwareError = pickerHardwareError;
			return getResult;
		}
		if (Track.Close())
		{
			PickerBoard.StopRoller();
			getResult.HardwareError = PickerHardwareError.TrackCloseTimeout;
			return getResult;
		}
		bool[] array = PickerBoard.ReadAsFlags();
		if (array[0] && !array[1])
		{
			pickerHardwareError = Gripper.PullFromLocation(1);
		}
		PerfFunctions.SpinWait(300);
		if (PickerBoard.RollerToPosition(RollerPosition.Position4, 6000))
		{
			if (PickerBoard.IsFull())
			{
				LogHelper.Instance.Log("Disk did not make it to sensor 4.", LogEntryType.Error);
				PickerBoard.LogPickerSensorState(LogEntryType.Error);
				PickerBoard.StartRollerIn();
				try
				{
					bool flag = ClearDiskFromPicker();
					Gripper.PushIntoSlot();
					LogHelper.Instance.Log(string.Format("GET: couldn't get disc, ClearDiscFromPicker returned {0}", flag ? "TIMEOUT" : "SUCCESS"), LogEntryType.Error);
					PickerBoard.LogPickerSensorState(LogEntryType.Error);
					getResult.Code = (PickerBoard.IsFull() ? GetResultCode.PickerObstructed : GetResultCode.ItemStuck);
					if (testOnEmpty)
					{
						deck.UpdateEmptyStuck(slot);
					}
					return getResult;
				}
				finally
				{
					PickerBoard.StopRoller();
				}
			}
			LogHelper.Instance.Log("GET: no disk in picker after pull.", LogEntryType.Info);
			PickerBoard.LogPickerSensorState(LogEntryType.Info);
			getResult.Code = GetResultCode.SlotEmpty;
			if (ControllerConfiguration.Instance.TestSlotOnEmpty && testOnEmpty)
			{
				LocationTestResult locationTestResult = (getResult.PeekResult = Gripper.Peek());
				string message = $"GET disk at Deck = {deck.Number} Slot = {slot} shows SLOTEMPTY; PEEK returns {locationTestResult.ToString()}";
				LogHelper.Instance.Log(message, LogEntryType.Info);
				if (!locationTestResult.TestOk || locationTestResult.PeekResult == GripperPeekResult.Full)
				{
					deck.UpdateEmptyStuck(slot);
					getResult.Code = GetResultCode.ItemStuck;
					return getResult;
				}
			}
		}
		else if (!PickerBoard.IsFull())
		{
			LogHelper.Instance.Log("GET: wait to block 4,5 didn't time out, but there's nothing in the picker. Inventory issue or sensor problem?", LogEntryType.Error);
			PickerBoard.LogPickerSensorState(LogEntryType.Error);
			getResult.Code = GetResultCode.SlotEmpty;
		}
		deck.FillResult(getResult, slot, reset: true);
		return getResult;
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
		bool flag = ClearDiskFromPicker();
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
		PickerEdgeCheckResult pickerEdgeCheckResult = CheckPickerEdges();
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
			PickerEdgeCheckResult pickerEdgeCheckResult2 = CheckPickerEdges();
			if (PickerEdgeCheckResult.Clear == pickerEdgeCheckResult2)
			{
				UpdateAggressivePutCounter();
				return putResult;
			}
			Track.Open();
			Gripper.PushIntoSlot();
			Track.Close();
			pickerEdgeCheckResult2 = CheckPickerEdges();
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
		PickerBoard.Instance.LogPickerSensorState(LogEntryType.Error);
		PerfFunctions.SpinWait(100);
		PickerBoard.StopRoller();
		bool flag4 = PickerBoard.IsFull();
		LogHelper.Instance.Log($"PUT: move to sensor 5 returns {flag4}", LogEntryType.Info);
		putResult.Code = PutResultCode.PickerObstructed;
		return putResult;
	}

	private PushOutStatus GripperPushOutInner()
	{
		if (Track.CloseIfOpen())
		{
			LogHelper.Instance.Log("PushItemOut: unable to close the track.", LogEntryType.Error);
			return PushOutStatus.HardwareError;
		}
		for (int i = 0; i < 3; i++)
		{
			PickerSensorReadResult pickerSensorReadResult = PickerBoard.ReadSensors();
			if (pickerSensorReadResult.ReadError)
			{
				return PushOutStatus.HardwareError;
			}
			bool[] sensors = pickerSensorReadResult.Sensors;
			if (!sensors[0] && !sensors[1] && !sensors[2] && !sensors[3] && !sensors[4] && !sensors[5])
			{
				return PushOutStatus.Success;
			}
			if (sensors[3])
			{
				PickerBoard.StartRollerOut();
				PickerBoard.WaitToClearSensor(PickerSensor.Sensor4, 4000);
				if (ControllerConfiguration.Instance.PushOutSleepTime2 > 0)
				{
					PerfFunctions.Wait(ControllerConfiguration.Instance.PushOutSleepTime2);
				}
				PickerBoard.StopRoller();
				continue;
			}
			if (sensors[0] || sensors[1] || sensors[2])
			{
				if (PickerBoard.RollerToPosition(RollerPosition.Position4, 4000))
				{
					LogHelper.Instance.Log("PushOut: roller to 4 timed out.", LogEntryType.Error);
					PickerBoard.LogPickerSensorState(LogEntryType.Error);
					continue;
				}
				PerfFunctions.SpinWait(50);
				PickerBoard.StartRollerOut();
				PickerBoard.WaitToClearSensor(PickerSensor.Sensor4, 4000);
				PickerBoard.StopRoller();
				continue;
			}
			return PushOutStatus.Success;
		}
		PickerSensorReadResult pickerSensorReadResult2 = PickerBoard.ReadSensors();
		if (pickerSensorReadResult2.ReadError)
		{
			return PushOutStatus.HardwareError;
		}
		bool[] sensors2 = pickerSensorReadResult2.Sensors;
		if (sensors2[0] || sensors2[1] || sensors2[2] || sensors2[3])
		{
			return PushOutStatus.Failure;
		}
		return PushOutStatus.Success;
	}

	private PickerStatus GetPickerStatusInner()
	{
		PickerSensorReadResult pickerSensorReadResult = PickerBoard.ReadSensors();
		if (pickerSensorReadResult.ReadError)
		{
			return PickerStatus.HardwareError;
		}
		int num = pickerSensorReadResult.BlockedCount();
		if (num == 0)
		{
			if (!VendDoor.Close())
			{
				return PickerStatus.Empty;
			}
			return PickerStatus.Obstructed;
		}
		if (num > 3)
		{
			return PickerStatus.Obstructed;
		}
		if (!VendDoor.Close())
		{
			return PickerStatus.DiscInPicker;
		}
		return PickerStatus.Obstructed;
	}

	private PickerDiskCount PickerBlockedStatus()
	{
		PickerSensorReadResult pickerSensorReadResult = PickerBoard.ReadSensors();
		if (pickerSensorReadResult.ReadError)
		{
			return PickerDiskCount.HardwareError;
		}
		int num = pickerSensorReadResult.BlockedCount();
		if (num == 0)
		{
			return PickerDiskCount.Empty;
		}
		if (num > 3)
		{
			return PickerDiskCount.MulitpleDisk;
		}
		return PickerDiskCount.OneDisk;
	}
}
