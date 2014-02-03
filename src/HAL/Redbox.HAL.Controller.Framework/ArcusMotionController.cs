using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class ArcusMotionController : MotionController
{
	internal delegate bool MotorStopped(int status);

	private const int ExpectedArcusAllQueryLength = 1;

	private const string HaltMotorCommand = "RSTOP";

	private bool? m_debugEnabled = null;

	private object m_lock = new object();

	private List<string> m_movePreamble;

	private string m_waitXCommand;

	private string m_waitYCommand;

	private string m_waitXYCommand;

	private string m_lspdCommand;

	private string[] m_initCommand;

	private string m_correctiveToleranceCommand;

	private DeviceDescriptor m_arcusDescriptor;

	private static ArcusMotionController m_instance;

	internal bool XAxisHomed { get; private set; }

	internal bool YAxisHomed { get; private set; }

	private bool DebugEnabled
	{
		get
		{
			if (m_debugEnabled.HasValue)
			{
				return m_debugEnabled.Value;
			}
			m_debugEnabled = RegistryOptions.Instance.GetSecretValue<bool?>("SOFTWARE\\Redbox\\HAL", "ArcusDebug") ?? false;
			return m_debugEnabled.Value;
		}
	}

	private MotionControlChannel Channel { get; set; }

	public override bool CheckAndCorrect(IFormattedLog log)
	{
		if (!ReadLimitsRaw(out var _, out var lower))
		{
			return false;
		}
		if (!lower)
		{
			return true;
		}
		log.WriteFormatted("Lower limit shows blocked; execute init.");
		InitializeCommunication();
		using HomeAxesResult homeAxesResult = InitAxes(log);
		return homeAxesResult.Succeded;
	}

	public override bool Restart(IFormattedLog appLog)
	{
		int millisecondsTimeout = 3000;
		Thread.Sleep(millisecondsTimeout);
		InitializeCommunication();
		using HomeAxesResult homeAxesResult = InitAxes(appLog);
		if (homeAxesResult.Succeded)
		{
			appLog.WriteFormatted("RestartMotionControl: SUCCESS");
			return true;
		}
		return false;
	}

	public override bool ResetDeviceDriver()
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
			else if (ControllerConfiguration.Instance.CountHardwareErrors)
			{
				CounterManager.Instance.Increment("MCCorrectedCommErrors");
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unable to reset Proteus driver.", e);
			if (ControllerConfiguration.Instance.CountHardwareErrors)
			{
				CounterManager.Instance.Increment("MCUnCorrectedCommErrors");
			}
			flag = false;
		}
		return flag;
	}

	public override bool CommunicationOk()
	{
		string[] array = OnSendCommands(new string[1] { "$" });
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

	protected override void InitializeCommandInner()
	{
		OnSendCommands(m_initCommand);
	}

	protected override HomeAxesResult InitAxesInner(IFormattedLog appLog)
	{
		HomeAxesResult homeAxesResult = new HomeAxesResult();
		appLog.WriteFormatted("Home the X motor.");
		Gripper.Instance.ClearFront();
		for (int i = 0; i < 2; i++)
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
		return Channel.ClosePort();
	}

	protected override bool IsInitializedInner()
	{
		if (ControllerMode.Emulated == ControllerConfiguration.Instance.Mode || !ControllerConfiguration.Instance.ValidateControllerHomeStatus)
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
		LogHelper.Instance.Log($"Home axis {axis.ToString()}", LogEntryType.Info);
		if (axis != Axis.X)
		{
			return HomeYAxis();
		}
		return HomeXAxis();
	}

	protected override bool GetCurrentLocationInner(Axis axis, ref MoveTarget target)
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

	protected override MotionResult MoveToVendInner(MoveMode mode)
	{
		int num = ControllerConfiguration.Instance.VendYPosition;
		if (ControllerConfiguration.Instance.VendPositionReceiveOffset.HasValue)
		{
			switch (mode)
			{
			case MoveMode.Get:
				num -= ControllerConfiguration.Instance.VendPositionReceiveOffset.Value;
				break;
			case MoveMode.Put:
				num += ControllerConfiguration.Instance.VendPositionReceiveOffset.Value;
				break;
			}
		}
		if (ControllerConfiguration.Instance.QueryPositionForVendMove)
		{
			MoveTarget target = default(MoveTarget);
			if (GetCurrentLocationInner(Axis.Y, ref target) && target.YCoordinate.Value == num)
			{
				if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
				{
					LogHelper.Instance.Log($"Picker currently at position Y = {target.YCoordinate.Value}", LogEntryType.Info);
				}
				ControllerService.Instance.IsAtVendLocation = true;
				return MotionResult.Success;
			}
		}
		if (!ClearYMotorError())
		{
			return MotionResult.Error;
		}
		MotionResult motionResult = MoveAbsoluteInternal(Axis.Y, num, null);
		ControllerService.Instance.IsAtVendLocation = motionResult == MotionResult.Success;
		return motionResult;
	}

	protected override MotionResult ControllerErrorStatusInner(Axis axis)
	{
		if (!DoorSensors.Instance.SensorsEnabled)
		{
			return MotionResult.Success;
		}
		int num = 1;
		string[] commands = new string[1] { "???" };
		MotionResult motionResult = MotionResult.InvalidResponse;
		string[] array = OnSendCommands(commands);
		int num2 = 28;
		if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
		{
			WriteToLog(string.Format("Query response: {0}", array));
		}
		motionResult = DecodeMotionResponse(array, num);
		if (motionResult != MotionResult.Success)
		{
			return motionResult;
		}
		if (array.Length != num)
		{
			return MotionResult.InvalidResponse;
		}
		string[] array2 = array[0].Trim().Split(',');
		if (array2.Length < num2)
		{
			LogHelper.Instance.Log("the length of the ??? query is less than 19.", LogEntryType.Error);
			return MotionResult.InvalidResponse;
		}
		string s = array2[num2 - 1].TrimEnd('.');
		if (!int.TryParse(s, out var result))
		{
			LogHelper.Instance.Log($"The enable output status entry returned an unexpected value {array2[18]}", LogEntryType.Error);
			return MotionResult.InvalidResponse;
		}
		if (result > 3)
		{
			LogHelper.Instance.Log($"Enable output status returned {result} (unexpected value)", LogEntryType.Error);
			return MotionResult.InvalidResponse;
		}
		MotionResult motionResult2 = MotionResult.InvalidResponse;
		switch (result)
		{
		case 2:
			motionResult2 = MotionResult.FrontDoor;
			break;
		case 3:
			motionResult2 = MotionResult.Success;
			break;
		}
		if (motionResult2 != MotionResult.Success)
		{
			LogHelper.Instance.Log($"ControllerErrorStatus returned an error status {motionResult2.ToString()}", LogEntryType.Info);
		}
		return motionResult2;
	}

	protected override string[] ReadLimitsInner()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return new string[2] { "UPPER LIMIT: CLEAR", "LOWER LIMIT: BLOCKED" };
		}
		if (!ReadLimitsRaw(out var upper, out var lower))
		{
			return new string[1] { "INVALID QUERY RESPONSE" };
		}
		LogHelper.Instance.Log($"Upper = {upper}, lower = {lower}", LogEntryType.Info);
		return new string[2]
		{
			string.Format("UPPER LIMIT: {0}", upper ? "BLOCKED" : "CLEAR"),
			string.Format("LOWER LIMIT: {0}", lower ? "BLOCKED" : "CLEAR")
		};
	}

	protected override bool IsMotorErroredInner(Axis axis, out MotionResult status)
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

	internal static ArcusMotionController MakeInstance()
	{
		if (m_instance == null)
		{
			m_instance = new ArcusMotionController();
		}
		return m_instance;
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
		m_movePreamble = null;
		m_movePreamble = new List<string>
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
		};
		m_waitXCommand = $"WAITX {ControllerConfiguration.Instance.MoveTimeout}";
		m_waitYCommand = $"WAITY {ControllerConfiguration.Instance.MoveTimeout}";
		m_waitXYCommand = $"WAITXY {ControllerConfiguration.Instance.MoveTimeout}";
		m_lspdCommand = $"LSPD {(int)ControllerConfiguration.Instance.GearX.GetStepRatio() * 100}";
	}

	internal string WaitMotor(Axis axis, int? timeOut, MotorStopped motorRunning)
	{
		TimeSpan timespan = new TimeSpan(0, 0, 0, 0, ControllerConfiguration.Instance.ArcusReadWait);
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
			if (!Channel.Open())
			{
				LogHelper.Instance.Log($"MotionControl: unable to open port {Channel.Name}.", LogEntryType.Error);
				Channel.ClosePort();
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
			Channel.ResetBuffers();
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
		string text = "HOMEX-";
		if (ControllerConfiguration.Instance.XHomeMethod == XHomeMethod.Encoder)
		{
			text = "ZHOMEX-";
		}
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
			text,
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
		if (ControllerConfiguration.Instance.HomeYDropBack.HasValue)
		{
			int? num = ControllerConfiguration.Instance.HomeYDropBack;
			if (Math.Sign(num.Value) == 1)
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
		if (ControllerConfiguration.Instance.HomeYDropBack.HasValue && array[num2] == "TIMEOUT")
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
		return Channel.SendCommand(command);
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
		if (int.TryParse(s, out position))
		{
			return true;
		}
		return false;
	}

	private bool SetArcusPosition(string instruction, int position)
	{
		string text = instruction + "=" + position;
		OnSendCommands(new string[1] { text });
		if (GetArcusPosition(instruction, out var position2) && position2 == position)
		{
			return true;
		}
		return false;
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
		if ("TIMEOUT" == response || "COMMUNICATION ERROR" == response || "DOOR OPEN" == response)
		{
			return true;
		}
		return false;
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
		ControllerConfiguration.Instance.ConfigurationChangeEnd += ComputeMoveCommands;
		ComputeMoveCommands();
		Channel = new MotionControlChannel(this, DebugEnabled);
		m_arcusDescriptor = new DeviceDescriptor("1589", "a001", "Proteus XES");
	}
}
