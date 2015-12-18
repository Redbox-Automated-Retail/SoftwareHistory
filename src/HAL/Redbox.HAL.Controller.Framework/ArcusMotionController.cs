using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Controller.Framework;

internal sealed class ArcusMotionController : MotionController
{
	internal delegate bool MotorStopped(int status);

	private const string HaltMotorCommand = "RSTOP";

	private const string NopCommand = "NOP";

	private const string WaitCommand = "WAIT";

	private const string WaitAfterHomeCommand = "WAITH";

	private const string HomeXCommand = "HOMEX";

	private const string WaitYDropBackCommand = "WAITYDB";

	private const string WaitXYCommand = "WAITXY";

	private readonly bool DebugEnabled;

	private readonly PortModes Mode;

	private readonly object m_lock = new object();

	private readonly List<string> m_movePreamble = new List<string>();

	private readonly string[] CommOkCommand = new string[1] { "$" };

	private readonly DeviceDescriptor m_arcusDescriptor;

	private RedboxSerialPort Port;

	private string m_waitXCommand;

	private string m_waitYCommand;

	private string m_waitXYCommand;

	private string m_lspdCommand;

	private string[] m_initCommand;

	private string m_correctiveToleranceCommand;

	private static ArcusMotionController m_instance;

	internal bool XAxisHomed { get; private set; }

	internal bool YAxisHomed { get; private set; }

	public override bool CommunicationOk()
	{
		string[] array = OnSendCommands(CommOkCommand);
		string text = array[0];
		if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
		{
			LogHelper.Instance.Log($"Arcus comm ok: command returns {text}", LogEntryType.Info);
		}
		if (IsResponseAnError(text))
		{
			return false;
		}
		return text.StartsWith("OK");
	}

	public override bool GetCurrentLocation(Axis axis, ref MoveTarget target)
	{
		string instruction = ((axis == Axis.X) ? "EX" : "PY");
		if (!GetArcusPosition(instruction, out var position))
		{
			return false;
		}
		if (axis == Axis.X)
		{
			target.XCoordinate = position;
		}
		else if (Axis.Y == axis)
		{
			target.YCoordinate = position;
		}
		return true;
	}

	public override bool IsMotorErrored(Axis axis, out MotionResult status)
	{
		status = MotionResult.Success;
		if (Axis.XY == axis || Axis.Y == axis)
		{
			string s = OnSendCommands(new string[2] { "MIOY", "MECLEARY" })[0].Trim();
			if (int.TryParse(s, out var result))
			{
				status = MotionResult.Error;
				if (result > 0)
				{
					return result < 8;
				}
				return false;
			}
			return false;
		}
		return false;
	}

	protected override HomeAxesResult InitAxesInner(IFormattedLog appLog, bool quick)
	{
		HomeAxesResult homeAxesResult = new HomeAxesResult();
		appLog.WriteFormatted("Home the X motor.");
		int num = (quick ? 1 : 2);
		for (int i = 0; i < num; i++)
		{
			MotionResult motionResult = HomeAxis(Axis.X);
			if (motionResult != MotionResult.Success)
			{
				string text = $"The HOMEX operation failed: {motionResult.ToString().ToUpper()}.";
				homeAxesResult.ErrorList.Add(Error.NewError("E999", "Init axes error.", text));
				homeAxesResult.FailedAxis = Axis.X;
				appLog.WriteFormatted(text, LogEntryType.Error);
				return homeAxesResult;
			}
			if (i == 0)
			{
				MoveTarget target = new MoveTarget
				{
					Axis = Axis.X,
					XCoordinate = -200
				};
				MoveToTargetInner(target);
			}
		}
		appLog.WriteFormatted("Home the Y Motor.");
		MotionResult motionResult2 = HomeAxis(Axis.Y);
		if (motionResult2 != MotionResult.Success)
		{
			string text2 = $"The HOMEY operation failed: {motionResult2.ToString().ToUpper()}.";
			homeAxesResult.ErrorList.Add(Error.NewError("E999", "Init axes error.", text2));
			homeAxesResult.FailedAxis = Axis.Y;
			appLog.WriteFormatted(text2, LogEntryType.Error);
			return homeAxesResult;
		}
		homeAxesResult.Succeded = true;
		return homeAxesResult;
	}

	protected override bool ClosePortInner()
	{
		bool xAxisHomed = (YAxisHomed = false);
		XAxisHomed = xAxisHomed;
		return Port.Close();
	}

