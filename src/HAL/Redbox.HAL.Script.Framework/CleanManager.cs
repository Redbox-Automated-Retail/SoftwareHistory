using System;
using System.Collections.Generic;
using System.Timers;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Script.Framework;

internal sealed class CleanManager : IStatusChangeObserver
{
	private readonly IPersistentMap PersistentMap;

	private readonly object Lock = new object();

	private readonly RedboxTimer CleanTimer;

	private readonly TimeSpan CleanEnd = new TimeSpan(4, 0, 0);

	private TimeSpan? m_startTime = null;

	private string m_scheduledCleanID;

	private DateTime? m_lastCleanDate = null;

	private DateTime? m_lastUserClean = null;

	internal static CleanManager Instance => Singleton<CleanManager>.Instance;

	private DateTime? LastCleanDate
	{
		get
		{
			if (m_lastCleanDate.HasValue)
			{
				return m_lastCleanDate.Value;
			}
			string value = PersistentMap.GetValue("LastVMZClean", "NONE");
			if (value == "NONE")
			{
				return m_lastCleanDate = null;
			}
			try
			{
				m_lastCleanDate = DateTime.Parse(value);
				return m_lastCleanDate;
			}
			catch
			{
				return m_lastCleanDate = null;
			}
		}
		set
		{
			m_lastCleanDate = value;
			PersistentMap.SetValue("LastVMZClean", (!m_lastCleanDate.HasValue) ? "NONE" : m_lastCleanDate.Value.ToString());
		}
	}

	private DateTime? LastUserCleanDate
	{
		get
		{
			if (m_lastUserClean.HasValue)
			{
				return m_lastUserClean.Value;
			}
			string value = PersistentMap.GetValue("LastUserClean", "NONE");
			if (value == "NONE")
			{
				return m_lastUserClean = null;
			}
			try
			{
				m_lastUserClean = DateTime.Parse(value);
				return m_lastUserClean;
			}
			catch
			{
				return m_lastUserClean = null;
			}
		}
		set
		{
			m_lastUserClean = value;
			PersistentMap.SetValue("LastUserClean", (!m_lastUserClean.HasValue) ? "NONE" : m_lastUserClean.Value.ToString());
		}
	}

	private string ScheduledCleanID
	{
		get
		{
			if (m_scheduledCleanID != null)
			{
				return m_scheduledCleanID;
			}
			m_scheduledCleanID = PersistentMap.GetValue("ScheduledCleanID", "<NONE>");
			if (!(m_scheduledCleanID == "<NONE>"))
			{
				return m_scheduledCleanID;
			}
			return m_scheduledCleanID = null;
		}
		set
		{
			m_scheduledCleanID = value;
			PersistentMap.SetValue("ScheduledCleanID", (m_scheduledCleanID == null) ? "<NONE>" : m_scheduledCleanID);
		}
	}

	private TimeSpan? StartTime
	{
		get
		{
			if (m_startTime.HasValue)
			{
				return m_startTime.Value;
			}
			string value = PersistentMap.GetValue("ScheduledCleanStart", "<NONE>");
			if ("<NONE>" == value)
			{
				return null;
			}
			string[] array = value.Split(':');
			int hours = (int.TryParse(array[0], out var result) ? result : 2);
			int minutes = (int.TryParse(array[1], out result) ? result : 0);
			m_startTime = new TimeSpan(hours, minutes, 0);
			return m_startTime.Value;
		}
		set
		{
			m_startTime = value;
			string value2 = "<NONE>";
			if (m_startTime.HasValue)
			{
				value2 = $"{m_startTime.Value.Hours}:{m_startTime.Value.Minutes}";
			}
			PersistentMap.SetValue("ScheduledCleanStart", value2);
		}
	}

	public void StatusChange(ExecutionContextStatus status)
	{
		if (ExecutionContextStatus.Completed == status || ExecutionContextStatus.Errored == status || ExecutionContextStatus.Stopped == status)
		{
			LogHelper.Instance.Log("Clean job {0} ends with status {1}", string.IsNullOrEmpty(ScheduledCleanID) ? "UNKNOWN" : ScheduledCleanID, status.ToString());
			ScheduledCleanID = null;
		}
	}

