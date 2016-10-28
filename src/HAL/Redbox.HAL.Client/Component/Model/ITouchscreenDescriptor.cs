namespace Redbox.HAL.Component.Model;

internal interface ITouchscreenDescriptor : IDeviceDescriptor
{
	bool SoftReset();

	string ReadFirmware();
}
