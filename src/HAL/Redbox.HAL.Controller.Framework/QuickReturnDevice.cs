using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using Redbox.HAL.Core.Descriptors;

namespace Redbox.HAL.Controller.Framework;

public sealed class QuickReturnDevice
{
	public enum LED
	{
		Unknown,
		Background,
		GreenButton,
		RedButton,
		GreenArrow,
		RedArrow
	}

	public enum LEDMode
	{
		Unknown,
		On,
		Off,
		Blink
	}

	public enum GraphicWriteTarget
	{
		None,
		EEPROM,
		Display
	}

	private class LedState
	{
		internal LED Led { get; set; }

		internal LEDMode Mode { get; set; }
	}

	private class QuickReturnState
	{
		internal string GraphicsAddress { get; set; }

		internal string TextAddress { get; set; }

		internal string Name { get; set; }

		internal List<LedState> LedStates { get; private set; }

		internal QuickReturnState()
		{
			LedStates = new List<LedState>();
			LedStates.Add(new LedState
			{
				Led = LED.GreenButton,
				Mode = LEDMode.Off
			});
			LedStates.Add(new LedState
			{
				Led = LED.RedButton,
				Mode = LEDMode.Off
			});
			LedStates.Add(new LedState
			{
				Led = LED.GreenArrow,
				Mode = LEDMode.Off
			});
			LedStates.Add(new LedState
			{
				Led = LED.RedArrow,
				Mode = LEDMode.Off
			});
			LedStates.Add(new LedState
			{
				Led = LED.Background,
				Mode = LEDMode.Off
			});
		}

		internal void Dump()
		{
			LogHelper.Instance.Log($"State {Name} showing at page {GraphicsAddress}", LogEntryType.Info);
		}
	}

	private readonly string ImagesDirectory;

	private readonly string StateFile;

	private readonly string GraphicsPath;

	private RedboxSerialPort Port;

	private int m_currentGraphicsAddress;

	private readonly RedboxSerialPortConfiguration Configuration;

	private readonly object Lock = new object();

	private readonly List<QuickReturnState> States = new List<QuickReturnState>();

	private const int PageSize = 2048;

	private const string MemorySize = "1FFF";

	private const string GraphicsWidth = "10";

	private const int MaxPages = 16;

	private const string PreferredDisplayAddress = "0800";

	public static QuickReturnDevice Instance => Singleton<QuickReturnDevice>.Instance;

	public bool IsConfigured
	{
		get
		{
			string quickReturnComPort = ControllerConfiguration.Instance.QuickReturnComPort;
			if (!string.IsNullOrEmpty(quickReturnComPort))
			{
				return quickReturnComPort.StartsWith("COM", StringComparison.CurrentCultureIgnoreCase);
			}
			return false;
		}
	}

	public string ProbePort()
	{
		string result = "NONE";
		RedboxSerialPort redboxSerialPort = PortManager.Instance.Scan(Configuration, TestPort, PortModes.Async);
		if (redboxSerialPort != null)
		{
			result = redboxSerialPort.Name;
			redboxSerialPort.Dispose();
		}
		return result;
	}

	public void ConfigurationChange()
	{
		ClosePort();
	}

	public void DumpStateTable()
	{
		foreach (QuickReturnState state in States)
		{
			state.Dump();
		}
	}

	public bool ClosePort()
	{
		lock (Lock)
		{
			if (Port == null)
			{
				return false;
			}
			Port.Dispose();
			Port = null;
			return true;
		}
	}

