using System;
using System.Collections.Generic;

namespace Redbox.HAL.Core;

public struct DisposeableList<T> : IDisposable
{
	private readonly List<T> TheList;

	private bool m_disposed;

	public void Dispose()
	{
		if (!m_disposed)
		{
			m_disposed = true;
			TheList.Clear();
		}
	}

	public DisposeableList(List<T> list)
	{
		m_disposed = false;
		TheList = list;
	}
}
