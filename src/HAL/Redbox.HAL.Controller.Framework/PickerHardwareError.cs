namespace Redbox.HAL.Controller.Framework;

public enum PickerHardwareError
{
	Success,
	GripperExtendTimeout,
	GripperRetractTimeout,
	GripperOpenTimeout,
	GripperCloseTimeout,
	GripperRentTimeout,
	TrackOpenTimeout,
	TrackCloseTimeout,
	SensorReadError
}