	public bool Reset()
	{
		lock (Lock)
		{
			States.Clear();
			m_currentGraphicsAddress = 0;
			if (!OpenPort())
			{
				LogHelper.Instance.Log("Could not find a port for the Quick Return device.");
				return false;
			}
			LogHelper.Instance.Log(">> QuickReturnDevice.Reset()", LogEntryType.Debug);
			Thread.Sleep(2000);
			if (!ExecuteQRCommand(CommandType.SetTextColumns, "1E"))
			{
				LogHelper.Instance.Log("Unable to set text columns.", LogEntryType.Error);
				return false;
			}
			if (!ExecuteQRCommand(CommandType.SetGraphicsColumns, "10"))
			{
				LogHelper.Instance.Log(string.Format("Unable to set graphics columns to {0}", "10"), LogEntryType.Error);
				return false;
			}
			if (!ExecuteQRCommand(CommandType.TurnOffGreenArrowLed))
			{
				LogHelper.Instance.Log("Unable to turn off green arrow LED.", LogEntryType.Error);
				return false;
			}
			if (!ExecuteQRCommand(CommandType.TurnOffGreenButtonLed))
			{
				LogHelper.Instance.Log("Unable to turn off green button LED.", LogEntryType.Error);
				return false;
			}
			if (!ExecuteQRCommand(CommandType.TurnOffRedArrowLed))
			{
				LogHelper.Instance.Log("Unable to turn off red arrow LED.", LogEntryType.Error);
				return false;
			}
			if (!ExecuteQRCommand(CommandType.TurnOffRedButtonLed))
			{
				LogHelper.Instance.Log("Unable to turn off red button LED.", LogEntryType.Error);
				return false;
			}
			ExecuteQRCommand(CommandType.TurnOnBackLight);
			if (!ExecuteQRCommand(CommandType.SetStartTextPagePointer, "0000"))
			{
				LogHelper.Instance.Log("Unable to set text page pointer.", LogEntryType.Error);
				return false;
			}
			if (!ExecuteQRCommand(CommandType.SetStartGraphicsPagePointer, "0800"))
			{
				LogHelper.Instance.Log("Unable to set text page pointer.", LogEntryType.Error);
				return false;
			}
			if (!ExecuteQRCommand(CommandType.SetMemoryWritePointer, ConvertIntToHexString(m_currentGraphicsAddress)))
			{
				LogHelper.Instance.Log($"Unable to set memory pointer to {m_currentGraphicsAddress}", LogEntryType.Error);
				return false;
			}
			if (!ExecuteQRCommand(CommandType.DisplaySettingOR))
			{
				LogHelper.Instance.Log("Unable to set display setting to OR.", LogEntryType.Error);
				return false;
			}
			if (!ExecuteQRCommand(CommandType.SetGraphicsOnlyDisplayMode))
			{
				LogHelper.Instance.Log("Unable to set text only mode.", LogEntryType.Error);
				return false;
			}
			if (!ExecuteQRCommand(CommandType.SetMemoryWritePointer, "0000"))
			{
				LogHelper.Instance.Log("Unable to set write pointer to 0000.", LogEntryType.Error);
				return false;
			}
			CoreResponse coreResponse = ServiceLocator.Instance.GetService<ICoreCommandExecutor>().ExecuteControllerCommand(CoreCommand.Create(CommandType.SideTerminalVersion, Port));
			string arg = (coreResponse.CommError ? "COMMUNICATION ERROR" : coreResponse.OpCodeResponse.TrimStart());
			if (!ReadStateTable(StateFile))
			{
				LogHelper.Instance.Log($"Unable to read state file {StateFile}", LogEntryType.Info);
				return false;
			}
			ExecuteQRCommand(CommandType.SetMemoryWritePointer, "0000");
			ExecuteQRCommand(CommandType.ClearDisplayMemory, "1FFF");
			LogHelper.Instance.Log($"<< QuickReturnDevice.Reset() ({arg})", LogEntryType.Debug);
			return true;
		}
	}

