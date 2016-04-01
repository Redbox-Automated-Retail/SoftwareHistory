namespace Redbox.HAL.Component.Model;

internal interface IFraudSensor
{
	ErrorCodes LastPostResult { get; }

	ErrorCodes Initialize(IFormattedLog log);

	ErrorCodes Shutdown();

	ErrorCodes Reset(IFormattedLog log);

	ErrorCodes StartScan();

	ErrorCodes StopScan();

	IFraudScanResult Validate();
}
