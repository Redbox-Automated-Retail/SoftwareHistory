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
		this = default(ResultsReader);
		Context = context;
		Context.ResultsLock.EnterReadLock();
	}
}
