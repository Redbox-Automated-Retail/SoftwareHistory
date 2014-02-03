using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public class Gripper
{
	private const int OpTries = 2;

	public static Gripper Instance => Singleton<Gripper>.Instance;

	public CoreResponse TestFinger(GripperFingerState state)
	{
		return SetFingerInner(state);
	}

	public CoreResponse TestRetract()
	{
		return RetractInner();
	}

	public CoreResponse TestExtend()
	{
		CoreResponse coreResponse = ExtendToSensorInner(null);
		if (coreResponse.TimedOut)
		{
			RetractInner();
		}
		return coreResponse;
	}

	public void TimedExtend(int? timeout)
	{
		TimedExtendInner((!timeout.HasValue) ? ControllerConfiguration.Instance.PushTime : timeout.Value);
	}

	public bool ExtendArmToSensor()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedSetGripperArmResult;
		}
		return ExtendToSensorInner(null).TimedOut;
	}

	public bool PushToSensor(int timeout)
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedSetGripperArmResult;
		}
		return PushToSensorInner(timeout).TimedOut;
	}

	public bool RetractArm()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedSetGripperArmResult;
		}
		return RetractInner().TimedOut;
	}

	public bool SetFinger(GripperFingerState state)
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return false;
		}
		return SetFingerInner(state).TimedOut;
	}

	public bool ClearFront()
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
			PushToSensorInner(ControllerConfiguration.Instance.PushTime);
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
			PushToSensorInner(ControllerConfiguration.Instance.PushTime);
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

	public LocationTestResult Peek()
	{
		LogHelper.Instance.Log(">> ControllerService.GripperPeek", LogEntryType.Info);
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			LocationTestResult locationTestResult = new LocationTestResult();
			locationTestResult.PeekResult = ControllerEmulationState.Instance.ExpectedGripperPeekResult;
			return locationTestResult;
		}
		return PeekInner();
	}

	public GripperErrorModes PushInDisk()
	{
		return PushIntoSlot();
	}

	internal GripperErrorModes PushWithArm(int timeout)
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
			PushToSensorInner(timeout);
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

	internal GripperErrorModes PushIntoSlot()
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

	internal PickerHardwareError PullFromLocation(int pulls)
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
				QLM.Instance.ExtendForGet();
			}
			else if (ExtendToSensorInner(null).TimedOut)
			{
				ResetForSecondExtend();
				if (ExtendToSensorInner(null).TimedOut)
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

	internal PickerHardwareError SettleDiskInSlot()
	{
		try
		{
			if (SetFingerInner(GripperFingerState.Open).TimedOut)
			{
				return PickerHardwareError.GripperOpenTimeout;
			}
			if (ExtendToSensorInner(null).TimedOut)
			{
				ResetForSecondExtend();
				if (ExtendToSensorInner(null).TimedOut)
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

	internal GripperFingerState GetFingerState()
	{
		CoreResponse coreResponse = CoreController.ExecuteSingleControllerCommand(CommandType.SensorBarRead);
		if (coreResponse.IsBitSet(4))
		{
			return GripperFingerState.Closed;
		}
		if (coreResponse.IsBitSet(6))
		{
			return GripperFingerState.Rent;
		}
		if (!coreResponse.IsBitSet(5))
		{
			return GripperFingerState.Unknown;
		}
		return GripperFingerState.Open;
	}

	internal bool CheckForMove(out MotionResult result)
	{
		result = MotionResult.Success;
		string[] array = PickerBoard.Instance.ReadInputs();
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
			PickerBoard.Instance.LogInputs(LogEntryType.Error);
			result = MotionResult.SensorError;
			return false;
		}
		return true;
	}

	internal void TimedExtendInner(int timeout)
	{
		try
		{
			if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
			{
				LogHelper.Instance.Log($"ExtendGripperArmForTime {timeout} ms", LogEntryType.Debug);
			}
			CoreController.ExecuteSingleControllerCommand(CommandType.ExtendGripperArmForTime);
			PerfFunctions.Wait(timeout);
		}
		finally
		{
			CoreController.ExecuteSingleControllerCommand(CommandType.GripperExtendHalt);
		}
	}

	private LocationTestResult PeekInner()
	{
		LocationTestResult locationTestResult = new LocationTestResult();
		if (ControllerService.Instance.Location.IsWideSlot)
		{
			PickerHardwareError pickerHardwareError = SettleDiskInSlot();
			if (pickerHardwareError != PickerHardwareError.Success)
			{
				locationTestResult.Error = pickerHardwareError;
				return locationTestResult;
			}
		}
		if (SetFingerInner(GripperFingerState.Closed).TimedOut)
		{
			locationTestResult.Error = PickerHardwareError.GripperCloseTimeout;
			return locationTestResult;
		}
		bool timedOut = ExtendToSensorInner(ControllerConfiguration.Instance.TestExtendTime).TimedOut;
		if (RetractInner().TimedOut)
		{
			locationTestResult.Error = PickerHardwareError.GripperRetractTimeout;
			return locationTestResult;
		}
		if (SetFingerInner(GripperFingerState.Rent).TimedOut)
		{
			locationTestResult.Error = PickerHardwareError.GripperRentTimeout;
			return locationTestResult;
		}
		locationTestResult.Error = PickerHardwareError.Success;
		locationTestResult.PeekResult = ((!timedOut) ? GripperPeekResult.Empty : GripperPeekResult.Full);
		return locationTestResult;
	}

	private CoreResponse SetFingerInner(GripperFingerState state)
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
			coreResponse = CoreController.ExecuteSingleWaitableCommand(type, 5000);
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
		PickerBoard.Instance.LogInputs(LogEntryType.Error);
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

	private CoreResponse PushToSensorInner(int timeout)
	{
		CoreResponse coreResponse = CoreController.ExecuteSingleWaitableCommand(CommandType.GripperExtend, timeout);
		if (coreResponse.CommunicationError)
		{
			LogHelper.Instance.Log("Communication error for gripper extend.", LogEntryType.Debug);
		}
		return coreResponse;
	}

	private CoreResponse ExtendToSensorInner(int? to)
	{
		_ = ControllerService.Instance.Location;
		if (!to.HasValue)
		{
			to = ControllerConfiguration.Instance.GripperArmExtendRetractTimeout;
		}
		if (ControllerService.Instance.Location.IsQlmDeck)
		{
			to = ControllerConfiguration.Instance.QlmExtendTime;
		}
		CoreResponse coreResponse = CoreController.ExecuteSingleWaitableCommand(CommandType.GripperExtend, to.Value);
		if (!coreResponse.TimedOut)
		{
			return coreResponse;
		}
		if (coreResponse.CommunicationError)
		{
			LogHelper.Instance.Log("Communication error during gripper extend.", LogEntryType.Error);
			return coreResponse;
		}
		string[] array = PickerBoard.Instance.ReadInputs();
		if (array.Length > 0 && array[0] == "1")
		{
			LogHelper.Instance.Log("GripperExtend status returned timeout; however, picker sensors read shows the forward sensor reached.", LogEntryType.Info);
			coreResponse.TimedOut = false;
			return coreResponse;
		}
		LogHelper.Instance.Log("SetGripperArm( Extend ) timed out.", LogEntryType.Error);
		PickerBoard.Instance.LogInputs(LogEntryType.Error);
		CounterManager.Instance.Increment("GripperExtendTimeout");
		return coreResponse;
	}

	private GripperFingerState ComputeFingerStateForGet()
	{
		if (!ControllerService.Instance.Location.IsWideSlot)
		{
			return GripperFingerState.Rent;
		}
		return GripperFingerState.Open;
	}

	private void ResetForSecondExtend()
	{
		RetractInner();
		SetFingerInner(GripperFingerState.Closed);
		GripperFingerState gripperFingerState = ComputeFingerStateForGet();
		LogHelper.Instance.Log($"Timeout for GripperArmExtend; attempt to set fingers to {gripperFingerState.ToString()}", LogEntryType.Info);
		SetFingerInner(gripperFingerState);
	}

	private CoreResponse RetractInner()
	{
		CoreResponse coreResponse = null;
		int gripperArmExtendRetractTimeout = ControllerConfiguration.Instance.GripperArmExtendRetractTimeout;
		for (int i = 0; i < 2; i++)
		{
			coreResponse = CoreController.ExecuteSingleWaitableCommand(CommandType.GripperRetract, gripperArmExtendRetractTimeout);
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
		string[] array = PickerBoard.Instance.ReadInputs();
		if (array.Length > 0 && array[1] == "1")
		{
			LogHelper.Instance.Log("GripperRetract status returned timeout; however, picker sensors read shows the sensor tripped.", LogEntryType.Info);
			return coreResponse;
		}
		LogHelper.Instance.Log($"SetGripperArm( RETRACT ) timed out after {2} retries.", LogEntryType.Error);
		PickerBoard.Instance.LogInputs(LogEntryType.Error);
		CounterManager.Instance.Increment("GripperRetractTimeout");
		return coreResponse;
	}

	private Gripper()
	{
	}
}
