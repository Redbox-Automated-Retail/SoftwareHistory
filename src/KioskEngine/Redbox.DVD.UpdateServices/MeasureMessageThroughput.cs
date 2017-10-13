using System;
using System.Diagnostics;
using System.Threading;

namespace Redbox.Messaging;

internal class MeasureMessageThroughput
{
	private long m_messageCount;

	private readonly Stopwatch m_stopWatch = new Stopwatch();

	public Redbox.Messaging.MessageThroughput Throughput => new Redbox.Messaging.MessageThroughput(m_messageCount, m_stopWatch.Elapsed);

	public TimeSpan Elapsed => m_stopWatch.Elapsed;

	public void Start()
	{
		m_messageCount = 0L;
		m_stopWatch.Start();
	}

	public void Stop()
	{
		m_stopWatch.Stop();
	}

	public void Increment()
	{
		Increment(1);
	}

	public void Increment(int count)
	{
		Interlocked.Add(ref m_messageCount, count);
	}
}
