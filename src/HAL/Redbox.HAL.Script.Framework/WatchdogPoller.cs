using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[Poller(Name = "Watchdog")]
internal sealed class WatchdogPoller : ScriptPoller
{
	private int m_gcCount;

	protected override int PollSleep => 1800000;

	protected override string ThreadName => "Watchdog Poller";

	protected override string JobName => "job-watchdog";

	protected override string JobLabel => "Watchdog process";

	protected override ProgramPriority Priority => ProgramPriority.Normal;

	protected override bool CoreExecute()
	{
		int num = TrimLogs();
		if (num > 0)
		{
			LogHelper.Instance.Log("Watchdog removed {0} files", num);
		}
		if (ControllerConfiguration.Instance.ResetTimeoutCountersWeekly)
		{
			ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob("reset-counters-job", string.Empty, DateTime.Now, ProgramPriority.Low);
			executionContext.Resume();
			executionContext.WaitForCompletion();
			executionContext.Trash();
		}
		else
		{
			LogHelper.Instance.Log("Watchdog is configured to not reset timeout counters weekly.");
		}
		if (++m_gcCount > 2 && ExecutionEngine.Instance.ActiveContext == null)
		{
			GC.Collect();
			m_gcCount = 0;
		}
		int num2 = CleanErroredJobs();
		if (num2 > 0)
		{
			LogHelper.Instance.Log("Watchdog cleaned {0} errored jobs.", num2);
		}
		num2 = CleanupJobs();
		if (num2 > 0)
		{
			LogHelper.Instance.Log("Watchdog cleaned {0} jobs.", num2);
		}
		return true;
	}

	internal override void Initialize()
	{
		IExecutionService service = ServiceLocator.Instance.GetService<IExecutionService>();
		service.AddObserver(this);
	}

	private WatchdogPoller()
	{
	}

	private int CleanErroredJobs()
	{
		DateTime now = DateTime.Now;
		List<ExecutionContext> list = ExecutionEngine.Instance.SafeGetJobList();
		int num = 0;
		try
		{
			foreach (ExecutionContext item in list)
			{
				if (!(item.Status != ExecutionContextStatus.Errored) && !item.IsImmediate && now >= item.CreatedOn.AddDays(5.0))
				{
					num++;
					item.Trash();
				}
			}
			ExecutionEngine.Instance.CleanupJobs(force: false);
			return num;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Watchdog: clean errored jobs had an exception.", e);
			return 0;
		}
	}

	private int TrimLogs()
	{
		int num = 0;
		DateTime now = DateTime.Now;
		if (now.DayOfWeek != DayOfWeek.Sunday)
		{
			return num;
		}
		if (now.Hour < 2 || now.Hour > 4)
		{
			return num;
		}
		IFormattedLogFactoryService service = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
		string[] directories = Directory.GetDirectories(service.LogsBasePath);
		string[] array = directories;
		foreach (string text in array)
		{
			if (!text.Contains("Service") && !text.Contains("ErrorLogs"))
			{
				num += TrimFilesInSubdirectory(text, "*.log", ControllerConfiguration.Instance.ApplogDayAge);
			}
		}
		return num + TrimFilesInSubdirectory("c:\\Program Files\\Redbox\\HALService\\Video", "*.jpg", ControllerConfiguration.Instance.WatchdogImageDayAge);
	}

	private int TrimFilesInSubdirectory(string directory, string filePattern, int ageInDays)
	{
		int num = 0;
		string[] files = Directory.GetFiles(directory, filePattern);
		string[] array = files;
		foreach (string text in array)
		{
			FileInfo fileInfo = new FileInfo(text);
			if (DateTime.Now.Subtract(fileInfo.LastWriteTime).Days < ageInDays)
			{
				continue;
			}
			try
			{
				if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
				{
					LogHelper.Instance.Log($"Deleting file {text}", LogEntryType.Debug);
				}
				File.Delete(text);
				num++;
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"Unable to delete file {text}", e);
			}
		}
		if (num > 0)
		{
			LogHelper.Instance.Log("AppLog Trimmer: deleted {0} files of type {1} from {2}", num, (filePattern == "*.log") ? "Application Log" : "image", directory);
		}
		return num;
	}

	private bool JobIsIgnored(ExecutionContext context, string[] ignoreList)
	{
		if (ignoreList.Length == 0)
		{
			return false;
		}
		foreach (string text in ignoreList)
		{
			if (text.Equals(context.ProgramName, StringComparison.CurrentCultureIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	private int CleanupJobs()
	{
		string[] ignoreList = new string[8] { "quick-return", "qlm-unload", "thin", "unload-thin", "sync", "sync-locations", "clean-vmz", "thin-vmz" };
		DateTime now = DateTime.Now;
		int num = 1;
		List<ExecutionContext> list = ExecutionEngine.Instance.SafeGetJobList();
		int num2 = 0;
		try
		{
			foreach (ExecutionContext item in list)
			{
				if ((!(item.Status != ExecutionContextStatus.Stopped) || !(item.Status != ExecutionContextStatus.Completed)) && !item.IsImmediate && item.ConnectionState != ConnectionState.Connected && !JobIsIgnored(item, ignoreList) && item.ExecutionStart.HasValue)
				{
					DateTime dateTime = item.ExecutionStart.Value;
					if (item.Result.ExecutionTime.HasValue)
					{
						dateTime = dateTime.Add(item.Result.ExecutionTime.Value);
					}
					else
					{
						num++;
					}
					if (now >= dateTime.AddHours(num))
					{
						num2++;
						item.Trash();
					}
				}
			}
			ExecutionEngine.Instance.CleanupJobs(force: false);
			return num2;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Watchdog: clean jobs had an exception.", e);
			return 0;
		}
	}
}
