using System.Threading;

namespace Redbox.HAL.Core;

public sealed class AtomicFlag
{
	private const int Flagged = 1;

	private const int Unflagged = 0;

	private int m_state;

	public bool IsFlagged => Thread.VolatileRead(ref m_state) == 1;

	public bool Set()
	{
		return Interlocked.CompareExchange(ref m_state, 1, 0) == 0;
	}

	public bool Clear()
	{
		return Interlocked.CompareExchange(ref m_state, 0, 1) == 1;
	}

	public AtomicFlag(bool initial)
	{
		Interlocked.Exchange(ref m_state, initial ? 1 : 0);
	}
}
