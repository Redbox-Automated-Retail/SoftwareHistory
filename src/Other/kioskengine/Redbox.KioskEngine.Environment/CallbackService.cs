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
		LogHelper.Instance.Log("Clear callback service queue.");
		lock (m_syncObject)
		{
			m_entries.Clear();
		}
	}

	public void Flush()
	{
		LogHelper.Instance.Log("Flush callback service queue.");
		lock (m_syncObject)
		{
			while (InvokeNextCallback())
			{
			}
		}
	}

	public void Resume()
	{
		LogHelper.Instance.Log("Resume callback service queue.");
		m_isRunning = true;
	}

	public void Suspend()
	{
		LogHelper.Instance.Log("Suspend callback service queue.");
		m_isRunning = false;
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
			m_entries.Enqueue(callbackEntry);
		}
	}

	private CallbackService()
	{
	}
}