	protected override bool IsInitializedInner()
	{
		if (!ControllerConfiguration.Instance.ValidateControllerHomeStatus)
		{
			return true;
		}
		int num = 0;
		if (!XAxisHomed)
		{
			num++;
			LogHelper.Instance.Log("The X axis did not init.");
		}
		if (!YAxisHomed)
		{
			num++;
			LogHelper.Instance.Log("The Y axis did not init.");
		}
		return num == 0;
	}

	protected override MotionResult MoveToTargetInner(MoveTarget target)
	{
		if (!ClearYMotorError())
		{
			return MotionResult.Error;
		}
		if (target.Axis == Axis.X)
		{
			return MoveAbsoluteInternal(target.Axis, target.XCoordinate.Value, null);
		}
		if (target.Axis == Axis.Y)
		{
			return MoveAbsoluteInternal(target.Axis, target.YCoordinate.Value, null);
		}
		return MoveAbsoluteInternal(target.Axis, target.XCoordinate.Value, target.YCoordinate.Value);
	}

	protected override MotionResult HomeAxisInner(Axis axis)
	{
		LogHelper.Instance.Log("Home axis {0}", axis.ToString());
		if (axis != Axis.X)
		{
			return HomeYAxis();
		}
		return HomeXAxis();
	}

	protected override MotionResult MoveToVendInner(MoveMode mode)
	{
		int num = ControllerConfiguration.Instance.VendYPosition;
		if (ControllerConfiguration.Instance.VendPositionReceiveOffset != 0)
		{
			switch (mode)
			{
			case MoveMode.Get:
				num -= ControllerConfiguration.Instance.VendPositionReceiveOffset;
				break;
			case MoveMode.Put:
				num += ControllerConfiguration.Instance.VendPositionReceiveOffset;
				break;
			}
		}
		if (ControllerConfiguration.Instance.QueryPositionForVendMove)
		{
			MoveTarget target = default(MoveTarget);
			if (GetCurrentLocation(Axis.Y, ref target) && target.YCoordinate.Value == num)
			{
				if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
				{
					LogHelper.Instance.Log($"Picker currently at position Y = {target.YCoordinate.Value}", LogEntryType.Info);
				}
				return MotionResult.Success;
			}
		}
		if (!ClearYMotorError())
		{
			return MotionResult.Error;
		}
		return MoveAbsoluteInternal(Axis.Y, num, null);
	}

	protected override string[] ReadLimitsInner()
	{
		if (!ReadLimitsRaw(out var upper, out var lower))
		{
			return new string[1] { "INVALID QUERY RESPONSE" };
		}
		LogHelper.Instance.Log("Upper = {0}, lower = {1}", upper, lower);
		return new string[2]
		{
			string.Format("UPPER LIMIT: {0}", upper ? "BLOCKED" : "CLEAR"),
			string.Format("LOWER LIMIT: {0}", lower ? "BLOCKED" : "CLEAR")
		};
	}

	internal static ArcusMotionController MakeInstance()
	{
		if (m_instance == null)
		{
			m_instance = new ArcusMotionController();
		}
		return m_instance;
	}

