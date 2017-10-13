using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Redbox.HAL.Client;
using Redbox.HAL.Client.Executors;
using Redbox.HAL.Component.Model;

namespace HALUtilities;

internal sealed class QuickReturnHelper
{
	private readonly string EEPROMUpdateLog = "UpdateEEPROMError.log";

	private readonly string QRStatusLogFile = "qr-status.log";

	private readonly HardwareService Service;

	private readonly IRuntimeService RuntimeService;

	internal void DumpCounters(bool reset)
	{
		string path = RuntimeService.RuntimePath("QRstats.log");
		if (File.Exists(path))
		{
			try
			{
				RuntimeService.CreateBackup(path, BackupAction.Move);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to backup statistics file: {0}", ex.Message);
				return;
			}
		}
		new CounterHelper();
		StreamWriter sf = new StreamWriter(path);
		try
		{
			sf.WriteLine("QuickReturn stats of {0}", DateTime.Now.ToShortDateString());
			using (QuickReturnGetCounters quickReturnGetCounters = new QuickReturnGetCounters(Service))
			{
				quickReturnGetCounters.Run();
				quickReturnGetCounters.Results.ForEach(delegate(ProgramResult pr)
				{
					sf.WriteLine("{0}:{1}", pr.Code, pr.Message);
				});
			}
			FindCommunicationErrors(sf);
		}
		finally
		{
			if (sf != null)
			{
				((IDisposable)sf).Dispose();
			}
		}
		if (reset)
		{
			using (QuickReturnResetCounters quickReturnResetCounters = new QuickReturnResetCounters(Service))
			{
				quickReturnResetCounters.Run();
			}
		}
	}

	internal bool ProperlyConfigured()
	{
		List<string> list = new List<string>();
		try
		{
			using (HardwareSurveyExecutor hardwareSurveyExecutor = new HardwareSurveyExecutor(Service))
			{
				hardwareSurveyExecutor.Run();
				bool num = (hardwareSurveyExecutor.QuickReturn & DeviceStatus.Configured) != 0;
				bool flag = (hardwareSurveyExecutor.QuickReturn & DeviceStatus.Found) != 0;
				if (num & flag)
				{
					return true;
				}
				list.Add($"Quick return status is {hardwareSurveyExecutor.QuickReturn.ToString()}");
			}
			LogErrors(QRStatusLogFile, list);
			return list.Count == 0;
		}
		finally
		{
			list.Clear();
		}
	}

	internal void UpdateEEPROM(int waitTime)
	{
		List<string> updateErrors = new List<string>();
		try
		{
			if (Service == null)
			{
				updateErrors.Add("Unable to talk to service.");
			}
			else
			{
				using UpdateQREEPROMJob updateQREEPROMJob = new UpdateQREEPROMJob(Service);
				updateQREEPROMJob.Run();
				if (HardwareJobStatus.Completed != updateQREEPROMJob.EndStatus)
				{
					updateQREEPROMJob.Errors.ForEach(delegate(Error err)
					{
						updateErrors.Add($"[{err.Code}] {err.Description} :\n{err.Details}");
					});
				}
			}
			LogErrors(EEPROMUpdateLog, updateErrors);
		}
		finally
		{
			updateErrors.Clear();
		}
	}

	internal string FormatTime(DateTime t)
	{
		return $"{t.ToShortDateString()} {t.ToShortTimeString()}";
	}

	internal QuickReturnHelper(HardwareService service)
	{
		RuntimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
		Service = service;
	}

	private void LogErrors(string fileName, List<string> errors)
	{
		if (errors.Count <= 0)
		{
			return;
		}
		string path = RuntimeService.RuntimePath(fileName);
		if (File.Exists(path))
		{
			try
			{
				File.Delete(path);
			}
			catch
			{
			}
		}
		StreamWriter log = new StreamWriter(path);
		try
		{
			errors.ForEach(delegate(string err)
			{
				log.WriteLine("{0} {1}", DateTime.Now.ToString(), err);
			});
		}
		finally
		{
			if (log != null)
			{
				((IDisposable)log).Dispose();
			}
		}
	}

	private void FindCommunicationErrors(TextWriter writer)
	{
		Process process = new Process();
		ProcessStartInfo startInfo = process.StartInfo;
		startInfo.FileName = "find.exe";
		startInfo.UseShellExecute = false;
		startInfo.RedirectStandardError = true;
		startInfo.RedirectStandardOutput = true;
		startInfo.WorkingDirectory = "c:\\Program Files\\Redbox\\KioskLogs\\ErrorLogs";
		startInfo.Arguments = "/c \"communicate\" *.*";
		process.Start();
		string arg = process.StandardOutput.ReadToEnd();
		process.WaitForExit();
		writer.WriteLine("Result = {0}", arg);
	}
}
