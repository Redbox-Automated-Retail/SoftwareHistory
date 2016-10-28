using System;
using System.Runtime.InteropServices;

namespace Redbox.Core;

internal class ThreadExecutionTimer
{
	private long? m_endTimeStamp;

	private long m_startTimeStamp;

	public TimeSpan Elapsed => TimeSpan.FromMilliseconds(((m_endTimeStamp ?? GetThreadTimes()) - m_startTimeStamp) / 10000);

	public bool IsRunning { get; private set; }

	public void Start()
	{
		IsRunning = true;
		m_startTimeStamp = GetThreadTimes();
	}

	public void Stop()
	{
		IsRunning = false;
		m_endTimeStamp = GetThreadTimes();
	}

	public void Reset()
	{
		m_startTimeStamp = 0L;
		m_endTimeStamp = null;
	}

	[DllImport("Kernel32.dll")]
	private static extern long GetThreadTimes(IntPtr threadHandle, out long createionTime, out long exitTime, out long kernelTime, out long userTime);

	[DllImport("Kernel32.dll")]
	private static extern IntPtr GetCurrentThread();

	private static long GetThreadTimes()
	{
		long threadTimes = GetThreadTimes(GetCurrentThread(), out long exitTime, out exitTime, out var kernelTime, out var userTime);
		if (!Convert.ToBoolean(threadTimes))
		{
			throw new Exception($"failed to get timestamp. error code: {threadTimes}");
		}
		return kernelTime + userTime;
	}
}
