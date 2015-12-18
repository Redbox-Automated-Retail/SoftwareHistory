using System;
using System.Threading;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal abstract class ScriptPoller : IStatusChangeObserver
{
	private const int Polling = 1;

	private const int NotPolling = 0;

	protected readonly ManualResetEvent JobCompletionHandle = new ManualResetEvent(initialState: false);

	protected readonly ManualResetEvent PollEvent = new ManualResetEvent(initialState: false);

	private readonly object SyncLock = new object();

	private int m_pollingState;

	protected abstract string ThreadName { get; }

	protected abstract string JobName { get; }

	protected virtual string JobLabel => string.Empty;

	protected virtual bool IsConfigured => true;

	protected abstract int PollSleep { get; }

	protected abstract ProgramPriority Priority { get; }

	protected Thread PollerThread { get; private set; }

	internal bool Running
	{
		get
		{
			return Thread.VolatileRead(ref m_pollingState) == 1;
		}
		set
		{
			Interlocked.Exchange(ref m_pollingState, value ? 1 : 0);
		}
	}

	public void StatusChange(ExecutionContextStatus status)
	{
		if (IsComplete(status))
		{
			JobCompletionHandle.Set();
		}
	}

	protected virtual ExecutionContext ScheduleAndBlockOnJob()
	{
		ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob(JobName, JobLabel, DateTime.Now, Priority);
		executionContext.AddObserver(this);
		executionContext.Resume();
		JobCompletionHandle.WaitOne();
		executionContext.RemoveObserver(this);
		JobCompletionHandle.Reset();
		return executionContext;
	}

	protected virtual bool OnPollStart()
	{
		return true;
	}

	protected virtual void OnPollDone()
	{
	}

	protected abstract bool CoreExecute();

	protected bool IsComplete(ExecutionContextStatus status)
	{
		if (status != ExecutionContextStatus.Errored && status != ExecutionContextStatus.Completed && status != ExecutionContextStatus.Garbage)
		{
			return status == ExecutionContextStatus.Stopped;
		}
		return true;
	}

	internal virtual void OnConfigChangeStart()
	{
	}

	internal virtual void OnConfigChangeComplete()
	{
	}

	internal virtual void OnEngineModeChange(EngineModes newMode)
	{
	}

	internal virtual void Initialize()
	{
	}

	internal void Start()
	{
		lock (SyncLock)
		{
			if (IsConfigured && !Running)
			{
				PollEvent.Reset();
				JobCompletionHandle.Reset();
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

	internal void Shutdown()
	{
		Shutdown(joinThread: true);
	}

	internal void Shutdown(bool joinThread)
	{
		lock (SyncLock)
		{
			if (IsConfigured && Running)
			{
				Running = false;
				PollEvent.Set();
				if (joinThread && PollerThread != null && !PollerThread.Join(5000))
				{
					LogHelper.Instance.Log("Shutdown: the thread did not join.");
				}
				PollerThread = null;
				LogHelper.Instance.Log("[{0}] is shutdown.", ThreadName);
			}
		}
	}

	private void Execute()
	{
		try
		{
			LogHelper.Instance.Log("Start of the {0} thread.", ThreadName);
			if (OnPollStart())
			{
				while (Running && CoreExecute() && !PollEvent.WaitOne(PollSleep))
				{
				}
				OnPollDone();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"{ThreadName} thread exiting due to exception.", e);
		}
		finally
		{
			LogHelper.Instance.Log("{0} thread exiting.", ThreadName);
			Running = false;
		}
	}
}
