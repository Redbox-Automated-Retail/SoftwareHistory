using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class WatchdogPoller : ScriptPoller
{
	private const int BackDays = 60;

	private const int BackDaysForImages = 168;

	private DateTime? m_lastReset = null;

	private readonly string[] SupportCounters = new string[12]
	{
		"FingerCloseTimeout", "FingerOpenTimeout", "FingerRentTimeout", "GripperExtendTimeout", "GripperRetractTimeout", "QlmDisengageTimeout", "QlmEngageTimeout", "TrackCloseTimeout", "TrackOpenTimeout", "VendDoorCloseTimeout",
		"VendDoorRentTimeout", "HomeYOnInventoryError"
	};

	internal static WatchdogPoller Instance => Singleton<WatchdogPoller>.Instance;

	protected override string ThreadName => "Job Cleaner";

	protected override string JobName => "job-watchdog";

	protected override string JobLabel => "Watchdog process";

	protected override ProgramPriority Priority => ProgramPriority.Normal;

	protected override bool IsConfigured => true;

	private DateTime? LastCounterReset
	{
		get
		{
			if (m_lastReset.HasValue)
			{
				return m_lastReset.Value;
			}
			string value = SecretOptions.Instance.GetValue("LastCounterReset", "NONE");
			if (value == "NONE")
			{
				return m_lastReset = null;
			}
			try
			{
				m_lastReset = DateTime.Parse(value);
				return m_lastReset;
			}
			catch
			{
				return m_lastReset = null;
			}
		}
		set
		{
			m_lastReset = value;
			SecretOptions.Instance.SetValue("LastCounterReset", (!m_lastReset.HasValue) ? "NONE" : m_lastReset.Value.ToString());
		}
	}

	protected override void ExecuteInner()
	{
		int num = 0;
		while (!PollEvent.WaitOne(1800000))
		{
			int num2 = TrimLogs();
			if (num2 > 0)
			{
				LogHelper.Instance.Log($"Watchdog removed {num2} files", LogEntryType.Info);
			}
			ResetCounters();
			if (++num > 2 && ExecutionEngine.Instance.ActiveContext == null)
			{
				GC.Collect();
				num = 0;
			}
			List<ExecutionContext> list = ExecutionEngine.Instance.SafeGetJobList();
			if (list.Count == 0)
			{
				break;
			}
			int num3 = CleanErroredJobs();
			if (num3 > 0)
			{
				LogHelper.Instance.Log($"Watchdog cleaned {num3} errored jobs.", LogEntryType.Info);
			}
			num3 = CleanupJobs();
			if (num3 > 0)
			{
				LogHelper.Instance.Log($"Watchdog cleaned {num3} jobs.", LogEntryType.Info);
			}
		}
	}

	protected override bool ShouldScheduleJob()
	{
		return false;
	}

	private int CleanErroredJobs()
	{
		DateTime now = DateTime.Now;
		List<ExecutionContext> list = ExecutionEngine.Instance.SafeGetJobList();
		int count = list.Count;
		try
		{
			foreach (ExecutionContext item in list)
			{
				if (!(item.Status != ExecutionContextStatus.Errored) && !item.IsImmediate && now >= item.CreatedOn.AddDays(5.0))
				{
					item.Trash();
				}
			}
			ExecutionEngine.Instance.CleanupJobs(force: false);
			int num = ExecutionEngine.Instance.ContextCount();
			return count - num;
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
		string[] directories = Directory.GetDirectories(ApplicationLog.LogsDirectory);
		string[] array = directories;
		foreach (string text in array)
		{
			if (!text.Contains("Service") && !text.Contains("ErrorLogs"))
			{
				num += TrimFilesInSubdirectory(text, "*.log", 60);
			}
		}
		return num + TrimFilesInSubdirectory("c:\\Program Files\\Redbox\\HALService\\Video", "*.jpg", 168);
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

	private string FormatTime(DateTime t)
	{
		return $"{t.ToShortDateString()} {t.ToShortTimeString()}";
	}

	private int CleanupJobs()
	{
		string[] ignoreList = new string[8] { "quick-return", "qlm-unload", "thin", "unload-thin", "sync", "sync-locations", "clean-vmz", "thin-vmz" };
		DateTime now = DateTime.Now;
		int num = 1;
		List<ExecutionContext> list = ExecutionEngine.Instance.SafeGetJobList();
		int count = list.Count;
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
						item.Trash();
					}
				}
			}
			ExecutionEngine.Instance.CleanupJobs(force: false);
			int num2 = ExecutionEngine.Instance.ContextCount();
			return count - num2;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Watchdog: clean jobs had an exception.", e);
			return 0;
		}
	}

	private void ResetCounters()
	{
		DateTime now = DateTime.Now;
		if ((LastCounterReset.HasValue && !(LastCounterReset.Value.Date != now.Date)) || !DateRange.NowIsBetweenHours(0, 1))
		{
			return;
		}
		LastCounterReset = now;
		LogHelper.Instance.Log("Watchdog resetting counters.");
		CounterManager.Instance.Reset("EmptyOrStuckCount");
		CounterManager.Instance.Reset("ReturnCameraFailures");
		if (now.DayOfWeek == DayOfWeek.Sunday)
		{
			LogHelper.Instance.Log("Reset timeout counters.", LogEntryType.Info);
			string[] supportCounters = SupportCounters;
			foreach (string name in supportCounters)
			{
				CounterManager.Instance.Reset(name);
			}
		}
	}

	private WatchdogPoller()
	{
	}
}
