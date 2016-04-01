using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class NilScanner : IScannerDevice, IDisposable
{
	public string Name => ScannerServices.Emulated.ToString();

	public bool RequiresExternalLighting => false;

	public bool IsConnected => false;

	public bool InError => true;

	public void Dispose()
	{
	}

	public bool TestConfiguration(IMessageSink sink)
	{
		return false;
	}

	public bool Start()
	{
		return false;
	}

	public ISnapResult Snap(string fileName)
	{
		return new SnapResult(fileName);
	}

	public ISnapResult Snap()
	{
		return new SnapResult(null);
	}

	public bool ResetDeviceDriver()
	{
		return false;
	}

	public bool Stop()
	{
		return false;
	}

	public void Disconnect()
	{
	}

	public bool Restart()
	{
		return false;
	}

	public IScanResult Scan()
	{
		return new NilScanResult("UNCONFIGURED DEVICE");
	}

	public string FindInstalledPort()
	{
		return "NONE";
	}

	internal NilScanner()
	{
	}
}
