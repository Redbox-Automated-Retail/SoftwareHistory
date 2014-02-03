using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Quartz;
using Quartz.Impl;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.TaskScheduler;

public class Scheduler : ITaskScheduler
{
	private class PersistTask
	{
		public string Name { get; set; }

		public string ScheduleType { get; set; }

		public string Payload { get; set; }

		public string PayloadType { get; set; }

		public string StartOffset { get; set; }

		public string StartDate { get; set; }

		public string StartTime { get; set; }

		public string EndTime { get; set; }

		public string EndDate { get; set; }

		public string CronExpression { get; set; }

		public string RepeatInterval { get; set; }

		public PersistTask()
		{
		}

		public PersistTask(ITask task)
		{
			Name = task.Name;
			ScheduleType = Enum.GetName(typeof(ScheduleType), task.ScheduleType);
			Payload = task.Payload;
			PayloadType = Enum.GetName(typeof(PayloadType), task.PayloadType);
			StartOffset = (task.StartOffset.HasValue ? task.StartOffset.ToString() : null);
			StartTime = (task.StartTime.HasValue ? task.StartTime.Value.ToLongTimeString() : null);
			StartDate = (task.StartTime.HasValue ? task.StartTime.Value.ToLongDateString() : null);
			EndTime = (task.EndTime.HasValue ? task.EndTime.Value.ToLongTimeString() : null);
			EndDate = (task.EndTime.HasValue ? task.EndTime.Value.ToLongDateString() : null);
			CronExpression = task.CronExpression;
			RepeatInterval = (task.RepeatInterval.HasValue ? task.RepeatInterval.Value.ToString() : null);
		}

		public Task Task()
		{
			Task result = new Task();
			ParsePayload(PayloadType, out var parsed);
			TimeSpan? startOffset = null;
			if (TimeSpan.TryParse(StartOffset, out var result2))
			{
				startOffset = result2;
			}
			DateTime? startTime;
			if (!string.IsNullOrEmpty(StartDate))
			{
				if (!DateTime.TryParse(StartDate, out var result3))
				{
					LogHelper.Instance.Log("Could not parse startDate, skipping task " + Name, LogEntryType.Error);
					return result;
				}
				if (!DateTime.TryParse(StartTime, out var result4))
				{
					LogHelper.Instance.Log("Could not parse startDate, skipping task " + Name, LogEntryType.Error);
					return result;
				}
				startTime = new DateTime(result3.Year, result3.Month, result3.Day, result4.Hour, result4.Minute, result4.Second);
			}
			else
			{
				startTime = null;
			}
			DateTime? endTime;
			if (!string.IsNullOrEmpty(EndDate))
			{
				if (!DateTime.TryParse(EndDate, out var result5))
				{
					LogHelper.Instance.Log("Could not parse endDate, skipping task " + Name, LogEntryType.Error);
					return result;
				}
				if (!DateTime.TryParse(EndTime, out var result6))
				{
					LogHelper.Instance.Log("Could not parse endTime, skipping task " + Name, LogEntryType.Error);
					return result;
				}
				endTime = new DateTime(result5.Year, result5.Month, result5.Day, result6.Hour, result6.Minute, result6.Second);
			}
			else
			{
				endTime = null;
			}
			switch (Enum<Redbox.UpdateManager.ComponentModel.ScheduleType>.ParseIgnoringCase(ScheduleType, Redbox.UpdateManager.ComponentModel.ScheduleType.Unknown))
			{
			case Redbox.UpdateManager.ComponentModel.ScheduleType.Cron:
				result = new Task(Name, Payload, parsed, startOffset, startTime, endTime, CronExpression);
				break;
			case Redbox.UpdateManager.ComponentModel.ScheduleType.Simple:
			{
				if (!TimeSpan.TryParse(RepeatInterval, out var result7))
				{
					LogHelper.Instance.Log("Could not parse RepeatInterval, skipping task " + Name, LogEntryType.Error);
				}
				else
				{
					result = new Task(Name, Payload, parsed, startOffset, startTime, endTime, result7);
				}
				break;
			}
			}
			return result;
		}
	}

