using System;
using System.Diagnostics;
using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class ArcusDeviceDescriptor : IDeviceDescriptor
{
	public string Vendor => "1589";

	public string Product => "a001";

	public string Friendlyname => "Proteus XES";

	public IDeviceSetupClass SetupClass { get; private set; }

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
		IUsbDeviceService service = ServiceLocator.Instance.GetService<IUsbDeviceService>();
		if (!service.SetDeviceState(this, DeviceState.Disable))
		{
			return false;
		}
		if (!service.SetDeviceState(this, DeviceState.Enable))
		{
			return false;
		}
		if (service.SetDeviceState(this, DeviceState.Enable))
		{
			return true;
		}
		bool flag = false;
		try
		{
			Process process = Process.Start("c:\\Program Files\\Redbox\\hal-tools\\halutilities.exe", "-resetProteus");
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
		IDeviceSetupClassFactory service = ServiceLocator.Instance.GetService<IDeviceSetupClassFactory>();
		SetupClass = service.Get(DeviceClass.None);
	}
}
