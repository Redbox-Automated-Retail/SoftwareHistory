using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using ClearImage;
using RBDM;
using Redbox.HAL.Configuration;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataMatrix.Framework;

internal class ClearImageReaderV2 : IBarcodeReader
{
	private sealed class ClearImageScanResult : IScanResult, IDisposable
	{
		private bool m_disposed;

		private readonly List<string> m_rawCodes = new List<string>();

		public TimeSpan ExecutionTime { get; set; }

		public string[] ReadCodes => m_rawCodes.ToArray();

		public void Dispose()
		{
			if (!m_disposed)
			{
				m_disposed = true;
				m_rawCodes.Clear();
			}
		}

		public ClearImageScanResult()
		{
			ExecutionTime = default(TimeSpan);
		}

		internal void ResetOnException()
		{
			m_rawCodes.Clear();
			ExecutionTime = default(TimeSpan);
		}

		internal void AddCode(string code)
		{
			m_rawCodes.Add(code);
		}
	}

	private readonly clsRBDM TheReader;

	public bool IsLicensed { get; private set; }

	public IScanResult Scan(string file)
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
					num = TheReader.Find(BarcodeConfiguration.Instance.ExpectedCodes);
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

	internal ClearImageReaderV2()
	{
		LogHelper.Instance.Log("Clearimage v2 reader.init()");
		Module module = GetType().Module;
		int hModule = Marshal.GetHINSTANCE(module).ToInt32();
		CiServer ciServer = new CiServerClass();
		try
		{
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
