#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Redbox.Core;

namespace Redbox.Messaging;

internal class ProduceMessages<T> : Redbox.Messaging.IProduceMessages where T : Redbox.Messaging.IMessageProvider, new()
{
	private class Worker
	{
		public Redbox.Messaging.IMessageProducer Producer;

		public AutoResetEvent ResetEvent;

		public Redbox.Messaging.IProviderConnection Connection;
	}

	private long m_messagesSent;

	private bool m_shareConnection = true;

	private int m_messageProducerCount = 10;

	private Redbox.Messaging.MessageThrottle m_messageThrottle;

	private readonly object m_lockobj = new object();

	private readonly T m_messageProvider = new T();

	private readonly List<Worker> m_workerList = new List<Worker>();

	private readonly Queue<Worker> m_workerQueue = new Queue<Worker>();

	private Action<Redbox.Messaging.IMessageProducer, List<Redbox.Messaging.IMessage>> m_afterSendMessage;

	private readonly List<Redbox.Messaging.IProviderConnection> m_connectionList = new List<Redbox.Messaging.IProviderConnection>();

	private Redbox.Messaging.MessageThroughput m_messageThroughput = new Redbox.Messaging.MessageThroughput(100L, new TimeSpan(0, 0, 1));

	public string Name { get; set; }

	public Redbox.Messaging.IMessageProvider MessageProvider => m_messageProvider;

	public int MessageProducerCount
	{
		get
		{
			return m_messageProducerCount;
		}
		set
		{
			m_messageProducerCount = value;
		}
	}

	public Redbox.Messaging.MessageThroughput MessageThroughput
	{
		get
		{
			return m_messageThroughput;
		}
		set
		{
			m_messageThroughput = value;
		}
	}

	public List<Redbox.Messaging.IMessageProducer> ProducerList => m_workerList.Select((Worker w) => w.Producer).ToList();

	public bool Throttle { get; set; }

	public bool Active { get; private set; }

	public bool ShareConnection
	{
		get
		{
			return m_shareConnection;
		}
		set
		{
			m_shareConnection = value;
		}
	}

	public Redbox.Messaging.IConnectionSettings Configuration { get; set; }

	public long MessagesSent
	{
		get
		{
			if (Active)
			{
				long count = 0L;
				ProducerList.ForEach(delegate(Redbox.Messaging.IMessageProducer c)
				{
					count += c.MessagesSent;
				});
				return count;
			}
			return m_messagesSent;
		}
	}

	public Redbox.Messaging.MessagingResult Start(Action<Redbox.Messaging.IMessageProducer, List<Redbox.Messaging.IMessage>> afterSendMesage)
	{
		Debug.Assert(Configuration != null, "ProducerMessages<T>.Start Failed.  Configuration cannot be null.");
		m_afterSendMessage = afterSendMesage;
		StartThrottle();
		m_messagesSent = 0L;
		Redbox.Messaging.MessagingResult messagingResult = CreateMessageProducers();
		if (messagingResult.Success)
		{
			Active = true;
		}
		else
		{
			Redbox.Messaging.MessagingResult messagingResult2 = Stop();
			if (!messagingResult2.Success)
			{
				messagingResult.AddErrorList(messagingResult2.Errors);
			}
		}
		return messagingResult;
	}

	public Redbox.Messaging.MessagingResult Stop()
	{
		Redbox.Messaging.MessagingResult messagingResult = new Redbox.Messaging.MessagingResult();
		Active = false;
		try
		{
			m_workerQueue.Clear();
			m_messagesSent = 0L;
			for (int i = 0; i < m_workerList.Count; i++)
			{
				Worker worker = m_workerList[i];
				if (worker.Producer != null)
				{
					m_messagesSent += worker.Producer.MessagesSent;
					LogHelper.Instance.Log($"Stopped ProduceMessages:{Name} worker: {i} Messages Sent {worker.Producer.MessagesSent}", LogEntryType.Info);
				}
			}
			m_workerList.ForEach(delegate(Worker w)
			{
				if (w.ResetEvent != null)
				{
					w.ResetEvent.Close();
				}
				if (w.Producer != null)
				{
					w.Producer.Dispose();
				}
			});
			m_workerList.Clear();
			m_connectionList.ForEach(delegate(Redbox.Messaging.IProviderConnection connection)
			{
				connection.Dispose();
			});
			m_connectionList.Clear();
			StopThrottle();
			messagingResult.Success = true;
			return messagingResult;
		}
		catch (Exception exception)
		{
			messagingResult.AddError(Redbox.Messaging.Error.NewError("PM0010", "ProduceMessages.Stop failed.", exception));
			return messagingResult;
		}
	}

	public Redbox.Messaging.MessagingResult SendMessage(Redbox.Messaging.IMessage message)
	{
		return SendMessage(new List<Redbox.Messaging.IMessage> { message });
	}

