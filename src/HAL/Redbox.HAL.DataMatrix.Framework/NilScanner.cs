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

	public bool Start(bool unused)
	{
		return false;
	}

	public bool Stop()
	{
		return false;
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

	public ISnapResult Snap()
	{
		return new SnapResult(null);
	}

	public ISnapResult Snap(string fileName)
	{
		return new SnapResult(fileName);
	}

	public bool ValidateSettings()
	{
		return false;
	}

	internal NilScanner()
	{
	}
}
