using System;
using System.Threading;

namespace Redbox.HAL.Core;

internal struct WithUpgradeableReadLock : IDisposable
{
	private readonly ReaderWriterLockSlim TheLock;

	private bool Disposed;

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			TheLock.ExitUpgradeableReadLock();
		}
	}

	public WithUpgradeableReadLock(ReaderWriterLockSlim _lock)
	{
		TheLock = _lock;
		Disposed = false;
		TheLock.EnterUpgradeableReadLock();
	}
}
