using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Kernel;

internal static class TimerFunctions
{
	[KernelFunction(Name = "kernel.timerwait")]
	internal static void TimerWait(int milliseconds)
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		service.Wait(milliseconds);
	}

	[KernelFunction(Name = "kernel.timercreate")]
	internal static void CreateTimer(string name, object dueTime, object period, TimerFiredHandler timerHandler)
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		int? dueTime2 = null;
		if (dueTime != null)
		{
			dueTime2 = Convert.ToInt32(dueTime);
		}
		int? period2 = null;
		if (period != null)
		{
			period2 = Convert.ToInt32(period);
		}
		service.CreateTimer(name, dueTime2, period2, delegate
		{
			timerHandler();
		});
	}

	[KernelFunction(Name = "kernel.timerstop")]
	internal static void StopTimer(string name)
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		service.GetTimer(name)?.Stop();
	}

	[KernelFunction(Name = "kernel.timerstart")]
	internal static void StartTimer(string name)
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		service.GetTimer(name)?.Start();
	}

	[KernelFunction(Name = "kernel.timerstartall")]
	internal static void StartAllTimers()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		service.StartAll();
	}

	[KernelFunction(Name = "kernel.timerstopall")]
	internal static void StopAllTimers()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		service.StopAll();
	}

	[KernelFunction(Name = "kernel.timerremove")]
	internal static void RemoveTimer(string name)
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		service.RemoveTimer(name);
	}

	[KernelFunction(Name = "kernel.timerexists")]
	internal static bool TimerExists(string name)
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		return service.GetTimer(name) != null;
	}

	[KernelFunction(Name = "kernel.gettimerperiod")]
	internal static int? GetTimerPeriod(string name)
	{
		int? result = 0;
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		ITimer timer = service.GetTimer(name);
		if (timer != null)
		{
			return timer.Period;
		}
		return result;
	}

	[KernelFunction(Name = "kernel.set_timer_period_and_duetime")]
	internal static void GetTimerPeriod(string timerName, int? period, int? dueTime)
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		ITimer timer = service.GetTimer(timerName);
		if (timer != null)
		{
			timer.Period = period;
			timer.DueTime = dueTime;
		}
	}

	internal static void Reset()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		service.Reset();
	}
}
