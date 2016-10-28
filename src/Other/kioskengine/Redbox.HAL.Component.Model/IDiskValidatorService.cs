namespace Redbox.HAL.Component.Model;

internal interface IDiskValidatorService : IConfigurationObserver
{
	bool IsConfigured { get; }

	IFraudSensor FraudSensor { get; }

	IFraudScanResult Validate(string matrix);

	IFraudScanResult Validate();

	void CorrectErroredPost(IFormattedLog log);
}
