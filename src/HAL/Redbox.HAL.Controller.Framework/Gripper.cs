using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Controller.Framework;

public static class Gripper
{
	private const int OpTries = 2;

	public static CoreResponse TestFinger(GripperFingerState state)
	{
		return SetFingerInner(state);
	}

	public static CoreResponse TestRetract()
	{
		return RetractInner();
	}

	public static CoreResponse TestExtend()
	{
		CoreResponse coreResponse = ExtendToSensorInner(null, check: true);
		if (coreResponse.TimedOut)
		{
			RetractInner();
		}
		return coreResponse;
	}

	public static void TimedExtend(int? timeout)
	{
		TimedExtendInner((!timeout.HasValue) ? ControllerConfiguration.Instance.PushTime : timeout.Value);
	}

	public static bool ExtendArmToSensor()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedSetGripperArmResult;
		}
		return ExtendToSensorInner(null, check: true).TimedOut;
	}

	public static bool PushToSensor(int timeout)
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedSetGripperArmResult;
		}
		return ExtendToSensorInner(timeout, check: false).TimedOut;
	}

	public static bool RetractArm()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedSetGripperArmResult;
		}
		return RetractInner().TimedOut;
	}

	public static bool SetFinger(GripperFingerState state)
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return false;
		}
		return SetFingerInner(state).TimedOut;
	}

	public static bool IsClear()
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
					PushWithArm(ControllerConfiguration.Instance.PushTime);
					PushIntoSlot();
				}
				finally
				{
					PickerBoard.StopRoller();
				}
				break;
			case PickerEdgeCheckResult.OverhangEnds:
				return false;
			case PickerEdgeCheckResult.OverhangLeft:
				SetFinger(GripperFingerState.Rent);
				PickerBoard.RollerToPosition(RollerPosition.Position5, 8000);
				break;
			case PickerEdgeCheckResult.OverhangRight:
				SetFinger(GripperFingerState.Rent);
				try
				{
					if (ControllerService.Instance.IsAtVendLocation && !VendDoor.IsClosed())
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

	public static bool ClearFront()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return true;
		}
		if (SetFingerInner(GripperFingerState.Open).TimedOut)
		{
			return false;
		}
		if (RetractInner().TimedOut)
		{
			SetFingerInner(GripperFingerState.Rent);
			if (RetractInner().TimedOut)
			{
				return false;
			}
		}
		if (SetFingerInner(GripperFingerState.Closed).TimedOut)
		{
			return false;
		}
		if (ControllerConfiguration.Instance.GripperToSensorOnPut)
		{
			ExtendToSensorInner(ControllerConfiguration.Instance.PushTime, check: false);
		}
		else
		{
			TimedExtendInner(ControllerConfiguration.Instance.PushTime);
		}
		if (SetFingerInner(GripperFingerState.Rent).TimedOut)
		{
			return false;
		}
		if (RetractInner().TimedOut)
		{
			return false;
		}
		if (SetFingerInner(GripperFingerState.Closed).TimedOut)
		{
			return false;
		}
		if (ControllerConfiguration.Instance.GripperToSensorOnPut)
		{
			ExtendToSensorInner(ControllerConfiguration.Instance.PushTime, check: false);
		}
		else
		{
			TimedExtendInner(ControllerConfiguration.Instance.PushTime);
		}
		if (RetractInner().TimedOut)
		{
			return false;
		}
		if (SetFingerInner(GripperFingerState.Rent).TimedOut)
		{
			return false;
		}
		return true;
	}

	public static LocationTestResult Peek()
	{
		LogHelper.Instance.Log(">> ControllerService.GripperPeek", LogEntryType.Info);
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			LocationTestResult locationTestResult = new LocationTestResult(1, 1);
			locationTestResult.PeekResult = ControllerEmulationState.Instance.ExpectedGripperPeekResult;
			return locationTestResult;
		}
		LocationTestResult locationTestResult2 = new LocationTestResult(ControllerService.Instance.Location.Deck, ControllerService.Instance.Location.Slot);
		if (ControllerService.Instance.Location.IsWideSlot)
		{
			PickerHardwareError pickerHardwareError = SettleDiskInSlot();
			if (pickerHardwareError != PickerHardwareError.Success)
			{
				locationTestResult2.Error = pickerHardwareError;
				return locationTestResult2;
			}
		}
		if (SetFingerInner(GripperFingerState.Closed).TimedOut)
		{
			locationTestResult2.Error = PickerHardwareError.GripperCloseTimeout;
			return locationTestResult2;
		}
		bool timedOut = ExtendToSensorInner(ControllerConfiguration.Instance.TestExtendTime, check: true).TimedOut;
		if (RetractInner().TimedOut)
		{
			locationTestResult2.Error = PickerHardwareError.GripperRetractTimeout;
			return locationTestResult2;
		}
		if (SetFingerInner(GripperFingerState.Rent).TimedOut)
		{
			locationTestResult2.Error = PickerHardwareError.GripperRentTimeout;
			return locationTestResult2;
		}
		locationTestResult2.Error = PickerHardwareError.Success;
		locationTestResult2.PeekResult = ((!timedOut) ? GripperPeekResult.Empty : GripperPeekResult.Full);
		return locationTestResult2;
	}

	public static GripperErrorModes PushInDisk()
	{
		return PushIntoSlot();
	}

	internal static GripperErrorModes PushWithArm(int timeout)
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return GripperErrorModes.Success;
		}
		if (SetFingerInner(GripperFingerState.Closed).TimedOut)
		{
			return GripperErrorModes.GripperFingerMovementFailure;
		}
		if (ControllerConfiguration.Instance.GripperToSensorOnPut)
		{
			ExtendToSensorInner(timeout, check: false);
		}
		else
		{
			TimedExtendInner(timeout);
		}
		if (SetFingerInner(GripperFingerState.Rent).TimedOut)
		{
			return GripperErrorModes.GripperFingerMovementFailure;
		}
		if (RetractInner().TimedOut)
		{
			return GripperErrorModes.GripperArmMovementFailure;
		}
		return GripperErrorModes.Success;
	}

	internal static GripperErrorModes PushIntoSlot()
	{
		GripperErrorModes gripperErrorModes = PushWithArm(ControllerConfiguration.Instance.RollInExtendTime);
		if (gripperErrorModes != GripperErrorModes.Success)
		{
			return gripperErrorModes;
		}
		gripperErrorModes = PushWithArm(ControllerConfiguration.Instance.PushTime);
		if (gripperErrorModes != GripperErrorModes.Success)
		{
			return gripperErrorModes;
		}
		if (ControllerConfiguration.Instance.AdditionalPutPush)
		{
			PushWithArm(ControllerConfiguration.Instance.PushTime);
		}
		return GripperErrorModes.Success;
	}

	internal static PickerHardwareError PullFromLocation(int pulls)
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return PickerHardwareError.Success;
		}
		GripperFingerState gripperFingerState = ComputeFingerStateForGet();
		for (int i = 0; i < pulls; i++)
		{
			if (SetFingerInner(gripperFingerState).TimedOut)
			{
				if (gripperFingerState != GripperFingerState.Open)
				{
					return PickerHardwareError.GripperRentTimeout;
				}
				return PickerHardwareError.GripperOpenTimeout;
			}
			if (ControllerService.Instance.Location.IsQlmDeck)
			{
				QLM.ExtendForGet();
			}
			else if (ExtendToSensorInner(null, check: true).TimedOut)
			{
				ResetForSecondExtend();
				if (ExtendToSensorInner(null, check: true).TimedOut)
				{
					RetractInner();
					return PickerHardwareError.GripperExtendTimeout;
				}
			}
			if (SetFingerInner(GripperFingerState.Closed).TimedOut)
			{
				return PickerHardwareError.GripperCloseTimeout;
			}
			if (RetractInner().TimedOut)
			{
				return PickerHardwareError.GripperRetractTimeout;
			}
		}
		if (SetFingerInner(GripperFingerState.Rent).TimedOut)
		{
			return PickerHardwareError.GripperRentTimeout;
		}
		return PickerHardwareError.Success;
	}

	internal static PickerHardwareError SettleDiskInSlot()
	{
		try
		{
			if (SetFingerInner(GripperFingerState.Open).TimedOut)
			{
				return PickerHardwareError.GripperOpenTimeout;
			}
			if (ExtendToSensorInner(null, check: true).TimedOut)
			{
				ResetForSecondExtend();
				if (ExtendToSensorInner(null, check: true).TimedOut)
				{
					RetractInner();
					return PickerHardwareError.GripperExtendTimeout;
				}
			}
			if (SetFingerInner(GripperFingerState.Closed).TimedOut)
			{
				return PickerHardwareError.GripperCloseTimeout;
			}
			if (SetFingerInner(GripperFingerState.Open).TimedOut)
			{
				return PickerHardwareError.GripperOpenTimeout;
			}
			if (RetractInner().TimedOut)
			{
				return PickerHardwareError.GripperRetractTimeout;
			}
			return PickerHardwareError.Success;
		}
		finally
		{
			SetFingerInner(GripperFingerState.Open);
			RetractInner();
		}
	}

	internal static bool CheckForMove(out MotionResult result)
	{
		result = MotionResult.Success;
		string[] array = ControlSystem.Instance.ReadInputs(ControlBoards.Picker);
		if (array.Length == 0)
		{
			return false;
		}
		if (array[1] == "0")
		{
			if (ControllerConfiguration.Instance.GripperRentOnMove && array[6] == "0" && SetFinger(GripperFingerState.Rent))
			{
				LogHelper.Instance.Log("MOVE: could not set the gripper fingers to rent.", LogEntryType.Error);
				result = MotionResult.GripperRentTimeout;
				return false;
			}
			if (RetractArm())
			{
				LogHelper.Instance.Log("MOVE: could not retract the gripper arm.", LogEntryType.Error);
				result = MotionResult.GripperNotRetracted;
				return false;
			}
		}
		if (!ControllerConfiguration.Instance.CheckGripperArmSensorsOnMove)
		{
			return true;
		}
		if (array[0] == "1" && array[1] == "1")
		{
			LogHelper.Instance.Log("MOVE: can't move because extend/retract both triggered.", LogEntryType.Error);
			ControlSystem.Instance.LogInputs(ControlBoards.Picker, LogEntryType.Error);
			result = MotionResult.SensorError;
			return false;
		}
		return true;
	}

	internal static void TimedExtendInner(int timeout)
	{
		try
		{
			if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
			{
				LogHelper.Instance.Log($"ExtendGripperArmForTime {timeout} ms", LogEntryType.Debug);
			}
			ControlSystem.Instance.ExecuteControllerCommand(CommandType.ExtendGripperArmForTime);
			PerfFunctions.Wait(timeout);
		}
		finally
		{
			ControlSystem.Instance.ExecuteControllerCommand(CommandType.GripperExtendHalt);
		}
	}

	internal static PickerEdgeCheckResult CheckPickerEdges()
	{
		PickerEdgeCheckResult result = PickerEdgeCheckResult.Clear;
		bool[] sensors = PickerBoard.ReadSensors().Sensors;
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

	private static CoreResponse SetFingerInner(GripperFingerState state)
	{
		CommandType type = CommandType.GripperClose;
		switch (state)
		{
		case GripperFingerState.Closed:
			type = CommandType.GripperClose;
			break;
		case GripperFingerState.Rent:
			type = CommandType.GripperRent;
			break;
		case GripperFingerState.Open:
			type = CommandType.GripperOpen;
			break;
		}
		CoreResponse coreResponse = null;
		for (int i = 0; i < 2; i++)
		{
			coreResponse = ControlSystem.Instance.ExecuteControllerCommand(type, 5000);
			if (coreResponse.CommunicationError)
			{
				LogHelper.Instance.Log("Communication error during finger operation", LogEntryType.Error);
				return coreResponse;
			}
			if (!coreResponse.TimedOut)
			{
				return coreResponse;
			}
		}
		LogHelper.Instance.Log($"RetryableFingerFunction {state.ToString().ToUpper()} timed out, even after {2} tries.", LogEntryType.Error);
		ControlSystem.Instance.LogInputs(ControlBoards.Picker, LogEntryType.Error);
		switch (state)
		{
		case GripperFingerState.Closed:
			CounterManager.Instance.Increment("FingerCloseTimeout");
			break;
		case GripperFingerState.Open:
			CounterManager.Instance.Increment("FingerOpenTimeout");
			break;
		default:
			CounterManager.Instance.Increment("FingerRentTimeout");
			break;
		}
		return coreResponse;
	}

	private static CoreResponse ExtendToSensorInner(int? to, bool check)
	{
		if (ControllerService.Instance.Location.IsQlmDeck)
		{
			to = ControllerConfiguration.Instance.QlmExtendTime;
		}
		else if (!to.HasValue)
		{
			to = ControllerConfiguration.Instance.GripperArmExtendRetractTimeout;
		}
		CoreResponse coreResponse = ControlSystem.Instance.ExecuteControllerCommand(CommandType.GripperExtend, to.Value);
		if (!coreResponse.TimedOut)
		{
			return coreResponse;
		}
		if (coreResponse.CommunicationError)
		{
			LogHelper.Instance.Log("Communication error during gripper extend.", LogEntryType.Error);
			return coreResponse;
		}
		if (check)
		{
			string[] array = ControlSystem.Instance.ReadInputs(ControlBoards.Picker);
			if (array.Length > 0 && array[0] == "1")
			{
				LogHelper.Instance.Log("GripperExtend status returned timeout; however, picker sensors read shows the forward sensor reached.", LogEntryType.Info);
				coreResponse.TimedOut = false;
				return coreResponse;
			}
			LogHelper.Instance.Log("SetGripperArm( Extend ) timed out.", LogEntryType.Error);
			ControlSystem.Instance.LogInputs(ControlBoards.Picker, LogEntryType.Error);
			CounterManager.Instance.Increment("GripperExtendTimeout");
		}
		return coreResponse;
	}

	private static GripperFingerState ComputeFingerStateForGet()
	{
		if (!ControllerService.Instance.Location.IsWideSlot)
		{
			return GripperFingerState.Rent;
		}
		return GripperFingerState.Open;
	}

	private static void ResetForSecondExtend()
	{
		RetractInner();
		SetFingerInner(GripperFingerState.Closed);
		GripperFingerState gripperFingerState = ComputeFingerStateForGet();
		LogHelper.Instance.Log($"Timeout for GripperArmExtend; attempt to set fingers to {gripperFingerState.ToString()}", LogEntryType.Info);
		SetFingerInner(gripperFingerState);
	}

	private static CoreResponse RetractInner()
	{
		CoreResponse coreResponse = null;
		int gripperArmExtendRetractTimeout = ControllerConfiguration.Instance.GripperArmExtendRetractTimeout;
		for (int i = 0; i < 2; i++)
		{
			coreResponse = ControlSystem.Instance.ExecuteControllerCommand(CommandType.GripperRetract, gripperArmExtendRetractTimeout);
			if (coreResponse.CommunicationError)
			{
				LogHelper.Instance.Log("Communication error during gripper retract.", LogEntryType.Error);
				return coreResponse;
			}
			if (!coreResponse.TimedOut)
			{
				return coreResponse;
			}
		}
		string[] array = ControlSystem.Instance.ReadInputs(ControlBoards.Picker);
		if (array.Length > 0 && array[1] == "1")
		{
			LogHelper.Instance.Log("GripperRetract status returned timeout; however, picker sensors read shows the sensor tripped.", LogEntryType.Info);
			return coreResponse;
		}
		LogHelper.Instance.Log($"SetGripperArm( RETRACT ) timed out after {2} retries.", LogEntryType.Error);
		ControlSystem.Instance.LogInputs(ControlBoards.Picker, LogEntryType.Error);
		CounterManager.Instance.Increment("GripperRetractTimeout");
		return coreResponse;
	}
}
