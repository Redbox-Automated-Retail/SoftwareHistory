using System;
using System.Timers;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class TSMonitor
{
	private class Window
	{
		internal readonly int Start;

		internal readonly int End;

		internal bool Includes(DateTime time)
		{
			if (time.Hour >= Start)
			{
				return time.Hour < End;
			}
			return false;
		}

		internal Window(int start, int end)
		{
			Start = start;
			End = end;
		}
	}

	private readonly Window Morning;

	private readonly Window Late;

	private readonly RedboxTimer StartTimer;

	private readonly RedboxTimer EndTimer;

	internal static TSMonitor Instance => Singleton<TSMonitor>.Instance;

	private bool Enabled { get; set; }

	internal void Start()
	{
		Enabled = ControllerConfiguration.Instance.EnableTouchScreenMonitor;
		if (Enabled)
		{
			DateTime now = DateTime.Now;
			if (Morning.Includes(now))
			{
				EndTimer.ScheduleAtNext(Morning.End, 0);
				Launch();
			}
			else if (Late.Includes(now))
			{
				EndTimer.ScheduleAtNext(Late.End, 0);
				Launch();
			}
			else if (now.Hour < Morning.Start || now.Hour >= Late.End)
			{
				StartTimer.ScheduleAtNext(Morning.Start, 0);
			}
			else
			{
				StartTimer.ScheduleAtNext(Late.Start, 0);
			}
		}
	}

	internal void Stop()
	{
		if (Enabled)
		{
			StartTimer.Disable();
			EndTimer.Disable();
		}
	}

	private void Launch()
	{
		try
		{
			ProcessStarter processStarter = new ProcessStarter("halutilities", "c:\\Program Files\\Redbox\\hal-tools\\halutilities.exe", "-tsMonitor:20");
			processStarter.Run();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Starter caught an exception.", e);
		}
	}

	private void WindowStart(object source, ElapsedEventArgs e)
	{
		StartTimer.Disable();
		LogHelper.Instance.Log("Touch start timer: launch monitor.");
		bool flag = DateTime.Now.Hour == Morning.Start;
		Launch();
		int hour = (flag ? Morning.End : Late.End);
		EndTimer.ScheduleAtNext(hour, 0);
	}

	private void WindowEnd(object source, ElapsedEventArgs e)
	{
		LogHelper.Instance.Log("TSMonitor: window end timer.");
		EndTimer.Disable();
		int hour = ((DateTime.Now.Hour == Late.End) ? Morning.Start : Late.Start);
		StartTimer.ScheduleAtNext(hour, 0);
	}

	private TSMonitor()
	{
		StartTimer = new RedboxTimer("Start TS Monitor", WindowStart);
		EndTimer = new RedboxTimer("End TS Monitor", WindowEnd);
		Morning = new Window(7, 10);
		Late = new Window(15, 23);
	}
}
