using System;
using System.Text;
using System.Threading;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class ControlSystem
{
	internal readonly ControlBoards[] ControllerBoards = new ControlBoards[3]
	{
		ControlBoards.Serial,
		ControlBoards.Picker,
		ControlBoards.Aux
	};

	internal readonly RedboxSerialPort Port;

	public static ControlSystem Instance => Singleton<ControlSystem>.Instance;

	public CoreResponse ExecuteControllerCommand(CommandType type)
	{
		return SendCommand(CoreCommand.Create(type, null, Port));
	}

	public string GetBoardVersions(out string[] versions)
	{
		versions = new string[ControllerBoards.Length];
		for (int i = 0; i < ControllerBoards.Length; i++)
		{
			versions[i] = GetBoardVersion(ControllerBoards[i]);
		}
		return "A";
	}

	public string GetBoardVersion(ControlBoards board)
	{
		CommandType type = CommandType.Version101;
		switch (board)
		{
		case ControlBoards.None:
			return null;
		case ControlBoards.Aux:
			type = CommandType.Version002;
			break;
		case ControlBoards.Picker:
			type = CommandType.Version001;
			break;
		case ControlBoards.Serial:
			type = CommandType.Version101;
			break;
		}
		CoreResponse coreResponse = ExecuteControllerCommand(type);
		if (!coreResponse.CommunicationError)
		{
			return coreResponse.OpCodeResponse;
		}
		return "TIMEOUT";
	}

	public CoreResponse Initialize()
	{
		CoreResponse coreResponse = ExecuteControllerCommand(CommandType.Reset);
		if (coreResponse.CommunicationError)
		{
			LogHelper.Instance.Log("Communication error during Serial board RESET.");
		}
		PerfFunctions.Wait(2000);
		return coreResponse;
	}

	public bool ClosePort(int? waitAfter)
	{
		bool result = Port.Close();
		if (waitAfter.HasValue)
		{
			Thread.Sleep(waitAfter.Value);
		}
		return result;
	}

	public string[] ReadInputs(ControlBoards board)
	{
		CommandType type = ((ControlBoards.Picker == board) ? CommandType.ReadPickerInputs : CommandType.AuxSensorsRead);
		CoreResponse coreResponse = Instance.ExecuteControllerCommand(type);
		if (coreResponse.CommunicationError)
		{
			return new string[0];
		}
		string[] array = new string[20];
		int num = 0;
		for (int i = 0; i < coreResponse.OpCodeResponse.Length; i++)
		{
			if (char.IsDigit(coreResponse.OpCodeResponse[i]))
			{
				array[num++] = coreResponse.OpCodeResponse[i].ToString();
			}
		}
		return array;
	}

	public void LogInputs(ControlBoards board, LogEntryType type)
	{
		string arg = ((ControlBoards.Picker == board) ? "Picker Inputs" : "AUX Inputs");
		CommandType type2 = ((ControlBoards.Picker == board) ? CommandType.ReadPickerInputs : CommandType.AuxSensorsRead);
		CoreResponse coreResponse = Instance.ExecuteControllerCommand(type2);
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

	internal CoreResponse ExecuteControllerCommand(CommandType type, int? timeout)
	{
		return SendCommand(CoreCommand.Create(type, timeout, Port));
	}

	internal CoreResponse ExecuteControllerCommand(CoreCommand command)
	{
		return SendCommand(command);
	}

	private ControlSystem()
	{
		Port = RedboxSerialPort.Create(new RedboxSerialPortConfiguration(ControllerConfiguration.Instance.ControllerPortName)
		{
			Baud = 9600,
			Test = ResponseValid,
			WriteTerminator = RedboxSerialPort.WriteCharsCR
		}, PortModes.Async, "Serial Board");
	}

	private CoreResponse SendCommand(CoreCommand command)
	{
		if (command.Port == null)
		{
			return CoreResponse.MakeErrorResponse();
		}
		if (!command.Port.Open())
		{
			LogHelper.Instance.Log($"ControlSystem.Instance: unable to open port {command.Port.Name}.", LogEntryType.Error);
			return CoreResponse.MakeErrorResponse();
		}
		try
		{
			return command.Execute();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"ControlSystem.Instance.SendCommands on port {command.Port.Name} caught an exception.", e);
			return CoreResponse.MakeErrorResponse();
		}
	}

	private bool ResponseValid(IChannelResponse response)
	{
		string text = Encoding.ASCII.GetString(response.RawResponse);
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		if (text.IndexOf("OK") != -1 || text.IndexOf("EER") != -1)
		{
			return true;
		}
		return false;
	}
}
