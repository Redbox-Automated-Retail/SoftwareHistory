using System;
using System.IO;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class ApplicationLog : IFormattedLog
{
	private const int MaxFileSize = 1024000;

	private const int BackDays = 60;

	private const int BackDaysForImages = 168;

	private object m_lock = new object();

	internal string Directory { get; set; }

	internal string FileName { get; set; }

	internal ExecutionContext Context { get; private set; }

	internal bool AppendToExisting { get; set; }

	internal bool FlushOnWrite { get; set; }

	internal bool IsValid
	{
		get
		{
			if (!string.IsNullOrEmpty(Directory))
			{
				return !string.IsNullOrEmpty(FileName);
			}
			return false;
		}
	}

	internal static string LogsDirectory => ControllerConfiguration.Instance.KioskLogsPath;

	private string TimeStamp => $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}";

	private TextWriter Log { get; set; }

	public void WriteFormatted(string msg)
	{
		Write(msg);
	}

	public void WriteFormatted(string msg, LogEntryType type)
	{
		Write(msg, type);
	}

	public void WriteFormatted(string fmt, params object[] parm)
	{
		string msg = string.Format(fmt, parm);
		Write(msg);
	}

	public void WriteFormatted(LogEntryType type, string fmt, params object[] parm)
	{
		string msg = string.Format(fmt, parm);
		Write(msg, type);
	}

	internal static ApplicationLog ConfigureLog(ExecutionContext ctx, bool logAppends, string subDirectory, bool flushOnWrite, string overrideName)
	{
		string fileName = ((!logAppends) ? $"{ctx.ProgramName}-{ctx.ID}.log" : $"{(string.IsNullOrEmpty(overrideName) ? ctx.ProgramName : overrideName)}.log");
		if (MerchandizingMode.VMZ_Gen2 == ControllerConfiguration.Instance.MerchandizingMode && ctx.ProgramName.Equals("qlm-unload"))
		{
			fileName = $"VMZ-Unload-{ctx.ID}.log";
		}
		string text = LogsDirectory;
		if (!string.IsNullOrEmpty(subDirectory))
		{
			text = Path.Combine(LogsDirectory, subDirectory);
			if (!System.IO.Directory.Exists(text))
			{
				try
				{
					System.IO.Directory.CreateDirectory(text);
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("Unable to create subdirectory.", e);
					text = LogsDirectory;
				}
			}
		}
		ApplicationLog applicationLog = ctx.AppLog;
		if (applicationLog == null)
		{
			LogHelper.Instance.Log($"The app {ctx.ProgramName} doesn't have an existing app log object.", LogEntryType.Error);
			ApplicationLog applicationLog2 = (ctx.AppLog = new ApplicationLog(ctx));
			applicationLog = applicationLog2;
		}
		applicationLog.Directory = text;
		applicationLog.FileName = fileName;
		applicationLog.FlushOnWrite = flushOnWrite;
		applicationLog.AppendToExisting = logAppends;
		applicationLog.Context = ctx;
		applicationLog.Open();
		return applicationLog;
	}

	internal static ApplicationLog ConfigureSublog(string name, string subDirectory)
	{
		string fileName = $"{name}.log";
		string text = LogsDirectory;
		if (!string.IsNullOrEmpty(subDirectory))
		{
			text = Path.Combine(LogsDirectory, subDirectory);
			if (!System.IO.Directory.Exists(text))
			{
				try
				{
					System.IO.Directory.CreateDirectory(text);
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("Unable to create subdirectory.", e);
					text = LogsDirectory;
				}
			}
		}
		ApplicationLog applicationLog = new ApplicationLog(ExecutionEngine.Instance.ActiveContext);
		applicationLog.Directory = text;
		applicationLog.FileName = fileName;
		applicationLog.FlushOnWrite = false;
		applicationLog.AppendToExisting = true;
		applicationLog.Open();
		return applicationLog;
	}

	internal void LogMachineFull()
	{
		string msg = "The machine is full.";
		Write(msg, LogEntryType.Error);
	}

	internal void LogLookupFailure(string id)
	{
		string msg = $"LOOKUP of item {id} returned NOT FOUND.";
		Write(msg, LogEntryType.Error);
	}

	internal void LogCaptureProblem(LogEntryType type)
	{
		Write("No image was captured.", type);
	}

	internal void Write(string msg)
	{
		lock (m_lock)
		{
			WriteInner(msg);
		}
	}

	internal void Write(string msg, LogEntryType type)
	{
		lock (m_lock)
		{
			WriteInner(msg);
			WriteToServiceLog(msg, type);
		}
	}

	internal void Close()
	{
		lock (m_lock)
		{
			if (Log != null)
			{
				Log.Flush();
				Log.Close();
				Log = null;
			}
		}
	}

	internal void Flush()
	{
		lock (m_lock)
		{
			if (Log != null)
			{
				Log.Flush();
			}
		}
	}

	internal void Open()
	{
		lock (m_lock)
		{
			if (IsValid)
			{
				OpenInner();
			}
		}
	}

	internal ApplicationLog(ExecutionContext ctx)
	{
		Context = ctx;
	}

	private void WriteToServiceLog(string msg, LogEntryType type)
	{
		ServiceLogHelper.Instance.WriteMessage(msg, type);
	}

	private void OpenInner()
	{
		lock (m_lock)
		{
			if (Log != null)
			{
				LogHelper.Instance.Log($"The application log for job {Context.ID} is open (file name = {FileName})");
				return;
			}
			string text = Path.Combine(Directory, FileName);
			if (AppendToExisting && File.Exists(text))
			{
				FileInfo fileInfo = new FileInfo(text);
				if (fileInfo.Length >= 1024000)
				{
					CreateBackup(text);
				}
			}
			try
			{
				Log = new StreamWriter(File.Open(text, FileMode.Append, FileAccess.Write, FileShare.Read))
				{
					AutoFlush = FlushOnWrite
				};
			}
			catch (Exception)
			{
				LogHelper.Instance.Log($"Unable to open file {text}", LogEntryType.Error);
				Log = null;
			}
		}
	}

	private void CreateBackup(string originalPath)
	{
		DateTime now = DateTime.Now;
		string path = $"{FileName}-m{now.Month}d{now.Day}y{now.Year}h{now.Hour}m{now.Minute}s{now.Second}.log";
		string text = Path.Combine(Directory, path);
		try
		{
			File.Move(originalPath, text);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"Unable to move {originalPath} to {text}.", e);
		}
	}

	private void WriteInner(string msg)
	{
		if (Log == null)
		{
			return;
		}
		try
		{
			string timeStamp = TimeStamp;
			string value = (AppendToExisting ? $"{timeStamp} {Context.ID} {msg}" : $"{DateTime.Now.ToShortTimeString()} {DateTime.Now.ToShortDateString()} {msg}");
			Log.WriteLine(value);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("APPLOG caught an exception writing data.", e);
		}
	}
}
