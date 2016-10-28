namespace Redbox.HAL.Component.Model;

internal interface IFraudScanResult
{
	bool Valid { get; }

	ErrorCodes ScanError { get; }
}
