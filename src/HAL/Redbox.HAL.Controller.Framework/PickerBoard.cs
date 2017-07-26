using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public static class PickerBoard
{
	public static ErrorCodes PushOut()
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		IControlSystem service2 = ServiceLocator.Instance.GetService<IControlSystem>();
		if (!service2.TrackClose().Success)
		{
			LogHelper.Instance.Log("[PickerBoard] PushOut: unable to close the track.", LogEntryType.Error);
			return ErrorCodes.TrackCloseTimeout;
		}
		for (int i = 0; i < 3; i++)
		{
			IPickerSensorReadResult pickerSensorReadResult = service2.ReadSensors();
			if (pickerSensorReadResult.Error != ErrorCodes.Success)
			{
				return ErrorCodes.SensorReadError;
			}
			bool[] sensors = pickerSensorReadResult.Sensors;
			if (!pickerSensorReadResult.IsFull)
			{
				return ErrorCodes.Success;
			}
			if (sensors[3])
			{
				service2.StartRollerOut();
				WaitToClearSensor(PickerSensor.Sensor4, 4000);
				if (ControllerConfiguration.Instance.PushOutSleepTime2 > 0)
				{
					service.Wait(ControllerConfiguration.Instance.PushOutSleepTime2);
				}
				service2.StopRoller();
				continue;
			}
			if (sensors[0] || sensors[1] || sensors[2])
			{
				if (!service2.RollerToPosition(RollerPosition.Position4, 4000).Success)
				{
					LogHelper.Instance.Log("PushOut: roller to 4 timed out.", LogEntryType.Error);
					service2.LogPickerSensorState(LogEntryType.Error);
					continue;
				}
				service.SpinWait(50);
				service2.StartRollerOut();
				WaitToClearSensor(PickerSensor.Sensor4, 4000);
				service2.StopRoller();
				continue;
			}
			return ErrorCodes.Success;
		}
		IPickerSensorReadResult pickerSensorReadResult2 = service2.ReadSensors();
		if (pickerSensorReadResult2.Error != ErrorCodes.Success)
		{
			return ErrorCodes.SensorReadError;
		}
		bool[] sensors2 = pickerSensorReadResult2.Sensors;
		if (sensors2[0] || sensors2[1] || sensors2[2] || sensors2[3])
		{
			return ErrorCodes.PickerFull;
		}
		return ErrorCodes.Success;
	}

	public static bool IsFull()
	{
		IPickerSensorReadResult pickerSensorReadResult = ServiceLocator.Instance.GetService<IControlSystem>().ReadSensors();
		if (pickerSensorReadResult.Error == ErrorCodes.Success)
		{
			return pickerSensorReadResult.IsFull;
		}
		return true;
	}

	internal static bool WaitToClearSensor(PickerSensor sensor, int timeout)
	{
		return WaitSensor(sensor, state: false, timeout);
	}

	internal static bool WaitToBlockSensor(PickerSensor sensor, int timeout)
	{
		return WaitSensor(sensor, state: true, timeout);
	}

	private static bool WaitSensor(PickerSensor lowSensor, bool state, int timeout)
	{
		int waitSensorPauseTime = ControllerConfiguration.Instance.WaitSensorPauseTime;
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		IControlSystem service2 = ServiceLocator.Instance.GetService<IControlSystem>();
		int num = (int)(lowSensor - 1);
		bool result = false;
		using (ExecutionTimer executionTimer = new ExecutionTimer())
		{
			while (true)
			{
				service.SpinWait(waitSensorPauseTime);
				IPickerSensorReadResult pickerSensorReadResult = service2.ReadSensors();
				if (pickerSensorReadResult.Error != ErrorCodes.Success)
				{
					LogHelper.Instance.Log("WaittoClear: sensor read timed out.", LogEntryType.Error);
					return true;
				}
				if (pickerSensorReadResult.Sensors[num] == state)
				{
					result = false;
					break;
				}
				if (executionTimer.ElapsedMilliseconds > timeout)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}
}
