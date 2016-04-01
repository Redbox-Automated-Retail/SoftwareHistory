using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

public struct DiskValidatorHelper : IDisposable
{
	private class InvalidScanStateResult : IFraudScanResult
	{
		public bool Valid => false;

		public ErrorCodes ScanError { get; private set; }

		internal InvalidScanStateResult(ErrorCodes error)
		{
			ScanError = error;
		}
	}

	public readonly bool ScanConfigured;

	private readonly IDiskValidatorService DiskService;

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

	public IFraudScanResult Validate()
	{
		if (ScanError == ErrorCodes.Success)
		{
			return DiskService.Validate();
		}
		return new InvalidScanStateResult(ScanError);
	}

	public IFraudScanResult Validate(string matrix)
	{
		if (ScanError == ErrorCodes.Success)
		{
			return DiskService.Validate(matrix);
		}
		return new InvalidScanStateResult(ScanError);
	}

	public void ValidateAndNotify(string matrix, IExecutionContext ctx)
	{
		if (ScanConfigured)
		{
			IFraudScanResult scanResult = DiskService.Validate(matrix);
			NotifyContext(scanResult, matrix, ctx, ControllerConfiguration.Instance.EnableFraudScanResults);
		}
	}

	public void NotifyContext(IFraudScanResult scanResult, string matrix, IExecutionContext context, bool leaveResult)
	{
		if (scanResult.ScanError != ErrorCodes.Success)
		{
			if (leaveResult)
			{
				context.CreateResult("FraudScanError", $"Scan returned error code {scanResult.ScanError.ToString()}", matrix);
			}
			context.ContextLog.WriteFormatted("[DiskValidatorHelper] The scan returned error code {0}", scanResult.ScanError.ToString());
			return;
		}
		if (scanResult.Valid)
		{
			context.ContextLog.WriteFormatted("Security markers detected on barcode {0} ( {1} ID = {2} ).", matrix, context.ProgramName, context.ID);
			if (leaveResult)
			{
				context.CreateResult("FraudMarkersRead", "A security marker was scanned with the barcode.", matrix);
			}
		}
		else
		{
			context.ContextLog.WriteFormatted("The disk {0} had no security markers ( {1} ID = {2} ).", matrix, context.ProgramName, context.ID);
			if (leaveResult)
			{
				context.CreateResult("NoFraudMarkersDetected", "No security markers were detected.", matrix);
			}
		}
		if (leaveResult)
		{
			bool flag = DiskService.FraudSensor.LastPostResult != ErrorCodes.Success;
			context.CreateResult(flag ? "LastFraudPostError" : "LastFraudPostOK", flag ? $"Last POST failed with error {DiskService.FraudSensor.LastPostResult.ToString()}" : "Last POST was successful.", null);
		}
	}

	public DiskValidatorHelper(IDiskValidatorService diskService, JobsWithScan scanJob)
	{
		this = default(DiskValidatorHelper);
		Disposed = false;
		DiskService = diskService;
		JobsWithScan fraudScanJobBitmask = (JobsWithScan)ControllerConfiguration.Instance.FraudScanJobBitmask;
		ScanConfigured = (fraudScanJobBitmask & scanJob) == scanJob && DiskService.IsConfigured;
		if (ScanConfigured)
		{
			ScanError = DiskService.FraudSensor.StartScan();
		}
	}
}
