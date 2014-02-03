using System;

namespace Redbox.Core;

internal class SingletonException : ApplicationException
{
	public SingletonException(string message)
		: base(message)
	{
	}
}
