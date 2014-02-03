using System;

namespace Redbox.HAL.Script.Framework;

public class ExecutionEngineException : Exception
{
	public ExecutionEngineException(string message)
		: base(message)
	{
	}

	public ExecutionEngineException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
