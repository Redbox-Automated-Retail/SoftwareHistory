using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class FraudScanResult : IFraudScanResult
{
	internal static readonly IFraudScanResult NotConfiguredResult = new FraudScanResult
	{
		Valid = false,
		ScanError = ErrorCodes.DeviceNotConfigured
	};

	public bool Valid { get; internal set; }

	public ErrorCodes ScanError { get; internal set; }

	internal FraudScanResult()
	{
		Valid = false;
		ScanError = ErrorCodes.Success;
	}
}
