using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class SchedulerFunctions
{
	[KernelFunction(Name = "scheduler_services.start")]
	internal static void Start()
	{
		ServiceLocator.Instance.GetService<ISchedulerService>()?.Start();
	}

	[KernelFunction(Name = "scheduler_services.shutdown")]
	internal static void Shutdown()
	{
		ServiceLocator.Instance.GetService<ISchedulerService>()?.Shutdown();
	}

	[KernelFunction(Name = "scheduler_services.clear")]
	internal static void Clear()
	{
		ServiceLocator.Instance.GetService<ISchedulerService>()?.Clear();
	}

	[KernelFunction(Name = "scheduler_services.remove_entry")]
	internal static bool RemoveScheduleEntry(string jobname)
	{
		return ServiceLocator.Instance.GetService<ISchedulerService>()?.RemoveScheduleEntry(jobname) ?? false;
	}

	[KernelFunction(Name = "scheduler_services.add_entry")]
	internal static void AddScheduleEntry(string jobname, string label, object startTime, object endTime, string cronExpression, string functionName, string programName, string misfireInstruction, object parameters)
	{
		ISchedulerService service = ServiceLocator.Instance.GetService<ISchedulerService>();
		if (service == null)
		{
			return;
		}
		DateTime? dateTime = null;
		DateTime? endTime2 = null;
		if (startTime is LuaTable luaTable)
		{
			dateTime = luaTable.ToDateTime();
		}
		if (endTime is LuaTable luaTable2)
		{
			endTime2 = luaTable2.ToDateTime();
		}
		List<object> list = new List<object>();
		if (parameters != null)
		{
			if (parameters is LuaTable luaTable3)
			{
				foreach (object key in luaTable3.Keys)
				{
					list.Add(luaTable3[key]);
				}
			}
			else
			{
				list.Add(parameters);
			}
		}
		service.AddScheduleEntry(jobname, label, dateTime ?? DateTime.UtcNow, endTime2, cronExpression, functionName, programName, misfireInstruction, list.ToArray());
	}

	[KernelFunction(Name = "scheduler_services.get_entries")]
	internal static LuaTable GetScheduleEntries()
	{
		IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
		if (service == null)
		{
			return null;
		}
		ISchedulerService service2 = ServiceLocator.Instance.GetService<ISchedulerService>();
		if (service2 == null)
		{
			return null;
		}
		int num = 1;
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		foreach (ISchedulerEntry scheduleEntry in service2.GetScheduleEntries())
		{
			new ErrorList();
			LuaTable luaTable2 = CreateResultTable(scheduleEntry.JobName);
			luaTable2["results"] = luaTable;
			LuaTable luaTable3 = new LuaTable(KernelService.Instance.LuaRuntime);
			luaTable3["id"] = scheduleEntry.JobName;
			luaTable3["status"] = "Pending";
			luaTable3["label"] = scheduleEntry.Label;
			luaTable3["program"] = scheduleEntry.Program;
			luaTable3["function"] = scheduleEntry.FunctionName;
			luaTable3["starttime"] = scheduleEntry.CronExpression;
			luaTable3["executiontime"] = string.Empty;
			luaTable3["priority"] = string.Empty;
			luaTable3["misfireinstruction"] = scheduleEntry.MisfireInstruction;
			luaTable[num++] = luaTable3;
		}
		return luaTable;
	}

	[KernelFunction(Name = "scheduler_services.get_entry")]
	internal static LuaTable GetScheduleEntry(string jobname)
	{
		IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
		if (service == null)
		{
			return null;
		}
		ISchedulerService service2 = ServiceLocator.Instance.GetService<ISchedulerService>();
		if (service2 == null)
		{
			return null;
		}
		ISchedulerEntry scheduleEntry = service2.GetScheduleEntry(jobname);
		if (scheduleEntry == null)
		{
			return null;
		}
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		luaTable["id"] = scheduleEntry.JobName;
		luaTable["status"] = "Pending";
		luaTable["label"] = scheduleEntry.Label;
		luaTable["program"] = scheduleEntry.Program;
		luaTable["function"] = scheduleEntry.FunctionName;
		luaTable["starttime"] = scheduleEntry.CronExpression;
		luaTable["executiontime"] = string.Empty;
		luaTable["priority"] = string.Empty;
		luaTable["misfireinstruction"] = scheduleEntry.MisfireInstruction;
		return luaTable;
	}

	private static LuaTable CreateResultTable(string jobname)
	{
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		luaTable["success"] = true;
		luaTable["job_id"] = jobname;
		LuaTable value = new LuaTable(KernelService.Instance.LuaRuntime);
		luaTable["errors"] = value;
		return luaTable;
	}
}
