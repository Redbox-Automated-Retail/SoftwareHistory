using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal struct FraudValidator : IDisposable
{
	private readonly bool ScanConfigured;

	private readonly ExecutionContext Context;

	private readonly IDiskValidatorService DiskService;

	private bool IsScanning;

	private ErrorCodes ScanError;

	private bool Disposed;

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			if (ScanConfigured)
			{
				DiskService.FraudSensor.StopScan();
			}
		}
	}

	internal IFraudScanResult MultipassValidate(string matrix)
	{
		return DiskService.Validate(matrix);
	}

	internal void Publish(IFraudScanResult scanResult, string matrix)
	{
		if (!ScanConfigured)
		{
			return;
		}
		if (scanResult.ScanError != ErrorCodes.Success)
		{
			Context.CreateResult("FraudScanError", $"Scan returned error code {scanResult.ScanError.ToString()}", matrix);
			Context.ContextLog.WriteFormatted("[DiskValidatorHelper] The scan returned error code {0}", scanResult.ScanError.ToString());
			return;
		}
		if (scanResult.Valid)
		{
			Context.ContextLog.WriteFormatted("Security markers detected on barcode {0} ( {1} ID = {2} ).", matrix, Context.ProgramName, Context.ID);
			Context.CreateResult("FraudMarkersRead", "A security marker was scanned with the barcode.", matrix);
		}
		else
		{
			Context.ContextLog.WriteFormatted("The disk {0} had no security markers ( {1} ID = {2} ).", matrix, Context.ProgramName, Context.ID);
			Context.CreateResult("NoFraudMarkersDetected", "No security markers were detected.", matrix);
		}
		Context.CreateInfoResult("LastFraudPostOK", "Last POST was successful.");
	}

	internal void StartScan()
	{
		if (ScanConfigured && !IsScanning)
		{
			ScanError = DiskService.FraudSensor.StartScan();
			IsScanning = ScanError == ErrorCodes.Success;
		}
	}

	internal FraudValidator(ExecutionContext context, bool startScan)
	{
		this = default(FraudValidator);
		DiskService = ServiceLocator.Instance.GetService<IDiskValidatorService>();
		Context = context;
		ScanConfigured = false;
		Disposed = false;
		ScanError = ErrorCodes.DeviceNotConfigured;
	}
}
