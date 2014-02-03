using System;
using System.Collections;
using System.Threading;
using Belikov.GenuineChannels.Logbook;

namespace Belikov.GenuineChannels.Utilities;

internal class AsyncThreadStarter
{
	private static ArrayList _workItems = new ArrayList();

	private static Thread _thread;

	private static ManualResetEvent _manualResetEvent = new ManualResetEvent(initialState: false);

	public static void QueueTask(IAsyncWorkItem iAsyncWorkItem)
	{
		lock (_workItems.SyncRoot)
		{
			if (_thread == null)
			{
				_thread = new Thread(ServiceWorkItems);
				_thread.IsBackground = true;
				_thread.Name = "GC.AsyncThreadStarter";
				_thread.Start();
			}
			_workItems.Add(iAsyncWorkItem);
			_manualResetEvent.Set();
		}
	}

	public static void ServiceWorkItems()
	{
		while (true)
		{
			_manualResetEvent.WaitOne();
			lock (_workItems.SyncRoot)
			{
				for (int i = 0; i < _workItems.Count; i++)
				{
					IAsyncWorkItem asyncWorkItem = (IAsyncWorkItem)_workItems[i];
					try
					{
						asyncWorkItem.StartAsynchronousOperation();
					}
					catch (Exception exception)
					{
						GenuineLoggingServices.BinaryLogWriter?.WriteEvent(LogCategory.ImplementationWarning, "AsyncThreadStarter.ServiceWorkItems", LogMessageType.Error, exception, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "Execution of \"{0}\" workitem has resulted in exception.", asyncWorkItem.GetType().FullName);
					}
				}
				_workItems.Clear();
				_manualResetEvent.Reset();
			}
		}
	}
}
