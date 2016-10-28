using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core.Descriptors;

internal sealed class MicrotouchDescriptor : ITouchscreenDescriptor, IDeviceDescriptor, IDriverDescriptor
{
	private readonly byte[] FirmwareCommand;

	public string Vendor => "0596";

	public string Product => "0001";

	public string Friendlyname => "3M";

	public IDeviceSetupClass SetupClass { get; private set; }

	public Version DriverVersion { get; private set; }

	public string Provider => "3M";

	public override string ToString()
	{
		return $"vid_{Vendor}&pid_{Product}";
	}

	public bool SoftReset()
	{
		throw new NotImplementedException();
	}

	public string ReadFirmware()
	{
		byte[] array = SendReceive(FirmwareCommand, 24);
		if (array == null)
		{
			return "UNKNOWN";
		}
		return $"{BitConverter.ToString(array, 3, 1)}.{BitConverter.ToString(array, 4, 1)}";
	}

	public bool ResetDriver()
	{
		throw new NotImplementedException();
	}

	public bool MatchDriver()
	{
		IUsbDeviceService service = ServiceLocator.Instance.GetService<IUsbDeviceService>();
		if (service.MatchDriver(this, this))
		{
			LogHelper.Instance.Log("Touch screen driver matched.");
			return true;
		}
		LogHelper.Instance.Log("Could not match TS driver.");
		return false;
	}

	public bool Locate()
	{
		IUsbDeviceService service = ServiceLocator.Instance.GetService<IUsbDeviceService>();
		return service.FindDevice(this).Found;
	}

	public DeviceStatus GetStatus()
	{
		throw new NotImplementedException();
	}

	internal MicrotouchDescriptor(DeviceClass clazz)
	{
		DriverVersion = new Version("7.13.14.0");
		IDeviceSetupClassFactory service = ServiceLocator.Instance.GetService<IDeviceSetupClassFactory>();
		SetupClass = service.Get(clazz);
		FirmwareCommand = new byte[8];
		for (int i = 0; i < FirmwareCommand.Length; i++)
		{
			byte b = 0;
			switch (i)
			{
			case 0:
				b = 192;
				break;
			case 1:
				b = 10;
				break;
			case 6:
				b = 24;
				break;
			}
			FirmwareCommand[i] = b;
		}
	}

	private byte[] SendReceive(byte[] command, int responseLength)
	{
		SafeFileHandle safeFileHandle = Win32.CreateFile("\\\\.\\TwTouchDevice1", Win32.AccessFlags.GENERIC_READ_WRITE, Win32.ShareFlags.FILE_SHARE_READ_WRITE, IntPtr.Zero, 3u, 128u, IntPtr.Zero);
		byte[] array = null;
		if (safeFileHandle.IsInvalid)
		{
			LogHelper.Instance.Log("[MicrotouchDescriptor] CreateFile failed: Error = {0}", Marshal.GetLastWin32Error());
			return array;
		}
		try
		{
			if (!Win32.WriteFile(safeFileHandle, command, command.Length, out var lpNumberOfBytesWritten, IntPtr.Zero))
			{
				LogHelper.Instance.Log("[MicrotouchDescriptor] WriteFile failed: Error = {0}", Marshal.GetLastWin32Error());
			}
			else
			{
				array = new byte[responseLength];
				if (!Win32.ReadFile(safeFileHandle, array, array.Length, out lpNumberOfBytesWritten, IntPtr.Zero))
				{
					array = null;
					LogHelper.Instance.Log("[MicrotouchDescriptor] ReadFile failed: Error = {0}", Marshal.GetLastWin32Error());
				}
			}
			return array;
		}
		finally
		{
			safeFileHandle.Close();
		}
	}
}
