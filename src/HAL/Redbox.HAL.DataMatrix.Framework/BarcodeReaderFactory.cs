using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

public sealed class BarcodeReaderFactory
{
	private readonly IBarcodeReader Reader;

	private static readonly BarcodeReaderFactory _Instance;

	public static BarcodeReaderFactory Instance => _Instance;

	public IBarcodeReader GetConfiguredReader()
	{
		return Reader;
	}

	private BarcodeReaderFactory()
	{
		try
		{
			Reader = new ClearImageReaderV2();
			if (!Reader.IsLicensed)
			{
				Reader = new NilReader();
			}
			LogHelper.Instance.Log("[Barcode Reader] The reader {0} licensed.", Reader.IsLicensed ? "is" : "is not");
		}
		catch
		{
			Reader = new NilReader();
		}
	}

	static BarcodeReaderFactory()
	{
		_Instance = new BarcodeReaderFactory();
	}
}
