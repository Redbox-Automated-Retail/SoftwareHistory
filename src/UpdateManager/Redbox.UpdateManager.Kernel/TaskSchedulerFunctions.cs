using System;
using System.Security.Cryptography;
using Redbox.Core;
using Redbox.Lua;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Kernel;

public static class TaskSchedulerFunctions
{
	[KernelFunction(Name = "kernel.getrandomnumber")]
	internal static string GetRandomNumber(int min, int max)
	{
		RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
		byte[] array = new byte[4];
		rNGCryptoServiceProvider.GetBytes(array);
		int num = BitConverter.ToInt32(array, 0);
		return (min + Math.Abs(num % (max - min))).ToString();
	}

	[KernelFunction(Name = "kernel.schedulecronjob")]
	internal static LuaTable ScheduleCronJob(string name, string payload, string payloadType, int startOffset, string cronExpression)
	{
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		ErrorList errorList = service.ScheduleCronJob(name, payload, payloadType, (startOffset > 0) ? new TimeSpan?(new TimeSpan(0, 0, startOffset)) : ((TimeSpan?)null), null, null, cronExpression);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.schedulecronscript")]
	internal static LuaTable ScheduleCronScript(string name, string script, int startOffset, string cronExpression)
	{
		return ScheduleCronJob(name, script, "script", startOffset, cronExpression);
	}

	[KernelFunction(Name = "kernel.schedulecronwork")]
	internal static LuaTable ScheduleCronWork(int startOffset, string cronExpression)
	{
		return new LuaTable(KernelService.Instance.LuaRuntime);
	}

	[KernelFunction(Name = "kernel.schedulesimplejob")]
	internal static LuaTable ScheduleSimpleJob(string name, string payload, string payloadTypeString, int startOffset, string repeatIntervalString)
	{
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		if (!TimeSpan.TryParse(repeatIntervalString, out var result))
		{
			ErrorList errorList = new ErrorList();
			errorList.Add(Error.NewError("I001", "Could not parse time interval", "Please use a parsable time interval"));
			return CreateResultTable(errorList);
		}
		ErrorList errorList2 = service.ScheduleSimpleJob(name, payload, payloadTypeString, (startOffset > 0) ? new TimeSpan?(new TimeSpan(0, 0, startOffset)) : ((TimeSpan?)null), null, null, result);
		if (errorList2.ContainsError())
		{
			errorList2.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList2);
	}

	[KernelFunction(Name = "kernel.scheduleonetimescript")]
	internal static LuaTable ScheduleSimpleJob(string name, string script, int startOffset, string timeString)
	{
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		if (!TimeSpan.TryParse(timeString, out var result))
		{
			ErrorList errorList = new ErrorList();
			errorList.Add(Error.NewError("I001", "Could not parse time interval", "Please use a parsable time interval"));
			return CreateResultTable(errorList);
		}
		ErrorList errorList2 = service.ScheduleSimpleJob(name, script, "script", (startOffset > 0) ? new TimeSpan?(new TimeSpan(0, 0, startOffset)) : ((TimeSpan?)null), DateTime.MinValue.Add(result), null, TimeSpan.Zero);
		if (errorList2.ContainsError())
		{
			errorList2.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList2);
	}

	[KernelFunction(Name = "kernel.schedulesimplescript")]
	internal static LuaTable ScheduleSimpleScript(string name, string payload, int startOffset, string repeatIntervalString)
	{
		return ScheduleSimpleJob(name, payload, "script", startOffset, repeatIntervalString);
	}

	[KernelFunction(Name = "kernel.schedulesimplerandomwork")]
	internal static LuaTable ScheduleSimpleRandomWork(int startOffset, string repeatIntervalString)
	{
		return new LuaTable(KernelService.Instance.LuaRuntime);
	}

	[KernelFunction(Name = "kernel.scheduleeventlist")]
	internal static LuaTable List()
	{
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		ErrorList errorList = service.List(out var tasks);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		int num = 1;
		foreach (ITask item in tasks)
		{
			luaTable[num] = $"{item.Name} {item.Payload}";
			num++;
		}
		LuaTable luaTable2 = CreateResultTable(errorList);
		luaTable2["events"] = luaTable;
		return luaTable2;
	}

	[KernelFunction(Name = "kernel.scheduleeventdelete")]
	internal static LuaTable Delete(string name)
	{
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		ErrorList errorList = service.Delete(name);
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.eraseschedule")]
	internal static void Erase()
	{
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		if (service == null)
		{
			return;
		}
		ErrorList errorList = service.Clear();
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
	}

	[KernelFunction(Name = "kernel.eraseschedulenodefault")]
	internal static LuaTable EraseNoDefault()
	{
		ErrorList errorList = new ErrorList();
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		errorList.AddRange(service.List(out var tasks));
		foreach (ITask item in tasks)
		{
			errorList.AddRange(service.Delete(item.Name));
		}
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.schedulerstop")]
	internal static LuaTable Stop()
	{
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		ErrorList errorList = service.Stop();
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.schedulerstart")]
	internal static LuaTable Start()
	{
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		ErrorList errorList = service.Start();
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	[KernelFunction(Name = "kernel.schedulerrestart")]
	internal static LuaTable Restart()
	{
		ITaskScheduler service = ServiceLocator.Instance.GetService<ITaskScheduler>();
		ErrorList errorList = service.Restart();
		if (errorList.ContainsError())
		{
			errorList.ForEach(delegate(Error e)
			{
				LogHelper.Instance.Log("{0} Details: {1}", e, e.Details);
			});
		}
		return CreateResultTable(errorList);
	}

	private static LuaTable CreateResultTable(ErrorList errors)
	{
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		luaTable["success"] = !errors.ContainsError();
		LuaTable luaTable2 = new LuaTable(KernelService.Instance.LuaRuntime);
		foreach (Error error in errors)
		{
			luaTable2[error.Code] = error.Description;
		}
		luaTable["errors"] = luaTable2;
		return luaTable;
	}
}
