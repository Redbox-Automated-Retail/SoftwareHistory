using System;
using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal sealed class ArcusMotionController : MotionController
{
	private const string HaltMotorCommand = "RSTOP";

	private const string NopCommand = "NOP";

	private readonly IDeviceDescriptor ArcusDescriptor = new ArcusDeviceDescriptor();

	private readonly bool DebugEnabled;

	private readonly PortModes Mode;

	private readonly List<string> m_movePreamble = new List<string>();

	private readonly string[] CommOkCommand = new string[1] { "$" };

	private readonly string[] InvalidQueryResponse = new string[1] { "INVALID QUERY RESPONSE" };

	private RedboxSerialPort Port;

	private string m_waitXCommand;

	private string m_waitYCommand;

	private string m_waitXYCommand;

	private string m_lspdCommand;

	private string[] m_initCommand;

	private string m_correctiveToleranceCommand;

	internal bool XAxisHomed { get; private set; }

	internal bool YAxisHomed { get; private set; }

	protected override void OnConfigurationChangeEnd()
	{
		LogHelper.Instance.Log("[ArcusMotionControl] On configuration end.");
		ComputeMoveCommands();
	}

	protected override void OnConfigurationChangeStart()
	{
	}

	protected override void OnConfigurationLoad()
	{
		LogHelper.Instance.Log("[ArcusMotionControl] Notify configuration loaded.");
		ComputeMoveCommands();
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

	protected override ErrorCodes MoveToTargetInner(ref MoveTarget target)
	{
		if (!ClearYMotorError())
		{
			return ErrorCodes.MotorError;
		}
		return MoveAbsoluteInternal(ref target);
	}

	protected override ErrorCodes HomeAxisInner(Axis axis)
	{
		if (axis != Axis.X)
		{
			return HomeYAxis();
		}
		return HomeXAxis();
	}

	protected override ErrorCodes MoveToVendInner(MoveMode mode)
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
			IControllerPosition controllerPosition = ReadPositions();
			if (controllerPosition.ReadOk && controllerPosition.YCoordinate.Value == num)
			{
				if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
				{
					LogHelper.Instance.Log($"Picker currently at position Y = {controllerPosition.YCoordinate.Value}", LogEntryType.Info);
				}
				return ErrorCodes.Success;
			}
		}
		if (!ClearYMotorError())
		{
			return ErrorCodes.MotorError;
		}
		MoveTarget target = new MoveTarget
		{
			Axis = Axis.Y,
			YCoordinate = num
		};
		return MoveAbsoluteInternal(ref target);
	}

	internal override bool CommunicationOk()
	{
		List<ArcusCommandResponse> list = OnSendCommands(CommOkCommand);
		using (new DisposeableList<ArcusCommandResponse>(list))
		{
			if (list.Count != 1)
			{
				return false;
			}
			ArcusCommandResponse arcusCommandResponse = list[0];
			if (arcusCommandResponse.Error != ErrorCodes.Success)
			{
				LogHelper.Instance.Log("[ArcusMotionController] CommOk returned error {0}", arcusCommandResponse.Error.ToString());
				return false;
			}
			LogHelper.Instance.Log(LogEntryType.Debug, "Arcus comm ok: command returns {0}", arcusCommandResponse.Response);
			return arcusCommandResponse.Response.StartsWith("OK");
		}
	}

	internal override IControllerPosition ReadPositions()
	{
		ArcusControllerPosition arcusControllerPosition = new ArcusControllerPosition();
		List<ArcusCommandResponse> list = OnSendCommands(new string[2] { "EX", "PY" });
		using (new DisposeableList<ArcusCommandResponse>(list))
		{
			if (list.Count != 2)
			{
				arcusControllerPosition.ReadOk = false;
				return arcusControllerPosition;
			}
			if (list[0].Error != ErrorCodes.Success || list[1].Error != ErrorCodes.Success)
			{
				arcusControllerPosition.ReadOk = false;
				return arcusControllerPosition;
			}
			arcusControllerPosition.ReadOk = true;
			string s = list[0].Response.Substring(list[0].Response.IndexOf('=') + 1);
			if (int.TryParse(s, out var result))
			{
				arcusControllerPosition.XCoordinate = result;
			}
			else
			{
				arcusControllerPosition.ReadOk = false;
			}
			string s2 = list[1].Response.Substring(list[1].Response.IndexOf('=') + 1);
			if (int.TryParse(s2, out result))
			{
				arcusControllerPosition.YCoordinate = result;
			}
			else
			{
				arcusControllerPosition.ReadOk = false;
			}
			return arcusControllerPosition;
		}
	}

	internal override bool ClosePort()
	{
		bool xAxisHomed = (YAxisHomed = false);
		XAxisHomed = xAxisHomed;
		return Port.Close();
	}

	internal override string[] ReadLimits()
	{
		string[] array = new string[1] { "???" };
		List<ArcusCommandResponse> list = OnSendCommands(array);
		using (new DisposeableList<ArcusCommandResponse>(list))
		{
			if (list.Count != array.Length)
			{
				WriteToLog("[ReadLimits] Query response returned length of {0}", list.Count);
				return InvalidQueryResponse;
			}
			ArcusCommandResponse arcusCommandResponse = list[0];
			if (arcusCommandResponse.Error != ErrorCodes.Success)
			{
				WriteToLog("[ReadLimits] Query response returned error {0}", arcusCommandResponse.Error.ToString());
				return InvalidQueryResponse;
			}
			string[] array2 = arcusCommandResponse.Response.Trim().Split(',');
			if (array2.Length != 28)
			{
				WriteToLog("[ReadLimits] ERROR: the length of the status query ( length = {0} ) is unexpected.", array2.Length);
				return InvalidQueryResponse;
			}
			string text = array2[13].TrimEnd('.');
			if (!int.TryParse(text, out var result))
			{
				WriteToLog("[ReadLimits] The enable output status entry returned an unexpected value {0}", text);
				return InvalidQueryResponse;
			}
			return new string[2]
			{
				string.Format("UPPER LIMIT: {0}", ((result & 0x10) != 0) ? "BLOCKED" : "CLEAR"),
				string.Format("LOWER LIMIT: {0}", ((result & 0x20) != 0) ? "BLOCKED" : "CLEAR")
			};
		}
	}

	internal override bool ResetDeviceDriver()
	{
		ClosePort();
		return ArcusDescriptor.ResetDriver();
	}

	internal override bool InitializeCommunication()
	{
		OnSendCommands(m_initCommand);
		return true;
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

	internal ArcusMotionController(MotionControlService controlService)
		: base(controlService)
	{
		IPersistentMapService service = ServiceLocator.Instance.GetService<IPersistentMapService>();
		IPersistentMap map = service.GetMap();
		DebugEnabled = map.GetValue("ArcusDebug", def: false);
		RedboxSerialPortConfiguration conf = new RedboxSerialPortConfiguration(ControllerConfiguration.Instance.MotionControllerPortName)
		{
			WriteTerminator = RedboxSerialPort.WriteCharsCRNull,
			WritePause = ControllerConfiguration.Instance.ArcusWritePause,
			WriteTimeout = ControllerConfiguration.Instance.MotionControllerTimeout,
			Test = (IChannelResponse response) => response.GetIndex(4) != -1
		};
		Mode = map.GetValue("ArcusPortMode", PortModes.Async);
		Port = RedboxSerialPort.Create(conf, Mode, "Motion Control");
		Port.EnableDebugging = DebugEnabled;
	}

	private ArcusCommandResponse SendPreamble(IEnumerable<string> commands)
	{
		return new ArcusCommandResponse(string.Empty, ErrorCodes.CommunicationError);
	}

	private ErrorCodes CheckPort()
	{
		if (Port.Open())
		{
			return ErrorCodes.Success;
		}
		LogHelper.Instance.Log(LogEntryType.Error, "[ArcusMotionControl] Unable to open port {0}.", Port.Name);
		Port.Close();
		return ErrorCodes.PortOpenFailure;
	}

	private ArcusCommandResponse WaitMotor(Axis axis, ref ArcusMotorStatus motorStatus)
	{
		TimeSpan timespan = new TimeSpan(0, 0, 0, 0, ControllerConfiguration.Instance.ArcusMotorQueryPause);
		using ExecutionTimer executionTimer = new ExecutionTimer();
		string command = "MST" + axis.ToString().ToUpper();
		ArcusCommandResponse arcusCommandResponse;
		do
		{
			ErrorCodes errorCodes = CanMove();
			if (errorCodes != ErrorCodes.Success)
			{
				WriteToLog($"Door sensor query returned {errorCodes.ToString()}: halting motion.");
				SendCommandInner("RSTOP");
				return new ArcusCommandResponse(string.Empty, errorCodes);
			}
			arcusCommandResponse = SendCommandInner(command);
			if (arcusCommandResponse.Error != ErrorCodes.Success)
			{
				return arcusCommandResponse;
			}
			motorStatus.TestResponse(arcusCommandResponse);
			if (!motorStatus.MotorRunning)
			{
				return new ArcusCommandResponse(arcusCommandResponse.Response, motorStatus.Error);
			}
			PerfFunctions.SpinWait(timespan);
		}
		while (executionTimer.ElapsedMilliseconds < motorStatus.Timeout);
		string text = $"Unable to reach target on axis {axis.ToString().ToUpper()}; reported error: {motorStatus.Diagnostic}";
		LogHelper.Instance.Log(text, LogEntryType.Error);
		WriteToLog(text);
		if (motorStatus.ArcusStatusCode > 7)
		{
			string text2 = $"The {axis.ToString()} axis motor returned an error code: {motorStatus.ArcusStatusCode} - {motorStatus.Diagnostic}";
			LogHelper.Instance.Log(text2, LogEntryType.Info);
			WriteToLog(text2);
		}
		SendCommandInner("RSTOP");
		return new ArcusCommandResponse(arcusCommandResponse.Response, ErrorCodes.Timeout);
	}

	private ArcusCommandResponse WaitMotor(Axis axis, int? timeOut, Predicate<int> motorRunning)
	{
		TimeSpan timespan = new TimeSpan(0, 0, 0, 0, ControllerConfiguration.Instance.ArcusMotorQueryPause);
		using ExecutionTimer executionTimer = new ExecutionTimer();
		string command = "MST" + axis.ToString().ToUpper();
		ArcusCommandResponse arcusCommandResponse;
		int result;
		do
		{
			ErrorCodes errorCodes = CanMove();
			if (errorCodes != ErrorCodes.Success)
			{
				WriteToLog($"Door sensor query returned {errorCodes.ToString()}: halting motion.");
				SendCommandInner("RSTOP");
				return new ArcusCommandResponse(string.Empty, errorCodes);
			}
			arcusCommandResponse = SendCommandInner(command);
			if (arcusCommandResponse.Error != ErrorCodes.Success)
			{
				return arcusCommandResponse;
			}
			if (!int.TryParse(arcusCommandResponse.Response, out result))
			{
				return new ArcusCommandResponse(arcusCommandResponse.Response, ErrorCodes.MotorError);
			}
			if (!motorRunning(result))
			{
				return arcusCommandResponse;
			}
			PerfFunctions.SpinWait(timespan);
		}
		while (!timeOut.HasValue || !(executionTimer.ElapsedMilliseconds >= timeOut));
		string text = $"Unable to reach target on axis {axis.ToString().ToUpper()}; reported error: {ConvertMSTCodeToText(result)}";
		LogHelper.Instance.Log(text, LogEntryType.Error);
		WriteToLog(text);
		if (result > 7)
		{
			string text2 = $"The {axis.ToString()} axis motor returned an error code: {result} - {ConvertMSTCodeToText(result)}";
			LogHelper.Instance.Log(text2, LogEntryType.Info);
			WriteToLog(text2);
		}
		SendCommandInner("RSTOP");
		return new ArcusCommandResponse(arcusCommandResponse.Response, ErrorCodes.Timeout);
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

	private List<ArcusCommandResponse> OnSendCommands(IEnumerable<string> commands)
	{
		List<ArcusCommandResponse> list = new List<ArcusCommandResponse>();
		if (CheckPort() != ErrorCodes.Success)
		{
			list.Add(new ArcusCommandResponse("NOP", ErrorCodes.PortOpenFailure));
			return list;
		}
		bool flag = false;
		foreach (string command in commands)
		{
			if (flag)
			{
				break;
			}
			if (string.IsNullOrEmpty(command) || string.Compare(command, "NOP", ignoreCase: true) == 0)
			{
				LogHelper.Instance.Log("ArcusMotionController: Instruction=NOP", LogEntryType.Debug);
				list.Add(new ArcusCommandResponse("NOP", ErrorCodes.Success));
				continue;
			}
			if (command.IndexOf("WAIT", StringComparison.InvariantCultureIgnoreCase) == -1)
			{
				ArcusCommandResponse arcusCommandResponse = SendCommandInner(command);
				if (arcusCommandResponse.Error != ErrorCodes.Success)
				{
					flag = true;
					list.Clear();
				}
				list.Add(arcusCommandResponse);
				continue;
			}
			string[] array = command.Split(' ');
			int num = 0;
			if (array.Length > 1)
			{
				int result = 0;
				if (int.TryParse(array[1], out result))
				{
					num = result;
				}
			}
			if (ControllerConfiguration.Instance.LowerLimitAsError)
			{
				ArcusMotorStatus motorStatus = new ArcusMotorStatus(array[0], num);
				Axis[] axes = motorStatus.Axes;
				foreach (Axis axis in axes)
				{
					ArcusCommandResponse arcusCommandResponse2 = WaitMotor(axis, ref motorStatus);
					if (arcusCommandResponse2.Error != ErrorCodes.Success)
					{
						flag = true;
						list.Clear();
					}
					list.Add(arcusCommandResponse2);
					if (flag)
					{
						break;
					}
				}
				continue;
			}
			Predicate<int> check = null;
			DecodeWait(array[0], out var axis2, out check);
			Axis[] array2 = axis2;
			foreach (Axis axis3 in array2)
			{
				ArcusCommandResponse arcusCommandResponse3 = WaitMotor(axis3, num, check);
				if (arcusCommandResponse3.Error != ErrorCodes.Success)
				{
					flag = true;
					list.Clear();
				}
				list.Add(arcusCommandResponse3);
				if (flag)
				{
					break;
				}
			}
		}
		Port.ResetBuffers();
		return list;
	}

	private ErrorCodes HomeXAxis()
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
		int index = 13;
		List<ArcusCommandResponse> list = OnSendCommands(array);
		using (new DisposeableList<ArcusCommandResponse>(list))
		{
			if (list.Count != array.Length)
			{
				return (list.Count == 1) ? list[0].Error : ErrorCodes.ArcusNotResponsive;
			}
			if (list[index].Error != ErrorCodes.Success)
			{
				return list[index].Error;
			}
			XAxisHomed = true;
			return ErrorCodes.Success;
		}
	}

	private ErrorCodes HomeYAxis()
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
		int index = list.Count - 1;
		list.AddRange(new string[2] { "HOMEY+", "WAITHY 60000" });
		int index2 = list.Count - 1;
		List<ArcusCommandResponse> list2 = OnSendCommands(list.ToArray());
		using (new DisposeableList<ArcusCommandResponse>(list2))
		{
			if (list2.Count != list.Count)
			{
				return (list2.Count == 1) ? list2[0].Error : ErrorCodes.ArcusNotResponsive;
			}
			if (ControllerConfiguration.Instance.HomeYDropBack != 0 && list2[index].Error != ErrorCodes.Success)
			{
				return list2[index].Error;
			}
			if (list2[index2].Error != ErrorCodes.Success)
			{
				return list2[index2].Error;
			}
			YAxisHomed = true;
			return ErrorCodes.Success;
		}
	}

	private void DecodeWait(string waitCommand, out Axis[] axis, out Predicate<int> check)
	{
		Predicate<int> predicate = (int status) => status != 0;
		Predicate<int> predicate2 = (int status) => status > 0;
		Predicate<int> predicate3 = (int status) => status > 0 && status < 15;
		IDoorSensorService service = ServiceLocator.Instance.GetService<IDoorSensorService>();
		if (service.SensorsEnabled)
		{
			predicate = DefaultMotorRunningCheck;
			predicate2 = DefaultMotorRunningCheck;
		}
		check = predicate2;
		string text = null;
		if (waitCommand.Equals("WAITYDB", StringComparison.CurrentCultureIgnoreCase))
		{
			check = predicate3;
			axis = new Axis[1] { Axis.Y };
			return;
		}
		if (waitCommand.StartsWith("WAITH"))
		{
			check = predicate;
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

	private ArcusCommandResponse SendCommandInner(string command)
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
				WriteToLog("[ArcusMotionController] Send command {0}; error = {1}", command, ErrorCodes.ArcusNotResponsive.ToString());
				return new ArcusCommandResponse(string.Empty, ErrorCodes.ArcusNotResponsive);
			}
			break;
		case PortModes.Async:
			if (!channelResponse.CommOk)
			{
				LogHelper.Instance.Log("ArcusMotionControl.SendCommand(): channel error returned {0}", channelResponse.Error);
				WriteToLog("[ArcusMotionController] Send command {0}; error = {1}", command, ErrorCodes.ArcusNotResponsive.ToString());
				return new ArcusCommandResponse(string.Empty, ErrorCodes.ArcusNotResponsive);
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
		return new ArcusCommandResponse(text, ErrorCodes.Success);
	}

	private bool DefaultMotorRunningCheck(int status)
	{
		return status != 0;
	}

	private ErrorCodes MoveAbsoluteInternal(ref MoveTarget target)
	{
		List<string> list = new List<string>();
		string item = string.Empty;
		string empty = string.Empty;
		List<string> list2 = new List<string>();
		switch (target.Axis)
		{
		case Axis.X:
		{
			empty = $"X{target.XCoordinate.Value}";
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
			empty = $"Y{target.YCoordinate.Value}";
			list.Add(empty);
			item = m_waitYCommand;
			break;
		case Axis.XY:
		{
			list.Add($"X{target.XCoordinate.Value}");
			list.Add($"Y{target.YCoordinate.Value}");
			item = m_waitXYCommand;
			List<string> list3 = new List<string>();
			list3.Add(m_lspdCommand);
			list3.Add(m_correctiveToleranceCommand);
			list3.Add($"Y{target.YCoordinate.Value}X{target.XCoordinate.Value}");
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
		int index = list5.Count - 1;
		list5.AddRange(list2);
		List<ArcusCommandResponse> list6 = OnSendCommands(list5.ToArray());
		using (new DisposeableList<ArcusCommandResponse>(list6))
		{
			ErrorCodes status = ErrorCodes.Success;
			if (IsMotorErrored(Axis.XY, out status))
			{
				return status;
			}
			if (list6.Count != list5.Count)
			{
				return list6[0].Error;
			}
			ErrorCodes errorCodes = ((list2 != null) ? list6[list5.Count - 2].Error : ErrorCodes.Success);
			if (ControllerConfiguration.Instance.PrintEncoderPositionAfterMove2)
			{
				IControllerPosition controllerPosition = ReadPositions();
				if (controllerPosition.ReadOk)
				{
					LogHelper.Instance.Log("MoveAbsoluteInternal: encoder positions (x,y) = {0}, {1}.", controllerPosition.XCoordinate.Value, controllerPosition.YCoordinate.Value);
				}
				else
				{
					LogHelper.Instance.Log("Unable to determine encoder position from ARCUS.", LogEntryType.Info);
				}
			}
			if (errorCodes != ErrorCodes.Success)
			{
				return errorCodes;
			}
			return list6[index].Error;
		}
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

	private bool IsMotorErrored(Axis axis, out ErrorCodes status)
	{
		status = ErrorCodes.Success;
		if (Axis.XY == axis || Axis.Y == axis)
		{
			string s = OnSendCommands(new string[2] { "MIOY", "MECLEARY" })[0].Response.Trim();
			if (int.TryParse(s, out var result))
			{
				status = ErrorCodes.MotorError;
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
}
