using System;
using System.Collections.Generic;
using Redbox.HAL.Core;

namespace Redbox.HAL.Client;

public abstract class JobExecutor : IDisposable
{
	protected readonly ClientHelper Helper;

	protected readonly HardwareService Service;

	private HardwareJobStatus m_endStatus = HardwareJobStatus.Errored;

	private bool m_disposed;

	public IDictionary<string, string> Symbols { get; private set; }

	public List<ProgramResult> Results { get; private set; }

	public ErrorList Errors { get; private set; }

	public HardwareJobStatus EndStatus => m_endStatus;

	public string ID { get; private set; }

	protected abstract string JobName { get; }

	protected virtual string Label => string.Empty;

	protected virtual HardwareJobPriority JobPriority => HardwareJobPriority.Highest;

	public void Dispose()
	{
		if (!m_disposed)
		{
			m_disposed = true;
			DisposeInner();
			Results.Clear();
			Results = null;
			Errors.Clear();
			Errors = null;
		}
	}

	public void Run()
	{
		HardwareJobSchedule hardwareJobSchedule = new HardwareJobSchedule();
		hardwareJobSchedule.StartTime = DateTime.Now;
		hardwareJobSchedule.Priority = JobPriority;
		HardwareJobSchedule schedule = hardwareJobSchedule;
		HardwareJob job = null;
		HardwareCommandResult hardwareCommandResult = Service.ScheduleJob(JobName, Label, enableDebugging: false, schedule, out job);
		if (!hardwareCommandResult.Success)
		{
			Errors.Add(Error.NewError("E785", "Job schedule failure.", "Failed to schedule job."));
			return;
		}
		ID = job.ID;
		SetupJob(job);
		if (!Helper.WaitForJob(job, out m_endStatus))
		{
			Errors.Add(Error.NewError("E786", "Job wait failure.", "Failed to wait for job."));
			return;
		}
		if (!job.GetResults(out var results).Success)
		{
			Errors.Add(Error.NewError("E787", "GetResults failure.", "Failed to get program results. I'm sorry!"));
		}
		else
		{
			Results.AddRange(results);
			results = null;
		}
		if (job.GetErrors(out var errors).Success)
		{
			Errors.AddRange(errors);
			errors.Clear();
		}
		if (job.GetSymbols(out var symbols).Success)
		{
			foreach (string key in symbols.Keys)
			{
				Symbols[key] = symbols[key];
			}
		}
		job.Trash();
	}

	protected virtual void SetupJob(HardwareJob job)
	{
	}

	protected virtual void DisposeInner()
	{
	}

	protected JobExecutor(HardwareService service)
	{
		Service = service;
		Helper = new ClientHelper(service);
		Results = new List<ProgramResult>();
		Errors = new ErrorList();
		Symbols = new Dictionary<string, string>();
	}
}
