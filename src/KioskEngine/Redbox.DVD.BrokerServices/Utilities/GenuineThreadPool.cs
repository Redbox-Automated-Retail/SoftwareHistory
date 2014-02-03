using System;
using System.Collections;
using System.Text;
using System.Threading;
using Belikov.GenuineChannels.Logbook;

namespace Belikov.GenuineChannels.Utilities;

internal class GenuineThreadPool
{
	private static int _threadPoolAvailableThreads = 12;

	private static object _threadPoolAvailableThreadsLock = new object();

	private static int _maximumThreads = 500;

	private static object _maximumThreadsLock = new object();

	private static TimeSpan _closeAfterInactivity = TimeSpan.FromMinutes(4.0);

	private static object _closeAfterInactivityLock = new object();

	private static bool _unsafeQueuing = true;

	private static object _unsafeQueuingLock = new object();

	private static int _maximumRequestsQueued = 500;

	private static bool _allowRequestQueuing = true;

	internal static Queue _requestsQueued = new Queue();

	private static ArrayList _threads = new ArrayList();

	private static object _threadsLock = new object();

	private static GenuineThreadPoolStrategy _genuineThreadPoolStrategy = GenuineThreadPoolStrategy.AlwaysThreads;

	private static object _genuineThreadPoolStrategyLock = new object();

	private static int _QueueUserWorkItem_OperationNumber = 0;

	private static int _threadStatisticFrequency = 50;

	private static object _threadStatisticFrequencyLock = new object();

	public static int ThreadPoolAvailableThreads
	{
		get
		{
			lock (_threadPoolAvailableThreadsLock)
			{
				return _threadPoolAvailableThreads;
			}
		}
		set
		{
			lock (_threadPoolAvailableThreadsLock)
			{
				_threadPoolAvailableThreads = value;
			}
		}
	}

	public static int MaximumThreads
	{
		get
		{
			lock (_maximumThreadsLock)
			{
				return _maximumThreads;
			}
		}
		set
		{
			lock (_maximumThreadsLock)
			{
				_maximumThreads = value;
			}
		}
	}

	public static TimeSpan CloseAfterInactivity
	{
		get
		{
			lock (_closeAfterInactivityLock)
			{
				return _closeAfterInactivity;
			}
		}
		set
		{
			lock (_closeAfterInactivityLock)
			{
				_closeAfterInactivity = value;
			}
		}
	}

	public static bool UnsafeQueuing
	{
		get
		{
			lock (_unsafeQueuingLock)
			{
				return _unsafeQueuing;
			}
		}
		set
		{
			lock (_unsafeQueuingLock)
			{
				_unsafeQueuing = value;
			}
		}
	}

	public static int MaximumRequestsQueued
	{
		get
		{
			lock (_unsafeQueuingLock)
			{
				return _maximumRequestsQueued;
			}
		}
		set
		{
			lock (_unsafeQueuingLock)
			{
				_maximumRequestsQueued = value;
			}
		}
	}

	public static bool AllowRequestQueuing
	{
		get
		{
			lock (_unsafeQueuingLock)
			{
				return _allowRequestQueuing;
			}
		}
		set
		{
			lock (_unsafeQueuingLock)
			{
				_allowRequestQueuing = value;
			}
		}
	}

	public static GenuineThreadPoolStrategy GenuineThreadPoolStrategy
	{
		get
		{
			lock (_genuineThreadPoolStrategyLock)
			{
				return _genuineThreadPoolStrategy;
			}
		}
		set
		{
			lock (_genuineThreadPoolStrategyLock)
			{
				_genuineThreadPoolStrategy = value;
			}
		}
	}

	public static int ThreadStatisticFrequency
	{
		get
		{
			lock (_threadStatisticFrequencyLock)
			{
				return _threadStatisticFrequency;
			}
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			lock (_threadStatisticFrequencyLock)
			{
				_threadStatisticFrequency = value;
			}
		}
	}

	private GenuineThreadPool()
	{
	}

