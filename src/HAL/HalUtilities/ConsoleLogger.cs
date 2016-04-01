using System;
using Redbox.HAL.Component.Model;

namespace HALUtilities;

internal sealed class ConsoleLogger : ILogger
{
	public void Log(string message, Exception e)
	{
		Console.WriteLine(message);
	}

	public void Log(string message, LogEntryType type)
	{
		Console.WriteLine(message);
	}

	public void Log(string message, Exception e, LogEntryType type)
	{
		Console.WriteLine(message);
	}

	public bool IsLevelEnabled(LogEntryType entryLevel)
	{
		return true;
	}

	internal ConsoleLogger()
	{
		ServiceLocator.Instance.AddService(typeof(ILogger), this);
	}
}
