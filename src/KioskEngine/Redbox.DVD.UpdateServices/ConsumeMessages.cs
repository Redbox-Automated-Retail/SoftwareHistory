#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Redbox.Core;

namespace Redbox.Messaging;

internal class ConsumeMessages<T> : Redbox.Messaging.IConsumeMessages where T : Redbox.Messaging.IMessageProvider, new()
{
	private long m_messagesReceived;

	private bool m_shareConnection = true;

	private int m_messageConsumerCount = 10;

	private Redbox.Messaging.MessageThrottle m_messageThrottle;

	private readonly T m_messageProvider = new T();

	private readonly List<Redbox.Messaging.IMessageConsumer> m_consumerList = new List<Redbox.Messaging.IMessageConsumer>();

	private readonly List<Redbox.Messaging.Listener<Redbox.Messaging.IMessageConsumer>> m_listenerList = new List<Redbox.Messaging.Listener<Redbox.Messaging.IMessageConsumer>>();

	private readonly List<Redbox.Messaging.IProviderConnection> m_connectionList = new List<Redbox.Messaging.IProviderConnection>();

	private Redbox.Messaging.MessageThroughput m_messageThroughput = new Redbox.Messaging.MessageThroughput(100L, new TimeSpan(0, 0, 1));

	private bool m_batchConsume = true;

	private int m_batchCount = 10;

	private TimeSpan m_receiveTimeout = new TimeSpan(0, 0, 0, 0, 10);

	public string Name { get; set; }

	public Redbox.Messaging.IMessageProvider MessageProvider => m_messageProvider;

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

	public List<Redbox.Messaging.IMessageConsumer> ConsumerList => m_consumerList;

	public int MessageConsumerCount
	{
		get
		{
			return m_messageConsumerCount;
		}
		set
		{
			m_messageConsumerCount = value;
		}
	}

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

	public long MessagesReceived
	{
		get
		{
			if (Active)
			{
				long count = 0L;
				if (m_consumerList != null)
				{
					m_consumerList.ForEach(delegate(Redbox.Messaging.IMessageConsumer c)
					{
						count += c.MessagesReceived;
					});
				}
				return count;
			}
			return m_messagesReceived;
		}
	}

	public Action<Redbox.Messaging.MessagingResult, Redbox.Messaging.IConsumeMessages> ListenFailed { get; set; }

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

	public Redbox.Messaging.MessagingResult Start(Action<Redbox.Messaging.IMessageConsumer, List<Redbox.Messaging.IMessage>> receiveMessage, Action<Redbox.Messaging.IMessageConsumer, List<Redbox.Messaging.IMessage>> afterReceiveMessage)
	{
		Debug.Assert(Configuration != null, "ConsumeMessages<T>.Start, Configuration cannot be null.");
		Redbox.Messaging.MessagingResult result = new Redbox.Messaging.MessagingResult();
		Action<Redbox.Messaging.MessagingResult> action = delegate(Redbox.Messaging.MessagingResult mr)
		{
			result.AddErrorList(mr.Errors);
			Redbox.Messaging.MessagingResult messagingResult3 = Stop();
			if (!messagingResult3.Success)
			{
				result.AddErrorList(messagingResult3.Errors);
			}
		};
		StartThrottle();
		m_messagesReceived = 0L;
		Redbox.Messaging.MessagingResult messagingResult = CreateMessageConsumers(receiveMessage, afterReceiveMessage);
		if (!messagingResult.Success)
		{
			action(messagingResult);
			return result;
		}
		int num = 0;
		foreach (Redbox.Messaging.Listener<Redbox.Messaging.IMessageConsumer> listener in m_listenerList)
		{
			Redbox.Messaging.MessagingResult messagingResult2 = listener.Start();
			if (!messagingResult2.Success)
			{
				action(messagingResult2);
				return result;
			}
			LogHelper.Instance.Log($"Started ConsumeMessages: {Name} worker: {num++}", LogEntryType.Info);
		}
		Active = true;
		result.Success = true;
		return result;
	}

