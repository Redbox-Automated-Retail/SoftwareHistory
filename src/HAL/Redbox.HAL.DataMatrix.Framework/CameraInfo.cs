using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class CameraInfo
{
	private readonly DeviceHelper Helper;

	internal DeviceStatus Status { get; private set; }

	internal KnownCameras Revision { get; private set; }

	internal DeviceDescriptor Descriptor
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

	internal DriverDescriptor DriverDescriptor
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

	internal bool FindActiveCamera()
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

	internal bool ResetDriver()
	{
		if (!SetCameraState(DeviceState.Disable))
		{
			return false;
		}
		PerfFunctions.Wait(1500);
		bool result = SetCameraState(DeviceState.Enable);
		PerfFunctions.SpinWait(1500);
		return result;
	}

	public bool SetCameraState(DeviceState state)
	{
		return Helper.ChangeByHWID(Descriptor, DeviceClassGuids.Image, state);
	}

	internal bool CorrectDriverRunning()
	{
		if (DriverDescriptor == null || Descriptor == null)
		{
			return false;
		}
		return Helper.MatchDriver(Descriptor, DriverDescriptor, DeviceClassGuids.Image);
	}

	internal CameraInfo(DeviceHelper helper)
	{
		Status = DeviceStatus.None;
		Revision = KnownCameras.None;
		Helper = helper;
	}
}
