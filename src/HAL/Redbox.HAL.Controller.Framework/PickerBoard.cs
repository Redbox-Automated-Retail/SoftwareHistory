using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public static class PickerBoard
{
	public static PickerSensorReadResult ImmediateReadSensors()
	{
		return PickerSensorBar.ReadSensors();
	}

	public static PushOutStatus PushOut()
	{
		if (Track.CloseIfOpen())
		{
			LogHelper.Instance.Log("[PickerBoard] PushOut: unable to close the track.", LogEntryType.Error);
			return PushOutStatus.HardwareError;
		}
		for (int i = 0; i < 3; i++)
		{
			PickerSensorReadResult pickerSensorReadResult = ReadSensors();
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
				StartRollerOut();
				WaitToClearSensor(PickerSensor.Sensor4, 4000);
				if (ControllerConfiguration.Instance.PushOutSleepTime2 > 0)
				{
					PerfFunctions.Wait(ControllerConfiguration.Instance.PushOutSleepTime2);
				}
				StopRoller();
				continue;
			}
			if (sensors[0] || sensors[1] || sensors[2])
			{
				if (RollerToPosition(RollerPosition.Position4, 4000))
				{
					LogHelper.Instance.Log("PushOut: roller to 4 timed out.", LogEntryType.Error);
					LogPickerSensorState(LogEntryType.Error);
					continue;
				}
				PerfFunctions.SpinWait(50);
				StartRollerOut();
				WaitToClearSensor(PickerSensor.Sensor4, 4000);
				StopRoller();
				continue;
			}
			return PushOutStatus.Success;
		}
		PickerSensorReadResult pickerSensorReadResult2 = ReadSensors();
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

	public static bool IsFull()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedIsPickerFullResult.Next();
		}
		if (Track.CloseIfOpen())
		{
			return true;
		}
		PickerSensorReadResult pickerSensorReadResult = PickerSensorBar.ReadSensors();
		return pickerSensorReadResult.IsFull;
	}

	public static string SetSensors(bool on)
	{
		CoreResponse coreResponse = ControlSystem.Instance.ExecuteControllerCommand(on ? CommandType.SensorBarOn : CommandType.SensorBarOff);
		if (!coreResponse.CommunicationError)
		{
			return "OK";
		}
		return "PCB Not responsive.";
	}

	public static bool WaitToClear(PickerSensor start, PickerSensor end, int timeout)
	{
		return PickerSensorBar.WaitToClear(start, end, timeout);
	}

	public static bool WaitToClearSensor(PickerSensor sensor, int timeout)
	{
		return PickerSensorBar.WaitSensor(sensor, state: false, timeout);
	}

	public static bool WaitToBlockSensor(PickerSensor sensor, int timeout)
	{
		return PickerSensorBar.WaitSensor(sensor, state: true, timeout);
	}

	public static PickerSensorReadResult ReadSensors()
	{
		Track.CloseIfOpen();
		return PickerSensorBar.ReadSensors();
	}

	public static void LogPickerSensorState()
	{
		ReadSensors().LogSensorState();
	}

	public static bool StopRoller()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedStopRollerResult;
		}
		return ControlSystem.Instance.ExecuteControllerCommand(CommandType.RollerStop).TimedOut;
	}

	public static bool StartRollerIn()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedRollerInResult;
		}
		return ControlSystem.Instance.ExecuteControllerCommand(CommandType.RollerIn).TimedOut;
	}

	public static bool StartRollerOut()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedRollerOutResult;
		}
		return ControlSystem.Instance.ExecuteControllerCommand(CommandType.RollerOut).TimedOut;
	}

	public static bool RollerToPosition(RollerPosition position, int opTimeout)
	{
		return RollerToPosition(position, opTimeout, logError: true);
	}

	public static void ToggleRingLight(RinglightState state, int? sleepAfter)
	{
		if (ControllerConfiguration.Instance.Mode != ControllerMode.Emulated)
		{
			CommandType commandType = CommandType.RinglightOn;
			switch (ControllerConfiguration.Instance.RinglightConnector)
			{
			case 1:
				commandType = ((state == RinglightState.On) ? CommandType.RinglightOn1 : CommandType.RinglightOff1);
				break;
			case 2:
				commandType = ((state == RinglightState.On) ? CommandType.RinglightOn : CommandType.RinglightOff);
				break;
			case 3:
				commandType = ((state == RinglightState.On) ? CommandType.RinglightOn3 : CommandType.RinglightOff3);
				break;
			case 4:
				commandType = ((state == RinglightState.On) ? CommandType.RinglightOn4 : CommandType.RinglightOff4);
				break;
			default:
				LogHelper.Instance.Log("Invalid connector specified.");
				return;
			}
			ControlSystem.Instance.ExecuteControllerCommand(commandType);
			if (sleepAfter.HasValue)
			{
				PerfFunctions.Wait(sleepAfter.Value);
			}
		}
	}

	internal static PickerDiskCount PickerBlockedStatus()
	{
		PickerSensorReadResult pickerSensorReadResult = ReadSensors();
		if (pickerSensorReadResult.ReadError)
		{
			return PickerDiskCount.HardwareError;
		}
		if (pickerSensorReadResult.BlockedCount == 0)
		{
			return PickerDiskCount.Empty;
		}
		if (pickerSensorReadResult.BlockedCount > 3)
		{
			return PickerDiskCount.MulitpleDisk;
		}
		return PickerDiskCount.OneDisk;
	}

	internal static bool ClearDiskFromPicker()
	{
		int milliseconds = 300;
		bool flag = WaitToClear(PickerSensor.Sensor2, PickerSensor.Sensor5, 8000);
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
					WaitToClear(PickerSensor.Sensor2, PickerSensor.Sensor5, 3000);
				}
				array = ReadAsFlags();
				flag2 = array[1] || array[2] || array[3] || array[4];
				PerfFunctions.SpinWait(milliseconds);
			}
			array = ReadAsFlags();
			flag = array[1] || array[2] || array[3] || array[4];
		}
		return flag;
	}

	internal static bool RollerToPosition(RollerPosition position, int opTimeout, bool logError)
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedRollerToPositionResult;
		}
		CommandType type = CommandType.RollerToPos1;
		switch (position)
		{
		case RollerPosition.Position1:
			type = CommandType.RollerToPos1;
			break;
		case RollerPosition.Position2:
			type = CommandType.RollerToPos2;
			break;
		case RollerPosition.Position3:
			type = CommandType.RollerToPos3;
			break;
		case RollerPosition.Position4:
			type = CommandType.RollerToPos4;
			break;
		case RollerPosition.Position5:
			type = CommandType.RollerToPos5;
			break;
		case RollerPosition.Position6:
			type = CommandType.RollerToPos6;
			break;
		}
		CoreResponse coreResponse = ControlSystem.Instance.ExecuteControllerCommand(type, opTimeout);
		if (coreResponse.TimedOut && logError)
		{
			LogHelper.Instance.Log(LogEntryType.Error, "Roller to position {0} timed out.", position.ToString());
			LogPickerSensorState(LogEntryType.Error);
		}
		return coreResponse.TimedOut;
	}

	internal static bool[] ReadAsFlags()
	{
		return PickerSensorBar.ReadAsFlags();
	}

	internal static void LogPickerSensorState(LogEntryType type)
	{
		if (ControllerMode.Emulated != ControllerConfiguration.Instance.Mode)
		{
			ReadSensors().LogSensorState(type);
		}
	}
}
