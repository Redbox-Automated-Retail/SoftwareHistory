namespace Redbox.HAL.Controller.Framework;

public enum AuxHardwareError
{
	Success,
	CommunicationError,
	VendDoorCloseTimeout,
	VendDoorRentTimeout,
	QlmEngageTimeout,
	QlmDisengageTimeout,
	QlmHomeXTimeout
}