	internal override bool ResetDeviceDriver()
	{
		ClosePortInner();
		Thread.Sleep(3000);
		using (DeviceHelper deviceHelper = new DeviceHelper())
		{
			if (deviceHelper.ResetDriver(m_arcusDescriptor))
			{
				return true;
			}
		}
		Process process = null;
		bool flag = false;
		try
		{
			process = Process.Start("c:\\Program Files\\Redbox\\hal-tools\\halutilities.exe", "-resetProteus");
			process.WaitForExit();
			flag = process.ExitCode == 0;
			string text = string.Format("{0} corrected an Arcus communication error.", flag ? "Successfully" : "Unsuccessfully");
			WriteToLog(text);
			if (!flag)
			{
				LogHelper.Instance.Log(text, LogEntryType.Error);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unable to reset Proteus driver.", e);
			flag = false;
		}
		return flag;
	}

	internal override void InitializeCommunication()
	{
		OnSendCommands(m_initCommand);
	}

	internal void ComputeMoveCommands()
	{
		LogHelper.Instance.Log("Re-computing move commands.");
		m_initCommand = new string[19]
		{
			"$",
			"$",
			"$",
			"RSTOP",
			"MECLEARX",
			"MECLEARY",
			"MECLEARZ",
			"MECLEARU",
			"I1=4204544",
			"I2=3136",
			"I3=" + ControllerConfiguration.Instance.GearX.GetEncoderRatio(),
			"I4=0",
			"I5=0",
			"I6=0",
			"I7=10",
			"I8=0",
			"I9=2000",
			"I10=0",
			"I11=2"
		};
		m_correctiveToleranceCommand = $"I7={(ControllerConfiguration.Instance.WidenArcusTolerance ? 5 : 2)}";
		m_movePreamble.Clear();
		m_movePreamble.AddRange(new string[10]
		{
			"$",
			"RSTOP",
			"MECLEARY",
			"MECLEARX",
			ControllerConfiguration.Instance.MoveAxisXYSpeed.GetHighSpeedCommand(),
			ControllerConfiguration.Instance.MoveAxisXYSpeed.GetLowSpeedCommand(),
			ControllerConfiguration.Instance.MoveAxisXYSpeed.GetAccelerationCommand(),
			"ABS",
			"I7=15",
			"I2=3136"
		});
		m_waitXCommand = $"WAITX {ControllerConfiguration.Instance.MoveTimeout}";
		m_waitYCommand = $"WAITY {ControllerConfiguration.Instance.MoveTimeout}";
		m_waitXYCommand = $"WAITXY {ControllerConfiguration.Instance.MoveTimeout}";
		m_lspdCommand = $"LSPD {(int)ControllerConfiguration.Instance.GearX.GetStepRatio() * 100}";
	}

	internal string WaitMotor(Axis axis, int? timeOut, MotorStopped motorRunning)
	{
		TimeSpan timespan = new TimeSpan(0, 0, 0, 0, ControllerConfiguration.Instance.ArcusMotorQueryPause);
		using ExecutionTimer executionTimer = new ExecutionTimer();
		string command = "MST" + axis.ToString().ToUpper();
		while (true)
		{
			MotionResult motionResult = CheckDoorSensors();
			if (motionResult != MotionResult.Success)
			{
				WriteToLog($"Door sensor query returned {motionResult.ToString()}: halting motion.");
				SendCommandInner("RSTOP");
				return "DOOR OPEN";
			}
			string text = SendCommandInner(command);
			if (text == "COMMUNICATION ERROR")
			{
				return text;
			}
			if (!int.TryParse(text, out var result))
			{
				return "INVALID MOTOR STATE";
			}
			if (!motorRunning(result))
			{
				break;
			}
			PerfFunctions.SpinWait(timespan);
			if (timeOut.HasValue && executionTimer.ElapsedMilliseconds >= timeOut)
			{
				string text2 = $"Unable to reach target on axis {axis.ToString().ToUpper()}; reported error: {ConvertMSTCodeToText(result)}";
				LogHelper.Instance.Log(text2, LogEntryType.Error);
				WriteToLog(text2);
				if (result > 7)
				{
					string text3 = $"The {axis.ToString()} axis motor returned an error code: {result} - {ConvertMSTCodeToText(result)}";
					LogHelper.Instance.Log(text3, LogEntryType.Info);
					WriteToLog(text3);
				}
				SendCommandInner("RSTOP");
				return "TIMEOUT";
			}
		}
		return "OK";
	}

	private string[] OnSendCommands(string[] commands)
	{
		lock (m_lock)
		{
			List<string> list = new List<string>();
			int num = 0;
			if (!Port.Open())
			{
				LogHelper.Instance.Log($"MotionControl: unable to open port {Port.Name}.", LogEntryType.Error);
				Port.Close();
				return new string[1] { "COMMUNICATION ERROR" };
			}
			int num2 = num;
			while (num2 < commands.Length)
			{
				string text = commands[num2];
				if (string.IsNullOrEmpty(text) || string.Compare(text, "NOP", ignoreCase: true) == 0)
				{
					LogHelper.Instance.Log("ArcusMotionController: Instruction=NOP", LogEntryType.Debug);
					list.Add("OK");
				}
				else
				{
					string text2 = SendCommand(text);
					list.Add(text2);
					if (IsResponseAnError(text2))
					{
						list.Clear();
						list.Add(text2);
						break;
					}
				}
				num2++;
				num++;
			}
			Port.ResetBuffers();
			return list.ToArray();
		}
	}

	private string ConvertMSTCodeToText(int num)
	{
		string text = "";
		return num switch
		{
			8 => "Upper Limit Error", 
			16 => "Lower Limit Error", 
			_ => num.ToString(), 
		};
	}

	private MotionResult HomeXAxis()
	{
		XAxisHomed = false;
		string[] array = new string[18]
		{
			"$",
			"RSTOP",
			"MECLEARX",
			"I1=4204544",
			ControllerConfiguration.Instance.InitAxisXSpeed.GetHighSpeedCommand(),
			ControllerConfiguration.Instance.InitAxisXSpeed.GetLowSpeedCommand(),
			ControllerConfiguration.Instance.InitAxisXSpeed.GetAccelerationCommand(),
			"INC",
			"EX=0",
			"PX=0",
			"I2=3072",
			"WAITX 5000",
			"HOMEX-",
			"WAITHX 64000",
			"I2=3136",
			"I7=15",
			"WAITX 5000",
			"MECLEARX"
		};
		int num = 13;
		string[] array2 = OnSendCommands(array);
		MotionResult motionResult = DecodeMotionResponse(array2, array.Length);
		if (motionResult != MotionResult.Success)
		{
			return motionResult;
		}
		if (array2[num] == "TIMEOUT")
		{
			return MotionResult.Timeout;
		}
		XAxisHomed = true;
		return MotionResult.Success;
	}

	private MotionResult HomeYAxis()
	{
		YAxisHomed = false;
		List<string> list = new List<string>();
		list.AddRange(new string[9]
		{
			"$",
			"RSTOP",
			"MECLEARY",
			ControllerConfiguration.Instance.InitAxisYSpeed.GetHighSpeedCommand(),
			ControllerConfiguration.Instance.InitAxisYSpeed.GetLowSpeedCommand(),
			ControllerConfiguration.Instance.InitAxisYSpeed.GetAccelerationCommand(),
			"INC",
			"EY=0",
			"PY=0"
		});
		if (ControllerConfiguration.Instance.HomeYDropBack != 0)
		{
			int num = ControllerConfiguration.Instance.HomeYDropBack;
			if (Math.Sign(num) == 1)
			{
				num = -num;
			}
			list.Add("MIOY");
			list.Add($"Y{num}");
			list.Add(string.Format("{0} 6000", "WAITYDB"));
			list.Add("MECLEARY");
		}
		int num2 = list.Count - 1;
		list.AddRange(new string[2] { "HOMEY+", "WAITHY 60000" });
		int num3 = list.Count - 1;
		string[] array = OnSendCommands(list.ToArray());
		MotionResult motionResult = DecodeMotionResponse(array, list.Count);
		if (motionResult != MotionResult.Success)
		{
			return motionResult;
		}
		if (ControllerConfiguration.Instance.HomeYDropBack != 0 && array[num2] == "TIMEOUT")
		{
			return MotionResult.Timeout;
		}
		if (array[num3] == "TIMEOUT")
		{
			return MotionResult.Timeout;
		}
		YAxisHomed = true;
		return MotionResult.Success;
	}

	private void DecodeWait(string waitCommand, out bool isHome, out Axis[] axis, out MotorStopped check)
	{
		MotorStopped motorStopped = (int status) => status != 0;
		MotorStopped motorStopped2 = (int status) => status > 0;
		MotorStopped motorStopped3 = (int status) => status > 0 && status < 15;
		if (DoorSensors.Instance.SensorsEnabled)
		{
			motorStopped = DefaultMotorRunningCheck;
			motorStopped2 = DefaultMotorRunningCheck;
		}
		isHome = false;
		check = motorStopped2;
		string text = null;
		if (waitCommand.Equals("WAITYDB", StringComparison.CurrentCultureIgnoreCase))
		{
			check = motorStopped3;
			axis = new Axis[1] { Axis.Y };
			return;
		}
		if (waitCommand.StartsWith("WAITH"))
		{
			isHome = true;
			check = motorStopped;
			text = waitCommand.Substring("WAITH".Length);
		}
		else
		{
			text = waitCommand.Substring("WAIT".Length);
		}
		if ("X".Equals(text.ToUpper()))
		{
			Axis[] array = new Axis[1];
			axis = array;
		}
		else if ("Y".Equals(text.ToUpper()))
		{
			axis = new Axis[1] { Axis.Y };
		}
		else if ("XY".Equals(text.ToUpper()))
		{
			axis = new Axis[2]
			{
				Axis.Y,
				Axis.X
			};
		}
		else
		{
			axis = null;
		}
	}

	private string SendCommand(string command)
	{
		if (command.IndexOf("WAIT", StringComparison.InvariantCultureIgnoreCase) == -1)
		{
			return SendCommandInner(command);
		}
		string[] array = command.Split(' ');
		int? timeOut = null;
		if (array.Length > 1)
		{
			int result = 0;
			if (int.TryParse(array[1], out result))
			{
				timeOut = result;
			}
		}
		MotorStopped check = null;
		DecodeWait(array[0], out var _, out var axis, out check);
		if (axis == null)
		{
			LogHelper.Instance.Log($"DecodeWait {array[0]} didn't decode an axis", LogEntryType.Error);
			return "INVALID MOTOR STATE";
		}
		string text = "TIMEOUT";
		Axis[] array2 = axis;
		foreach (Axis axis2 in array2)
		{
			text = WaitMotor(axis2, timeOut, check);
			if (IsResponseAnError(text))
			{
				return text;
			}
		}
		return text;
	}

	private string SendCommandInner(string command)
	{
		bool flag = LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug) || DebugEnabled;
		StringBuilder stringBuilder = (flag ? new StringBuilder() : null);
		if (flag)
		{
			stringBuilder.AppendFormat("ArcusMotionController: Instruction={0}", command);
		}
		using IChannelResponse channelResponse = Port.SendRecv(command, ControllerConfiguration.Instance.MotionControllerTimeout);
		switch (Mode)
		{
		case PortModes.Callback:
			if (-1 == channelResponse.GetIndex(4))
			{
				LogHelper.Instance.Log("ArcusMotionControl.SendCommand(): response was incomplete.");
				return "COMMUNICATION ERROR";
			}
			break;
		case PortModes.Async:
			if (!channelResponse.CommOk)
			{
				LogHelper.Instance.Log("ArcusMotionControl.SendCommand(): channel error returned {0}", channelResponse.Error);
				return "COMMUNICATION ERROR";
			}
			break;
		}
		int index = channelResponse.GetIndex(13);
		int count = ((index == -1) ? (channelResponse.RawResponse.Length - 1) : index);
		string text = Encoding.ASCII.GetString(channelResponse.RawResponse, 0, count);
		if (flag)
		{
			stringBuilder.AppendFormat(", ChannelError = {0}, Response={1}", channelResponse.Error, text);
			WriteToLog(stringBuilder.ToString());
		}
		return text;
	}

