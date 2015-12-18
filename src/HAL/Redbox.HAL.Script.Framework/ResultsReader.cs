using System;

namespace Redbox.HAL.Script.Framework;

public struct ResultsReader : IDisposable
{
	private readonly ExecutionContext Context;

	private bool Disposed;

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			Context.ResultsLock.ExitReadLock();
		}
	}

	public ResultsReader(ExecutionContext context)
	{
		Context = context;
		Disposed = false;
		Context.ResultsLock.EnterReadLock();
	}
}
