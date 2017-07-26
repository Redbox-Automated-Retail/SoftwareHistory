using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal struct FraudValidator : IDisposable
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

	internal readonly bool ScanConfigured;

	private readonly ExecutionContext Context;

	private readonly IDiskValidatorService DiskService;

	private ErrorCodes ScanError;

	private bool Disposed;

	private bool IsScanning;

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
		IFraudScanResult fraudScanResult = Validate(matrix);
		if (fraudScanResult.ScanError == ErrorCodes.Success && !fraudScanResult.Valid)
		{
			IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
			service.RollerToPosition(RollerPosition.Position6);
			fraudScanResult = Validate(matrix);
			service.RollerToPosition(RollerPosition.Position3);
		}
		return fraudScanResult;
	}

	internal IFraudScanResult MultipassValidateChecked(string matrix)
	{
		if (!ControllerConfiguration.Instance.FraudScanMultiPass)
		{
			return Validate(matrix);
		}
		return MultipassValidate(matrix);
	}

	internal void StartScan()
	{
		if (ScanConfigured && !IsScanning)
		{
			ScanError = DiskService.FraudSensor.StartScan();
			IsScanning = ScanError == ErrorCodes.Success;
		}
	}

	internal void Publish(IFraudScanResult scanResult, string matrix)
	{
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
		bool flag = DiskService.FraudSensor.LastPostResult != ErrorCodes.Success;
		Context.CreateResult(flag ? "LastFraudPostError" : "LastFraudPostOK", flag ? $"Last POST failed with error {DiskService.FraudSensor.LastPostResult.ToString()}" : "Last POST was successful.", null);
	}

	internal FraudValidator(ExecutionContext context, bool startScan)
	{
		this = default(FraudValidator);
		DiskService = ServiceLocator.Instance.GetService<IDiskValidatorService>();
		Context = context;
		ScanConfigured = DiskService.IsConfigured;
		Disposed = (IsScanning = false);
		if (startScan)
		{
			StartScan();
		}
	}

	internal FraudValidator(ExecutionContext context, FraudScanJobMask scanJob)
	{
		this = default(FraudValidator);
		DiskService = ServiceLocator.Instance.GetService<IDiskValidatorService>();
		Context = context;
		bool flag = ((uint)ControllerConfiguration.Instance.FraudScanJobBitmask2 & (uint)scanJob) != 0;
		ScanConfigured = flag && DiskService.IsConfigured;
		Disposed = (IsScanning = false);
	}

	private IFraudScanResult Validate(string matrix)
	{
		if (ScanError == ErrorCodes.Success)
		{
			return DiskService.Validate(matrix);
		}
		return new InvalidScanStateResult(ScanError);
	}
}