	private const string ScheduleLabel = "config-schedule";

	private const string JobGroupname = "updatemanager";

	private const string DefaultScheduleFileName = "default-schedule.lua";

	private static readonly object FileLock = new object();

	private readonly ISchedulerFactory m_schedulerFactory = new StdSchedulerFactory();

	private List<Task> m_tasks = new List<Task>();

	public static Scheduler Instance => Singleton<Scheduler>.Instance;

	public bool IsRunning { get; private set; }

	public ErrorList ForceTask(string name, out bool success)
	{
		ErrorList errorList = new ErrorList();
		success = false;
		try
		{
			Task task = m_tasks.Find((Task t) => t.Name == name);
			if (task != null)
			{
				task.Execute(null);
				success = true;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("E999", "Unhandled exception while forcing task: " + name, e));
		}
		return errorList;
	}

	public ErrorList ScheduleCronJob(string name, string payload, string payloadTypeString, TimeSpan? startOffset, DateTime? startTime, DateTime? endTime, string cronExpression)
	{
		ErrorList errorList = new ErrorList();
		errorList.AddRange(ParsePayload(payloadTypeString, out var parsed));
		if (!errorList.ContainsError())
		{
			errorList.AddRange(ScheduleCronJob(name, payload, parsed, startOffset, startTime, endTime, cronExpression));
		}
		return errorList;
	}

	public ErrorList ScheduleCronJob(string name, string payload, PayloadType payloadType, TimeSpan? startOffset, DateTime? startTime, DateTime? endTime, string cronExpression)
	{
		ErrorList errors = new ErrorList();
		if (!CronExpression.IsValidExpression(cronExpression))
		{
			errors.Add(Error.NewError("E428", "The cron expression is not valid.", $"The expression '{cronExpression}' is not valid, please specify a valid cron expression."));
		}
		else
		{
			errors.AddRange(ReadWriteFile(delegate(List<Task> tasks)
			{
				Task item = new Task(name, payload, payloadType, startOffset, startTime, endTime, cronExpression);
				if (!tasks.Any((Task task) => task.Name == name))
				{
					tasks.Add(item);
				}
				else
				{
					errors.Add(Error.NewError("S489", "Task with same name already scheduled", "Please use a unique name."));
				}
				return tasks;
			}));
		}
		return errors;
	}

	public ErrorList ScheduleSimpleJob(string name, string payload, string payloadTypeString, TimeSpan? startOffset, DateTime? startTime, DateTime? endTime, TimeSpan repeatInterval)
	{
		ErrorList errorList = new ErrorList();
		errorList.AddRange(ParsePayload(payloadTypeString, out var parsed));
		if (!errorList.ContainsError())
		{
			errorList.AddRange(ScheduleSimpleJob(name, payload, parsed, startOffset, startTime, endTime, repeatInterval));
		}
		return errorList;
	}

	public ErrorList ScheduleSimpleJob(string name, string payload, PayloadType payloadType, TimeSpan? startOffset, DateTime? startTime, DateTime? endTime, TimeSpan repeatInterval)
	{
		ErrorList errors = new ErrorList();
		errors.AddRange(ReadWriteFile(delegate(List<Task> tasks)
		{
			Task item = new Task(name, payload, payloadType, startOffset, startTime, endTime, repeatInterval);
			if (!tasks.Any((Task task) => task.Name == name))
			{
				tasks.Add(item);
			}
			else
			{
				errors.Add(Error.NewError("S489", "Task with same name already scheduled", "Please use a unique name."));
			}
			return tasks;
		}));
		return errors;
	}

