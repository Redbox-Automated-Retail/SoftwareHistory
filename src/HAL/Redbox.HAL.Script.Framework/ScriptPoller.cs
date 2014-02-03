using System;
using System.Threading;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal abstract class ScriptPoller
{
	protected readonly ManualResetEvent JobCompletionHandle = new ManualResetEvent(initialState: false);

	protected readonly AutoResetEvent PollEvent = new AutoResetEvent(initialState: false);

	private object m_enabledBarrier = new object();

	private bool m_running;

	internal bool Running
	{
		get
		{
			lock (m_enabledBarrier)
			{
				return m_running;
			}
		}
		set
		{
			lock (m_enabledBarrier)
			{
				m_running = value;
			}
		}
	}

	protected abstract string ThreadName { get; }

	protected abstract string JobName { get; }

	protected abstract string JobLabel { get; }

	protected abstract bool IsConfigured { get; }

	protected abstract ProgramPriority Priority { get; }

	protected Thread PollerThread { get; private set; }

	internal void Start()
	{
		if (IsConfigured)
		{
			CreatePollerThread();
		}
	}

	internal void Shutdown(bool joinThread)
	{
		if (Running)
		{
			Running = false;
			PollEvent.Set();
			if (joinThread && PollerThread != null)
			{
				PollerThread.Join();
			}
			PollerThread = null;
		}
	}

	protected virtual ExecutionContext ScheduleAndBlockOnJob()
	{
		if (!ShouldScheduleJob())
		{
			return null;
		}
		ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob(JobName, JobLabel, DateTime.Now, Priority);
		executionContext.StatusChangeCallback = HandleStatusChange;
		executionContext.Resume();
		JobCompletionHandle.WaitOne();
		executionContext.StatusChangeCallback = null;
		JobCompletionHandle.Reset();
		return executionContext;
	}

	protected virtual void HandleStatusChange(ExecutionContextStatus status)
	{
		if (IsComplete(status))
		{
			JobCompletionHandle.Set();
		}
	}

	protected virtual bool ShouldScheduleJob()
	{
		return true;
	}

	protected bool IsComplete(ExecutionContextStatus status)
	{
		if (status != ExecutionContextStatus.Errored && status != ExecutionContextStatus.Completed && status != ExecutionContextStatus.Garbage)
		{
			return status == ExecutionContextStatus.Stopped;
		}
		return true;
	}

	protected abstract void ExecuteInner();

	private void Execute()
	{
		try
		{
			LogHelper.Instance.Log("Start of the {0} thread.", ThreadName);
			ExecuteInner();
			Running = false;
			LogHelper.Instance.Log("{0} thread exiting.", ThreadName);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"{ThreadName} thread exiting due to exception.", e);
		}
		finally
		{
			Shutdown(joinThread: false);
		}
	}

	private void CreatePollerThread()
	{
		if (!Running)
		{
			Running = true;
			PollerThread = new Thread(Execute)
			{
				Name = ThreadName,
				IsBackground = true
			};
			PollerThread.SetApartmentState(ApartmentState.STA);
			PollerThread.Start();
		}
	}
}
