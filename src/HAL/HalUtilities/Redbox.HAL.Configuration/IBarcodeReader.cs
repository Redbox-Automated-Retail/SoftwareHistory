namespace Redbox.HAL.Configuration;

public interface IBarcodeReader
{
	bool IsLicensed { get; }

	IScanResult Scan(string file);
}