	public Redbox.Messaging.MessagingResult Stop()
	{
		Redbox.Messaging.MessagingResult result = new Redbox.Messaging.MessagingResult();
		Active = false;
		try
		{
			m_listenerList.ForEach(delegate(Redbox.Messaging.Listener<Redbox.Messaging.IMessageConsumer> l)
			{
				Redbox.Messaging.MessagingResult messagingResult = l.Stop(wait: false);
				if (!messagingResult.Success)
				{
					result.AddErrorList(messagingResult.Errors);
				}
			});
			m_listenerList.ForEach(delegate(Redbox.Messaging.Listener<Redbox.Messaging.IMessageConsumer> l)
			{
				Redbox.Messaging.MessagingResult messagingResult = l.Stop();
				if (!messagingResult.Success)
				{
					result.AddErrorList(messagingResult.Errors);
				}
			});
			m_messagesReceived = 0L;
			int each = 0;
			ConsumerList.ForEach(delegate(Redbox.Messaging.IMessageConsumer consumer)
			{
				m_messagesReceived += consumer.MessagesReceived;
				LogHelper.Instance.Log($"Stopped ConsumeMessages:{Name} worker: {each++} Messages Consumed {consumer.MessagesReceived}", LogEntryType.Info);
			});
			ConsumerList.ForEach(delegate(Redbox.Messaging.IMessageConsumer c)
			{
				c.Dispose();
			});
			ConsumerList.Clear();
			m_listenerList.Clear();
			m_connectionList.ForEach(delegate(Redbox.Messaging.IProviderConnection connection)
			{
				connection.Close();
			});
			m_connectionList.Clear();
			StopThrottle();
			result.Success = result.Errors.Count == 0;
			return result;
		}
		catch (Exception exception)
		{
			result.AddError(Redbox.Messaging.Error.NewError("CM0010", "ConsumeMessages.Stop failed.", exception));
			return result;
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

	private void GetNextConnection()
	{
		if (ShareConnection)
		{
			if (m_connectionList.Count == 0)
			{
				Redbox.Messaging.IProviderConnection item = MessageProvider.CreateConnection(Configuration);
				m_connectionList.Add(item);
			}
		}
		else
		{
			Redbox.Messaging.IProviderConnection item = MessageProvider.CreateConnection(Configuration);
			m_connectionList.Add(item);
		}
	}

	private Redbox.Messaging.MessagingResult CreateMessageConsumers(Action<Redbox.Messaging.IMessageConsumer, List<Redbox.Messaging.IMessage>> receiveMessage, Action<Redbox.Messaging.IMessageConsumer, List<Redbox.Messaging.IMessage>> afterReceiveMessage)
	{
		Redbox.Messaging.MessagingResult messagingResult = new Redbox.Messaging.MessagingResult();
		try
		{
			for (int i = 0; i < MessageConsumerCount; i++)
			{
				GetNextConnection();
			}
			foreach (Redbox.Messaging.IProviderConnection connection in m_connectionList)
			{
				Redbox.Messaging.MessagingResult messagingResult2 = connection.Open();
				if (!messagingResult2.Success)
				{
					return messagingResult2;
				}
			}
			for (int i = 0; i < MessageConsumerCount; i++)
			{
				Redbox.Messaging.IProviderConnection providerConnection = (ShareConnection ? m_connectionList.First() : m_connectionList[i]);
				Redbox.Messaging.IMessageConsumer messageConsumer = providerConnection.CreateConsumer();
				Redbox.Messaging.Listener<Redbox.Messaging.IMessageConsumer> listener = new Redbox.Messaging.Listener<Redbox.Messaging.IMessageConsumer>(messageConsumer, delegate(Redbox.Messaging.IMessageConsumer c)
				{
					Redbox.Messaging.MessagingResult messagingResult3;
					List<Redbox.Messaging.IMessage> messageList;
					if (BatchConsume)
					{
						messagingResult3 = c.ReceiveList(ReceiveTimeout, BatchCount, out messageList);
					}
					else
					{
						messagingResult3 = c.Receive(ReceiveTimeout, out var message);
						messageList = new List<Redbox.Messaging.IMessage> { message };
					}
					if (!messagingResult3.Success)
					{
						return messagingResult3;
					}
					if (messageList == null || messageList.Count == 0)
					{
						return new Redbox.Messaging.MessagingResult
						{
							Success = true
						};
					}
					if (receiveMessage != null)
					{
						receiveMessage(c, messageList);
					}
					if (Throttle)
					{
						m_messageThrottle.Increment(messageList.Count);
					}
					if (afterReceiveMessage != null)
					{
						afterReceiveMessage(c, messageList);
					}
					return new Redbox.Messaging.MessagingResult
					{
						Success = true
					};
				});
				listener.ListenFailed = delegate(Redbox.Messaging.MessagingResult arg, Redbox.Messaging.IMessageConsumer messageConsumer2)
				{
					if (ListenFailed != null)
					{
						ListenFailed(arg, this);
					}
				};
				Redbox.Messaging.Listener<Redbox.Messaging.IMessageConsumer> item = listener;
				m_listenerList.Add(item);
				ConsumerList.Add(messageConsumer);
				Redbox.Messaging.MessagingResult messagingResult2 = messageConsumer.Open();
				if (!messagingResult2.Success)
				{
					return messagingResult2;
				}
			}
			messagingResult.Success = true;
			return messagingResult;
		}
		catch (Exception exception)
		{
			messagingResult.AddError(Redbox.Messaging.Error.NewError("CM0020", "ConsumeMessages.CreateMessageConsumers failed.", exception));
			return messagingResult;
		}
	}
}
