using System;
using System.Diagnostics;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal sealed class ArcusDeviceDescriptor : IDeviceDescriptor
{
	public string Vendor => "1589";

	public string Product => "a001";

	public string Friendlyname => "Proteus XES";

	public DeviceClass DeviceClazz => DeviceClass.None;

	public override string ToString()
	{
		return $"vid_{Vendor}&pid_{Product}";
	}

	public bool MatchDriver()
	{
		return false;
	}

	public bool Locate()
	{
		return false;
	}

	public DeviceStatus GetStatus()
	{
		return DeviceStatus.Unknown;
	}

	public bool ResetDriver()
	{
		Thread.Sleep(3000);
		using (DeviceHelper deviceHelper = new DeviceHelper(debug: true))
		{
			if (!deviceHelper.SetDeviceState(this, DeviceState.Disable))
			{
				return false;
			}
			if (!deviceHelper.SetDeviceState(this, DeviceState.Enable))
			{
				return false;
			}
			if (deviceHelper.SetDeviceState(this, DeviceState.Enable))
			{
				return true;
			}
		}
		Process process = null;
		bool flag = false;
		try
		{
			process = Process.Start("c:\\Program Files\\Redbox\\hal-tools\\halutilities.exe", "-resetProteus");
			process.WaitForExit();
			return process.ExitCode == 0;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unable to reset Proteus driver.", e);
			return false;
		}
	}

	internal ArcusDeviceDescriptor()
	{
	}
}