	public Redbox.Messaging.MessagingResult SendMessage(List<Redbox.Messaging.IMessage> messageList)
	{
		Redbox.Messaging.MessagingResult messagingResult = new Redbox.Messaging.MessagingResult();
		Worker worker = null;
		try
		{
			worker = DequeueWorker();
			if (worker == null)
			{
				messagingResult.AddError(Redbox.Messaging.Error.NewError("PM0020", "ProduceMessages.SendMessage: null worker", string.Empty));
				return messagingResult;
			}
			Redbox.Messaging.MessagingResult messagingResult2 = worker.Producer.SendMessageList(messageList);
			if (!messagingResult2.Success)
			{
				return messagingResult2;
			}
			AfterSendMessage(worker, messageList);
			messagingResult.Success = true;
			return messagingResult;
		}
		catch (Exception exception)
		{
			messagingResult.AddError(Redbox.Messaging.Error.NewError("PM0030", "ProduceMessages.SendMessage failed", exception));
			return messagingResult;
		}
		finally
		{
			if (worker != null)
			{
				EnqueueWorker(worker);
			}
		}
	}

	private void StartThrottle()
	{
		if (Throttle)
		{
			m_messageThrottle = new Redbox.Messaging.MessageThrottle(m_messageThroughput);
			m_messageThrottle.Start();
		}
	}

	private void StopThrottle()
	{
		if (Throttle && m_messageThrottle != null)
		{
			m_messageThrottle.Stop();
			m_messageThrottle = null;
		}
	}

	private Redbox.Messaging.MessagingResult CreateMessageProducers()
	{
		Redbox.Messaging.MessagingResult messagingResult = new Redbox.Messaging.MessagingResult();
		try
		{
			for (int i = 0; i < MessageProducerCount; i++)
			{
				Redbox.Messaging.IProviderConnection nextConnection = GetNextConnection();
				Worker worker = new Worker();
				worker.Connection = nextConnection;
				worker.ResetEvent = new AutoResetEvent(initialState: false);
				Worker item = worker;
				m_workerList.Add(item);
				m_workerQueue.Enqueue(item);
			}
			foreach (Redbox.Messaging.IProviderConnection connection in m_connectionList)
			{
				Redbox.Messaging.MessagingResult messagingResult2 = connection.Open();
				if (!messagingResult2.Success)
				{
					return messagingResult2;
				}
			}
			int num = 0;
			foreach (Worker worker2 in m_workerList)
			{
				worker2.Producer = worker2.Connection.CreateProducer();
				Redbox.Messaging.MessagingResult messagingResult3 = worker2.Producer.Open();
				if (!messagingResult3.Success)
				{
					return messagingResult3;
				}
				LogHelper.Instance.Log($"Started ProduceMessages:{Name} worker: {num++}", LogEntryType.Info);
			}
			messagingResult.Success = true;
			return messagingResult;
		}
		catch (Exception exception)
		{
			messagingResult.AddError(Redbox.Messaging.Error.NewError("PM0040", "ProduceMessages.CreateMessageProducers failed.", exception));
			return messagingResult;
		}
	}

	private Redbox.Messaging.IProviderConnection GetNextConnection()
	{
		Redbox.Messaging.IProviderConnection providerConnection;
		if (ShareConnection)
		{
			if (m_connectionList.Count == 0)
			{
				providerConnection = MessageProvider.CreateConnection(Configuration);
				m_connectionList.Add(providerConnection);
				return providerConnection;
			}
			return m_connectionList.First();
		}
		providerConnection = MessageProvider.CreateConnection(Configuration);
		m_connectionList.Add(providerConnection);
		return providerConnection;
	}

	private Worker DequeueWorker()
	{
		Worker worker = null;
		while (Active)
		{
			lock (m_lockobj)
			{
				if (m_workerQueue.Count > 0)
				{
					worker = m_workerQueue.Dequeue();
				}
			}
			if (worker == null)
			{
				AutoResetEvent[] waitHandles = m_workerList.Select((Worker w) => w.ResetEvent).ToArray();
				WaitHandle.WaitAny(waitHandles, 10);
				continue;
			}
			break;
		}
		return worker;
	}

	private void EnqueueWorker(Worker worker)
	{
		lock (m_lockobj)
		{
			m_workerQueue.Enqueue(worker);
		}
		worker.ResetEvent.Set();
	}

	private void AfterSendMessage(Worker worker, List<Redbox.Messaging.IMessage> messages)
	{
		if (Throttle)
		{
			m_messageThrottle.Increment(messages.Count);
		}
		if (m_afterSendMessage != null)
		{
			m_afterSendMessage(worker.Producer, messages);
		}
	}
}
