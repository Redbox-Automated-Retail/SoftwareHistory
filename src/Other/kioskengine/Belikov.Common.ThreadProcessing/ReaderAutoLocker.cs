using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Belikov.Common.ThreadProcessing;

[ComVisible(true)]
public struct ReaderAutoLocker : IDisposable
{
	private bool wasFreed;

	private bool wasHeld;

	private ReaderWriterLock locker;

	public ReaderAutoLocker(ReaderWriterLock locker)
	{
		wasFreed = false;
		this.locker = locker;
		wasHeld = true;
		if (this.locker.IsReaderLockHeld || this.locker.IsWriterLockHeld)
		{
			wasHeld = false;
			return;
		}
		locker.AcquireReaderLock(30000);
		if (locker.IsReaderLockHeld)
		{
			return;
		}
		throw new ApplicationException("Can't acquire reader rights. CPU is overloaded or deadlock.");
	}

	public void Dispose()
	{
		if (!wasFreed)
		{
			if (wasHeld)
			{
				locker.ReleaseReaderLock();
			}
			wasFreed = true;
		}
	}
}
