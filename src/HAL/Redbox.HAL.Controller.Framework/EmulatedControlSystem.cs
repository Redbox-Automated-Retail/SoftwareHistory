using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class EmulatedControlSystem : IControlSystem, ICoreCommandExecutor
{
	internal readonly ControlBoards[] ControllerBoards = new ControlBoards[3]
	{
		ControlBoards.Serial,
		ControlBoards.Picker,
		ControlBoards.Aux
	};

	private VendDoorState m_vendDoorState;

	private TrackState m_trackState;

	public VendDoorState VendDoorState => m_vendDoorState;

	public TrackState TrackState => m_trackState;

	public IControlResponse StopRoller()
	{
		return SuccessResponse();
	}

	public IControlResponse StartRollerIn()
	{
		return SuccessResponse();
	}

	public IControlResponse StartRollerOut()
	{
		return SuccessResponse();
	}

	public IControlResponse RollerToPosition(RollerPosition position, int opTimeout, bool logSensors)
	{
		return SuccessResponse();
	}

	public IControlResponse RollerToPosition(RollerPosition position, int opTimeout)
	{
		return SuccessResponse();
	}

	public IPickerSensorReadResult ReadSensors()
	{
		return new EmulatedPickerSensorRead(ErrorCodes.Success);
	}

	public IPickerSensorReadResult ReadImmediateSensors()
	{
		return new EmulatedPickerSensorRead(ErrorCodes.Success);
	}

	public IControlResponse SetSensors(bool on)
	{
		return SuccessResponse();
	}

	public ErrorCodes Center(CenterDiskMethod method)
	{
		return ErrorCodes.Success;
	}

	public void TimedExtend()
	{
	}

	public void TimedExtend(int timeout)
	{
	}

	public IControlResponse ExtendArm()
	{
		return SuccessResponse();
	}

	public IControlResponse ExtendArm(int timeout)
	{
		return SuccessResponse();
	}

	public IControlResponse RetractArm()
	{
		return SuccessResponse();
	}

	public IControlResponse SetFinger(GripperFingerState state)
	{
		return SuccessResponse();
	}

	public VendDoorState ReadVendDoorPosition()
	{
		return m_vendDoorState;
	}

	public IControlResponse VendDoorRent()
	{
		m_vendDoorState = VendDoorState.Rent;
		return SuccessResponse();
	}

	public IControlResponse VendDoorClose()
	{
		m_vendDoorState = VendDoorState.Closed;
		return SuccessResponse();
	}

	public IControlResponse TrackOpen()
	{
		m_trackState = TrackState.Closed;
		return SuccessResponse();
	}

	public IControlResponse TrackClose()
	{
		m_trackState = TrackState.Closed;
		return SuccessResponse();
	}

	public IControlResponse TrackCycle()
	{
		return SuccessResponse();
	}

	public void ToggleRingLight(bool on, int? sleepAfter)
	{
	}

	public IControlResponse SetAudio(AudioChannelState newState)
	{
		return SuccessResponse();
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
		return $"Emulated {board.ToString()} version.";
	}

	public IControlResponse Initialize()
	{
		return SuccessResponse();
	}

	public void Shutdown()
	{
	}

	public string[] ReadInputs(ControlBoards board)
	{
		return new string[20];
	}

	public void LogPickerSensorState()
	{
	}

	public void LogPickerSensorState(LogEntryType type)
	{
	}

	public void LogInputs(ControlBoards board)
	{
	}

	public void LogInputs(ControlBoards board, LogEntryType type)
	{
	}

	public CoreResponse ExecuteControllerCommand(CommandType type)
	{
		CoreCommand command = CoreCommand.Create(type, null);
		return ExecuteControllerCommand(command);
	}

	public CoreResponse ExecuteControllerCommand(CommandType type, int? timeout)
	{
		CoreCommand command = CoreCommand.Create(type, null);
		return ExecuteControllerCommand(command);
	}

	public CoreResponse ExecuteControllerCommand(CoreCommand command)
	{
		CoreResponse coreResponse = new CoreResponse(command.Address);
		coreResponse.Error = ErrorCodes.Success;
		return coreResponse;
	}

	internal EmulatedControlSystem()
	{
	}

	private IControlResponse SuccessResponse()
	{
		return new EmulatedControlResponse(ErrorCodes.Success, string.Empty);
	}
}
