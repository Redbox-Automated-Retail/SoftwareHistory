namespace Redbox.HAL.Component.Model;

public interface IUsbDeviceService
{
	IDeviceDescriptor FindTouchScreen(bool matchDriver);

	IDeviceDescriptor FindTouchScreen();

	IDeviceDescriptor FindActiveCamera(bool matchDriver);
}
