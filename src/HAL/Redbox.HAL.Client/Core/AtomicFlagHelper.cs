using System;

namespace Redbox.HAL.Core;

internal struct AtomicFlagHelper : IDisposable
{
	private bool Disposed;

	private readonly AtomicFlag Flag;

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			Flag.Clear();
		}
	}

	public AtomicFlagHelper(AtomicFlag flag)
	{
		this = default(AtomicFlagHelper);
		Disposed = false;
		Flag = flag;
	}
}
