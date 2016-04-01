using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal sealed class LegacyDeviceDescriptor : IDeviceDescriptor
{
	private readonly IDriverDescriptor DriverDescriptor;

	private readonly DeviceHelper Helper;

	public string Vendor { get; private set; }

	public string Product { get; private set; }

	public string Friendlyname { get; private set; }

	public DeviceClass DeviceClazz => DeviceClass.Image;

	public override string ToString()
	{
		return $"vid_{Vendor}&pid_{Product}";
	}

	public bool MatchDriver()
	{
		return Helper.MatchDriver(this, DriverDescriptor, DeviceClassGuids.Image);
	}

	public bool Locate()
	{
		DeviceStatus deviceStatus = Helper.FindDeviceStatus(DeviceClassGuids.Image, this);
		return DeviceStatus.None != deviceStatus;
	}

	public DeviceStatus GetStatus()
	{
		return Helper.FindDeviceStatus(DeviceClassGuids.Image, this);
	}

	public bool ResetDriver()
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

	internal LegacyDeviceDescriptor(DeviceHelper helper, string vendor, string product, string friendly, IDriverDescriptor driver)
	{
		Helper = helper;
		Vendor = vendor.ToLower();
		Product = product.ToLower();
		Friendlyname = friendly;
		DriverDescriptor = driver;
	}

	private bool SetCameraState(DeviceState state)
	{
		return Helper.ChangeByHWID(this, DeviceClassGuids.Image, state);
	}
}
