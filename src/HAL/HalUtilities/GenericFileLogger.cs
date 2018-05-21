using System;
using System.IO;
using Redbox.HAL.Component.Model;

namespace HALUtilities;

internal sealed class GenericFileLogger : ILogger, IDisposable
{
	private readonly TextWriter m_logFile;

	private readonly string LogFileName;

	private const string LogDirectory = "c:\\Program Files\\Redbox\\CCR_Test";

	private bool Disposed { get; set; }

	public void Log(string message, Exception e)
	{
		WriteToLogFile(message);
		WriteToLogFile(e.Message);
		WriteToLogFile(e.StackTrace);
	}

	public void Log(string message, LogEntryType type)
	{
		WriteToLogFile(message);
	}

	public void Log(string message, Exception e, LogEntryType type)
	{
		WriteToLogFile(message);
		WriteToLogFile(e.Message);
		WriteToLogFile(e.StackTrace);
	}

	public bool IsLevelEnabled(LogEntryType entryLogLevel)
	{
		return true;
	}

	public void Dispose()
	{
		if (Disposed)
		{
			return;
		}
		Disposed = true;
		try
		{
			if (m_logFile != null)
			{
				m_logFile.Flush();
				m_logFile.Close();
			}
		}
		catch
		{
		}
		GC.SuppressFinalize(this);
	}

	internal GenericFileLogger(string fileName)
		: this("c:\\Program Files\\Redbox\\CCR_Test", fileName)
	{
	}

	internal GenericFileLogger(string path, string fileName)
	{
		LogFileName = fileName;
		string path2 = Path.Combine(path, LogFileName);
		try
		{
			m_logFile = new StreamWriter(File.Open(path2, FileMode.Append, FileAccess.Write, FileShare.Read))
			{
				AutoFlush = true
			};
		}
		catch
		{
			m_logFile = StreamWriter.Null;
		}
		ServiceLocator.Instance.AddService(typeof(ILogger), this);
	}

	private void WriteToLogFile(string msg)
	{
		try
		{
			string value = $"{DateTime.Now}: {msg}";
			m_logFile.WriteLine(value);
		}
		catch (Exception)
		{
			Console.WriteLine("Write to log file caught an exception.");
		}
	}
}
