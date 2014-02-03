using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public class HomeAxesResult : IDisposable
{
	private bool m_disposed;

	private ErrorList m_errorList = new ErrorList();

	public bool Succeded { get; internal set; }

	public Axis FailedAxis { get; internal set; }

	public ErrorList ErrorList => m_errorList;

	public void Dispose()
	{
		if (!m_disposed)
		{
			m_disposed = true;
			m_errorList.Clear();
			GC.SuppressFinalize(this);
		}
	}
}
