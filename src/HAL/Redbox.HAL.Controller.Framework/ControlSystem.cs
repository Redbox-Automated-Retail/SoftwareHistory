using System;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal sealed class ControlSystem : IControlSystem, ICoreCommandExecutor
{
	private const int OpTries = 2;

	internal readonly ControlBoards[] ControllerBoards = new ControlBoards[3]
	{
		ControlBoards.Serial,
		ControlBoards.Picker,
		ControlBoards.Aux
	};

	private readonly RedboxSerialPort Port;

	private readonly ITrack Track;

	private readonly IVendDoor VendDoor;

	private readonly bool UseAltRinglightCommand;

	public TrackState TrackState => Track.TrackState;

	public VendDoorState VendDoorState { get; private set; }

	public IControlResponse StopRoller()
	{
		return ExecuteControllerCommand(CommandType.RollerStop);
	}

	public IControlResponse StartRollerIn()
	{
		return ExecuteControllerCommand(CommandType.RollerIn);
	}

	public IControlResponse StartRollerOut()
	{
		return ExecuteControllerCommand(CommandType.RollerOut);
	}

	public IControlResponse RollerToPosition(RollerPosition position, int opTimeout, bool logSensors)
	{
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
		CoreResponse coreResponse = ExecuteControllerCommand(type, opTimeout);
		if (coreResponse.Error != ErrorCodes.Success && logSensors)
		{
			LogHelper.Instance.Log(LogEntryType.Error, "Roller to position {0} timed out.", position.ToString());
			LogPickerSensorState(LogEntryType.Error);
		}
		return coreResponse;
	}

	public IControlResponse RollerToPosition(RollerPosition position, int opTimeout)
	{
		return RollerToPosition(position, opTimeout, logSensors: true);
	}

	public IPickerSensorReadResult ReadSensors()
	{
		if (TrackState.Closed != Track.TrackState)
		{
			IControlResponse controlResponse = TrackClose();
			if (!controlResponse.Success)
			{
				LogHelper.Instance.Log(LogEntryType.Error, "[ControlSystem] Track close reported an error {0} during picker sensor read.", controlResponse.ToString());
				return new PickerSensorReadResult(ErrorCodes.TrackCloseTimeout);
			}
		}
		return ReadPickerSensors();
	}

	public IPickerSensorReadResult ReadImmediateSensors()
	{
		return ReadPickerSensors();
	}

	public IControlResponse SetSensors(bool on)
	{
		return ExecuteControllerCommand(on ? CommandType.SensorBarOn : CommandType.SensorBarOff);
	}

	public ErrorCodes Center(CenterDiskMethod method)
	{
		ErrorCodes result = ErrorCodes.Success;
		if (method == CenterDiskMethod.None)
		{
			return result;
		}
		int milliseconds = 250;
		int opTimeout = 8000;
		RollerPosition rollerPosition = ((CenterDiskMethod.DrumAndBack == method) ? RollerPosition.Position1 : RollerPosition.Position6);
		if (!RollerToPosition(rollerPosition, opTimeout).Success)
		{
			result = ((RollerPosition.Position1 == rollerPosition) ? ErrorCodes.RollerToPos1Timeout : ErrorCodes.RollerToPos6Timeout);
			string message = $"CenterItem: roll to position {rollerPosition.ToString()} timed out.";
			LogHelper.Instance.Log(message, LogEntryType.Error);
			LogPickerSensorState(LogEntryType.Error);
		}
		PerfFunctions.SpinWait(milliseconds);
		rollerPosition = ((CenterDiskMethod.DrumAndBack == method) ? RollerPosition.Position5 : RollerPosition.Position3);
		if (!RollerToPosition(rollerPosition, opTimeout).Success)
		{
			result = ((RollerPosition.Position5 == rollerPosition) ? ErrorCodes.RollerToPos5Timeout : ErrorCodes.RollerToPos3Timeout);
			string message2 = $"CenterItem: roll to position {rollerPosition.ToString()} timed out.";
			LogHelper.Instance.Log(message2, LogEntryType.Error);
			LogPickerSensorState(LogEntryType.Error);
		}
		PerfFunctions.SpinWait(milliseconds);
		TrackCycle();
		return result;
	}

	public void TimedExtend()
	{
		TimedExtend(ControllerConfiguration.Instance.PushTime);
	}

	public void TimedExtend(int timeout)
	{
		try
		{
			if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
			{
				LogHelper.Instance.Log($"ExtendGripperArmForTime {timeout} ms", LogEntryType.Debug);
			}
			ExecuteControllerCommand(CommandType.ExtendGripperArmForTime);
			PerfFunctions.Wait(timeout);
		}
		finally
		{
			ExecuteControllerCommand(CommandType.GripperExtendHalt);
		}
	}

	public IControlResponse ExtendArm()
	{
		int timeout = ControllerConfiguration.Instance.GripperArmExtendRetractTimeout;
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		IMotionControlService service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
		ILocation currentLocation = service2.CurrentLocation;
		bool flag = false;
		if (currentLocation != null)
		{
			IDeck byNumber = service.GetByNumber(currentLocation.Deck);
			flag = byNumber.IsQlm;
		}
		if (flag)
		{
			timeout = ControllerConfiguration.Instance.QlmExtendTime;
		}
		return ExtendArm(timeout);
	}

	public IControlResponse ExtendArm(int timeout)
	{
		CoreResponse coreResponse = ExecuteControllerCommand(CommandType.GripperExtend, timeout);
		if (coreResponse.Success)
		{
			return coreResponse;
		}
		if (coreResponse.CommError)
		{
			LogHelper.Instance.Log("Communication error during gripper extend.", LogEntryType.Error);
			return coreResponse;
		}
		string[] array = ReadInputs(ControlBoards.Picker);
		if (array.Length > 0 && array[0] == "1")
		{
			LogHelper.Instance.Log("GripperExtend status returned timeout; however, picker sensors read shows the forward sensor reached.", LogEntryType.Info);
			coreResponse.Error = ErrorCodes.Success;
			return coreResponse;
		}
		LogHelper.Instance.Log("SetGripperArm( Extend ) timed out.", LogEntryType.Error);
		LogInputs(ControlBoards.Picker, LogEntryType.Error);
		IPersistentCounterService service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		service.Increment(TimeoutCounters.GripperExtend);
		return coreResponse;
	}

	public IControlResponse RetractArm()
	{
		CoreResponse coreResponse = null;
		int gripperArmExtendRetractTimeout = ControllerConfiguration.Instance.GripperArmExtendRetractTimeout;
		for (int i = 0; i < 2; i++)
		{
			coreResponse = ExecuteControllerCommand(CommandType.GripperRetract, gripperArmExtendRetractTimeout);
			if (coreResponse.CommError)
			{
				LogHelper.Instance.Log("Communication error during gripper retract.", LogEntryType.Error);
				return coreResponse;
			}
			if (!coreResponse.TimedOut)
			{
				return coreResponse;
			}
		}
		string[] array = ReadInputs(ControlBoards.Picker);
		if (array.Length > 0 && array[1] == "1")
		{
			LogHelper.Instance.Log("GripperRetract status returned timeout; however, picker sensors read shows the sensor tripped.", LogEntryType.Info);
			return coreResponse;
		}
		LogHelper.Instance.Log($"SetGripperArm( RETRACT ) timed out after {2} retries.", LogEntryType.Error);
		LogInputs(ControlBoards.Picker, LogEntryType.Error);
		IPersistentCounterService service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		service.Increment(TimeoutCounters.GripperRetract);
		return coreResponse;
	}

	public IControlResponse SetFinger(GripperFingerState state)
	{
		CommandType type = CommandType.GripperClose;
		switch (state)
		{
		case GripperFingerState.Closed:
			type = CommandType.GripperClose;
			break;
		case GripperFingerState.Rent:
			type = ((!ControllerConfiguration.Instance.EnableSecureDiskValidator) ? CommandType.GripperRent : CommandType.GripperOpen);
			break;
		case GripperFingerState.Open:
			if (ControllerConfiguration.Instance.EnableSecureDiskValidator)
			{
				return CoreResponse.TimedOutResponse();
			}
			type = CommandType.GripperOpen;
			break;
		}
		CoreResponse coreResponse = null;
		for (int i = 0; i < 2; i++)
		{
			coreResponse = ExecuteControllerCommand(type, 5000);
			if (coreResponse.CommError)
			{
				LogHelper.Instance.Log("Communication error during finger operation", LogEntryType.Error);
				return coreResponse;
			}
			if (!coreResponse.TimedOut)
			{
				return coreResponse;
			}
		}
		LogHelper.Instance.Log($"RetryableFingerFunction {state.ToString().ToUpper()} timed out, even after {2} tries.", LogEntryType.Error);
		LogInputs(ControlBoards.Picker, LogEntryType.Error);
		IPersistentCounterService service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		switch (state)
		{
		case GripperFingerState.Closed:
			service.Increment(TimeoutCounters.FingerClose);
			break;
		case GripperFingerState.Open:
			service.Increment(TimeoutCounters.FingerOpen);
			break;
		default:
			service.Increment(TimeoutCounters.FingerRent);
			break;
		}
		return coreResponse;
	}

	public VendDoorState ReadVendDoorPosition()
	{
		return VendDoor.ReadState();
	}

	public IControlResponse VendDoorRent()
	{
		IControlResponse controlResponse = VendDoor.Rent();
		VendDoorState = (controlResponse.Success ? VendDoorState.Rent : VendDoorState.Unknown);
		return controlResponse;
	}

	public IControlResponse VendDoorClose()
	{
		IControlResponse controlResponse = VendDoor.Close();
		VendDoorState = (controlResponse.Success ? VendDoorState.Closed : VendDoorState.Unknown);
		return controlResponse;
	}

	public IControlResponse TrackOpen()
	{
		return Track.Open();
	}

	public IControlResponse TrackClose()
	{
		return Track.Close();
	}

	public IControlResponse TrackCycle()
	{
		return Track.Cycle();
	}

	public void ToggleRingLight(bool on, int? sleepAfter)
	{
		CommandType type = (on ? CommandType.RinglightOn : CommandType.RinglightOff);
		if (UseAltRinglightCommand)
		{
			type = (on ? CommandType.RinglightOn3 : CommandType.RinglightOff3);
		}
		ExecuteControllerCommand(type);
		if (sleepAfter.HasValue)
		{
			PerfFunctions.Wait(sleepAfter.Value);
		}
	}

	public IControlResponse SetAudio(AudioChannelState newState)
	{
		return ExecuteControllerCommand((AudioChannelState.On == newState) ? CommandType.AudioOn : CommandType.AudioOff);
	}

	public string[] GetBoardVersions()
	{
		string[] array = new string[ControllerBoards.Length];
		for (int i = 0; i < ControllerBoards.Length; i++)
		{
			array[i] = GetBoardVersion(ControllerBoards[i]);
		}
		return array;
	}

	public string GetBoardVersion(ControlBoards board)
	{
		CommandType type = CommandType.Version101;
		switch (board)
		{
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
		if (coreResponse.Success)
		{
			return coreResponse.OpCodeResponse;
		}
		return "TIMEOUT";
	}

	public IControlResponse Initialize()
	{
		CoreResponse result = ExecuteControllerCommand(CommandType.Reset);
		PerfFunctions.Wait(2000);
		SetAudio(AudioChannelState.Off);
		VendDoorState = VendDoor.ReadState();
		return result;
	}

	public void Shutdown()
	{
		Port.Close();
	}

	public string[] ReadInputs(ControlBoards board)
	{
		CommandType type = ((ControlBoards.Picker == board) ? CommandType.ReadPickerInputs : CommandType.AuxSensorsRead);
		CoreResponse coreResponse = ExecuteControllerCommand(type);
		if (coreResponse.CommError)
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

	public void LogPickerSensorState()
	{
		LogPickerSensorState(LogEntryType.Info);
	}

	public void LogPickerSensorState(LogEntryType type)
	{
		ReadSensors().LogSensorState(type);
	}

	public void LogInputs(ControlBoards board)
	{
		LogInputs(board, LogEntryType.Info);
	}

	public void LogInputs(ControlBoards board, LogEntryType type)
	{
		string text = ((ControlBoards.Picker == board) ? "Picker Inputs" : "AUX Inputs");
		CommandType type2 = ((ControlBoards.Picker == board) ? CommandType.ReadPickerInputs : CommandType.AuxSensorsRead);
		CoreResponse coreResponse = ExecuteControllerCommand(type2);
		if (coreResponse.CommError)
		{
			LogHelper.Instance.Log(coreResponse.Diagnostic);
			return;
		}
		LogHelper.Instance.Log(type, "{0} {1}", text, coreResponse.OpCodeResponse);
	}

	public CoreResponse ExecuteControllerCommand(CommandType type)
	{
		return SendCommand(CoreCommand.Create(type, null, Port));
	}

	public CoreResponse ExecuteControllerCommand(CommandType type, int? timeout)
	{
		return SendCommand(CoreCommand.Create(type, timeout, Port));
	}

	public CoreResponse ExecuteControllerCommand(CoreCommand command)
	{
		return SendCommand(command);
	}

	internal ControlSystem()
	{
		VendDoor = new VendDoorImpl();
		VendDoorState = VendDoorState.Unknown;
		Track = new TrackImpl(this);
		Port = RedboxSerialPort.Create(new RedboxSerialPortConfiguration(ControllerConfiguration.Instance.ControllerPortName)
		{
			Baud = 9600,
			Test = ResponseValid,
			WriteTerminator = RedboxSerialPort.WriteCharsCR
		}, PortModes.Async, "Serial Board");
		IPersistentMapService service = ServiceLocator.Instance.GetService<IPersistentMapService>();
		UseAltRinglightCommand = service.GetMap().GetValue("AltRinglight", def: false);
	}

	private PickerSensorReadResult ReadPickerSensors()
	{
		try
		{
			CoreResponse coreResponse = ExecuteControllerCommand(CommandType.SensorBarOn);
			if (coreResponse.CommError)
			{
				return new PickerSensorReadResult(coreResponse);
			}
			PerfFunctions.SpinWait(10);
			CoreResponse response = ExecuteControllerCommand(CommandType.ReadPickerInputs);
			PerfFunctions.SpinWait(10);
			return new PickerSensorReadResult(response);
		}
		finally
		{
			ExecuteControllerCommand(CommandType.SensorBarOff);
		}
	}

	private CoreResponse SendCommand(CoreCommand command)
	{
		if (command.Port == null)
		{
			return CoreResponse.CommErrorResponse();
		}
		if (!command.Port.Open())
		{
			LogHelper.Instance.Log($"ControlSystem.Instance: unable to open port {command.Port.Name}.", LogEntryType.Error);
			return CoreResponse.CommErrorResponse();
		}
		try
		{
			return command.Execute();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"ControlSystem.Instance.SendCommands on port {command.Port.Name} caught an exception.", e);
			return CoreResponse.CommErrorResponse();
		}
	}

	private bool ResponseValid(IChannelResponse response)
	{
		string text = Encoding.ASCII.GetString(response.RawResponse);
		if (!string.IsNullOrEmpty(text))
		{
			if (text.IndexOf("OK") == -1)
			{
				return text.IndexOf("EER") != -1;
			}
			return true;
		}
		return false;
	}
}