	internal void RandomizeCleanTime()
	{
		if (ControllerConfiguration.Instance.EnableAutoVMZClean)
		{
			DateTime? nextFireTime = CleanTimer.NextFireTime;
			CleanTimer.Disable();
			StartTime = RandomizeStart();
			ConfigureTimer();
			DateTime? nextFireTime2 = CleanTimer.NextFireTime;
			if (!nextFireTime2.HasValue)
			{
				LogHelper.Instance.Log("RandomizeClean: there is no next fire date.", LogEntryType.Error);
			}
			else if (nextFireTime.HasValue && nextFireTime2.Value.Date == nextFireTime.Value.Date && DateTime.Now.Date == nextFireTime2.Value.Date && nextFireTime2.Value < nextFireTime.Value && nextFireTime2.Value < DateTime.Now)
			{
				ScheduleDefaultClean();
			}
		}
	}

	internal void Register(ExecutionContext ctx)
	{
		if (ControllerConfiguration.Instance.EnableAutoVMZClean && !ctx.IsAutoScheduled)
		{
			LastUserCleanDate = DateTime.Now;
		}
	}

	internal void CleanSlot(ref CleanResult result, ExecutionContext context, SegmentManager manager)
	{
		MerchTransferManager merchTransferManager = new MerchTransferManager(context, ThinHelper.Debug);
		try
		{
			using VMZIterator vMZIterator = new VMZIterator();
			_ = context.AppLog;
			do
			{
				if (vMZIterator.IsEmpty)
				{
					if (!CompressToEmpty(vMZIterator, manager, merchTransferManager, ref result))
					{
						return;
					}
				}
				else if (vMZIterator.Location.IsStuck)
				{
					context.ServiceLog("CleanManager: don't process location {0} stuckFlags {1}", vMZIterator.Location.ToString(), vMZIterator.Location.IsStuck.ToString());
				}
				else if (manager.IsNonThinType(vMZIterator.Flags))
				{
					MerchandizeResult merchandizeResult = manager.MoveNonThin(vMZIterator.Location, vMZIterator.Location, merchTransferManager);
					if (merchandizeResult == MerchandizeResult.Success)
					{
						CompressToEmpty(vMZIterator, manager, merchTransferManager, ref result);
						return;
					}
					if (MerchandizeResult.MachineFull == merchandizeResult)
					{
						result.VisitedAllSlots = true;
						return;
					}
					if (MerchandizeResult.EmptyStuck != merchandizeResult)
					{
						result.Process(merchandizeResult);
						return;
					}
				}
			}
			while (vMZIterator.Up());
			result.VisitedAllSlots = true;
		}
		finally
		{
			result.Accumulate(merchTransferManager);
		}
	}

	internal bool ScheduledCleanIsRunningOutsideWindow()
	{
		if (!ControllerConfiguration.Instance.EnableAutoVMZClean)
		{
			return false;
		}
		return !NowInCleanWindow();
	}

	internal void Init()
	{
		if (!ControllerConfiguration.Instance.IsVMZMachine || !ControllerConfiguration.Instance.EnableAutoVMZClean)
		{
			return;
		}
		try
		{
			ControllerConfiguration.Instance.ConfigurationChangeStart += ConfigurationUpdateStart;
			ControllerConfiguration.Instance.ConfigurationChangeEnd += ConfigurationUpdateFinish;
			ExecutionContext executionContext = FindAnyAutoClean();
			if (executionContext == null)
			{
				ScheduledCleanID = null;
				if (NowInCleanWindow())
				{
					ScheduleDefaultClean();
				}
				return;
			}
			ScheduledCleanID = (executionContext.IsComplete ? null : executionContext.ID);
			if (!LastCleanDate.HasValue && executionContext.ExecutionStart.HasValue)
			{
				LastCleanDate = executionContext.ExecutionStart.Value;
			}
			else if (NowInCleanWindow())
			{
				ScheduleDefaultClean();
			}
		}
		finally
		{
			ConfigureTimer();
		}
	}

	private void ConfigurationUpdateStart()
	{
		LogHelper.Instance.Log("CleanManager.ConfigurationUpdateStart()");
		if (CleanTimer.Started)
		{
			CleanTimer.Disable();
		}
	}

	private bool CompressToEmpty(VMZIterator iterator, SegmentManager manager, MerchTransferManager xferMgr, ref CleanResult result)
	{
		MerchSegment merchSegment = manager.FindContainingOrHigher(iterator);
		if (merchSegment == null)
		{
			if (ThinHelper.Debug)
			{
				LogHelper.Instance.Log("[CleanSlot] Did not find an enclosing segment for location {0}", iterator.Location.ToString());
			}
			return true;
		}
		if (ThinHelper.Debug)
		{
			LogHelper.Instance.Log("[CleanSlot] Found enclosing segment [flags = {0}] for location {1}", merchSegment.Flags.ToString(), iterator.Location.ToString());
		}
		VMZSearchResult result2 = default(VMZSearchResult);
		merchSegment.FindHigh(ref result2);
		if (result2.Location.IsStuck)
		{
			return true;
		}
		MerchandizeResult merchandizeResult = manager.MoveThin(result2.Location, iterator.Location, xferMgr);
		result.Process(merchandizeResult);
		return MerchandizeResult.EmptyStuck == merchandizeResult;
	}