	private bool GetArcusPosition(string instruction, out int position)
	{
		if ("EY".Equals(instruction, StringComparison.CurrentCultureIgnoreCase))
		{
			instruction = "PY";
		}
		position = 0;
		string text = OnSendCommands(new string[1] { instruction })[0];
		if (IsResponseAnError(text))
		{
			return false;
		}
		string s = text.Substring(text.IndexOf('=') + 1);
		return int.TryParse(s, out position);
	}

	private MotionResult DecodeMotionResponse(string[] motionResult, int expectedCount)
	{
		if (motionResult.Length == 1)
		{
			if (motionResult[0].StartsWith("COMMUNICATION ERROR"))
			{
				return MotionResult.ArcusNotResponsive;
			}
			if (motionResult[0].StartsWith("DOOR OPEN"))
			{
				return MotionResult.DoorOpen;
			}
		}
		if (motionResult.Length != expectedCount)
		{
			return MotionResult.Timeout;
		}
		return MotionResult.Success;
	}

	private bool DefaultMotorRunningCheck(int status)
	{
		return status != 0;
	}

	private string[] ArcusGetIvars()
	{
		List<string> list = new List<string>();
		for (int num = 48; num >= 1; num--)
		{
			list.Add($"I{num}");
		}
		return OnSendCommands(list.ToArray());
	}

