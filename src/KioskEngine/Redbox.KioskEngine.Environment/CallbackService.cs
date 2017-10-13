using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class CallbackService : ICallbackService
{
	private bool m_isRunning;

	private readonly object m_syncObject = new object();

	private readonly Queue<ICallbackEntry> m_entries = new Queue<ICallbackEntry>();

	public static CallbackService Instance => Singleton<CallbackService>.Instance;

	public void Reset()
	{
		lock (m_syncObject)
		{
			LogHelper.Instance.Log($"Clear callback service queue.  Current queue count: {m_entries.Count}");
			m_entries.Clear();
		}
	}

	public void Flush()
	{
		lock (m_syncObject)
		{
			LogHelper.Instance.Log($"Flush callback service queue. Current queue count: {m_entries.Count}");
			while (InvokeNextCallback())
			{
			}
		}
	}

	public void Resume()
	{
		lock (m_syncObject)
		{
			LogHelper.Instance.Log($"Resume callback service queue.  Current queue count: {m_entries.Count}");
			m_isRunning = true;
		}
	}

	public void Suspend()
	{
		lock (m_syncObject)
		{
			LogHelper.Instance.Log($"Suspend callback service queue.  Current queue count: {m_entries.Count}");
			m_isRunning = false;
		}
	}

	public bool InvokeNextCallback()
	{
		try
		{
			lock (m_syncObject)
			{
				if (!m_isRunning)
				{
					return false;
				}
				if (m_entries.Count == 0)
				{
					return false;
				}
				ICallbackEntry callbackEntry = m_entries.Dequeue();
				callbackEntry.Invoke();
				if (callbackEntry is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was rasied by CallbackService.InvokeNextCallback.", e);
		}
		return true;
	}

	public void EnqueueCallback(ICallbackEntry callbackEntry)
	{
		lock (m_syncObject)
		{
			if (!m_isRunning)
			{
				LogHelper.Instance.Log("Enqueing of callback entry rejected because the Callback queue is suspended.");
			}
			else
			{
				m_entries.Enqueue(callbackEntry);
			}
		}
	}

	private CallbackService()
	{
	}
}