	public ErrorList Stop()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			LogHelper.Instance.Log("Stopping scheduled events.", LogEntryType.Info);
			IScheduler scheduler = m_schedulerFactory.GetScheduler();
			if (scheduler.IsStarted)
			{
				string[] jobNames = scheduler.GetJobNames("updatemanager");
				LinqExtensions.ForEach(jobNames, delegate(string j)
				{
					scheduler.DeleteJob(j, "updatemanager");
				});
				string[] triggerNames = scheduler.GetTriggerNames("updatemanager");
				LinqExtensions.ForEach(triggerNames, delegate(string t)
				{
					scheduler.UnscheduleJob(t, "updatemanager");
				});
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("E999", "Unhandled exception throw in stop.", e));
		}
		finally
		{
			IsRunning = false;
		}
		return errorList;
	}

	public ErrorList Start()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			if (IsRunning)
			{
				ErrorList errorList2 = new ErrorList();
				errorList2.Add(Error.NewError("E987", "Scheduler is already running.", ""));
				return errorList2;
			}
			IsRunning = true;
			IScheduler scheduler = m_schedulerFactory.GetScheduler();
			if (!scheduler.IsStarted)
			{
				scheduler.Start();
			}
			LogHelper.Instance.Log("Starting scheduled events.", LogEntryType.Info);
			errorList.AddRange(ReadFile(out m_tasks));
			foreach (Task task in m_tasks)
			{
				try
				{
					LogHelper.Instance.Log("Scheduling task: " + task.ToString());
					JobDetail jobDetail = new JobDetail(task.Name, "updatemanager", typeof(Task));
					jobDetail.JobDataMap["Task"] = task;
					DateTime startTimeUtc = (task.StartTime.HasValue ? task.StartTime.Value : DateTime.UtcNow);
					if (task.StartOffset.HasValue)
					{
						startTimeUtc = startTimeUtc.Add(task.StartOffset.Value);
					}
					if (task.ScheduleType == ScheduleType.Cron)
					{
						CronTrigger cronTrigger = new CronTrigger(task.Name, "updatemanager", task.Name, "updatemanager", startTimeUtc, task.EndTime, task.CronExpression);
						cronTrigger.MisfireInstruction = 2;
						CronTrigger trigger = cronTrigger;
						scheduler.ScheduleJob(jobDetail, trigger);
					}
					if (task.ScheduleType == ScheduleType.Simple && task.RepeatInterval.HasValue)
					{
						int repeatCount = ((!(task.RepeatInterval.Value.TotalMilliseconds < 1.0)) ? (-1) : 0);
						SimpleTrigger simpleTrigger = new SimpleTrigger(task.Name, "updatemanager", task.Name, "updatemanager", startTimeUtc, task.EndTime, repeatCount, task.RepeatInterval.Value);
						simpleTrigger.MisfireInstruction = 5;
						SimpleTrigger trigger2 = simpleTrigger;
						scheduler.ScheduleJob(jobDetail, trigger2);
					}
				}
				catch (Exception e)
				{
					errorList.Add(Error.NewError("SCH899", "Unhandled exception throw when scheduling a task in scheduler.start.", e));
				}
			}
		}
		catch (Exception e2)
		{
			errorList.Add(Error.NewError("E929", "Unhandled exception throw in start.", e2));
		}
		return errorList;
	}

	public ErrorList Restart()
	{
		ErrorList errorList = new ErrorList();
		errorList.AddRange(Stop());
		errorList.AddRange(Start());
		return errorList;
	}

	public ErrorList Delete(string name)
	{
		return ReadWriteFile(delegate(List<Task> tasks)
		{
			tasks.RemoveAll((Task task) => task.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
			return tasks;
		});
	}

	public ErrorList List(out List<ITask> outTasks)
	{
		ErrorList result = ReadFile(out var tasks);
		outTasks = tasks.Cast<ITask>().ToList();
		return result;
	}

	public void Initialize()
	{
		ServiceLocator.Instance.AddService(typeof(ITaskScheduler), this);
		lock (FileLock)
		{
			GetRaw();
		}
	}

	ErrorList ITaskScheduler.Clear()
	{
		return Clear();
	}

	public static ErrorList Clear()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
			lock (FileLock)
			{
				service.Delete("config-schedule");
				RunDefaultSchedule();
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("E949", "Unhandled exception when deleting the schedule file.", e));
		}
		return errorList;
	}

	private Scheduler()
	{
	}

	private static string GetRaw()
	{
		IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
		string raw = service.GetRaw("config-schedule");
		if (string.IsNullOrEmpty(raw))
		{
			RunDefaultSchedule();
			raw = service.GetRaw("config-schedule");
		}
		else
		{
			try
			{
				StringExtensions.ToObject<List<PersistTask>>(raw);
			}
			catch (Exception)
			{
				service.Delete("config-schedule");
				RunDefaultSchedule();
				raw = service.GetRaw("config-schedule");
			}
		}
		return raw;
	}

	private static ErrorList ReadFile(out List<Task> tasks)
	{
		ErrorList errorList = new ErrorList();
		tasks = new List<Task>();
		try
		{
			string raw;
			lock (FileLock)
			{
				raw = GetRaw();
			}
			if (!string.IsNullOrEmpty(raw))
			{
				tasks = PersistTasksToTasks(StringExtensions.ToObject<List<PersistTask>>(raw));
			}
		}
		catch (Exception e)
		{
			errorList.Add(Error.NewError("E990", "Unhandled exception when reading the schedule file.", e));
		}
		return errorList;
	}

	private static ErrorList ReadWriteFile(Func<List<Task>, List<Task>> operation)
	{
		try
		{
			IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
			lock (FileLock)
			{
				string raw = GetRaw();
				List<PersistTask> persistTasks = new List<PersistTask>();
				if (!string.IsNullOrEmpty(raw))
				{
					persistTasks = StringExtensions.ToObject<List<PersistTask>>(raw);
				}
				List<Task> arg = PersistTasksToTasks(persistTasks);
				arg = operation(arg);
				persistTasks = TasksToPersistTasks(arg);
				service.Set("config-schedule", persistTasks);
			}
		}
		catch (Exception e)
		{
			ErrorList errorList = new ErrorList();
			errorList.Add(Error.NewError("E909", "Unhandled exception when accessing the schedule file.", e));
			return errorList;
		}
		return new ErrorList();
	}

	private static void RunDefaultSchedule()
	{
		try
		{
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			string text = Path.Combine(service.ExpandProperties("${Scripts}"), "default-schedule.lua");
			IDataStoreService service2 = ServiceLocator.Instance.GetService<IDataStoreService>();
			if (!File.Exists(text))
			{
				LogHelper.Instance.Log("RunDefaultSchedule: Script: {0} not found.", text);
				return;
			}
			service2.Set("config-schedule", new object());
			IKernelService service3 = ServiceLocator.Instance.GetService<IKernelService>();
			string chunk = File.ReadAllText(text);
			LogHelper.Instance.Log("RunDefaultSchedule: Executing lua script at {0}", text);
			service3.ExecuteChunk(chunk);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unknown Error in RunDefaultSchedule", e);
		}
	}

	private static List<Task> PersistTasksToTasks(IEnumerable<PersistTask> persistTasks)
	{
		return persistTasks.Select((PersistTask persistTask) => persistTask.Task()).ToList();
	}

	private static List<PersistTask> TasksToPersistTasks(IEnumerable<Task> tasks)
	{
		return tasks.Select((Task task) => new PersistTask(task)).ToList();
	}

	private static ErrorList ParsePayload(string text, out PayloadType parsed)
	{
		ErrorList errorList = new ErrorList();
		parsed = Enum<PayloadType>.ParseIgnoringCase(text, PayloadType.Unknown);
		if (parsed == PayloadType.Unknown)
		{
			errorList.Add(Error.NewError("P001", "Cannot Parse PayloadType", "Cannot parse PayloadType " + text));
		}
		return errorList;
	}
}
