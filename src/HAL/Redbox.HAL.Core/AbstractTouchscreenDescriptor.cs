using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal abstract class AbstractTouchscreenDescriptor : IDeviceDescriptor
{
	private readonly IDriverDescriptor DriverDesc;

	private readonly DeviceHelper Helper;

	public string Vendor { get; private set; }

	public string Product { get; private set; }

	public string Friendlyname { get; private set; }

	public DeviceClass DeviceClazz => DeviceClass.Mouse;

	protected abstract DeviceClass CoreDeviceClazz { get; }

	protected abstract Guid DeviceGuid { get; }

	public override string ToString()
	{
		return $"vid_{Vendor}&pid_{Product}";
	}

	public bool MatchDriver()
	{
		if (Helper.MatchDriver(this, DriverDesc, DeviceGuid))
		{
			LogHelper.Instance.Log("Touch screen driver matched.");
			return true;
		}
		LogHelper.Instance.Log("Could not match TS driver.");
		return false;
	}

	public bool Locate()
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		try
		{
			Helper.FindDevice(this, list, list2);
			return list2.Count > 0;
		}
		finally
		{
			list2.Clear();
			list.Clear();
		}
	}

	public DeviceStatus GetStatus()
	{
		return OnGetDeviceStatus();
	}

	public bool ResetDriver()
	{
		return OnDriverReset();
	}

	protected virtual bool OnDriverReset()
	{
		return false;
	}

	protected virtual DeviceStatus OnGetDeviceStatus()
	{
		return DeviceStatus.Unknown;
	}

	protected AbstractTouchscreenDescriptor(DeviceHelper helper, string v, string p, string f, IDriverDescriptor desc)
	{
		Helper = helper;
		Vendor = v;
		Product = p;
		Friendlyname = f;
		DriverDesc = desc;
	}
}
