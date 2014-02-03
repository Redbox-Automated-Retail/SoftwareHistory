using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal class PickerSensorBar
{
	private static readonly int[] m_pickerSensorBits = new int[6] { 13, 11, 15, 10, 14, 9 };

	internal static PickerSensorBar Instance => Singleton<PickerSensorBar>.Instance;

	internal CoreResponse Read()
	{
		try
		{
			CoreResponse coreResponse = CoreController.ExecuteSingleControllerCommand(CommandType.SensorBarOn);
			if (coreResponse.CommunicationError)
			{
				return coreResponse;
			}
			PerfFunctions.SpinWait(10);
			CoreResponse result = CoreController.ExecuteSingleControllerCommand(CommandType.SensorBarRead);
			PerfFunctions.SpinWait(10);
			return result;
		}
		finally
		{
			CoreController.ExecuteSingleControllerCommand(CommandType.SensorBarOff);
		}
	}

	internal CoreResponse TurnOn()
	{
		return CoreController.ExecuteSingleControllerCommand(CommandType.SensorBarOn);
	}

	internal CoreResponse TurnOff()
	{
		return CoreController.ExecuteSingleControllerCommand(CommandType.SensorBarOff);
	}

	internal PickerSensorReadResult ReadSensors()
	{
		PickerSensorReadResult pickerSensorReadResult = new PickerSensorReadResult();
		pickerSensorReadResult.ReadError = false;
		PickerSensorReadResult pickerSensorReadResult2 = pickerSensorReadResult;
		pickerSensorReadResult2.Sensors = new bool[m_pickerSensorBits.Length];
		int num = 0;
		CoreResponse coreResponse = Read();
		if (coreResponse.CommunicationError)
		{
			pickerSensorReadResult2.ReadError = true;
			for (int i = 0; i < m_pickerSensorBits.Length; i++)
			{
				pickerSensorReadResult2.Sensors[i] = true;
			}
			return pickerSensorReadResult2;
		}
		int[] pickerSensorBits = m_pickerSensorBits;
		foreach (int bit in pickerSensorBits)
		{
			pickerSensorReadResult2.Sensors[num++] = coreResponse.IsBitSet(bit);
		}
		return pickerSensorReadResult2;
	}

	internal bool[] ReadAsFlags()
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

	internal bool WaitSensor(PickerSensor lowSensor, bool state, int timeout)
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

	internal bool WaitToClear(PickerSensor lowSensor, PickerSensor highSensor, int timeout)
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

	private PickerSensorBar()
	{
	}
}
