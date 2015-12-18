using System;

namespace Redbox.HAL.DataMatrix.Framework;

public static class ScannerDeviceFactory
{
	private sealed class NilScanner : IScannerDevice, IDisposable
	{
		public bool RequiresExternalLighting => false;

		public bool IsConnected => false;

		public bool InError => true;

		public void Dispose()
		{
		}

		public bool Start()
		{
			return false;
		}

		public SnapResult Snap(string fileName)
		{
			return new SnapResult();
		}

		public SnapResult Snap()
		{
			return new SnapResult();
		}

		public bool Stop()
		{
			return false;
		}

		public void TurnOff()
		{
		}

		public bool Restart()
		{
			return false;
		}

		public ScanResult Scan()
		{
			ScanResult scanResult = new ScanResult(new SnapResult());
			scanResult.ResetOnException();
			return scanResult;
		}

		public string FindInstalledPort()
		{
			return "NONE";
		}

		internal NilScanner()
		{
		}
	}

	private static NilScanner m_nilInstance = new NilScanner();

	public static IScannerDevice GetConfiguredDevice()
	{
		return GetConfiguredDevice(BarcodeConfiguration.Instance.ScannerService);
	}

	public static IScannerDevice GetConfiguredDevice(ScannerServices _service)
	{
		IScannerDevice result = null;
		switch (_service)
		{
		case ScannerServices.Legacy:
			result = LegacyScanner.Instance;
			break;
		case ScannerServices.Cortex:
			result = CortexService.Instance;
			break;
		case ScannerServices.Emulated:
			result = m_nilInstance;
			break;
		}
		return result;
	}
}
