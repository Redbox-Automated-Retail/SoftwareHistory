using System;
using System.Collections.Generic;
using System.Threading;

namespace Redbox.Core;

internal class Redbox_002ECommand_002ETokenizer2091_002EWorkerPool
{
	internal sealed class WorkItemEntry
	{
		public Redbox_002ECommand_002ETokenizer2091_002EWorkItem Item;

		public object State;

		public void Execute()
		{
			if (Item != null)
			{
				Item(State);
			}
		}
	}

	private int m_runningThreadCount;

	private volatile bool m_isRunning;

	private readonly object m_syncObject = new object();

	private readonly Queue<WorkItemEntry> m_workItems = new Queue<WorkItemEntry>();

	public int MinimumThreads { get; set; }

	public int MaximumThreads { get; set; }

	public Redbox_002ECommand_002ETokenizer2091_002EWorkerPool(int minThreads, int maxThreads)
	{
		MinimumThreads = minThreads;
		MaximumThreads = maxThreads;
	}

	public void Start()
	{
		m_workItems.Clear();
		m_isRunning = true;
		for (int i = 0; i < MaximumThreads; i++)
		{
			Thread thread = new Thread(ItemWorker);
			thread.Name = $"IPC Worker Pool #{i}";
			Thread thread2 = thread;
			thread2.Start();
		}
	}

	public void Shutdown()
	{
		lock (m_syncObject)
		{
			m_isRunning = false;
			Monitor.PulseAll(m_syncObject);
		}
	}

	public bool QueueWorkItem(Redbox_002ECommand_002ETokenizer2091_002EWorkItem item)
	{
		return QueueWorkItem(item, null);
	}

	public bool QueueWorkItem(Redbox_002ECommand_002ETokenizer2091_002EWorkItem item, object state)
	{
		lock (m_syncObject)
		{
			if (!m_isRunning || item == null || m_runningThreadCount + 1 > MaximumThreads)
			{
				return false;
			}
			m_workItems.Enqueue(new WorkItemEntry
			{
				Item = item,
				State = state
			});
			Monitor.PulseAll(m_syncObject);
			return true;
		}
	}

	internal WorkItemEntry GetWorkItem()
	{
		lock (m_syncObject)
		{
			while (m_workItems.Count == 0)
			{
				if (!m_isRunning)
				{
					return null;
				}
				Monitor.Wait(m_syncObject);
			}
			return m_workItems.Dequeue();
		}
	}

	private void ItemWorker()
	{
		while (m_isRunning)
		{
			try
			{
				WorkItemEntry workItem = GetWorkItem();
				if (workItem != null)
				{
					try
					{
						Interlocked.Increment(ref m_runningThreadCount);
						workItem.Execute();
					}
					finally
					{
						Interlocked.Decrement(ref m_runningThreadCount);
					}
				}
			}
			catch (Exception e)
			{
				Redbox_002ECommand_002ETokenizer2091_002ELogHelper.Instance.Log("An unhandled exception was raised in WorkerPool.ItemWorker.", e);
			}
		}
	}
}