	private MotionResult MoveAbsoluteInternal(Axis axis, int encoderUnits, int? secondEncoderUnits)
	{
		List<string> list = new List<string>();
		string item = string.Empty;
		string empty = string.Empty;
		List<string> list2 = null;
		switch (axis)
		{
		case Axis.X:
		{
			empty = $"X{encoderUnits}";
			list.Add(empty);
			item = m_waitXCommand;
			List<string> list4 = new List<string>();
			list4.Add(m_lspdCommand);
			list4.Add(m_correctiveToleranceCommand);
			list4.Add(empty);
			list4.Add(item);
			list4.Add("I7=10");
			list2 = list4;
			break;
		}
		case Axis.Y:
			empty = $"Y{encoderUnits}";
			list.Add(empty);
			item = m_waitYCommand;
			break;
		case Axis.XY:
		{
			list.Add($"X{encoderUnits}");
			list.Add($"Y{secondEncoderUnits.Value}");
			item = m_waitXYCommand;
			List<string> list3 = new List<string>();
			list3.Add(m_lspdCommand);
			list3.Add(m_correctiveToleranceCommand);
			list3.Add($"Y{secondEncoderUnits.Value}X{encoderUnits}");
			list3.Add(item);
			list3.Add("I7=10");
			list2 = list3;
			break;
		}
		}
		List<string> list5 = new List<string>();
		list5.AddRange(m_movePreamble);
		foreach (string item2 in list)
		{
			list5.Add(item2);
		}
		list5.Add(item);
		int num = list5.Count - 1;
		if (list2 != null)
		{
			list5.AddRange(list2);
		}
		string[] array = OnSendCommands(list5.ToArray());
		MotionResult status = MotionResult.Success;
		if (IsMotorErrored(Axis.XY, out status))
		{
			return status;
		}
		MotionResult motionResult = DecodeMotionResponse(array, list5.Count);
		if (motionResult != MotionResult.Success)
		{
			return motionResult;
		}
		string text = ((list2 != null && array.Length == list5.Count) ? array[list5.Count - 2] : "OK");
		if (ControllerConfiguration.Instance.PrintEncoderPositionAfterMove2)
		{
			if (GetArcusPosition("EX", out var position) && GetArcusPosition("PY", out var position2))
			{
				LogHelper.Instance.Log($"MoveAbsoluteInternal: encoder positions (x,y) = {position}, {position2}.", LogEntryType.Info);
			}
			else
			{
				LogHelper.Instance.Log("Unable to determine encoder position from ARCUS.", LogEntryType.Info);
			}
		}
		if (!(array[num] == "TIMEOUT") && !(text == "TIMEOUT"))
		{
			return MotionResult.Success;
		}
		return MotionResult.Timeout;
	}

