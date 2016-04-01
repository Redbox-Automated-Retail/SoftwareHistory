using System;
using System.IO;
using System.Runtime.Serialization;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal sealed class ApplicationLog : IFormattedLog
{
	private const int MaxFileSize = 1024000;

	private const int BackDays = 60;

	private const int BackDaysForImages = 168;

	internal readonly ExecutionContext Context;

	private bool m_open;

	private bool Configured;

	private TextWriter Log = StreamWriter.Null;

	private string Directory;

	private string FileName;

	internal bool AppendToExisting { get; private set; }

	internal bool FlushOnWrite { get; private set; }

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

	internal void Serialize(SerializationInfo info)
	{
		info.AddValue("AppHasLog", Configured);
		if (Configured)
		{
			info.AddValue("AppLogDirectory", Directory);
			info.AddValue("AppLogName", FileName);
			info.AddValue("AppLogFlushOnWrite", FlushOnWrite);
			info.AddValue("AppLogAppendToExisting", AppendToExisting);
		}
	}

	internal static ApplicationLog Deserialize(SerializationInfo info, ExecutionContext context)
	{
		ApplicationLog applicationLog = new ApplicationLog(context);
		if (!info.GetBoolean("AppHasLog"))
		{
			return applicationLog;
		}
		applicationLog.Directory = info.GetString("AppLogDirectory");
		applicationLog.FileName = info.GetString("AppLogName");
		applicationLog.AppendToExisting = info.GetBoolean("AppLogAppendToExisting");
		applicationLog.FlushOnWrite = info.GetBoolean("AppLogFlushOnWrite");
		applicationLog.Configured = true;
		return applicationLog;
	}

	internal static ApplicationLog ConfigureLog(ExecutionContext ctx, bool logAppends, string subDirectory, bool flushOnWrite, string overrideName)
	{
		string fileName = ((!logAppends) ? $"{ctx.ProgramName}-{ctx.ID}.log" : $"{(string.IsNullOrEmpty(overrideName) ? ctx.ProgramName : overrideName)}.log");
		if (ControllerConfiguration.Instance.IsVMZMachine && ctx.ProgramName.Equals("qlm-unload"))
		{
			fileName = $"VMZ-Unload-{ctx.ID}.log";
		}
		ctx.AppLog.ConfigureAndOpen(subDirectory, fileName, flushOnWrite, logAppends);
		return ctx.AppLog;
	}

	internal static ApplicationLog ConfigureAdditionalLog(string name, string subDirectory, ExecutionContext context)
	{
		string fileName = $"{name}.log";
		ApplicationLog applicationLog = new ApplicationLog(context);
		applicationLog.ConfigureAndOpen(subDirectory, fileName, flushOnWrite: false, appends: true);
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

	internal void LogCaptureProblem()
	{
		WriteAndFlush("No image was captured.");
	}

	internal void Write(string msg)
	{
		WriteAndFlush(msg);
	}

	internal void Write(string msg, LogEntryType type)
	{
		WriteAndFlush(msg);
		Context.ServiceLog(msg, type);
	}

	internal void Close()
	{
		if (m_open)
		{
			Log.Flush();
			Log.Close();
			m_open = false;
			Log = StreamWriter.Null;
		}
	}

	internal ApplicationLog(ExecutionContext ctx)
	{
		Context = ctx;
	}

	internal bool Open()
	{
		if (!Configured)
		{
			return false;
		}
		if (m_open)
		{
			return true;
		}
		string text = Path.Combine(Directory, FileName);
		try
		{
			Log = new StreamWriter(File.Open(text, FileMode.Append, FileAccess.Write, FileShare.Read));
			return m_open = true;
		}
		catch (Exception)
		{
			LogHelper.Instance.Log($"Unable to open file {text}", LogEntryType.Error);
			Log = StreamWriter.Null;
			return false;
		}
	}

	private void ConfigureAndOpen(string directory, string fileName, bool flushOnWrite, bool appends)
	{
		IFormattedLogFactoryService service = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
		string text = service.LogsBasePath;
		if (ControllerMode.Emulated == ControllerConfiguration.Instance.Mode)
		{
			IRuntimeService service2 = ServiceLocator.Instance.GetService<IRuntimeService>();
			text = service2.RuntimePath("Logs");
		}
		string text2 = text;
		if (!string.IsNullOrEmpty(directory))
		{
			text2 = Path.Combine(text, directory);
			if (!System.IO.Directory.Exists(text2))
			{
				try
				{
					System.IO.Directory.CreateDirectory(text2);
				}
				catch (Exception e)
				{
					LogHelper.Instance.Log("Unable to create subdirectory.", e);
					text2 = text;
				}
			}
		}
		Configured = true;
		Directory = text2;
		FileName = fileName;
		FlushOnWrite = flushOnWrite;
		AppendToExisting = appends;
		string text3 = Path.Combine(Directory, FileName);
		if (AppendToExisting && File.Exists(text3))
		{
			FileInfo fileInfo = new FileInfo(text3);
			if (fileInfo.Length >= 1024000)
			{
				CreateBackup(text3);
			}
		}
		Open();
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

	private void WriteAndFlush(string msg)
	{
		try
		{
			string arg = $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}";
			string value = (AppendToExisting ? $"{arg} {Context.ID} {msg}" : $"{DateTime.Now.ToShortTimeString()} {DateTime.Now.ToShortDateString()} {msg}");
			Log.WriteLine(value);
			if (FlushOnWrite)
			{
				Log.Flush();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("APPLOG caught an exception writing data.", e);
		}
	}
}
