using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class HomeAxesResult : IDisposable
{
	public readonly ErrorList ErrorList = new ErrorList();

	private bool m_disposed;

	public bool Succeded { get; internal set; }

	public Axis FailedAxis { get; internal set; }

	public void Dispose()
	{
		if (!m_disposed)
		{
			m_disposed = true;
			ErrorList.Clear();
			GC.SuppressFinalize(this);
		}
	}
}
