using System.Reflection;
using System.Runtime.InteropServices;
using Redbox.HAL.ClearimageV1;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataMatrix.Framework;

public sealed class BarcodeReader
{
	public static BarcodeReader Instance => Singleton<BarcodeReader>.Instance;

	private bool IsLicensed { get; set; }

	private IBarcodeReader Reader { get; set; }

	public ScanResult Scan(string bitmapFile, int expectedCodes)
	{
		if (BarcodeConfiguration.Instance.Mode == BarcodeReaderMode.Emulated)
		{
			ScanResult scanResult = new ScanResult();
			scanResult.AddCode(BarcodeEmulationState.Instance.ExpectedScanResults.Next());
			return scanResult;
		}
		ScanResult scanResult2 = new ScanResult();
		if (!IsLicensed)
		{
			scanResult2.ResetOnException();
			return scanResult2;
		}
		scanResult2.FromResult(Reader.Scan(bitmapFile, expectedCodes));
		return scanResult2;
	}

	private BarcodeReader()
	{
		BarcodeReaderMode mode = BarcodeConfiguration.Instance.Mode;
		if (mode == BarcodeReaderMode.ClearImage)
		{
			try
			{
				Module module = GetType().Module;
				int data = Marshal.GetHINSTANCE(module).ToInt32();
				IClearImageReader clearImageReader = null;
				clearImageReader = ((RuntimeService.Instance.GetPlatform() != Platform.Windows7) ? ((IClearImageReader)new Reader()) : ((IClearImageReader)new ClearImageReaderV2()));
				IsLicensed = clearImageReader.Init(data);
				Reader = clearImageReader;
				return;
			}
			catch
			{
				IsLicensed = false;
				Reader = new NilReader();
				return;
			}
		}
		Reader = new NilReader();
	}
}
