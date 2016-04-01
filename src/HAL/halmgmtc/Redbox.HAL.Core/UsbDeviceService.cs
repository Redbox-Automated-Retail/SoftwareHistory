using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

public sealed class UsbDeviceService : IUsbDeviceService
{
	public IDeviceDescriptor FindTouchScreen(bool matchDriver)
	{
		using TouchscreenHelper touchscreenHelper = new TouchscreenHelper();
		return touchscreenHelper.FindTouchScreen(matchDriver);
	}

	public IDeviceDescriptor FindTouchScreen()
	{
		return FindTouchScreen(matchDriver: false);
	}

	public IDeviceDescriptor FindActiveCamera(bool matchDriver)
	{
		CameraDeviceHelper cameraDeviceHelper = new CameraDeviceHelper();
		IDeviceDescriptor deviceDescriptor = cameraDeviceHelper.FindActiveCamera();
		if (deviceDescriptor == null || !deviceDescriptor.MatchDriver())
		{
			return null;
		}
		return deviceDescriptor;
	}
}