	public int UpdateDeviceImages()
	{
		if (Port == null && !OpenPort())
		{
			return 0;
		}
		if (!ExecuteQRCommand(CommandType.SetMemoryWritePointer, "0000"))
		{
			return 0;
		}
		if (!ExecuteQRCommand(CommandType.SetEEPROMPointer, "0000"))
		{
			return 0;
		}
		if (!ExecuteQRCommand(CommandType.SetEEPROMReadSize, "0010"))
		{
			return 0;
		}
		string[] files = Directory.GetFiles(ImagesDirectory, "*.grf");
		Array.Sort(files);
		int result = 0;
		string text = StateFile + ".bak";
		bool flag = false;
		try
		{
			File.Move(StateFile, text);
			flag = true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unable to back up state file.", e);
			return 0;
		}
		using (StreamWriter streamWriter = new StreamWriter(StateFile))
		{
			string[] array = files;
			foreach (string text2 in array)
			{
				string[] data = ReadStringFile(text2);
				if (LoadData(data, string.Empty, GraphicWriteTarget.EEPROM) == 0)
				{
					return 0;
				}
				string fileName = Path.GetFileName(text2);
				streamWriter.WriteLine($"{fileName}={result++}");
			}
		}
		if (flag)
		{
			try
			{
				File.Delete(text);
			}
			catch (Exception e2)
			{
				LogHelper.Instance.Log("Unable to delete backup defs file.", e2);
			}
		}
		ClosePort();
		LogHelper.Instance.Log("Update device images complete.");
		return result;
	}

	public void ShowStates(int waitTime)
	{
		ExecuteQRCommand(CommandType.SetGraphicsOnlyDisplayMode);
		ExecuteQRCommand(CommandType.SetStartGraphicsPagePointer, "0800");
		ExecuteQRCommand(CommandType.SetMemoryWritePointer, "0800");
		foreach (QuickReturnState state in States)
		{
			ExecuteQRCommand(CommandType.SetMemoryWritePointer, "0800");
			ExecuteQRCommand(CommandType.SetEEPROMPointer, state.GraphicsAddress);
			ExecuteQRCommand(CommandType.SetEEPROMReadSize, ConvertIntToHexString(2048));
			ExecuteQRCommand(CommandType.LoadFromEEPROMToDisplayMemory);
			Thread.Sleep(waitTime);
		}
	}

	public bool ClearAttentionButtonStatus()
	{
		lock (Lock)
		{
			return ExecuteQRCommand(CommandType.ClearQRButtonStatus);
		}
	}

	public bool CheckAttButton(out bool commError)
	{
		lock (Lock)
		{
			commError = false;
			if (Port == null)
			{
				return false;
			}
			CoreResponse coreResponse = ServiceLocator.Instance.GetService<ICoreCommandExecutor>().ExecuteControllerCommand(CoreCommand.Create(CommandType.ReadQRButton, Port));
			LogHelper.Instance.Log(string.Format("QR status returned {0}", (coreResponse != null) ? coreResponse.OpCodeResponse : "COMMUNICATION ERROR"), LogEntryType.Debug);
			if (coreResponse.CommError)
			{
				LogHelper.Instance.Log("CheckAttButton: there was a timeout or communication error with the device.", LogEntryType.Error);
				commError = true;
				return false;
			}
			return coreResponse.OpCodeResponse.TrimStart().StartsWith("02");
		}
	}

	public bool TurnOnLED(LED led, bool blink)
	{
		CommandType commandType = CommandType.TurnOnGreenButtonLed;
		switch (led)
		{
		case LED.GreenButton:
			commandType = (blink ? CommandType.BlinkGreenButtonLed : CommandType.TurnOnGreenButtonLed);
			break;
		case LED.GreenArrow:
			commandType = (blink ? CommandType.BlinkGreenArrowLed : CommandType.TurnOnGreenArrowLed);
			break;
		case LED.RedArrow:
			commandType = (blink ? CommandType.BlinkRedArrowLed : CommandType.TurnOnRedArrowLed);
			break;
		case LED.RedButton:
			commandType = (blink ? CommandType.BlinkRedButtonLed : CommandType.TurnOnRedButtonLed);
			break;
		case LED.Background:
			commandType = (blink ? CommandType.BlinkBackLight : CommandType.TurnOnBackLight);
			break;
		default:
			LogHelper.Instance.Log($"Invalid option for LED: {led}", LogEntryType.Error);
			return false;
		}
		return ExecuteQRCommand(commandType);
	}

