using System;
using System.IO;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Script.Framework;

public static class HardwareSupport
{
	private const string LogName = "Timeouts.log";

	internal static readonly string[] SupportCounters = new string[11]
	{
		"FingerCloseTimeout", "FingerOpenTimeout", "FingerRentTimeout", "GripperExtendTimeout", "GripperRetractTimeout", "QlmDisengageTimeout", "QlmEngageTimeout", "TrackCloseTimeout", "TrackOpenTimeout", "VendDoorCloseTimeout",
		"VendDoorRentTimeout"
	};

	public static void ResetCounters()
	{
		ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob("reset-support-counters", string.Empty, DateTime.Now, ProgramPriority.AboveNormal);
		if (executionContext == null || executionContext.Result.Errors.ContainsError())
		{
			LogHelper.Instance.Log("Failed to schedule ResetCounters job.");
			{
				foreach (Error error in executionContext.Result.Errors)
				{
					LogHelper.Instance.Log("{0} {1}", error.Description, error.Details);
				}
				return;
			}
		}
		executionContext.IsAutoScheduled = true;
		executionContext.Resume();
		executionContext.WaitForCompletion();
	}

	internal static void LogCurrent()
	{
		string path = Path.Combine(ControllerConfiguration.Instance.KioskLogsPath, "ErrorLogs");
		string path2 = Path.Combine(path, "Timeouts.log");
		using StreamWriter streamWriter = new StreamWriter(File.Open(path2, FileMode.Append, FileAccess.Write, FileShare.Read));
		streamWriter.WriteLine("Counter sample on {0} {1}", DateTime.Now.ToShortTimeString(), DateTime.Now.ToShortDateString());
		string[] supportCounters = SupportCounters;
		foreach (string text in supportCounters)
		{
			int num = 0;
			Counter counter = CounterManager.Instance.Find(text);
			if (counter != null)
			{
				num = counter.Value;
			}
			streamWriter.WriteLine($" {text}: {num}");
		}
		int num2 = CounterManager.Instance.Find("EmptyOrStuckCount")?.Value ?? 0;
		streamWriter.WriteLine(string.Format(" {0}: {1}", "EmptyOrStuckCount", num2));
		streamWriter.WriteLine();
	}
}
