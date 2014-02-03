#define DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Redbox.Core;

namespace Redbox.Messaging;

internal class ConsumeMessages<T> : IConsumeMessages where T : IMessageProvider, new()
{
	private long m_messagesReceived;

	private bool m_shareConnection = true;

	private int m_messageConsumerCount = 10;

	private MessageThrottle m_messageThrottle;

	private readonly T m_messageProvider = new T();

	private readonly List<IMessageConsumer> m_consumerList = new List<IMessageConsumer>();

	private readonly List<Listener<IMessageConsumer>> m_listenerList = new List<Listener<IMessageConsumer>>();

	private readonly List<IProviderConnection> m_connectionList = new List<IProviderConnection>();

	private MessageThroughput m_messageThroughput = new MessageThroughput(100L, new TimeSpan(0, 0, 1));

	private bool m_batchConsume = true;

	private int m_batchCount = 10;

	private TimeSpan m_receiveTimeout = new TimeSpan(0, 0, 0, 0, 10);

	public string Name { get; set; }

	public IMessageProvider MessageProvider => m_messageProvider;

	public MessageThroughput MessageThroughput
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

	public List<IMessageConsumer> ConsumerList => m_consumerList;

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

	public IConnectionSettings Configuration { get; set; }

	public long MessagesReceived
	{
		get
		{
			if (Active)
			{
				long count = 0L;
				if (m_consumerList != null)
				{
					m_consumerList.ForEach(delegate(IMessageConsumer c)
					{
						count += c.MessagesReceived;
					});
				}
				return count;
			}
			return m_messagesReceived;
		}
	}

	public Action<MessagingResult, IConsumeMessages> ListenFailed { get; set; }

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

	public MessagingResult Start(Action<IMessageConsumer, List<IMessage>> receiveMessage, Action<IMessageConsumer, List<IMessage>> afterReceiveMessage)
	{
		Debug.Assert(Configuration != null, "ConsumeMessages<T>.Start, Configuration cannot be null.");
		MessagingResult result = new MessagingResult();
		Action<MessagingResult> action = delegate(MessagingResult mr)
		{
			result.AddErrorList(mr.Errors);
			MessagingResult messagingResult3 = Stop();
			if (!messagingResult3.Success)
			{
				result.AddErrorList(messagingResult3.Errors);
			}
		};
		StartThrottle();
		m_messagesReceived = 0L;
		MessagingResult messagingResult = CreateMessageConsumers(receiveMessage, afterReceiveMessage);
		if (!messagingResult.Success)
		{
			action(messagingResult);
			return result;
		}
		int num = 0;
		foreach (Listener<IMessageConsumer> listener in m_listenerList)
		{
			MessagingResult messagingResult2 = listener.Start();
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

	public MessagingResult Stop()
	{
		MessagingResult result = new MessagingResult();
		Active = false;
		try
		{
			m_listenerList.ForEach(delegate(Listener<IMessageConsumer> l)
			{
				MessagingResult messagingResult = l.Stop(wait: false);
				if (!messagingResult.Success)
				{
					result.AddErrorList(messagingResult.Errors);
				}
			});
			m_listenerList.ForEach(delegate(Listener<IMessageConsumer> l)
			{
				MessagingResult messagingResult = l.Stop();
				if (!messagingResult.Success)
				{
					result.AddErrorList(messagingResult.Errors);
				}
			});
			m_messagesReceived = 0L;
			int each = 0;
			ConsumerList.ForEach(delegate(IMessageConsumer consumer)
			{
				m_messagesReceived += consumer.MessagesReceived;
				LogHelper.Instance.Log($"Stopped ConsumeMessages:{Name} worker: {each++} Messages Consumed {consumer.MessagesReceived}", LogEntryType.Info);
			});
			ConsumerList.ForEach(delegate(IMessageConsumer c)
			{
				c.Dispose();
			});
			ConsumerList.Clear();
			m_listenerList.Clear();
			m_connectionList.ForEach(delegate(IProviderConnection connection)
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
			result.AddError(Error.NewError("CM0010", "ConsumeMessages.Stop failed.", exception));
			return result;
		}
	}

	private void StartThrottle()
	{
		if (Throttle)
		{
			m_messageThrottle = new MessageThrottle(m_messageThroughput);
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
				IProviderConnection item = MessageProvider.CreateConnection(Configuration);
				m_connectionList.Add(item);
			}
		}
		else
		{
			IProviderConnection item = MessageProvider.CreateConnection(Configuration);
			m_connectionList.Add(item);
		}
	}

	private MessagingResult CreateMessageConsumers(Action<IMessageConsumer, List<IMessage>> receiveMessage, Action<IMessageConsumer, List<IMessage>> afterReceiveMessage)
	{
		MessagingResult messagingResult = new MessagingResult();
		try
		{
			for (int i = 0; i < MessageConsumerCount; i++)
			{
				GetNextConnection();
			}
			foreach (IProviderConnection connection in m_connectionList)
			{
				MessagingResult messagingResult2 = connection.Open();
				if (!messagingResult2.Success)
				{
					return messagingResult2;
				}
			}
			for (int i = 0; i < MessageConsumerCount; i++)
			{
				IProviderConnection providerConnection = (ShareConnection ? m_connectionList.First() : m_connectionList[i]);
				IMessageConsumer messageConsumer = providerConnection.CreateConsumer();
				Listener<IMessageConsumer> listener = new Listener<IMessageConsumer>(messageConsumer, delegate(IMessageConsumer c)
				{
					MessagingResult messagingResult3;
					List<IMessage> messageList;
					if (BatchConsume)
					{
						messagingResult3 = c.ReceiveList(ReceiveTimeout, BatchCount, out messageList);
					}
					else
					{
						messagingResult3 = c.Receive(ReceiveTimeout, out var message);
						messageList = new List<IMessage> { message };
					}
					if (!messagingResult3.Success)
					{
						return messagingResult3;
					}
					if (messageList == null || messageList.Count == 0)
					{
						return new MessagingResult
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
					return new MessagingResult
					{
						Success = true
					};
				});
				listener.ListenFailed = delegate(MessagingResult arg, IMessageConsumer messageConsumer2)
				{
					if (ListenFailed != null)
					{
						ListenFailed(arg, this);
					}
				};
				Listener<IMessageConsumer> item = listener;
				m_listenerList.Add(item);
				ConsumerList.Add(messageConsumer);
				MessagingResult messagingResult2 = messageConsumer.Open();
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
			messagingResult.AddError(Error.NewError("CM0020", "ConsumeMessages.CreateMessageConsumers failed.", exception));
			return messagingResult;
		}
	}
}