	public bool TurnOffLED(LED led)
	{
		lock (Lock)
		{
			CommandType commandType = CommandType.TurnOffGreenButtonLed;
			switch (led)
			{
			case LED.GreenButton:
				commandType = CommandType.TurnOffGreenButtonLed;
				break;
			case LED.RedButton:
				commandType = CommandType.TurnOffRedButtonLed;
				break;
			case LED.GreenArrow:
				commandType = CommandType.TurnOffGreenArrowLed;
				break;
			case LED.RedArrow:
				commandType = CommandType.TurnOffRedArrowLed;
				break;
			case LED.Background:
				commandType = CommandType.TurnOffBackLight;
				break;
			default:
				LogHelper.Instance.Log($"Invalid option for LED: {led}", LogEntryType.Error);
				return false;
			}
			return ExecuteQRCommand(commandType);
		}
	}

	public bool CreateState(string name, string file, string ledStates)
	{
		lock (Lock)
		{
			QuickReturnState quickReturnState = States.Find((QuickReturnState each) => string.Compare(name.ToUpper(), each.Name, ignoreCase: true) == 0);
			if (quickReturnState != null)
			{
				List<LedState> list = ParseLedStates(ledStates);
				if (list.Count > 0)
				{
					quickReturnState.LedStates.Clear();
					quickReturnState.LedStates.AddRange(list);
				}
				LogHelper.Instance.Log($"State {name} already defined; returning.", LogEntryType.Debug);
				return true;
			}
			if (!ExecuteQRCommand(CommandType.SetGraphicsColumns, "10"))
			{
				LogHelper.Instance.Log("CreateState: unable to set graphics columns to 10", LogEntryType.Error);
				return false;
			}
			if (!ExecuteQRCommand(CommandType.SetGraphicsOnlyDisplayMode))
			{
				LogHelper.Instance.Log("CreateState: unable to set graphics only mode.", LogEntryType.Error);
				return false;
			}
			string text = ConvertIntToHexString(m_currentGraphicsAddress);
			quickReturnState = new QuickReturnState
			{
				Name = name.ToUpper(),
				GraphicsAddress = (string.IsNullOrEmpty(file) ? null : text)
			};
			string[] data = null;
			if (!string.IsNullOrEmpty(file))
			{
				data = ReadStringFile(file);
			}
			List<LedState> list2 = ParseLedStates(ledStates);
			if (list2.Count > 0)
			{
				quickReturnState.LedStates.Clear();
				quickReturnState.LedStates.AddRange(list2);
			}
			if (LoadData(data, quickReturnState.GraphicsAddress, GraphicWriteTarget.Display) > 0)
			{
				m_currentGraphicsAddress += 2048;
			}
			States.Add(quickReturnState);
			LogHelper.Instance.Log($"Created state {quickReturnState.Name}, graphics addr = {quickReturnState.GraphicsAddress}", LogEntryType.Info);
		}
		return true;
	}

	public bool DisplayState(string name)
	{
		lock (Lock)
		{
			if (Port == null)
			{
				return false;
			}
			QuickReturnState quickReturnState = States.Find((QuickReturnState each) => string.Compare(name.ToUpper(), each.Name, ignoreCase: true) == 0);
			if (quickReturnState == null)
			{
				LogHelper.Instance.Log($"Unable to find state {name.ToUpper()}; bailing.", LogEntryType.Error);
				return false;
			}
			if (!ExecuteQRCommand(CommandType.SetMemoryWritePointer, "0800"))
			{
				LogHelper.Instance.Log(string.Format("State {0}: unable to set graphics page pointer to {1}", quickReturnState.Name, "0800"), LogEntryType.Error);
				return false;
			}
			if (!ExecuteQRCommand(CommandType.SetEEPROMPointer, quickReturnState.GraphicsAddress))
			{
				LogHelper.Instance.Log($"State {quickReturnState.Name}: unable to set EEPROM pointer to {quickReturnState.GraphicsAddress}", LogEntryType.Error);
				return false;
			}
			string text = ConvertIntToHexString(2048);
			if (!ExecuteQRCommand(CommandType.SetEEPROMReadSize, text))
			{
				LogHelper.Instance.Log($"State {quickReturnState.Name}: unable to set EEPROM read size {text}", LogEntryType.Error);
				return false;
			}
			if (!ExecuteQRCommand(CommandType.LoadFromEEPROMToDisplayMemory))
			{
				LogHelper.Instance.Log(string.Format("State {0}: unable to load graphics from EEPROM", quickReturnState.Name, text), LogEntryType.Error);
				return false;
			}
			foreach (LedState ledState in quickReturnState.LedStates)
			{
				if (ledState.Mode == LEDMode.On || ledState.Mode == LEDMode.Blink)
				{
					if (!TurnOnLED(ledState.Led, ledState.Mode == LEDMode.Blink))
					{
						return false;
					}
				}
				else if (!TurnOffLED(ledState.Led))
				{
					return false;
				}
			}
			return true;
		}
	}

