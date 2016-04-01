using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal sealed class CameraDeviceHelper : IDisposable
{
	private readonly IDriverDescriptor Gen4DriverDescriptor = new DriverDescriptor(new Version("2.1.0.0"), "AVEO");

	private readonly IDriverDescriptor Gen3DriverDescriptor = new DriverDescriptor(new Version("5.7.19104.104"), "Sonix");

	private readonly List<IDeviceDescriptor> Devices = new List<IDeviceDescriptor>();

	private readonly DeviceHelper Helper;

	private bool Disposed;

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			Devices.Clear();
		}
	}

	internal IDeviceDescriptor FindActiveCamera()
	{
		foreach (IDeviceDescriptor device in Devices)
		{
			if (device.Locate())
			{
				LogHelper.Instance.Log("FindActiveCamera: HWID found {0}", device.ToString());
				return device;
			}
		}
		return null;
	}

	internal CameraDeviceHelper()
	{
		Helper = new DeviceHelper(debug: true);
		Devices.Add(new Gen5DeviceDescriptor(Helper));
		Devices.Add(new LegacyDeviceDescriptor(Helper, "1871", "0f01", "4th Gen", Gen4DriverDescriptor));
		Devices.Add(new LegacyDeviceDescriptor(Helper, "0c45", "627b", "3rd Gen", Gen3DriverDescriptor));
	}
}
