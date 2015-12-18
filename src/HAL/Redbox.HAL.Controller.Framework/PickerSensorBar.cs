using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal static class PickerSensorBar
{
	internal static PickerSensorReadResult ReadSensors()
	{
		try
		{
			CoreResponse coreResponse = ControlSystem.Instance.ExecuteControllerCommand(CommandType.SensorBarOn);
			if (coreResponse.CommunicationError)
			{
				return new PickerSensorReadResult(coreResponse);
			}
			PerfFunctions.SpinWait(10);
			CoreResponse response = ControlSystem.Instance.ExecuteControllerCommand(CommandType.ReadPickerInputs);
			PerfFunctions.SpinWait(10);
			return new PickerSensorReadResult(response);
		}
		finally
		{
			ControlSystem.Instance.ExecuteControllerCommand(CommandType.SensorBarOff);
		}
	}

	internal static bool[] ReadAsFlags()
	{
		if (ControllerMode.Emulated == ControllerConfiguration.Instance.Mode)
		{
			bool[] array = new bool[6];
			if (ControllerEmulationState.Instance.ExpectedIsPickerFullResult.Next())
			{
				bool flag;
				array[3] = (flag = (array[4] = true));
				array[2] = flag;
			}
			return array;
		}
		return ReadSensors().Sensors;
	}

	internal static bool WaitSensor(PickerSensor lowSensor, bool state, int timeout)
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedWaitForPickerSensorsToBlockResult;
		}
		int num = (int)(lowSensor - 1);
		bool result = false;
		using (ExecutionTimer executionTimer = new ExecutionTimer())
		{
			while (true)
			{
				PerfFunctions.SpinWait(20);
				PickerSensorReadResult pickerSensorReadResult = ReadSensors();
				if (pickerSensorReadResult.ReadError)
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

	internal static bool WaitToClear(PickerSensor lowSensor, PickerSensor highSensor, int timeout)
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedWaitForPickerSensorsToBlockResult;
		}
		int num = (int)(lowSensor - 1);
		int num2 = (int)(highSensor - 1);
		if (highSensor < lowSensor)
		{
			int num3 = num;
			num = num2;
			num2 = num3;
		}
		bool result = false;
		using (ExecutionTimer executionTimer = new ExecutionTimer())
		{
			while (true)
			{
				PerfFunctions.SpinWait(50);
				PickerSensorReadResult pickerSensorReadResult = ReadSensors();
				if (pickerSensorReadResult.ReadError)
				{
					LogHelper.Instance.Log("WaittoClear: sensor read timed out.", LogEntryType.Error);
					return true;
				}
				int num4 = 0;
				for (int i = num; i <= num2; i++)
				{
					if (pickerSensorReadResult.Sensors[i])
					{
						num4++;
					}
				}
				if (num4 == 0)
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
