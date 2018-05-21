using System;
using System.Diagnostics;
using System.IO;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

internal abstract class ScannerAdapter : IDisposable
{
	protected readonly ScannerServices Service;

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
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		string path = service.GenerateUniqueFile("jpg");
		return Path.Combine(BarcodeConfiguration.Instance.WorkingPath, path);
	}

	protected void InsertStat(ScanResult result, bool clearImage, double totalSeconds)
	{
	}

	protected ScannerAdapter(ScannerServices service)
	{
		Service = service;
	}

	protected abstract void DisposeInner();

	private void DumpProcessMemoryMetrics()
	{
		Process currentProcess = Process.GetCurrentProcess();
		LogHelper.Instance.Log(" --- Memory dump --- ");
		LogHelper.Instance.Log("    Working set: {0} bytes", currentProcess.WorkingSet64);
		LogHelper.Instance.Log("    VM size: {0} bytes", currentProcess.VirtualMemorySize64);
		LogHelper.Instance.Log("    Private memory size: {0} bytes", currentProcess.PrivateMemorySize64);
		LogHelper.Instance.Log("    Paged memory size: {0} bytes", currentProcess.PagedMemorySize64);
	}
}
