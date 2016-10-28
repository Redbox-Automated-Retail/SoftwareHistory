using System;
using System.Threading;
using Belikov.GenuineChannels.Logbook;

namespace Belikov.GenuineChannels.Utilities;

internal class CustomThreadInfo
{
	private static int _threadOrderNumber = 0;

	public Thread Thread;

	public ManualResetEvent ManualResetEvent = new ManualResetEvent(initialState: false);

	public bool IsIdle = true;

	public bool IsAlive = true;

	public WaitCallback WorkCallback;

	public object CallbackState;

	public object CommonThreadInfoLock;

	public CustomThreadInfo(object commonThreadInfoLock)
	{
		CommonThreadInfoLock = commonThreadInfoLock;
		Thread = new Thread(ThreadEntry);
		Thread.IsBackground = true;
		Thread.Name = "GC.GTP.#" + Interlocked.Increment(ref _threadOrderNumber);
		Thread.Start();
	}

	public void ThreadEntry()
	{
		BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
		try
		{
			while (true)
			{
				if (!ManualResetEvent.WaitOne(GenuineThreadPool.CloseAfterInactivity, exitContext: false))
				{
					lock (CommonThreadInfoLock)
					{
						if (WorkCallback == null)
						{
							IsAlive = false;
							break;
						}
					}
				}
				try
				{
					while (true)
					{
						try
						{
							WorkCallback(CallbackState);
						}
						catch (Exception exception)
						{
							if (binaryLogWriter != null && binaryLogWriter[LogCategory.StatisticCounters] > 0)
							{
								binaryLogWriter.WriteEvent(LogCategory.StatisticCounters, "CustomThreadInfo.ThreadEntry", LogMessageType.Error, exception, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "The execution of the workitem has resulted in the exception.");
							}
						}
						lock (CommonThreadInfoLock)
						{
							GenuineThreadPoolWorkItem queuedRequestIfPossible = GenuineThreadPool.GetQueuedRequestIfPossible();
							if (queuedRequestIfPossible != null)
							{
								WorkCallback = queuedRequestIfPossible.WorkCallback;
								CallbackState = queuedRequestIfPossible.CallbackState;
								continue;
							}
						}
						break;
					}
				}
				finally
				{
					lock (CommonThreadInfoLock)
					{
						ManualResetEvent.Reset();
						WorkCallback = null;
						CallbackState = null;
						IsIdle = true;
					}
				}
			}
		}
		catch (Exception)
		{
		}
		finally
		{
			lock (CommonThreadInfoLock)
			{
				IsAlive = false;
				if (WorkCallback != null)
				{
					GenuineThreadPool.QueueUserWorkItem(WorkCallback, CallbackState, longDuration: true);
					WorkCallback = null;
					CallbackState = null;
				}
			}
		}
	}
}
