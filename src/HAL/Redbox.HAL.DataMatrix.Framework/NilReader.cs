using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

internal class NilReader : IBarcodeReader
{
	private readonly NilScanResult Result = new NilScanResult("UNLICENSED READER");

	public bool IsLicensed => false;

	public IScanResult Scan(string file)
	{
		return Result;
	}

	public IScanResult Scan(string file, int notUsed)
	{
		return Result;
	}
}
