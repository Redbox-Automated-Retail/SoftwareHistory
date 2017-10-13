namespace Redbox.HAL.Component.Model;

public interface IControlSystem
{
	VendDoorState VendDoorState { get; }

	TrackState TrackState { get; }

	IControlResponse Initialize();

	bool Shutdown();

	IControlResponse SetAudio(AudioChannelState newState);

	IControlResponse ToggleRingLight(bool on, int? sleepAfter);

	IControlResponse VendDoorRent();

	IControlResponse VendDoorClose();

	VendDoorState ReadVendDoorPosition();

	IControlResponse TrackOpen();

	IControlResponse TrackClose();

	IControlResponse TrackCycle();

	void TimedExtend();

	void TimedExtend(int timeout);

	IControlResponse ExtendArm();

	IControlResponse ExtendArm(int timeout);

	IControlResponse RetractArm();

	IControlResponse SetFinger(GripperFingerState state);

	ErrorCodes Center(CenterDiskMethod method);

	string GetBoardVersion(ControlBoards board);

	string[] GetBoardVersions();

	string[] ReadInputs(ControlBoards board);

	void LogPickerSensorState();

	void LogPickerSensorState(LogEntryType type);

	void LogInputs(ControlBoards board);

	void LogInputs(ControlBoards board, LogEntryType type);

	IControlResponse SetSensors(bool on);

	IPickerSensorReadResult ReadSensors();

	IPickerSensorReadResult ReadImmediateSensors();

	IControlResponse StopRoller();

	IControlResponse StartRollerIn();

	IControlResponse StartRollerOut();

	IControlResponse RollerToPosition(RollerPosition position);

	IControlResponse RollerToPosition(RollerPosition position, int opTimeout);

	IControlResponse RollerToPosition(RollerPosition position, int opTimeout, bool logSensors);

	QlmStatus GetQlmStatus();

	ErrorCodes EngageQlm(IFormattedLog log);

	ErrorCodes EngageQlm(bool home, IFormattedLog log);

	ErrorCodes DisengageQlm(IFormattedLog log);

	ErrorCodes DisengageQlm(bool home, IFormattedLog log);

	IControlResponse DropQlm();

	IControlResponse LiftQlm();

	IControlResponse HaltQlmOperation();
}
