using Redbox.HAL.Configuration;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataMatrix.Framework;

public sealed class BarcodeReader
{
	private readonly bool IsLicensed;

	private readonly IBarcodeReader Reader;

	public static BarcodeReader Instance => Singleton<BarcodeReader>.Instance;

	public void Scan(string bitmapFile, ScanResult scanResult)
	{
		if (!IsLicensed)
		{
			scanResult.ResetOnException();
		}
		else
		{
			scanResult.FromResult(Reader.Scan(bitmapFile));
		}
	}

	private BarcodeReader()
	{
		try
		{
			Reader = new ClearImageReaderV2();
			IsLicensed = Reader.IsLicensed;
			LogHelper.Instance.Log("[Barcode Reader] The reader {0} licensed.", IsLicensed ? "is" : "is not");
		}
		catch
		{
			IsLicensed = false;
			Reader = new NilReader();
		}
	}
}
