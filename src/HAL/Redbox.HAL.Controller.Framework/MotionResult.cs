namespace Redbox.HAL.Controller.Framework;

public enum MotionResult
{
	Success,
	Error,
	ArcusNotResponsive,
	ObstructionDetected,
	DeckOutOfRange,
	SlotOutOfRange,
	UnitsOutOfRange,
	Timeout,
	EncoderError,
	VendDoorNotClosed,
	GripperNotRetracted,
	InvalidMoveToSecureLocation,
	LocationNotFound,
	SensorError,
	GripperRentTimeout,
	InvalidResponse,
	FrontDoor,
	DoorOpen,
	MotorNotHomed
}
