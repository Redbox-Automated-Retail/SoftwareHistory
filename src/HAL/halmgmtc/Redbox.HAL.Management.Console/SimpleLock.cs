using System.Threading;

namespace Redbox.HAL.Management.Console;

internal sealed class SimpleLock
{
	private const int Locked = 1;

	private const int Free = 0;

	private int m_lockImpl;

	internal bool TryLock()
	{
		return 1 != Interlocked.CompareExchange(ref m_lockImpl, 1, 0);
	}

	internal void Unlock()
	{
		Interlocked.Exchange(ref m_lockImpl, 0);
	}

	internal SimpleLock()
	{
	}
}
