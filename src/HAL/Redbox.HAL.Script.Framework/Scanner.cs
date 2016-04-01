using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal sealed class Scanner : IDisposable
{
	private readonly IControlSystem ControlSystem;

	private bool Disposed;

	private static readonly CenterDiskMethod[] ReadFromDoor = new CenterDiskMethod[2]
	{
		CenterDiskMethod.None,
		CenterDiskMethod.DrumAndBack
	};

	private static readonly CenterDiskMethod[] ReadFromDrum = new CenterDiskMethod[2]
	{
		CenterDiskMethod.None,
		CenterDiskMethod.VendDoorAndBack
	};

	internal bool CameraStarted { get; private set; }

	internal IScannerDevice Device { get; private set; }

	public void Dispose()
	{
		if (Disposed)
		{
			return;
		}
		Disposed = true;
		if (Device != null)
		{
			if (Device.RequiresExternalLighting)
			{
				ControlSystem.ToggleRingLight(on: false, null);
			}
			Device = null;
		}
		GC.SuppressFinalize(this);
	}

	internal ScanResult Read()
	{
		using IScanResult sr = Device.Scan();
		return new ScanResult(sr);
	}

	internal Scanner()
	{
		ControlSystem = ServiceLocator.Instance.GetService<IControlSystem>();
		Device = ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice();
		CameraStarted = Device.Start();
		if (CameraStarted && Device.RequiresExternalLighting)
		{
			ControlSystem.ToggleRingLight(on: true, ControllerConfiguration.Instance.SyncRinglightWarmupTime);
		}
	}

	internal ScanResult SmartReadDisk(CenterDiskMethod[] methods)
	{
		ScanResult scanResult = null;
		bool flag = false;
		foreach (CenterDiskMethod method in methods)
		{
			ControlSystem.Center(method);
			while (true)
			{
				scanResult = Read();
				if (scanResult.SnapOk)
				{
					break;
				}
				if (!ControllerConfiguration.Instance.EnableCameraReset || flag)
				{
					return scanResult;
				}
				Device.Restart();
				flag = true;
			}
			if (scanResult.ReadCode)
			{
				return scanResult;
			}
		}
		return scanResult;
	}
}
