using System.IO;

namespace Redbox.HAL.Component.Model;

internal interface IUsbDeviceService
{
	IDeviceDescriptor FindTouchScreen(bool matchDriver);

	IDeviceDescriptor FindTouchScreen();

	IDeviceDescriptor FindActiveCamera(bool matchDriver);

	IDeviceDescriptor FromQueryString(string s);

	bool SetDeviceState(IDeviceDescriptor descriptor, DeviceState state);

	bool ChangeByHWID(IDeviceDescriptor descriptor, DeviceState state);

	DeviceStatus FindDeviceStatus(IDeviceDescriptor deviceInfo);

	bool MatchDriverByVendor(IDeviceDescriptor desc, IDriverDescriptor driverInfo);

	bool MatchDriver(IDeviceDescriptor descriptor, IDriverDescriptor driverInfo);

	IUsbDeviceSearchResult FindDeviceByVendor(IDeviceDescriptor desc);

	IUsbDeviceSearchResult FindDevice(IDeviceDescriptor descriptor);

	IUsbDeviceSearchResult FindVendorDevices(string _vendor);

	void EnumDevices(DeviceClass clazz, TextWriter writer);
}
