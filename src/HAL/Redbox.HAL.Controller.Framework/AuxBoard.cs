using System.Collections.Generic;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class AuxBoard
{
	public static AuxBoard Instance => Singleton<AuxBoard>.Instance;

	public bool IsSensorBlocked(AuxSensor sensor)
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedIsAuxSensorBlocked[(int)sensor];
		}
		CoreResponse coreResponse = CoreController.ExecuteSingleControllerCommand(CommandType.AuxSensorsRead);
		if (coreResponse.CommunicationError)
		{
			return false;
		}
		return coreResponse.IsBitSet((int)sensor);
	}

	public bool IsSensorBlocked(AuxSensor sensor, CoreResponse response)
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedIsAuxSensorBlocked[(int)sensor];
		}
		return response.IsBitSet((int)sensor);
	}

	public string[] ImmediateReadInputs()
	{
		CoreResponse coreResponse = CoreController.ExecuteSingleControllerCommand(CommandType.AuxSensorsRead);
		if (coreResponse.CommunicationError)
		{
			return new string[0];
		}
		List<string> list = new List<string>();
		for (int i = 0; i < coreResponse.OpCodeResponse.Length; i++)
		{
			if (char.IsDigit(coreResponse.OpCodeResponse[i]))
			{
				list.Add(coreResponse.OpCodeResponse[i].ToString());
			}
		}
		return list.ToArray();
	}

	public void LogInputs(LogEntryType type)
	{
		string arg = "AUX Inputs";
		CoreResponse coreResponse = CoreController.ExecuteSingleControllerCommand(CommandType.AuxSensorsRead);
		if (coreResponse.CommunicationError)
		{
			string message = $"{arg} - no response from board.";
			LogHelper.Instance.Log(message, LogEntryType.Info);
		}
		else
		{
			string message2 = $"{arg} {coreResponse.OpCodeResponse}";
			LogHelper.Instance.Log(message2, type);
		}
	}

	internal CoreResponse ReadInputs()
	{
		return CoreController.ExecuteSingleControllerCommand(CommandType.AuxSensorsRead);
	}

	private AuxBoard()
	{
	}
}
