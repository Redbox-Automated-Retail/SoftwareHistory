namespace DeviceService.ComponentModel;

public enum ErrorState
{
	None,
	FailedToInitializeConnection,
	NoDeviceFound,
	ConnectionError,
	EncryptionError,
	Unknown
}
