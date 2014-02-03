using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Redbox.HAL.Core;

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

	public enum Mode
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

		internal Mode Mode { get; set; }
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
				Mode = Mode.Off
			});
			LedStates.Add(new LedState
			{
				Led = LED.RedButton,
				Mode = Mode.Off
			});
			LedStates.Add(new LedState
			{
				Led = LED.GreenArrow,
				Mode = Mode.Off
			});
			LedStates.Add(new LedState
			{
				Led = LED.RedArrow,
				Mode = Mode.Off
			});
			LedStates.Add(new LedState
			{
				Led = LED.Background,
				Mode = Mode.Off
			});
		}

		internal void Dump()
		{
			LogHelper.Instance.Log($"State {Name} showing at page {GraphicsAddress}", LogEntryType.Info);
		}
	}

	private const int SideTerminalPixelWidth = 240;

	private const int SideTerminalPixelHeight = 128;

	private const int ChamferTerminalPixelWidth = 128;

	private const int ChamferTerminalPixelHeight = 128;

	private const int StartGraphicsAddress = 0;

	private const int ChamferPageSize = 2048;

	private const string DefaultPort = "COM4";

	private const int ChamferMaxPages = 16;

	private const string ChamferGraphicsSize = "10";

	private const string ChamferMemorySize = "1FFF";

	private const string PortTag = "Port";

	private const string DeviceNameTag = "QRTerminal";

	private int m_currentGraphicsAddress;

	private string m_preferredDisplayAddress = "0800";

	private string m_imagesDirectory;

	private string m_stateTable;

	private string m_version;

	private Dictionary<string, object> m_configAttributes = new Dictionary<string, object>();

	private readonly List<QuickReturnState> States = new List<QuickReturnState>();

	private string m_grfPath;

	public static QuickReturnDevice Instance => Singleton<QuickReturnDevice>.Instance;

	public string Version
	{
		get
		{
			if (string.IsNullOrEmpty(m_version))
			{
				CoreResponse coreResponse = CoreController.ExecuteCoreCommand(new CoreCommand(CommandType.SideTerminalVersion, Port));
				m_version = (coreResponse.CommunicationError ? (m_version = "COMMUNICATION ERROR") : coreResponse.OpCodeResponse.TrimStart());
			}
			return m_version;
		}
	}

	public int BaudRate => 115200;

	public bool IsConfigured => Port != null;

	internal RedboxSerialPort Port { get; private set; }

	private string QuickReturnCommonFiles { get; set; }

	private int PageSize => 2048;

	private string MemorySize => "1FFF";

	private string GraphicsWidth => "10";

	private int MaxPages => 16;

	private string ImagesDirectory
	{
		get
		{
			if (string.IsNullOrEmpty(m_imagesDirectory))
			{
				m_imagesDirectory = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "qr-chamfer");
			}
			return m_imagesDirectory;
		}
	}

	private string StateFile
	{
		get
		{
			if (string.IsNullOrEmpty(m_stateTable))
			{
				m_stateTable = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "qr.state");
			}
			return m_stateTable;
		}
	}

	private string GraphicsPath
	{
		get
		{
			if (string.IsNullOrEmpty(m_grfPath))
			{
				m_grfPath = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "qr-chamfer");
			}
			return m_grfPath;
		}
	}

	public static QuickReturnDevice MakeExternalInstance()
	{
		return new QuickReturnDevice();
	}

	public void ConfigurationChange()
	{
		if (Port != null)
		{
			Port.Dispose();
			Port = null;
		}
		ConfigurePort();
	}

	public void DumpStateTable()
	{
		if (States == null)
		{
			return;
		}
		foreach (QuickReturnState state in States)
		{
			state.Dump();
		}
	}

	public bool ClosePort()
	{
		if (Port == null)
		{
			return false;
		}
		LogHelper.Instance.Log($"QR Poller: shutting down port {Port.Name}", LogEntryType.Info);
		return Port.ClosePort();
	}

	public bool Reset()
	{
		lock (this)
		{
			States.Clear();
			m_currentGraphicsAddress = 0;
			LogHelper.Instance.Log(">> QuickReturnDevice.Reset()", LogEntryType.Debug);
			Thread.Sleep(2000);
			if (!ExecuteQRCommand(CommandType.SetTextColumns, "1E"))
			{
				LogHelper.Instance.Log("Unable to set text columns.", LogEntryType.Error);
				return false;
			}
			if (!ExecuteQRCommand(CommandType.SetGraphicsColumns, GraphicsWidth))
			{
				LogHelper.Instance.Log($"Unable to set graphics columns to {GraphicsWidth}", LogEntryType.Error);
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
			if (!ExecuteQRCommand(CommandType.SetStartGraphicsPagePointer, m_preferredDisplayAddress))
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
			string version = Version;
			if (!ReadStateTable(StateFile))
			{
				LogHelper.Instance.Log($"Unable to read state file {StateFile}", LogEntryType.Info);
				return false;
			}
			ExecuteQRCommand(CommandType.SetMemoryWritePointer, "0000");
			ExecuteQRCommand(CommandType.ClearDisplayMemory, "1FFF");
			LogHelper.Instance.Log($"<< QuickReturnDevice.Reset() ({version})", LogEntryType.Debug);
			return true;
		}
	}

	public int UpdateDeviceImages()
	{
		ExecuteQRCommand(CommandType.SetMemoryWritePointer, "0000");
		ExecuteQRCommand(CommandType.SetEEPROMPointer, "0000");
		ExecuteQRCommand(CommandType.SetEEPROMReadSize, "0010");
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
		}
		using (StreamWriter streamWriter = new StreamWriter(StateFile))
		{
			string[] array = files;
			foreach (string text2 in array)
			{
				string[] data = ReadStringFile(text2);
				LoadData(data, string.Empty, GraphicWriteTarget.EEPROM);
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
		string arg = "UNKNOWN";
		if (Port != null)
		{
			Port.ClosePort();
			try
			{
				arg = (Port.IsOpen ? "open" : "closed");
			}
			catch
			{
				arg = "UNKNOWN";
			}
		}
		LogHelper.Instance.Log($"Update EEPROM complete - port status is {arg}.", LogEntryType.Info);
		return result;
	}

	public void ShowStates(int waitTime)
	{
		ExecuteQRCommand(CommandType.SetGraphicsOnlyDisplayMode);
		ExecuteQRCommand(CommandType.SetStartGraphicsPagePointer, m_preferredDisplayAddress);
		ExecuteQRCommand(CommandType.SetMemoryWritePointer, m_preferredDisplayAddress);
		foreach (QuickReturnState state in States)
		{
			ExecuteQRCommand(CommandType.SetMemoryWritePointer, m_preferredDisplayAddress);
			ExecuteQRCommand(CommandType.SetEEPROMPointer, state.GraphicsAddress);
			ExecuteQRCommand(CommandType.SetEEPROMReadSize, ConvertIntToHexString(PageSize));
			ExecuteQRCommand(CommandType.LoadFromEEPROMToDisplayMemory);
			Thread.Sleep(waitTime);
		}
	}

	public object GetOption(string optionName)
	{
		return null;
	}

	public bool ClearAttentionButtonStatus()
	{
		return ExecuteQRCommand(CommandType.ClearQRButtonStatus);
	}

	public bool CheckAttButton(out bool commError)
	{
		commError = false;
		CoreResponse coreResponse = CoreController.ExecuteCoreCommand(new CoreCommand(CommandType.ReadQRButton, Port));
		LogHelper.Instance.Log(string.Format("QR status returned {0}", (coreResponse != null) ? coreResponse.OpCodeResponse : "COMMUNICATION ERROR"), LogEntryType.Debug);
		if (coreResponse.CommunicationError)
		{
			LogHelper.Instance.Log("CheckAttButton: there was a timeout or communication error with the device.", LogEntryType.Error);
			commError = true;
			return false;
		}
		string text = coreResponse.OpCodeResponse.TrimStart();
		return text.StartsWith("02");
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

	public bool CreateState(string name, string file, string ledStates)
	{
		lock (this)
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
			if (!ExecuteQRCommand(CommandType.SetGraphicsColumns, GraphicsWidth))
			{
				LogHelper.Instance.Log("CreateState: unable to set graphics columns to " + GraphicsWidth, LogEntryType.Error);
				return false;
			}
			if (!ExecuteQRCommand(CommandType.SetGraphicsOnlyDisplayMode))
			{
				LogHelper.Instance.Log("CreateState: unable to set graphics only mode.", LogEntryType.Error);
				return false;
			}
			string text = ConvertIntToHexString(m_currentGraphicsAddress);
			QuickReturnState quickReturnState2 = new QuickReturnState();
			quickReturnState2.Name = name.ToUpper();
			quickReturnState2.GraphicsAddress = (string.IsNullOrEmpty(file) ? null : text);
			quickReturnState = quickReturnState2;
			string[] data = null;
			if (!string.IsNullOrEmpty(file))
			{
				data = ReadFile(file);
			}
			List<LedState> list2 = ParseLedStates(ledStates);
			if (list2.Count > 0)
			{
				quickReturnState.LedStates.Clear();
				quickReturnState.LedStates.AddRange(list2);
			}
			if (LoadData(data, quickReturnState.GraphicsAddress, GraphicWriteTarget.Display) > 0)
			{
				m_currentGraphicsAddress += PageSize;
			}
			States.Add(quickReturnState);
			LogHelper.Instance.Log($"Created state {quickReturnState.Name}, graphics addr = {quickReturnState.GraphicsAddress}", LogEntryType.Info);
		}
		return true;
	}

	public bool DisplayState(string name)
	{
		lock (this)
		{
			QuickReturnState quickReturnState = States.Find((QuickReturnState each) => string.Compare(name.ToUpper(), each.Name, ignoreCase: true) == 0);
			if (quickReturnState == null)
			{
				LogHelper.Instance.Log($"Unable to find state {name.ToUpper()}; bailing.", LogEntryType.Error);
				return false;
			}
			if (!ExecuteQRCommand(CommandType.SetMemoryWritePointer, m_preferredDisplayAddress))
			{
				LogHelper.Instance.Log($"State {quickReturnState.Name}: unable to set graphics page pointer to {m_preferredDisplayAddress}", LogEntryType.Error);
				return false;
			}
			if (!ExecuteQRCommand(CommandType.SetEEPROMPointer, quickReturnState.GraphicsAddress))
			{
				LogHelper.Instance.Log($"State {quickReturnState.Name}: unable to set EEPROM pointer to {quickReturnState.GraphicsAddress}", LogEntryType.Error);
				return false;
			}
			string text = ConvertIntToHexString(PageSize);
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
				if (ledState.Mode == Mode.On || ledState.Mode == Mode.Blink)
				{
					if (!TurnOnLED(ledState.Led, ledState.Mode == Mode.Blink))
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

	public int LoadData(string[] data, string address, GraphicWriteTarget target)
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

	public string[] ReadFile(string fileName)
	{
		return ReadStringFile(fileName);
	}

	public static string ConvertShortToHex(short number)
	{
		string text = number.ToString("X");
		if (number < 16)
		{
			text = $"0{text}";
		}
		return text;
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
		return ExecuteQRCommand(CommandType.ClearDisplayMemory, MemorySize);
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

	private List<LedState> ParseLedStates(string states)
	{
		List<LedState> list = new List<LedState>();
		string[] array = states.Split(';');
		string[] array2 = array;
		foreach (string text in array2)
		{
			string[] array3 = text.Split(',');
			if (array3[0].StartsWith("led", StringComparison.CurrentCultureIgnoreCase) && array3[1].StartsWith("mode", StringComparison.CurrentCultureIgnoreCase))
			{
				string value = array3[0].Split('=')[1];
				LED led = Enum<LED>.Parse(value, LED.Background);
				string value2 = array3[1].Split('=')[1];
				Mode mode = Enum<Mode>.Parse(value2, Mode.Unknown);
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
		ConfigurePort();
	}

	private bool ExecuteQRCommand(CommandType type, string format)
	{
		return ExecuteCommand(new CoreCommand(type, Port, format));
	}

	private bool ExecuteQRCommand(CommandType type)
	{
		return ExecuteCommand(new CoreCommand(type, Port));
	}

	private bool ExecuteCommand(CoreCommand command)
	{
		CoreResponse coreResponse = CoreController.ExecuteCoreCommand(command);
		if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
		{
			LogHelper.Instance.Log(string.Format("The command {0} returned {1}", command.Command, (coreResponse == null) ? "ERROR" : coreResponse.OpCodeResponse), LogEntryType.Debug);
		}
		if (coreResponse.CommunicationError)
		{
			LogHelper.Instance.Log("QuickReturn: communication problem.", LogEntryType.Info);
			return false;
		}
		string text = coreResponse.OpCodeResponse.Trim();
		bool flag = text.EndsWith("OK");
		if (!flag)
		{
			LogHelper.Instance.Log($"QR: command {command.Command} failed", LogEntryType.Error);
		}
		return flag;
	}

	private bool ResponseValid(ChannelResponse response)
	{
		string text = Encoding.ASCII.GetString(response.RawResponse);
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		if (text.IndexOf("OK") == -1 && text.IndexOf("EER") == -1)
		{
			return false;
		}
		return true;
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
					string text2 = StringExtensions.RemoveString(array[0], ".grf");
					if (!int.TryParse(array[1], out var result))
					{
						LogHelper.Instance.Log($"Unable to parse state table entry {text}", LogEntryType.Error);
						continue;
					}
					QuickReturnState quickReturnState = new QuickReturnState();
					quickReturnState.Name = text2.ToUpper();
					quickReturnState.GraphicsAddress = ConvertIntToHexString(result * PageSize);
					QuickReturnState item = quickReturnState;
					States.Add(item);
				}
			}
		}
		return true;
	}

	private void ConfigurePort()
	{
		string quickReturnComPort = ControllerConfiguration.Instance.QuickReturnComPort;
		if (string.IsNullOrEmpty(ControllerConfiguration.Instance.QuickReturnComPort) || !quickReturnComPort.StartsWith("COM", StringComparison.CurrentCultureIgnoreCase))
		{
			Port = null;
			return;
		}
		RedboxSerialPortConfiguration redboxSerialPortConfiguration = new RedboxSerialPortConfiguration(ControllerConfiguration.Instance.QuickReturnComPort);
		redboxSerialPortConfiguration.Test = ResponseValid;
		Port = new RedboxSerialPort(redboxSerialPortConfiguration);
	}
}
