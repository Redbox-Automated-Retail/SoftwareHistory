using System.Threading;
using Redbox.HAL.Core;

namespace Redbox.HAL.Management.Console;

internal class MyLock
{
	private const int LockAcquisitionValue = 1;

	private long m_lockImpl;

	internal bool IsLocked => Interlocked.Read(ref m_lockImpl) == 1;

	internal string OwningThread { get; private set; }

	internal int SleepTime { get; set; }

	internal void SpinLock()
	{
		while (1 == LockInner())
		{
			LogHelper.Instance.Log("Thread will wait for lock.", LogEntryType.Debug);
			Thread.Sleep(SleepTime);
		}
		OwningThread = Thread.CurrentThread.Name;
	}

	internal bool TryLock()
	{
		if (1 == LockInner())
		{
			return false;
		}
		OwningThread = Thread.CurrentThread.Name;
		return true;
	}

	internal void Unlock()
	{
		OwningThread = string.Empty;
		Interlocked.Exchange(ref m_lockImpl, 0L);
	}

	internal MyLock()
	{
		SleepTime = 20;
	}

	private long LockInner()
	{
		return Interlocked.CompareExchange(ref m_lockImpl, 1L, 0L);
	}
}