	private void ConfigurationUpdateFinish()
	{
		LogHelper.Instance.Log("CleanManager.ConfigurationUpdateFinish()");
		if (ControllerConfiguration.Instance.EnableAutoVMZClean)
		{
			ConfigureTimer();
		}
	}

	private TimeSpan RandomizeStart()
	{
		int hours = 2;
		int minutes = 0;
		DateTime now = DateTime.Now;
		DateTime dateTime = new DateTime(now.Year, now.Month, now.Day, 1, 30, 0);
		DateTime dateTime2 = new DateTime(now.Year, now.Month, now.Day, 3, 0, 0);
		TimeSpan timeSpan = dateTime2 - dateTime;
		Random random = new Random();
		DateRange dateRange = DateRange.FromDateRange(dateTime, dateTime2);
		for (int i = 0; i < 10; i++)
		{
			TimeSpan timeSpan2 = new TimeSpan(0, random.Next(0, (int)timeSpan.TotalMinutes), 0);
			DateTime value = dateTime + timeSpan2;
			if (dateRange.Includes(value))
			{
				hours = value.Hour;
				minutes = value.Minute;
				break;
			}
		}
		return new TimeSpan(hours, minutes, 0);
	}

	private void ConfigureTimer()
	{
		if (!CleanTimer.Started)
		{
			CleanTimer.ScheduleAtNext(StartTime.Value);
		}
	}

	private void CleanTimerFired(object source, ElapsedEventArgs e)
	{
		LogHelper.Instance.Log("Clean VMZ timer firing.");
		if (ControllerConfiguration.Instance.EnableAutoVMZClean)
		{
			ScheduleDefaultClean();
			CleanTimer.Disable();
			ConfigureTimer();
		}
	}

	private void ScheduleDefaultClean()
	{
		lock (Lock)
		{
			DateTime now = DateTime.Now;
			if ((LastCleanDate.HasValue && !(LastCleanDate.Value.Date != now.Date)) || !NowInCleanWindow())
			{
				return;
			}
			if (LastUserCleanDate.HasValue)
			{
				DateRange dateRange = DateRange.FromDateRange(now.Date, now);
				if (dateRange.Includes(LastUserCleanDate.Value))
				{
					LogHelper.Instance.Log("There was a user clean prior to now; do not schedule an auto-clean.");
					return;
				}
			}
			ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob("clean-vmz", "Automated clean VMZ", DateTime.Now, ProgramPriority.BelowNormal);
			if (executionContext != null && !executionContext.Result.Errors.ContainsError())
			{
				LastCleanDate = DateTime.Now;
				executionContext.AddObserver(this);
				executionContext.IsAutoScheduled = true;
				LogHelper.Instance.Log("Auto-schedule clean VMZ job ( ID = {0} ).", executionContext.ID);
				ScheduledCleanID = executionContext.ID;
				executionContext.Resume();
			}
		}
	}

	private ExecutionContext FindAnyAutoClean()
	{
		string scheduledCleanID = ScheduledCleanID;
		if (!string.IsNullOrEmpty(scheduledCleanID))
		{
			ExecutionContext job = ExecutionEngine.Instance.GetJob(scheduledCleanID);
			if (job != null)
			{
				return job;
			}
		}
		List<ExecutionContext> list = ExecutionEngine.Instance.SafeGetJobList();
		foreach (ExecutionContext item in list)
		{
			if (item.ProgramName == "clean-vmz" && item.IsAutoScheduled)
			{
				return item;
			}
		}
		return null;
	}

	private bool NowInCleanWindow()
	{
		TimeSpan timeOfDay = DateTime.Now.TimeOfDay;
		TimeSpan value = timeOfDay;
		TimeSpan? startTime = StartTime;
		if (value >= startTime)
		{
			return timeOfDay < CleanEnd;
		}
		return false;
	}

	private CleanManager()
	{
		PersistentMap = StoreManager.Instance.GetDataStore();
		CleanTimer = new RedboxTimer("Clean VMZ", CleanTimerFired);
		if (!StartTime.HasValue)
		{
			StartTime = RandomizeStart();
		}
	}
}
