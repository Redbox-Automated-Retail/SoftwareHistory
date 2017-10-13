using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Redbox.Messaging;

internal class MessageThrottle
{
	private const int ThroughputCount = 10;

	private Redbox.Messaging.MeasureMessageThroughput m_total;

	private Redbox.Messaging.MeasureMessageThroughput m_current;

	private readonly object m_lock = new object();

	private readonly Queue<Redbox.Messaging.MeasureMessageThroughput> m_counterList = new Queue<Redbox.Messaging.MeasureMessageThroughput>();

	public Redbox.Messaging.MessageThroughput MaxThroughput { get; set; }

	public Redbox.Messaging.MessageThroughput CurrentThroughput
	{
		get
		{
			Redbox.Messaging.MessageThroughput throughput = new Redbox.Messaging.MessageThroughput();
			m_counterList.ToList().ForEach(delegate(Redbox.Messaging.MeasureMessageThroughput t)
			{
				throughput += t.Throughput;
			});
			return throughput;
		}
	}

	public Redbox.Messaging.MessageThroughput TotalThroughput => m_total.Throughput;

	public MessageThrottle(Redbox.Messaging.MessageThroughput maxThroughput)
	{
		MaxThroughput = maxThroughput;
	}

	public void Start()
	{
		m_counterList.Clear();
		m_total = new Redbox.Messaging.MeasureMessageThroughput();
		m_current = new Redbox.Messaging.MeasureMessageThroughput();
		m_counterList.Enqueue(m_current);
		m_total.Start();
		m_current.Start();
	}

	public void Stop()
	{
		m_current.Stop();
		m_total.Stop();
	}

	public void Increment()
	{
		Increment(1);
	}

	public void Increment(int count)
	{
		lock (m_lock)
		{
			m_total.Increment(count);
			m_current.Increment(count);
			if (m_current.Throughput.Messages >= MaxThroughput.Messages)
			{
				int num = (int)(MaxThroughput.Duration.TotalMilliseconds - m_current.Elapsed.TotalMilliseconds);
				if (num > 0)
				{
					ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
					manualResetEvent.WaitOne(num);
					manualResetEvent.Close();
					m_current.Stop();
				}
				m_current.Stop();
				m_current = new Redbox.Messaging.MeasureMessageThroughput();
				m_current.Start();
				m_counterList.Enqueue(m_current);
				if (m_counterList.Count() > 10)
				{
					m_counterList.Dequeue();
				}
			}
		}
	}
}
