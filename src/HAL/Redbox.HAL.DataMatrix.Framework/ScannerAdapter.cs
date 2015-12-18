using System;
using System.IO;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataMatrix.Framework;

internal abstract class ScannerAdapter : IDisposable
{
	private readonly ScannerServices Service;

	private bool m_disposed;

	public void Dispose()
	{
		if (!m_disposed)
		{
			m_disposed = true;
			DisposeInner();
		}
	}

	protected string GenerateSnapFileName()
	{
		string path = FileHelper.GenerateUniqueFile("jpg");
		return Path.Combine(BarcodeConfiguration.Instance.WorkingPath, path);
	}

	protected SnapResult TestSnap(string snapFile)
	{
		SnapResult snapResult = new SnapResult();
		try
		{
			if (snapFile == null || !File.Exists(snapFile))
			{
				ProcessHelper.DumpProcessMemoryMetrics();
				snapResult.SnapError();
			}
			else
			{
				snapResult.SnapOk = true;
				snapResult.Path = snapFile;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("CameraService.Snap() caught an exception.", e);
			snapResult.SnapError();
		}
		return snapResult;
	}

	protected void InsertStat(ScanResult result, bool clearImage, double totalSeconds)
	{
		if (BarcodeConfiguration.Instance.InsertReadStats)
		{
			CameraStatistics.InsertCameraStat(result.ReadCount > 0, result.SecureCount, clearImage, (int)Service, totalSeconds);
		}
	}

	protected ScannerAdapter(ScannerServices service)
	{
		Service = service;
	}

	protected abstract void DisposeInner();
}
