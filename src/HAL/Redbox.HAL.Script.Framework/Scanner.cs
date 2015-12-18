using System;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.Script.Framework;

public sealed class Scanner : IDisposable
{
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

	public bool CameraStarted { get; private set; }

	public IScannerDevice Device { get; private set; }

	private bool Disposed { get; set; }

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
				PickerBoard.ToggleRingLight(RinglightState.Off, null);
			}
			if (BarcodeConfiguration.Instance.CycleCameraOnUse)
			{
				Device.Stop();
			}
			Device = null;
		}
		GC.SuppressFinalize(this);
	}

	public RawReadResult RawRead()
	{
		return Device.Scan().ToRawReadResult();
	}

	public ScanResult Read()
	{
		ScanResult scanResult = Device.Scan();
		try
		{
			if (BarcodeConfiguration.Instance.LogDetailedScan)
			{
				LogHelper.Instance.Log($"***ReadDisc: Execution Time = {scanResult.ExecutionTime}, File = {scanResult.SnapResult.Path}, Matrix = {scanResult.ScannedMatrix} secure code count = {scanResult.SecureCount}", LogEntryType.Info);
			}
			if (scanResult.ReadCode && BarcodeConfiguration.Instance.DeleteBitmapOnSuccessfulRead)
			{
				FileHelper.SafeDeleteFile(scanResult.SnapResult.Path);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ReadDiskInPickerInner.", e);
		}
		return scanResult;
	}

	public Scanner()
	{
		Device = ScannerDeviceFactory.GetConfiguredDevice();
		CameraStarted = Device.Start();
		if (CameraStarted && Device.RequiresExternalLighting)
		{
			PickerBoard.ToggleRingLight(RinglightState.On, ControllerConfiguration.Instance.SyncRinglightWarmupTime);
		}
	}

	public ScanResult SmartReadDisk(CenterDiskMethod[] methods)
	{
		ScanResult scanResult = null;
		bool flag = false;
		foreach (CenterDiskMethod method in methods)
		{
			ControllerService.Instance.CenterItem(method);
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

	internal string Diagnostic(ScanResult scanResult)
	{
		if (!scanResult.SnapOk)
		{
			return "CAPTUREERROR";
		}
		return "SUCCESS";
	}
}
