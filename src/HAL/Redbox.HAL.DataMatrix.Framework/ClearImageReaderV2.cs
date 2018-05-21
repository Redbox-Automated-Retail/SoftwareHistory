using System;
using System.Reflection;
using System.Runtime.InteropServices;
using ClearImage;
using RBDM;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class ClearImageReaderV2 : IBarcodeReader
{
	private readonly clsRBDM TheReader;

	public bool IsLicensed { get; private set; }

	public IScanResult Scan(string file)
	{
		return Scan(file, BarcodeConfiguration.Instance.ExpectedCodes);
	}

	public IScanResult Scan(string file, int expectedCodes)
	{
		ScanResult scanResult = new ScanResult();
		if (!IsLicensed)
		{
			LogHelper.Instance.Log("This version isn't licensed.");
			scanResult.ResetOnException();
			return scanResult;
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
					scanResult.ExecutionTime = executionTimer.Elapsed;
				}
				if (num > 0)
				{
					foreach (CiBarcode barcode in TheReader.Barcodes)
					{
						foreach (string computeFilter in BarcodeConfiguration.Instance.ComputeFilterList)
						{
							if (!computeFilter.Equals(barcode.Text, StringComparison.CurrentCultureIgnoreCase))
							{
								scanResult.Add(barcode.Text);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				flag = true;
				LogHelper.Instance.Log("ClearImageScanner: exception during scan.", e);
				scanResult.ResetOnException();
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
				return scanResult;
			}
		}
		scanResult.ResetOnException();
		return scanResult;
	}

	internal ClearImageReaderV2()
	{
		LogHelper.Instance.Log("Clearimage v2 reader.init()");
		try
		{
			Module module = GetType().Module;
			int hModule = Marshal.GetHINSTANCE(module).ToInt32();
			CiServer ciServer = new CiServerClass();
			ciServer.OpenExt(hModule, 74391114, 0);
			TheReader = new clsRBDM
			{
				Ci = ciServer,
				Image = ciServer.CreateImage()
			};
			IsLicensed = true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BarcodeReader.InitializeClearImage.  Calling CiServer.OpenExt to send license key failed.", e);
			TheReader = null;
			IsLicensed = false;
		}
	}
}
