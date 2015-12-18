using Redbox.HAL.Core;

namespace Redbox.HAL.Camera.Framework;

public sealed class CameraInfo
{
	private readonly DeviceHelper Helper;

	public DeviceStatus Status { get; private set; }

	public KnownCameras Revision { get; private set; }

	public DeviceDescriptor Descriptor
	{
		get
		{
			if (Revision != KnownCameras.None)
			{
				return CameraDescriptors.FromKnownCamera(Revision);
			}
			return null;
		}
	}

	public DriverDescriptor DriverDescriptor
	{
		get
		{
			if (Revision != KnownCameras.None)
			{
				return CameraDescriptors.ForKnownCamera(Revision);
			}
			return null;
		}
	}

	public bool FindActiveCamera()
	{
		DeviceDescriptor deviceInfo = CameraDescriptors.FromKnownCamera(KnownCameras.FourthGen);
		DeviceStatus deviceStatus = Helper.FindDeviceStatus(DeviceClassGuids.Image, deviceInfo);
		if (deviceStatus != DeviceStatus.None)
		{
			Revision = KnownCameras.FourthGen;
			Status = deviceStatus;
			return true;
		}
		deviceInfo = CameraDescriptors.FromKnownCamera(KnownCameras.ThirdGen);
		deviceStatus = Helper.FindDeviceStatus(DeviceClassGuids.Image, deviceInfo);
		if (deviceStatus != DeviceStatus.None)
		{
			Revision = KnownCameras.ThirdGen;
			Status = deviceStatus;
			return true;
		}
		return false;
	}

	public bool ResetDriver()
	{
		if (!SetCameraStateInner(DeviceState.Disable))
		{
			return false;
		}
		PerfFunctions.Wait(1500);
		bool result = SetCameraStateInner(DeviceState.Enable);
		PerfFunctions.SpinWait(1500);
		return result;
	}

	public bool SetCameraState(DeviceState state)
	{
		return SetCameraStateInner(state);
	}

	public bool CorrectDriverRunning()
	{
		if (DriverDescriptor == null || Descriptor == null)
		{
			return false;
		}
		return Helper.MatchDriver(Descriptor, DriverDescriptor, DeviceClassGuids.Image);
	}

	public CameraInfo(DeviceHelper helper)
	{
		Status = DeviceStatus.None;
		Revision = KnownCameras.None;
		Helper = helper;
	}

	private bool SetCameraStateInner(DeviceState state)
	{
		return Helper.ChangeByHWID(Descriptor, DeviceClassGuids.Image, state);
	}
}