	private bool ClearYMotorError()
	{
		if (!IsMotorErrored(Axis.Y, out var status))
		{
			return true;
		}
		HomeAxis(Axis.Y);
		return !IsMotorErrored(Axis.Y, out status);
	}

	private bool IsResponseAnError(string response)
	{
		if (!("TIMEOUT" == response) && !("COMMUNICATION ERROR" == response))
		{
			return "DOOR OPEN" == response;
		}
		return true;
	}

	private bool ReadLimitsRaw(out bool upper, out bool lower)
	{
		string[] commands = new string[1] { "???" };
		MotionResult motionResult = MotionResult.InvalidResponse;
		upper = (lower = true);
		int num = 1;
		string[] array = OnSendCommands(commands);
		int num2 = 28;
		if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
		{
			WriteToLog(string.Format("Query response: {0}", array));
		}
		if (DecodeMotionResponse(array, num) != MotionResult.Success)
		{
			return false;
		}
		if (array.Length != num)
		{
			return false;
		}
		string[] array2 = array[0].Trim().Split(',');
		if (array2.Length < num2)
		{
			WriteToLog("ERROR: the length of the status query is less than 19.");
			return false;
		}
		string text = array2[13].TrimEnd('.');
		if (!int.TryParse(text, out var result))
		{
			LogHelper.Instance.Log($"The enable output status entry returned an unexpected value {text}", LogEntryType.Error);
			return false;
		}
		upper = (result & 0x10) != 0;
		lower = (result & 0x20) != 0;
		return true;
	}

	private ArcusMotionController()
	{
		IPersistentMap dataStore = StoreManager.Instance.GetDataStore();
		ControllerConfiguration.Instance.ConfigurationChangeEnd += ComputeMoveCommands;
		ComputeMoveCommands();
		DebugEnabled = dataStore.GetValue("ArcusDebug", def: false);
		RedboxSerialPortConfiguration conf = new RedboxSerialPortConfiguration(ControllerConfiguration.Instance.MotionControllerPortName)
		{
			WriteTerminator = RedboxSerialPort.WriteCharsCRNull,
			WritePause = ControllerConfiguration.Instance.ArcusWritePause,
			WriteTimeout = ControllerConfiguration.Instance.MotionControllerTimeout,
			Test = (IChannelResponse response) => response.GetIndex(4) != -1
		};
		Mode = dataStore.GetValue("ArcusPortMode", PortModes.Async);
		Port = RedboxSerialPort.Create(conf, Mode, "Motion Control");
		Port.EnableDebugging = DebugEnabled;
		m_arcusDescriptor = new DeviceDescriptor("1589", "a001", "Proteus XES");
	}
}
