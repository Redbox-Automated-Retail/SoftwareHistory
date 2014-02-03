using System;

namespace Redbox.Lua;

internal class HookExceptionEventArgs : EventArgs
{
	public Exception Exception { get; private set; }

	public HookExceptionEventArgs(Exception ex)
	{
		Exception = ex;
	}
}
