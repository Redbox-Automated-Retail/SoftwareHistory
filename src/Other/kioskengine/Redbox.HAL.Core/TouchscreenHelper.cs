using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal sealed class TouchscreenHelper : IDisposable
{
	private readonly DriverDescriptor Driver_3M = new DriverDescriptor(new Version("7.13.14.0"), "3M");

	private readonly DriverDescriptor Driver_Elo = new DriverDescriptor(new Version("5.5.1.4"), "Elo Touch Solutions");

	private readonly DriverDescriptor Driver_GeneralTouch_HID1 = new DriverDescriptor(new Version("4.2.2.1"), "General Touch Technology Co.,Ltd.");

	private readonly DriverDescriptor Driver_GeneralTouch_HID2 = new DriverDescriptor(new Version("2.10.1781.0"), "General Touch Technology Co., Ltd.");

	private readonly DriverDescriptor Driver_TouchBase = new DriverDescriptor(new Version("4.0.2.0"), "Touch-Base Ltd");

	private readonly List<IDeviceDescriptor> TouchScreenDescriptors = new List<IDeviceDescriptor>();

	private bool Disposed;

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			TouchScreenDescriptors.Clear();
		}
	}

	internal IDeviceDescriptor FindTouchScreen()
	{
		return FindTouchScreen(matchDriver: false);
	}

	internal IDeviceDescriptor FindTouchScreen(bool matchDriver)
	{
		foreach (IDeviceDescriptor touchScreenDescriptor in TouchScreenDescriptors)
		{
			LogHelper.Instance.Log("Searching for device {0}", touchScreenDescriptor.ToString());
			if (touchScreenDescriptor.Locate())
			{
				LogHelper.Instance.Log("Found Touchscreen device {0}.", touchScreenDescriptor.ToString());
				if (!matchDriver)
				{
					return touchScreenDescriptor;
				}
				if (touchScreenDescriptor.MatchDriver())
				{
					LogHelper.Instance.Log("Touch screen driver matched.");
					return touchScreenDescriptor;
				}
			}
		}
		LogHelper.Instance.Log("Could not match TS driver.");
		return null;
	}

	internal TouchscreenHelper()
	{
		TouchScreenDescriptors.Add(new GenericTouchscreenDescriptor("0596", "0001", "3M", Driver_3M, DeviceClass.HIDClass));
		TouchScreenDescriptors.Add(new GenericTouchscreenDescriptor("0596", "0001", "3M", Driver_3M, DeviceClass.Mouse));
		TouchScreenDescriptors.Add(new GenericTouchscreenDescriptor("04e7", "0020", "Elo", Driver_Elo, DeviceClass.Mouse));
		TouchScreenDescriptors.Add(new GenericTouchscreenDescriptor("04e7", "0042", "Elo_2", Driver_Elo, DeviceClass.Mouse));
		TouchScreenDescriptors.Add(new GenericTouchscreenDescriptor("0dfc", "0001", "General Touch", Driver_GeneralTouch_HID2, DeviceClass.HIDClass));
		TouchScreenDescriptors.Add(new GenericTouchscreenDescriptor("0dfc", "0001", "General Touch", Driver_GeneralTouch_HID2, DeviceClass.HIDClass));
		TouchScreenDescriptors.Add(new GenericTouchscreenDescriptor("0dfc", "0001", "General Touch", Driver_GeneralTouch_HID1, DeviceClass.Mouse));
		TouchScreenDescriptors.Add(new GenericTouchscreenDescriptor("14c8", "0003", "Zytronic", Driver_TouchBase, DeviceClass.Mouse));
	}
}