	public static void QueueUserWorkItem(WaitCallback callback, object state, bool longDuration)
	{
		GenuineThreadPoolStrategy genuineThreadPoolStrategy = GenuineThreadPoolStrategy;
		if (callback == null)
		{
			throw new NullReferenceException("callback");
		}
		CustomThreadInfo customThreadInfo = null;
		lock (_threadsLock)
		{
			BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.StatisticCounters] > 0 && Interlocked.Increment(ref _QueueUserWorkItem_OperationNumber) % ThreadStatisticFrequency == 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.StatisticCounters, "GenuineThreadPool.QueueUserWorkItem", LogMessageType.ThreadPoolUsage, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, _QueueUserWorkItem_OperationNumber, _threads.Count, 0, null, null, null, null, "GenuineThreadPool has received a request #{0} (object \"{1}\" entry \"{2}\"). The current number of working threads: {3}. ThreadPoolAvailableThreads: {4}. MaximumThreads: {5}. CloseAfterInactivity: {6}. UnsafeQueuing: {7}. GenuineThreadPoolStrategy: {8}.", _QueueUserWorkItem_OperationNumber, callback.Method.ReflectedType.ToString(), callback.Method.ToString(), _threads.Count, ThreadPoolAvailableThreads, MaximumThreads, CloseAfterInactivity.TotalMilliseconds, UnsafeQueuing, Enum.Format(typeof(GenuineThreadPoolStrategy), GenuineThreadPoolStrategy, "g"));
			}
			if (genuineThreadPoolStrategy != GenuineThreadPoolStrategy.AlwaysThreads && (genuineThreadPoolStrategy != GenuineThreadPoolStrategy.OnlyLongDuration || !longDuration))
			{
				int workerThreads = 0;
				int completionPortThreads = 0;
				if (genuineThreadPoolStrategy == GenuineThreadPoolStrategy.SwitchAfterExhaustion)
				{
					ThreadPool.GetAvailableThreads(out workerThreads, out completionPortThreads);
				}
				if (genuineThreadPoolStrategy == GenuineThreadPoolStrategy.AlwaysNative || genuineThreadPoolStrategy == GenuineThreadPoolStrategy.OnlyLongDuration || (Math.Min(workerThreads, completionPortThreads) >= ThreadPoolAvailableThreads && genuineThreadPoolStrategy == GenuineThreadPoolStrategy.SwitchAfterExhaustion))
				{
					if (UnsafeQueuing)
					{
						ThreadPool.UnsafeQueueUserWorkItem(callback, state);
					}
					else
					{
						ThreadPool.QueueUserWorkItem(callback, state);
					}
					return;
				}
			}
			int num = 0;
			while (num < _threads.Count)
			{
				customThreadInfo = (CustomThreadInfo)_threads[num];
				if (!customThreadInfo.IsAlive)
				{
					_threads.RemoveAt(num);
					customThreadInfo = null;
					continue;
				}
				if (customThreadInfo.IsIdle)
				{
					break;
				}
				customThreadInfo = null;
				num++;
			}
			if (customThreadInfo == null && _threads.Count >= MaximumThreads)
			{
				if (AllowRequestQueuing)
				{
					_requestsQueued.Enqueue(new GenuineThreadPoolWorkItem(callback, state));
					return;
				}
				throw GenuineExceptions.Get_Processing_ThreadPoolLimitExceeded();
			}
			if (customThreadInfo == null)
			{
				customThreadInfo = new CustomThreadInfo(_threadsLock);
				_threads.Add(customThreadInfo);
			}
			customThreadInfo.WorkCallback = callback;
			customThreadInfo.CallbackState = state;
			customThreadInfo.IsIdle = false;
			customThreadInfo.ManualResetEvent.Set();
		}
	}

	internal static GenuineThreadPoolWorkItem GetQueuedRequestIfPossible()
	{
		lock (_threadsLock)
		{
			if (_requestsQueued.Count > 0)
			{
				return (GenuineThreadPoolWorkItem)_requestsQueued.Dequeue();
			}
		}
		return null;
	}

	public static string TakeSnapshot()
	{
		StringBuilder stringBuilder = new StringBuilder();
		lock (_threadsLock)
		{
			for (int i = 0; i < _threads.Count; i++)
			{
				CustomThreadInfo customThreadInfo = (CustomThreadInfo)_threads[i];
				if (customThreadInfo.IsAlive && !customThreadInfo.IsIdle)
				{
					stringBuilder.AppendFormat("Thread {0} executes \"{1}\" entry \"{2}\".{3}", customThreadInfo.Thread.Name, customThreadInfo.WorkCallback.Method.ReflectedType.ToString(), customThreadInfo.WorkCallback.Method.ToString(), Environment.NewLine);
				}
			}
		}
		return stringBuilder.ToString();
	}

	public static int CalculateOccupiedThreads()
	{
		int num = 0;
		lock (_threadsLock)
		{
			for (int i = 0; i < _threads.Count; i++)
			{
				CustomThreadInfo customThreadInfo = (CustomThreadInfo)_threads[i];
				if (!customThreadInfo.IsIdle && customThreadInfo.IsAlive)
				{
					num++;
				}
			}
		}
		return num;
	}
}
