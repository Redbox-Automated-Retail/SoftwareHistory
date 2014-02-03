using System;
using System.Collections.Generic;
using System.Timers;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class CleanManager
{
	private TimeSpan? m_stopTime = null;

	private TimeSpan? m_startTime = null;

	private string m_scheduledCleanID;

	private DateTime? m_lastCleanDate = null;

	internal static CleanManager Instance => Singleton<CleanManager>.Instance;

	private DateTime? LastCleanDate
	{
		get
		{
			if (m_lastCleanDate.HasValue)
			{
				return m_lastCleanDate.Value;
			}
			string value = SecretOptions.Instance.GetValue("LastVMZClean", "NONE");
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
			SecretOptions.Instance.SetValue("LastVMZClean", (!m_lastCleanDate.HasValue) ? "NONE" : m_lastCleanDate.Value.ToString());
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
			m_scheduledCleanID = SecretOptions.Instance.GetValue("ScheduledCleanID", "<NONE>");
			if (!(m_scheduledCleanID == "<NONE>"))
			{
				return m_scheduledCleanID;
			}
			return m_scheduledCleanID = null;
		}
		set
		{
			m_scheduledCleanID = value;
			SecretOptions.Instance.SetValue("ScheduledCleanID", (m_scheduledCleanID == null) ? "<NONE>" : m_scheduledCleanID);
		}
	}

	private TimeSpan CleanEnd
	{
		get
		{
			if (!m_stopTime.HasValue)
			{
				m_stopTime = new TimeSpan(4, 0, 0);
			}
			return m_stopTime.Value;
		}
	}

	private TimeSpan StartTime
	{
		get
		{
			if (!RandomizeCleanStart)
			{
				m_startTime = new TimeSpan(2, 0, 0);
				return m_startTime.Value;
			}
			if (m_startTime.HasValue)
			{
				return m_startTime.Value;
			}
			string value = SecretOptions.Instance.GetValue("ScheduledCleanStart", "<NONE>");
			int num;
			int minutes;
			if ("<NONE>" == value)
			{
				Random random = new Random();
				num = random.Next(1, 3);
				minutes = 0;
				if (num < 11)
				{
					minutes = random.Next(0, 60);
				}
			}
			else
			{
				string[] array = value.Split(':');
				num = (int.TryParse(array[0], out var result) ? result : 2);
				minutes = (int.TryParse(array[1], out result) ? result : 0);
			}
			m_startTime = new TimeSpan(num, minutes, 0);
			return m_startTime.Value;
		}
		set
		{
			if (RandomizeCleanStart)
			{
				m_startTime = value;
				string value2 = "<NONE>";
				if (m_startTime.HasValue)
				{
					value2 = $"{m_startTime.Value.Hours}:{m_startTime.Value.Seconds}";
				}
				SecretOptions.Instance.SetValue("ScheduledCleanStart", value2);
			}
		}
	}

	private RedboxTimer CleanTimer { get; set; }

	private bool RandomizeCleanStart { get; set; }

	internal CleanResult CleanSlot()
	{
		ExecutionContext activeContext = ExecutionEngine.Instance.ActiveContext;
		_ = ThinHelper.Debug;
		CleanResult result = default(CleanResult);
		VMZSearchResult result2 = default(VMZSearchResult);
		VMZSearchResult result3 = default(VMZSearchResult);
		VMZ.Instance.FindLastLocation(MerchFlags.Rebalance, ref result3);
		VMZ.Instance.FindLastLocation(MerchFlags.Thin, ref result2);
		VMZIterator vmzLoc = default(VMZIterator);
		if (!result2.Found && !result3.Found)
		{
			if (ThinHelper.Debug)
			{
				LogHelper.Instance.Log("There are no thin types in the zone; process it all!");
			}
			vmzLoc.ResetToFirst();
			ProcessZone(ref vmzLoc, ref result);
			return result;
		}
		Location location = null;
		if (result3.Found)
		{
			location = VMZ.Instance.FindEmptyInteriorToHighThinType(ref result3);
			if (location != null)
			{
				if (ThinHelper.Debug)
				{
					LogHelper.Instance.Log("CleanSlot: empty location {0} interior to high rebalance {1}; move.", location.ToString(), result3.ToString());
				}
				TransferResult result4 = ControllerService.Instance.Transfer(result3.Location, location, activeContext.AppLog);
				result.Accumulate(result4);
				return result;
			}
			if (ThinHelper.Debug)
			{
				LogHelper.Instance.Log("There are no empties interior to high rebalance.");
			}
			List<Location> list = VMZ.Instance.FindNonThinsInteriorTo(ref result3);
			if (list.Count > 0)
			{
				foreach (Location item in list)
				{
					if (!item.IsStuck)
					{
						SwapNonWithHigh(result3.Location, item, ref result);
						return result;
					}
				}
			}
			if (ThinHelper.Debug)
			{
				LogHelper.Instance.Log("There are no non-thins interior to high rebalance.");
			}
		}
		if (result2.Found)
		{
			location = (result3.Found ? VMZ.Instance.FindEmptyInteriorToHighThinType(ref result2, ref result3) : VMZ.Instance.FindEmptyInteriorToHighThinType(ref result2));
			if (location != null)
			{
				if (ThinHelper.Debug)
				{
					LogHelper.Instance.Log("CleanSlot: empty location {0} interior to high thin {1}; move.", location.ToString(), result2.ToString());
				}
				TransferResult result5 = ControllerService.Instance.Transfer(result2.Location, location, ExecutionEngine.Instance.ActiveContext.AppLog);
				result.Accumulate(result5);
				return result;
			}
			if (ThinHelper.Debug)
			{
				LogHelper.Instance.Log("There are no empties interior to high thin.");
			}
			List<Location> list2 = (result3.Found ? VMZ.Instance.FindNonThinsInteriorTo(ref result2, result3.Location) : VMZ.Instance.FindNonThinsInteriorTo(ref result2));
			if (list2.Count > 0)
			{
				foreach (Location item2 in list2)
				{
					if (!item2.IsStuck)
					{
						SwapNonWithHigh(result2.Location, item2, ref result);
						return result;
					}
				}
			}
			if (ThinHelper.Debug)
			{
				LogHelper.Instance.Log("There are no non-thins interior to high thin.");
			}
		}
		if (ThinHelper.Debug)
		{
			LogHelper.Instance.Log("No interior non-thin or empty; process the rest of the zone.");
		}
		if (result2.Found)
		{
			vmzLoc.ResetTo(ref result2);
		}
		else
		{
			vmzLoc.ResetTo(ref result3);
		}
		ProcessZone(ref vmzLoc, ref result);
		return result;
	}

	internal void ProcessZone(ref VMZIterator vmzLoc, ref CleanResult result)
	{
		_ = ExecutionEngine.Instance.ActiveContext.AppLog;
		while (true)
		{
			if (vmzLoc.IsEmpty)
			{
				if (!vmzLoc.Up())
				{
					result.VisitedAllSlots = true;
					return;
				}
				continue;
			}
			if (!vmzLoc.IsStuck && !vmzLoc.IsThinType)
			{
				break;
			}
			if (vmzLoc.IsStuck)
			{
				ServiceLogHelper.Instance.WriteInfoMessage("ProcessZone: don't process location {0} stuckFlags {1}", vmzLoc.ToString(), vmzLoc.IsStuck.ToString());
			}
			if (!vmzLoc.Up())
			{
				result.VisitedAllSlots = true;
				return;
			}
		}
		List<Location> list = VMZ.Instance.FindEmptyOutsideOf(vmzLoc);
		if (list.Count == 0)
		{
			ServiceLogHelper.Instance.WriteInfoMessage($"ProcessZone: no empty found outside of {vmzLoc.ToString()}");
			result.NoEmptySlots = true;
		}
		else
		{
			TransferResult result2 = ControllerService.Instance.Transfer(vmzLoc.Location, list, ExecutionEngine.Instance.ActiveContext.AppLog);
			result.Accumulate(result2);
		}
	}

	internal void HaltScheduledClean()
	{
		lock (this)
		{
			FindCleanJob()?.Terminate();
		}
	}

	internal bool ScheduledCleanIsRunning()
	{
		lock (this)
		{
			ExecutionContext executionContext = FindCleanJob();
			return executionContext != null && !executionContext.IsComplete;
		}
	}

	internal bool ScheduledCleanIsRunningOutsideWindow()
	{
		return !NowInCleanWindow();
	}

	internal void Init()
	{
		if (ControllerConfiguration.Instance.MerchandizingMode == MerchandizingMode.QLM)
		{
			return;
		}
		ConfigureTimer();
		DateTime now = DateTime.Now;
		if (!NowInCleanWindow())
		{
			return;
		}
		if (!LastCleanDate.HasValue)
		{
			LogHelper.Instance.Log("There is no last clean date.");
			ExecutionContext executionContext = FindCleanJob();
			if (executionContext == null)
			{
				LogHelper.Instance.Log("No running clean; schedule job.");
				ScheduleDefaultClean();
			}
			else if (executionContext.IsComplete)
			{
				if (!executionContext.StartTime.HasValue)
				{
					LogHelper.Instance.Log("I found a completed job without a start time; schedule clean.");
					ScheduleDefaultClean();
				}
				else if (executionContext.StartTime.Value < now)
				{
					LogHelper.Instance.Log("I found a completed job started on {0} at {1}.", executionContext.StartTime.Value.ToShortDateString(), executionContext.StartTime.Value.ToShortTimeString());
					ScheduleDefaultClean();
				}
				else
				{
					LastCleanDate = new DateTime(now.Year, now.Month, now.Day, 2, 0, 0);
					ScheduledCleanID = null;
				}
			}
			else
			{
				LastCleanDate = new DateTime(now.Year, now.Month, now.Day, 2, 0, 0);
				LogHelper.Instance.Log("I found a running clean; attach to it.");
				ScheduledCleanID = executionContext.ID;
				executionContext.StatusChangeCallback = (ExecutionContext.ContextStatusChange)Delegate.Combine(executionContext.StatusChangeCallback, new ExecutionContext.ContextStatusChange(JobDone));
			}
			return;
		}
		LogHelper.Instance.Log("Last clean date: {0} at {1}", LastCleanDate.Value.ToShortDateString(), LastCleanDate.Value.ToShortTimeString());
		if (LastCleanDate.Value.Date == now.Date)
		{
			ExecutionContext executionContext2 = FindCleanJob();
			if (executionContext2 == null || executionContext2.IsComplete)
			{
				ScheduledCleanID = null;
				return;
			}
			LogHelper.Instance.Log("I found running clean today; attach to it.");
			executionContext2.StatusChangeCallback = (ExecutionContext.ContextStatusChange)Delegate.Combine(executionContext2.StatusChangeCallback, new ExecutionContext.ContextStatusChange(JobDone));
			return;
		}
		ExecutionContext executionContext3 = FindCleanJob();
		if (executionContext3 == null)
		{
			LogHelper.Instance.Log("There is no clean running; schedule default.");
			ScheduleDefaultClean();
		}
		else if (executionContext3.IsComplete)
		{
			if (!executionContext3.StartTime.HasValue)
			{
				LogHelper.Instance.Log("I found a completed clean without a start time; schedule clean.");
				ScheduleDefaultClean();
				return;
			}
			DateTime value = executionContext3.StartTime.Value;
			DateTime? lastCleanDate = LastCleanDate;
			if (value < lastCleanDate)
			{
				LogHelper.Instance.Log("I found a completed job started on {0} at {1}; schedule a clean.", executionContext3.StartTime.Value.ToShortDateString(), executionContext3.StartTime.Value.ToShortTimeString());
				ScheduleDefaultClean();
			}
			else
			{
				ScheduledCleanID = null;
			}
		}
		else
		{
			LogHelper.Instance.Log("There is still a running clean; attach to it..");
			ScheduledCleanID = executionContext3.ID;
			executionContext3.StatusChangeCallback = (ExecutionContext.ContextStatusChange)Delegate.Combine(executionContext3.StatusChangeCallback, new ExecutionContext.ContextStatusChange(JobDone));
		}
	}

	private void SwapNonWithHigh(Location highThin, Location nonThin, ref CleanResult result)
	{
		ExecutionContext activeContext = ExecutionEngine.Instance.ActiveContext;
		List<Location> list = VMZ.Instance.FindEmptyOutsideOfCompressedZone();
		if (list.Count == 0)
		{
			if (ThinHelper.Debug)
			{
				LogHelper.Instance.Log("There are no empty slots available.");
			}
			result.NoEmptySlots = true;
			return;
		}
		if (ThinHelper.Debug)
		{
			LogHelper.Instance.Log("SwapNonWithHigh: move {0} out and high {1} in.", nonThin.ToString(), highThin.ToString());
		}
		_ = nonThin.Matrix;
		TransferResult transferResult = ControllerService.Instance.Transfer(nonThin, list, activeContext.AppLog);
		if (!transferResult.Transferred && !transferResult.SourceEmpty)
		{
			result.Accumulate(transferResult);
			return;
		}
		TransferResult result2 = ControllerService.Instance.Transfer(highThin, nonThin, activeContext.AppLog);
		result.Accumulate(result2);
	}

	private void ConfigureTimer()
	{
		if (!CleanTimer.Started)
		{
			CleanTimer.ScheduleAtNext(StartTime);
		}
	}

	private void CleanTimerFired(object source, ElapsedEventArgs e)
	{
		LogHelper.Instance.Log("Clean VMZ timer firing.");
		ScheduleDefaultClean();
		CleanTimer.Disable();
		ConfigureTimer();
	}

	private void ScheduleDefaultClean()
	{
		lock (this)
		{
			DateTime now = DateTime.Now;
			if ((!LastCleanDate.HasValue || LastCleanDate.Value.Date != now.Date) && NowInCleanWindow())
			{
				ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob("clean-vmz", "Automated clean VMZ", DateTime.Now, ProgramPriority.BelowNormal);
				if (executionContext != null && !executionContext.Result.Errors.ContainsError())
				{
					executionContext.StatusChangeCallback = (ExecutionContext.ContextStatusChange)Delegate.Combine(executionContext.StatusChangeCallback, new ExecutionContext.ContextStatusChange(JobDone));
					executionContext.SymbolTable.Add("_CHECK_RUNTIME", true);
					LogHelper.Instance.Log("Auto-schedule clean VMZ job ( ID = {0} ).", executionContext.ID);
					ScheduledCleanID = executionContext.ID;
					executionContext.Resume();
					LastCleanDate = DateTime.Now;
				}
			}
		}
	}

	private void JobDone(ExecutionContextStatus status)
	{
		if (ExecutionContextStatus.Completed == status || ExecutionContextStatus.Errored == status || ExecutionContextStatus.Stopped == status)
		{
			LogHelper.Instance.Log("Clean job {0} ends with status {1}", string.IsNullOrEmpty(ScheduledCleanID) ? "UNKNOWN" : ScheduledCleanID, status.ToString());
			ScheduledCleanID = null;
		}
	}

	private ExecutionContext FindCleanJob()
	{
		string scheduledCleanID = ScheduledCleanID;
		if (string.IsNullOrEmpty(scheduledCleanID))
		{
			return null;
		}
		return ExecutionEngine.Instance.GetJob(scheduledCleanID);
	}

	private bool NowInCleanWindow()
	{
		TimeSpan timeOfDay = DateTime.Now.TimeOfDay;
		if (timeOfDay >= StartTime)
		{
			return timeOfDay < CleanEnd;
		}
		return false;
	}

	private CleanManager()
	{
		CleanTimer = new RedboxTimer("Clean VMZ", CleanTimerFired);
	}
}
