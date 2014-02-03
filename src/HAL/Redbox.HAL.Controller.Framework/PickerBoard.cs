using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public class PickerBoard
{
	public static PickerBoard Instance => Singleton<PickerBoard>.Instance;

	private PickerSensorBar SensorBar => PickerSensorBar.Instance;

	public PickerSensorReadResult ImmediateReadSensors()
	{
		return ReadSensorsInner();
	}

	public bool TestCommunication()
	{
		CoreResponse coreResponse = CoreController.ExecuteSingleControllerCommand(CommandType.Version001);
		if (coreResponse == null || coreResponse.OpCodeResponse == null || coreResponse.TimedOut)
		{
			return false;
		}
		return true;
	}

	public bool IsFull()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedIsPickerFullResult.Next();
		}
		if (Track.Instance.CloseIfOpen())
		{
			return true;
		}
		PickerSensorReadResult pickerSensorReadResult = SensorBar.ReadSensors();
		return pickerSensorReadResult.IsFull;
	}

	public string[] ReadInputs()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return new string[0];
		}
		return ReadInputsInner();
	}

	public void LogInputs(LogEntryType type)
	{
		string arg = "Picker Inputs";
		CoreResponse coreResponse = CoreController.ExecuteSingleControllerCommand(CommandType.ReadPickerControllerSensors);
		if (coreResponse.CommunicationError)
		{
			string message = $"{arg} - communication error.";
			LogHelper.Instance.Log(message, LogEntryType.Info);
		}
		else
		{
			string message2 = $"{arg} {coreResponse.OpCodeResponse}";
			LogHelper.Instance.Log(message2, type);
		}
	}

	public string SetSensors(bool on)
	{
		if (on)
		{
			if (!SensorBar.TurnOn().CommunicationError)
			{
				return "OK";
			}
			return "PCB Not responsive.";
		}
		CoreResponse coreResponse = SensorBar.TurnOff();
		if (coreResponse.CommunicationError)
		{
			return "PCB Not responsive.";
		}
		return "OK";
	}

	public bool WaitToClear(PickerSensor start, PickerSensor end, int timeout)
	{
		return SensorBar.WaitToClear(start, end, timeout);
	}

	public bool WaitToClearSensor(PickerSensor sensor, int timeout)
	{
		return SensorBar.WaitSensor(sensor, state: false, timeout);
	}

	public bool WaitToBlockSensor(PickerSensor sensor, int timeout)
	{
		return SensorBar.WaitSensor(sensor, state: true, timeout);
	}

	public PickerSensorReadResult ReadSensors()
	{
		Track.Instance.CloseIfOpen();
		return ReadSensorsInner();
	}

	public bool[] ReadPickerSensors()
	{
		return ReadAsFlags();
	}

	public void LogPickerSensorState()
	{
		LogPickerSensorState(ReadSensors(), LogEntryType.Info);
	}

	public bool StopRoller()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedStopRollerResult;
		}
		return CoreController.ExecuteSingleControllerCommand(CommandType.RollerStop).TimedOut;
	}

	public bool StartRollerIn()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedRollerInResult;
		}
		return CoreController.ExecuteSingleControllerCommand(CommandType.RollerIn).TimedOut;
	}

	public bool StartRollerOut()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedRollerOutResult;
		}
		return CoreController.ExecuteSingleControllerCommand(CommandType.RollerOut).TimedOut;
	}

	public bool RollerToPosition(RollerPosition position, int opTimeout)
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
		CoreResponse coreResponse = CoreController.ExecuteSingleWaitableCommand(type, opTimeout);
		if (coreResponse.TimedOut)
		{
			LogHelper.Instance.Log(LogEntryType.Error, "Roller to position {0} timed out.", position.ToString());
			LogPickerSensorState(LogEntryType.Error);
		}
		return coreResponse.TimedOut;
	}

	internal bool IsInputBlocked(int bit)
	{
		CoreResponse coreResponse = CoreController.ExecuteSingleControllerCommand(CommandType.ReadPickerControllerSensors);
		return coreResponse.IsBitSet(bit);
	}

	internal bool[] ReadAsFlags()
	{
		return SensorBar.ReadAsFlags();
	}

	internal void LogPickerSensorState(LogEntryType type)
	{
		if (ControllerMode.Emulated != ControllerConfiguration.Instance.Mode)
		{
			LogPickerSensorState(ReadSensors(), type);
		}
	}

	internal void LogPickerSensorState(PickerSensorReadResult result, LogEntryType type)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("PickerSensors: ");
		for (int i = 0; i < result.Sensors.Length; i++)
		{
			stringBuilder.Append($"{i + 1} = {SensorStateInner(result.Sensors[i])};");
		}
		LogHelper.Instance.Log(stringBuilder.ToString(), type);
	}

	private PickerSensorReadResult ReadSensorsInner()
	{
		PickerSensorReadResult pickerSensorReadResult = PickerSensorBar.Instance.ReadSensors();
		if (pickerSensorReadResult.ReadError)
		{
			LogHelper.Instance.Log("Unable to read picker sensors.", LogEntryType.Error);
		}
		return pickerSensorReadResult;
	}

	private string[] ReadInputsInner()
	{
		CoreResponse coreResponse = CoreController.ExecuteSingleControllerCommand(CommandType.ReadPickerControllerSensors);
		if (coreResponse.CommunicationError)
		{
			return new string[0];
		}
		string[] array = ConvertResponseToStringArray(coreResponse.OpCodeResponse);
		if (array.Length != 20)
		{
			return new string[0];
		}
		return array;
	}

	private string[] ConvertResponseToStringArray(string response)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < response.Length; i++)
		{
			if (char.IsDigit(response[i]))
			{
				list.Add(response[i].ToString());
			}
		}
		return list.ToArray();
	}

	private string SensorStateInner(bool state)
	{
		if (!state)
		{
			return "CLEAR";
		}
		return "BLOCKED";
	}

	private PickerBoard()
	{
	}
}