	public DeviceStatus GetStatus()
	{
		DeviceStatus deviceStatus = DeviceStatus.None;
		if (IsConfigured)
		{
			deviceStatus |= DeviceStatus.Configured;
		}
		if (new QuickReturnDescriptor().Locate())
		{
			deviceStatus |= DeviceStatus.Found;
		}
		return deviceStatus;
	}

	private string ConvertIntToHexString(int i)
	{
		return $"{i:x4}".ToUpper();
	}

	private bool SetGraphicWidth(string width)
	{
		LogHelper.Instance.Log($"Setting width to {width}", LogEntryType.Info);
		return ExecuteQRCommand(CommandType.SetGraphicsColumns, width);
	}

	private bool ClearAllDisplayMemory()
	{
		ExecuteQRCommand(CommandType.SetMemoryWritePointer, "0000");
		return ExecuteQRCommand(CommandType.ClearDisplayMemory, "1FFF");
	}

	private string[] ReadStringFile(string fileName)
	{
		if (string.IsNullOrEmpty(fileName))
		{
			return new string[0];
		}
		string path = Path.Combine(GraphicsPath, fileName);
		List<string> list = new List<string>();
		using (StreamReader streamReader = new StreamReader(path))
		{
			string text = "";
			while (text != null)
			{
				text = streamReader.ReadLine();
				if (text != null)
				{
					list.Add(text.Trim());
				}
			}
		}
		return list.ToArray();
	}

	private int LoadData(string[] data, string address, GraphicWriteTarget target)
	{
		bool flag = false;
		if (!string.IsNullOrEmpty(address))
		{
			flag = ExecuteQRCommand(CommandType.SetMemoryWritePointer, address);
			if (!flag)
			{
				LogHelper.Instance.Log($"Failed to read data into address {address}", LogEntryType.Error);
				return 0;
			}
			LogHelper.Instance.Log($"ReadDataIntoAddress: Set write pointer to {address}", LogEntryType.Info);
		}
		int num = 0;
		foreach (string text in data)
		{
			switch (target)
			{
			case GraphicWriteTarget.Display:
				flag = ExecuteQRCommand(CommandType.WriteGraphicData, text);
				break;
			case GraphicWriteTarget.EEPROM:
				flag = ExecuteQRCommand(CommandType.WriteGraphicDataToEEPROM, text);
				break;
			}
			if (!flag)
			{
				LogHelper.Instance.Log("Unable to write data.", LogEntryType.Error);
			}
			else
			{
				num += text.Length >> 1;
			}
		}
		LogHelper.Instance.Log("Read data into address.", LogEntryType.Debug);
		return num;
	}

