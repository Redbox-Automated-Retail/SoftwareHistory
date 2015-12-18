namespace Redbox.HAL.Configuration;

internal interface IBarcodeReader
{
	bool IsLicensed { get; }

	IScanResult Scan(string file);
}
