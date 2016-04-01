using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal sealed class Gen5DeviceDescriptor : IDeviceDescriptor
{
	private readonly DeviceHelper Helper;

	private readonly IDriverDescriptor DriverInfo = new DriverDescriptor(new Version("2.0.0.2"), "Scanner Manufacturer");

	public string Vendor => "CCVIRTPORT";

	public string Product => "CCVIRTPORT";

	public string Friendlyname => "Cortex Scanner";

	public DeviceClass DeviceClazz => DeviceClass.Ports;

	public override string ToString()
	{
		return Vendor;
	}

	public bool ResetDriver()
	{
		return false;
	}

	public bool MatchDriver()
	{
		return Helper.MatchDriver2(Vendor, DriverInfo, DeviceClassGuids.Ports);
	}

	public bool Locate()
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		try
		{
			DeviceHelper deviceHelper = new DeviceHelper(debug: true);
			deviceHelper.FindDeviceWithVid2(this, list, list2);
			return list2.Count > 0;
		}
		finally
		{
			list.Clear();
			list2.Clear();
		}
	}

	public DeviceStatus GetStatus()
	{
		return DeviceStatus.Enabled;
	}

	internal Gen5DeviceDescriptor(DeviceHelper helper)
	{
		Helper = helper;
	}
}
