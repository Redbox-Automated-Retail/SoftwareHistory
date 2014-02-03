namespace Redbox.HAL.Controller.Framework;

public enum CommandType
{
	[CommandProperties(Revision = "C", Address = AddressSelector.H002, Command = "X")]
	[CommandProperties(Revision = "*", Address = AddressSelector.H101, Command = "X", CommandWait = 60000)]
	Reset,
	[CommandProperties(Address = AddressSelector.H002, Command = "M1", StatusBit = 6, ResetCommand = "K1", WaitPauseTime = 500, MultiProcWaitTime = 100)]
	QlmEngage,
	[CommandProperties(Address = AddressSelector.H002, Command = "L1", StatusBit = 5, ResetCommand = "K1", WaitPauseTime = 500)]
	QlmDisengage,
	[CommandProperties(Address = AddressSelector.H002, Command = "K1")]
	QlmHalt,
	[CommandProperties(Revision = "C", Address = AddressSelector.H005, CommandWait = 16000, Command = "P1")]
	[CommandProperties(Revision = "*", Address = AddressSelector.H002, Command = "P2")]
	QlmDoorLock,
	[CommandProperties(Revision = "*", Address = AddressSelector.H002, Command = "O2")]
	[CommandProperties(Revision = "C", Address = AddressSelector.H005, CommandWait = 16000, Command = "O1")]
	QlmDoorUnlock,
	[CommandProperties(Address = AddressSelector.H001, Command = "O1")]
	SensorBarOn,
	[CommandProperties(Address = AddressSelector.H001, Command = "P1")]
	SensorBarOff,
	[CommandProperties(Address = AddressSelector.H001, Command = "R")]
	SensorBarRead,
	[CommandProperties(Revision = "C", Address = AddressSelector.H002, Command = "I")]
	[CommandProperties(Revision = "*", Address = AddressSelector.H001, Command = "I")]
	AudioOn,
	[CommandProperties(Revision = "C", Address = AddressSelector.H002, Command = "J")]
	[CommandProperties(Revision = "*", Address = AddressSelector.H101, Command = "J")]
	AudioOff,
	[CommandProperties(Address = AddressSelector.H001, Command = "J4")]
	RollerIn,
	[CommandProperties(Address = AddressSelector.H001, Command = "I4")]
	RollerOut,
	[CommandProperties(Address = AddressSelector.H001, Command = "K4")]
	RollerStop,
	[CommandProperties(Address = AddressSelector.H001, Command = "U1", StatusBit = 18, ResetCommand = "K4,P1")]
	RollerToPos1,
	[CommandProperties(Address = AddressSelector.H001, Command = "U2", StatusBit = 19, ResetCommand = "K4,P1")]
	RollerToPos2,
	[CommandProperties(Address = AddressSelector.H001, Command = "U3", StatusBit = 20, ResetCommand = "K4,P1")]
	RollerToPos3,
	[CommandProperties(Address = AddressSelector.H001, Command = "T4", StatusBit = 15, ResetCommand = "K4,P1")]
	RollerToPos4,
	[CommandProperties(Address = AddressSelector.H001, Command = "T5", StatusBit = 16, ResetCommand = "K4,P1")]
	RollerToPos5,
	[CommandProperties(Address = AddressSelector.H001, Command = "T6", StatusBit = 17, ResetCommand = "K4,P1")]
	RollerToPos6,
	[CommandProperties(Address = AddressSelector.H001, Command = "O2")]
	RinglightOn,
	[CommandProperties(Address = AddressSelector.H001, Command = "P2")]
	RinglightOff,
	[CommandProperties(Address = AddressSelector.H002, Command = "O1")]
	AuxSensorsOn,
	[CommandProperties(Address = AddressSelector.H002, Command = "P1")]
	AuxSensorsOff,
	[CommandProperties(Revision = "C", Address = AddressSelector.H005, Command = "R")]
	[CommandProperties(Revision = "*", Address = AddressSelector.H002, Command = "R")]
	AuxSensorsRead,
	[CommandProperties(Revision = "C", Address = AddressSelector.H005, Command = "M3", StatusBit = 10)]
	[CommandProperties(Revision = "*", Address = AddressSelector.H002, Command = "M2", StatusBit = 8, ResetCommand = "K2", WaitPauseTime = 60)]
	VendDoorOpen,
	[CommandProperties(Revision = "C", Address = AddressSelector.H005, Command = "U", StatusBit = 2)]
	[CommandProperties(Revision = "*", Address = AddressSelector.H002, Command = "V", StatusBit = 10, ResetCommand = "K2", WaitPauseTime = 60)]
	VendDoorRent,
	[CommandProperties(Address = AddressSelector.H002, Command = "L2", StatusBit = 7, ResetCommand = "K2", WaitPauseTime = 60)]
	VendDoorClose,
	[CommandProperties(Address = AddressSelector.H001, Command = "R")]
	ReadPickerControllerSensors,
	[CommandProperties(Address = AddressSelector.H001, Command = "K1")]
	GripperExtendHalt,
	[CommandProperties(Address = AddressSelector.H001, Command = "I1")]
	ExtendGripperArmForTime,
	[CommandProperties(Address = AddressSelector.H001, Command = "L1", StatusBit = 5, Timeout = 2000, ResetCommand = "K1", WaitPauseTime = 50)]
	GripperExtend,
	[CommandProperties(Address = AddressSelector.H001, Command = "M1", StatusBit = 6, ResetCommand = "K1", WaitPauseTime = 50)]
	GripperRetract,
	[CommandProperties(Address = AddressSelector.H001, Command = "M3", StatusBit = 10, ResetCommand = "K3", WaitPauseTime = 50, MultiProcWaitTime = 80)]
	GripperOpen,
	[CommandProperties(Address = AddressSelector.H001, Command = "V", StatusBit = 12, ResetCommand = "K3", WaitPauseTime = 50, MultiProcWaitTime = 80)]
	GripperRent,
	[CommandProperties(Address = AddressSelector.H001, Command = "L3", StatusBit = 9, ResetCommand = "K3", WaitPauseTime = 50, MultiProcWaitTime = 80)]
	GripperClose,
	[CommandProperties(Address = AddressSelector.H001, Command = "I1", StatusBit = 5, Timeout = 50, ResetCommand = "K1,J1,K1")]
	GripperPush,
	[CommandProperties(Address = AddressSelector.H001, Command = "M2", StatusBit = 8, ResetCommand = "K2", WaitPauseTime = 50)]
	TrackOpen,
	[CommandProperties(Address = AddressSelector.H001, Command = "L2", StatusBit = 7, ResetCommand = "K2", WaitPauseTime = 50)]
	TrackClose,
	[CommandProperties(Revision = "*", Address = AddressSelector.H101, Command = "Y")]
	Version101,
	[CommandProperties(Address = AddressSelector.H001, Command = "W")]
	Version001,
	[CommandProperties(Address = AddressSelector.H002, Command = "W")]
	Version002,
	[CommandProperties(Address = AddressSelector.H001, Command = "S")]
	Status001,
	[CommandProperties(Address = AddressSelector.H002, Command = "S")]
	Status002,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "I1")]
	TurnOnGreenButtonLed,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "I3")]
	TurnOffGreenButtonLed,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "I2")]
	BlinkGreenButtonLed,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "R1")]
	TurnOnRedButtonLed,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "R3")]
	TurnOffRedButtonLed,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "R2")]
	BlinkRedButtonLed,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "T1")]
	TurnOnGreenArrowLed,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "T2")]
	BlinkGreenArrowLed,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "T3")]
	TurnOffGreenArrowLed,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "Z1")]
	TurnOnRedArrowLed,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "Z2")]
	BlinkRedArrowLed,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "Z3")]
	TurnOffRedArrowLed,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "W1")]
	TurnOnBackLight,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "W3")]
	TurnOffBackLight,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "W2")]
	BlinkBackLight,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "S{0}")]
	SendText,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "X{0}")]
	ClearDisplayMemory,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "Y")]
	SideTerminalVersion,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "J")]
	ReadQRButton,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "K")]
	ClearQRButtonStatus,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "U90")]
	TurnOffPixels,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "U94")]
	SetTextOnlyDisplayMode,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "U98")]
	SetGraphicsOnlyDisplayMode,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "U80")]
	DisplaySettingOR,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "U81")]
	DisplaySettingEXOR,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "U83")]
	DisplaySettingAND,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "M{0}")]
	SetStartTextPagePointer,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "L{0}")]
	SetStartGraphicsPagePointer,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "V{0}")]
	SetMemoryWritePointer,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "O{0}")]
	SetTextColumns,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "N{0}")]
	SetGraphicsColumns,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "g{0}")]
	WriteGraphicData,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "l{0}")]
	WriteGraphicDataToEEPROM,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "d{0}")]
	SetEEPROMPointer,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "e{0}")]
	SetEEPROMReadSize,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "m")]
	LoadFromEEPROMToDisplayMemory,
	[CommandProperties(Revision = "A", Address = AddressSelector.H555, Command = "c")]
	TerminalRevision,
	[CommandProperties(Address = AddressSelector.H002, Command = "J2")]
	UnknownVendDoorCloseCommand,
	[CommandProperties(Address = AddressSelector.H002, Command = "I2")]
	UnknownVendDoorOpenCommand,
	[CommandProperties(Address = AddressSelector.H002, Command = "K2")]
	VendDoorKillCommand,
	[CommandProperties(Revision = "A", Address = AddressSelector.H002, Command = "O4")]
	KillPowerToAUXUSB,
	[CommandProperties(Revision = "A", Address = AddressSelector.H002, Command = "P4")]
	PowerAUXUSB,
	[CommandProperties(Revision = "*", Address = AddressSelector.H002, Command = "P2")]
	IceQubeAlarmResetOff,
	[CommandProperties(Revision = "*", Address = AddressSelector.H002, Command = "O2")]
	IceQubeAlarmResetOn
}
