using System;

namespace Redbox.Messaging;

internal class ProduceConsumeConfiguration
{
	private bool m_batchConsume = true;

	private int m_batchCount = 10;

	private TimeSpan m_receiveTimeout = new TimeSpan(0, 0, 0, 0, 10);

	public string Name { get; set; }

	public string ConnectionConfigurationName { get; set; }

	public int InstanceCount { get; set; }

	public bool ShareConnection { get; set; }

	public bool Throttle { get; set; }

	public Redbox.Messaging.MessageThroughput MaxThroughput { get; set; }

	public Redbox.Messaging.ProduceConsumeType Type { get; set; }

	public bool BatchConsume
	{
		get
		{
			return m_batchConsume;
		}
		set
		{
			m_batchConsume = value;
		}
	}

	public int BatchCount
	{
		get
		{
			return m_batchCount;
		}
		set
		{
			m_batchCount = value;
		}
	}

	public TimeSpan ReceiveTimeout
	{
		get
		{
			return m_receiveTimeout;
		}
		set
		{
			m_receiveTimeout = value;
		}
	}
}
