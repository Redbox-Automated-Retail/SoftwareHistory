using System;
using CiBcfKo86572612;
using ClearImage;
using Redbox.HAL.Core;

namespace Redbox.HAL.ClearimageV1;

public class Reader : IClearImageReader, IBarcodeReader
{
	private bool IsLicensed { get; set; }

	private CiBcFlow TheReader { get; set; }

	public IScanResult Scan(string file, int expectedCodes)
	{
		ClearImageScanResult clearImageScanResult = new ClearImageScanResult();
		if (!IsLicensed)
		{
			LogHelper.Instance.Log("This version isn't licensed.");
			clearImageScanResult.ResetOnException();
			return clearImageScanResult;
		}
		for (int i = 0; i < 2; i++)
		{
			bool flag = false;
			try
			{
				TheReader.Image.Open(file);
				int num = 0;
				using (ExecutionTimer executionTimer = new ExecutionTimer())
				{
					num = TheReader.Find(expectedCodes);
					executionTimer.Stop();
					clearImageScanResult.ExecutionTime = executionTimer.Elapsed;
				}
				if (num > 0)
				{
					foreach (CiBarcode barcode in TheReader.Barcodes)
					{
						clearImageScanResult.AddCode(barcode.Text);
					}
				}
			}
			catch (Exception e)
			{
				flag = true;
				LogHelper.Instance.Log("ClearImageScanner: exception during scan.", e);
				ProcessHelper.DumpProcessMemoryMetrics();
				clearImageScanResult.ResetOnException();
			}
			try
			{
				TheReader.Image.Close();
			}
			catch (Exception e2)
			{
				LogHelper.Instance.Log("Exception closing barcode image.", e2);
			}
			if (!flag)
			{
				return clearImageScanResult;
			}
		}
		clearImageScanResult.ResetOnException();
		return clearImageScanResult;
	}

	public bool Init(int data)
	{
		LogHelper.Instance.Log("Clearimage v1 reader.init()");
		CiServer ciServer = new CiServerClass();
		try
		{
			ciServer.OpenExt(data, 74391114, 0);
			TheReader = new CiBcFlowClass
			{
				Ci = ciServer,
				Image = ciServer.CreateImage()
			};
			return IsLicensed = true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BarcodeReader.InitializeClearImage.  Calling CiServer.OpenExt to send license key failed.", e);
			TheReader = null;
			return false;
		}
	}
}