	private List<LedState> ParseLedStates(string states)
	{
		List<LedState> list = new List<LedState>();
		string[] array = states.Split(';');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(',');
			if (array2[0].StartsWith("led", StringComparison.CurrentCultureIgnoreCase) && array2[1].StartsWith("mode", StringComparison.CurrentCultureIgnoreCase))
			{
				LED led = Enum<LED>.ParseIgnoringCase(array2[0].Split('=')[1], LED.Background);
				LEDMode mode = Enum<LEDMode>.ParseIgnoringCase(array2[1].Split('=')[1], LEDMode.Unknown);
				list.Add(new LedState
				{
					Led = led,
					Mode = mode
				});
			}
		}
		return list;
	}

	private QuickReturnDevice()
	{
		Configuration = new RedboxSerialPortConfiguration();
		Configuration.Test = ResponseValid;
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		ImagesDirectory = service.RuntimePath("qr-chamfer");
		StateFile = service.RuntimePath("qr.state");
		GraphicsPath = service.RuntimePath("qr-chamfer");
	}

	private bool ExecuteQRCommand(CommandType type, string format)
	{
		if (Port == null)
		{
			return false;
		}
		return ExecuteCommand(CoreCommand.Create(type, Port, format));
	}

	private bool ExecuteQRCommand(CommandType type)
	{
		if (Port == null)
		{
			return false;
		}
		return ExecuteCommand(CoreCommand.Create(type, Port));
	}

	private bool ExecuteCommand(CoreCommand command)
	{
		CoreResponse coreResponse = ServiceLocator.Instance.GetService<ICoreCommandExecutor>().ExecuteControllerCommand(command);
		if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
		{
			LogHelper.Instance.Log(string.Format("The command {0} returned {1}", command.Command, (coreResponse == null) ? "ERROR" : coreResponse.OpCodeResponse), LogEntryType.Debug);
		}
		if (coreResponse.CommError)
		{
			LogHelper.Instance.Log("QuickReturn: communication problem.", LogEntryType.Info);
			return false;
		}
		bool num = coreResponse.OpCodeResponse.Trim().EndsWith("OK");
		if (!num)
		{
			LogHelper.Instance.Log($"QR: command {command.Command} failed", LogEntryType.Error);
		}
		return num;
	}

	private bool ResponseValid(IChannelResponse response)
	{
		string text = Encoding.ASCII.GetString(response.RawResponse);
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		return text.IndexOf("OK") != -1;
	}

	private bool ReadStateTable(string fileName)
	{
		if (!File.Exists(fileName))
		{
			LogHelper.Instance.Log($"Unable to locate state table {fileName}", LogEntryType.Error);
			return false;
		}
		using (StreamReader streamReader = new StreamReader(fileName))
		{
			while (true)
			{
				string text = streamReader.ReadLine();
				if (text == null)
				{
					break;
				}
				string[] array = text.Split('=');
				if (array.Length == 2)
				{
					string text2 = RemoveString(array[0], ".grf");
					if (!int.TryParse(array[1], out var result))
					{
						LogHelper.Instance.Log($"Unable to parse state table entry {text}", LogEntryType.Error);
						continue;
					}
					QuickReturnState item = new QuickReturnState
					{
						Name = text2.ToUpper(),
						GraphicsAddress = ConvertIntToHexString(result * 2048)
					};
					States.Add(item);
				}
			}
		}
		return true;
	}

	private string RemoveString(string value, string token)
	{
		int num = value.IndexOf(token);
		if (num <= -1)
		{
			return value;
		}
		return value.Remove(num, token.Length);
	}

	private bool OpenPort()
	{
		if (!IsConfigured)
		{
			return false;
		}
		Port = PortManager.Instance.Scan(ControllerConfiguration.Instance.QuickReturnComPort, Configuration, TestPort, PortModes.Async);
		return Port != null;
	}

	private bool TestPort(RedboxSerialPort port)
	{
		CoreResponse coreResponse = ServiceLocator.Instance.GetService<ICoreCommandExecutor>().ExecuteControllerCommand(CoreCommand.Create(CommandType.SideTerminalVersion, port));
		if (coreResponse.CommError)
		{
			LogHelper.Instance.Log("[QuickReturn Scan test] There was a communication error trying to chat to the port.");
			return false;
		}
		if (coreResponse.OpCodeResponse.Contains("Leiwe"))
		{
			LogHelper.Instance.Log("[QuickReturn Scan test] Received recognized response '{0}'", coreResponse.OpCodeResponse);
			return true;
		}
		LogHelper.Instance.Log("[QuickReturn Scan test] Did not recognize response '{0}'.", coreResponse.OpCodeResponse);
		return false;
	}
}
