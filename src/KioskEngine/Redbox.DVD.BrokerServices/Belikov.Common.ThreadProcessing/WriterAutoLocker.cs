using System;
using System.Threading;

namespace Belikov.Common.ThreadProcessing;

internal struct WriterAutoLocker : IDisposable
{
	private bool wasFreed;

	private bool wasHeld;

	private bool upgraded;

	private LockCookie lockCookie;

	private ReaderWriterLock locker;

	public WriterAutoLocker(ReaderWriterLock locker)
	{
		wasFreed = false;
		this.locker = locker;
		wasHeld = true;
		upgraded = false;
		lockCookie = default(LockCookie);
		if (this.locker.IsWriterLockHeld)
		{
			wasHeld = false;
			return;
		}
		if (this.locker.IsReaderLockHeld)
		{
			upgraded = true;
			lockCookie = locker.UpgradeToWriterLock(30000);
		}
		else
		{
			locker.AcquireWriterLock(30000);
		}
		if (locker.IsWriterLockHeld)
		{
			return;
		}
		throw new ApplicationException("Can't acquire writer rights. Server overloaded or deadlock.");
	}

	public void Dispose()
	{
		if (wasFreed)
		{
			return;
		}
		if (wasHeld)
		{
			if (upgraded)
			{
				locker.DowngradeFromWriterLock(ref lockCookie);
			}
			else
			{
				locker.ReleaseWriterLock();
			}
		}
		wasFreed = true;
	}
}
