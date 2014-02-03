using System;

namespace Redbox.HAL.Core;

internal class ConsoleLogger : ILogger
{
	public void Log(string message, Exception e)
	{
		Console.WriteLine(message);
		Console.WriteLine(e.Message);
		Console.WriteLine(e.StackTrace);
	}

	public void Log(string message, LogEntryType type)
	{
		Console.WriteLine(message);
	}

	public void Log(string message, Exception e, LogEntryType type)
	{
		Console.WriteLine(message);
		Console.WriteLine(e.Message);
		Console.WriteLine(e.StackTrace);
	}

	public bool IsLevelEnabled(LogEntryType entryLogLevel)
	{
		return true;
	}
}
