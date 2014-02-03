using System.Collections.Generic;
using System.Text;
using System.Threading;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class SerialBoard
{
	private bool? m_debug = null;

	public static SerialBoard Instance => Singleton<SerialBoard>.Instance;

	public RedboxSerialPort Port { get; private set; }

	private bool Debug
	{
		get
		{
			if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
			{
				return false;
			}
			if (m_debug.HasValue)
			{
				return m_debug.Value;
			}
			m_debug = RegistryOptions.Instance.GetSecretValue("SOFTWARE\\Redbox\\HAL", "LeiweDebug", def: false);
			return m_debug.Value;
		}
	}

	public CoreResponse Reset()
	{
		CoreResponse coreResponse = CoreController.ExecuteSingleControllerCommand(CommandType.Reset);
		if (coreResponse.CommunicationError)
		{
			LogHelper.Instance.Log("Communication error during Serial board RESET.");
		}
		PerfFunctions.Wait(2000);
		return coreResponse;
	}

	public bool ClosePort(int? waitAfter)
	{
		bool flag = Port.ClosePort();
		if (waitAfter.HasValue)
		{
			Thread.Sleep(waitAfter.Value);
		}
		LogHelper.Instance.Log("SerialBoard.ClosePort() returned {0}", flag);
		return flag;
	}

	public string GetBoardVersion(KnownBoards board)
	{
		CommandType type = CommandType.Version101;
		switch (board)
		{
		case KnownBoards.None:
			return null;
		case KnownBoards.Aux:
			type = CommandType.Version002;
			break;
		case KnownBoards.Picker:
			type = CommandType.Version001;
			break;
		case KnownBoards.Serial:
			type = CommandType.Version101;
			break;
		}
		CoreResponse coreResponse = CoreController.ExecuteSingleControllerCommand(type);
		if (!coreResponse.CommunicationError)
		{
			return coreResponse.OpCodeResponse;
		}
		return "TIMEOUT";
	}

	internal string[] GetVersionInformation()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedVersions;
		}
		List<string> list = new List<string>();
		CommandType[] array = new CommandType[3]
		{
			CommandType.Version001,
			CommandType.Version002,
			CommandType.Version101
		};
		CommandType[] array2 = array;
		foreach (CommandType type in array2)
		{
			CoreResponse coreResponse = CoreController.ExecuteSingleControllerCommand(type);
			string item = ((coreResponse == null || coreResponse.OpCodeResponse == null || coreResponse.TimedOut) ? "TIMEOUT" : coreResponse.OpCodeResponse);
			list.Add(item);
		}
		return list.ToArray();
	}

	private SerialBoard()
	{
		Port = new RedboxSerialPort(new RedboxSerialPortConfiguration(ControllerConfiguration.Instance.ControllerPortName)
		{
			Baud = 9600,
			Test = ResponseValid,
			WriteTerminator = new byte[1] { 13 }
		});
		Port.EnableDebugging = Debug;
	}

	private bool ResponseValid(ChannelResponse response)
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
