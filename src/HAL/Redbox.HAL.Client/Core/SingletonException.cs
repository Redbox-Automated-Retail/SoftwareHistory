using System;

namespace Redbox.HAL.Core;

internal class SingletonException : ApplicationException
{
	public SingletonException(string message)
		: base(message)
	{
	}
}
