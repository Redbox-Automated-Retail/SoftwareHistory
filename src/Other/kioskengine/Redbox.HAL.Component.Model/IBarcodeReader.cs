namespace Redbox.HAL.Component.Model;

internal interface IBarcodeReader
{
	bool IsLicensed { get; }

	IScanResult Scan(string file);
}
