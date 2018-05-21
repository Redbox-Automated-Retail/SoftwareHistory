using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal sealed class Scanner : IDisposable
{
	private readonly IScannerDeviceService ScannerDeviceService;

	private readonly IControlSystem ControlSystem;

	private readonly IScannerDevice Device;

	private bool Disposed;

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			if (Device != null && Device.RequiresExternalLighting)
			{
				ControlSystem.ToggleRingLight(on: false, ScannerDeviceService.IRHardwareInstall.HasValue ? ControllerConfiguration.Instance.IRRinglightShutdownPause : 0);
			}
			GC.SuppressFinalize(this);
		}
	}

	internal static ScanResult SmartReadDisk(CenterDiskMethod[] methods)
	{
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		if (!ServiceLocator.Instance.GetService<IScannerDeviceService>().IRHardwareInstall.HasValue)
		{
			return OnNonIRScan(service, methods);
		}
		return OnIRScan(service, methods);
	}

	internal ScanResult Read()
	{
		using IScanResult sr = Device.Scan();
		ScanResult scanResult = new ScanResult(sr);
		IExecutionContext activeContext = ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext();
		if (!scanResult.SnapOk)
		{
			LogHelper.Instance.Log(LogEntryType.Error, "[{0}, {1}]: Read disk returned a camera error status CAMERA CAPTURE ERROR", activeContext.ProgramName, activeContext.ID);
			activeContext?.ContextLog.WriteFormatted("No image was captured.");
		}
		else if (!scanResult.ReadCode)
		{
			LogHelper.Instance.Log(LogEntryType.Error, "[{0}, {1}]: No barcodes read on image.", activeContext.ProgramName, activeContext.ID);
			activeContext?.ContextLog.WriteFormatted("No barcodes read on image.");
		}
		return scanResult;
	}

	internal Scanner(IControlSystem cs)
	{
		Disposed = false;
		ControlSystem = cs;
		ScannerDeviceService = ServiceLocator.Instance.GetService<IScannerDeviceService>();
		Device = ScannerDeviceService.GetConfiguredDevice();
		Device.Start();
		OnStartRinglight();
	}

	internal Scanner(CenterDiskMethod method, IControlSystem cs)
	{
		Disposed = false;
		ControlSystem = cs;
		ScannerDeviceService = ServiceLocator.Instance.GetService<IScannerDeviceService>();
		Device = ScannerDeviceService.GetConfiguredDevice();
		Device.Start();
		if (ScannerDeviceService.IRHardwareInstall.HasValue)
		{
			if (method != CenterDiskMethod.None)
			{
				ControlSystem.Center(method);
			}
			OnStartRinglight();
		}
		else
		{
			OnStartRinglight();
			if (method != CenterDiskMethod.None)
			{
				ControlSystem.Center(method);
			}
		}
	}

	private static ScanResult OnNonIRScan(IControlSystem cs, CenterDiskMethod[] methods)
	{
		ScanResult scanResult = null;
		using Scanner scanner = new Scanner(cs);
		foreach (CenterDiskMethod method in methods)
		{
			cs.Center(method);
			scanResult = scanner.OnReadWithRetry();
			if (!scanResult.SnapOk || scanResult.ReadCode)
			{
				return scanResult;
			}
		}
		return scanResult;
	}

	private static ScanResult OnIRScan(IControlSystem cs, CenterDiskMethod[] methods)
	{
		ScanResult scanResult = null;
		for (int i = 0; i < methods.Length; i++)
		{
			using Scanner scanner = new Scanner(methods[i], cs);
			scanResult = scanner.OnReadWithRetry();
			if (!scanResult.SnapOk || scanResult.ReadCode)
			{
				return scanResult;
			}
		}
		return scanResult;
	}

	private void OnStartRinglight()
	{
		if (Device.RequiresExternalLighting)
		{
			ControlSystem.ToggleRingLight(on: true, ScannerDeviceService.IRHardwareInstall.HasValue ? ControllerConfiguration.Instance.IRRinglightStartupPause : ControllerConfiguration.Instance.RinglightWarmupPause2);
		}
	}

	private ScanResult OnReadWithRetry()
	{
		ScanResult scanResult = Read();
		if (scanResult.SnapOk || !ControllerConfiguration.Instance.EnableCameraReset)
		{
			return scanResult;
		}
		if (Device.Restart())
		{
			scanResult = Read();
		}
		return scanResult;
	}
}
