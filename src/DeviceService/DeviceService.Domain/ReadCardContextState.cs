namespace DeviceService.Domain;

public enum ReadCardContextState
{
	Continue,
	Stop,
	Timeout,
	Cancelled,
	Tampered,
	Blocked
}
