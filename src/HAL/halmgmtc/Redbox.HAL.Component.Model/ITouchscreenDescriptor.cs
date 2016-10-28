namespace Redbox.HAL.Component.Model;

public interface ITouchscreenDescriptor : IDeviceDescriptor
{
	bool SoftReset();

	string ReadFirmware();
}
