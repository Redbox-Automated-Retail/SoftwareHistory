namespace Redbox.HAL.Controller.Framework;

internal enum DeviceErrorState
{
	Ok,
	WriteTimeout,
	PortUnavailable,
	ReadTimeout,
	CommunicationError
}
