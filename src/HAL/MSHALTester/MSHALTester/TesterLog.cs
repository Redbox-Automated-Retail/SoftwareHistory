using System;
using System.IO;
using Redbox.HAL.Core;

namespace Redbox.HAL.MSHALTester;

internal class TesterLog : ILogger, IDisposable
{
	private const string LogFileName = "MSHALTester.log";

	private TextWriter m_logFile;

	internal static TesterLog Instance => Singleton<TesterLog>.Instance;

	internal TextWriter CoreLog => m_logFile;

	private TextWriter LogFile
	{
		get
		{
			if (m_logFile == null)
			{
				string path = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "MSHALTester.log");
				m_logFile = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read))
				{
					AutoFlush = true
				};
			}
			return m_logFile;
		}
	}

	private bool Disposed { get; set; }

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

	public void Log(string message, Exception e)
	{
		WriteFormatted(message);
		WriteFormatted(e.Message);
	}

	public void Log(string message, LogEntryType type)
	{
		WriteFormatted(message);
	}

	public void Log(string message, Exception e, LogEntryType type)
	{
		WriteFormatted(message);
		WriteFormatted(e.Message);
	}

	public bool IsLevelEnabled(LogEntryType entryLogLevel)
	{
		return true;
	}

	private void WriteFormatted(string msg)
	{
		LogFile.WriteLine("{0}: {1}", DateTime.Now, msg);
	}

	private TesterLog()
	{
		LogHelper.Instance.Logger = this;
	}
}
