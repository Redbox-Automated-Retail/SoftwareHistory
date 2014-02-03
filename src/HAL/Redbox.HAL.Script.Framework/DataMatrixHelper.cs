using System;
using System.IO;
using Redbox.HAL.Camera.Framework;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;

namespace Redbox.HAL.Script.Framework;

internal class DataMatrixHelper
{
	private bool? m_irConfigured = null;

	internal static DataMatrixHelper Instance => Singleton<DataMatrixHelper>.Instance;

	internal bool IRCameraConfigured
	{
		get
		{
			if (m_irConfigured.HasValue)
			{
				return m_irConfigured.Value;
			}
			m_irConfigured = SecretOptions.Instance.GetValue("IRCamera", defValue: false);
			return m_irConfigured.Value;
		}
		set
		{
			m_irConfigured = value;
			SecretOptions.Instance.SetValue("IRCamera", m_irConfigured.Value);
		}
	}

	internal bool IsBarcodeUnknown(string code)
	{
		if (!"UNKNOWN".Equals(code, StringComparison.CurrentCultureIgnoreCase))
		{
			return "redbox".Equals(code, StringComparison.CurrentCultureIgnoreCase);
		}
		return true;
	}

	internal CameraResult CameraSnap(out string path)
	{
		return Snap(out path);
	}

	internal ScanResult ScanFrame(string path)
	{
		return ScanImageInner(path);
	}

	internal ReadDiskResult RawReadDiscInPicker(CenterDiskMethod method)
	{
		if (method != CenterDiskMethod.None)
		{
			ControllerService.Instance.CenterItem(method);
		}
		return ReadCenteredDiskInPicker(applyFilters: false);
	}

	internal ReadDiskResult ReadDiscInPicker(CenterDiskMethod method)
	{
		if (method != CenterDiskMethod.None)
		{
			ControllerService.Instance.CenterItem(method);
		}
		return ReadCenteredDiskInPicker(applyFilters: true);
	}

	private ReadDiskResult ReadCenteredDiskInPicker(bool applyFilters)
	{
		ReadDiskResult readDiskResult = new ReadDiskResult();
		string path = null;
		if (Snap(out path) == CameraResult.CaptureError)
		{
			if (ControllerConfiguration.Instance.CountHardwareErrors)
			{
				CounterManager.Instance.Increment("CameraCaptureFailures");
			}
			readDiskResult.ReadStatus = ReadDiscStatus.CaptureError;
			return readDiskResult;
		}
		ScanResult scanResult = ScanImageInner(path);
		readDiskResult.FromScanResult(scanResult);
		if (!applyFilters)
		{
			return readDiskResult;
		}
		if (IRCameraConfigured && !scanResult.FoundSecureCode)
		{
			readDiskResult.InvalidateSecureRead();
		}
		return readDiskResult;
	}

	private CameraResult Snap(out string path)
	{
		path = CameraService.Instance.Snap();
		if (string.IsNullOrEmpty(path) || !File.Exists(path))
		{
			LogHelper.Instance.Log("The image doesn't exist on the file system: CAMERA CAPTURE error.", LogEntryType.Error);
			path = string.Empty;
			return CameraResult.CaptureError;
		}
		return CameraResult.Success;
	}

	private ScanResult ScanImageInner(string path)
	{
		ScanResult scanResult = null;
		try
		{
			scanResult = BarcodeReader.Instance.Scan(path, IRCameraConfigured ? 7 : 4);
			bool flag = false;
			if (IRCameraConfigured)
			{
				LogHelper.Instance.Log(string.Format("***ReadDisc: Execution Time = {0}, File = {1}, Matrix = {2} found secure code = {3}", scanResult.ExecutionTime, path, scanResult.ScannedMatrix, scanResult.FoundSecureCode ? "YES" : "NO"), LogEntryType.Info);
				flag = scanResult.FoundSecureCode && scanResult.ScannedCodes > 0;
			}
			else
			{
				if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
				{
					LogHelper.Instance.Log($"***ReadDisc: Execution Time = {scanResult.ExecutionTime}, File = {path}, Matrix = {scanResult.ScannedMatrix}", LogEntryType.Info);
				}
				flag = scanResult.ScannedCodes > 0;
			}
			if (flag && CameraConfiguration.Instance.DeleteBitmapOnSuccessfulRead)
			{
				FileHelper.SafeDeleteFile(path);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ReadDiskInPickerInner.", e);
		}
		return scanResult;
	}

	private DataMatrixHelper()
	{
	}
}
