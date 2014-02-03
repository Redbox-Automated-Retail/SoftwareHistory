using System;
using System.Collections.Generic;
using System.Linq;

namespace Redbox.Messaging;

internal class MessageConsumer : IMessageConsumer, IMessageWorker, IDisposable
{
	private bool m_inTransaction;

	private IConnectionSettings m_configuration = new ConnectionSettings();

	public bool IsOpen { get; private set; }

	public long MessagesSent { get; set; }

	public long MessagesReceived { get; set; }

	internal IConnectionSettings Configuration
	{
		get
		{
			return m_configuration;
		}
		set
		{
			m_configuration = value;
		}
	}

	internal IDestination Destination { get; private set; }

	public MessageConsumer(IDestination destination)
		: this(destination, destination.Configuration)
	{
	}

	public MessageConsumer(IDestination destination, IConnectionSettings configuration)
	{
		Destination = destination;
		Configuration = configuration;
	}

	public void Dispose()
	{
		if (Destination != null)
		{
			Destination.Close();
			Destination = null;
		}
	}

	public MessagingResult Open()
	{
		MessagingResult messagingResult = Destination.Open();
		if (messagingResult.Success)
		{
			IsOpen = true;
		}
		return messagingResult;
	}

	public MessagingResult Close()
	{
		MessagingResult messagingResult = Destination.Close();
		if (messagingResult.Success)
		{
			IsOpen = false;
		}
		return messagingResult;
	}

	public MessagingResult Receive(out IMessage message)
	{
		return Receive(new TimeSpan(0, 0, 0, 0, 10), out message);
	}

	public MessagingResult Receive(TimeSpan wait, out IMessage message)
	{
		MessagingResult messagingResult = ReceiveList(wait, 1, out var messageList);
		message = ((messagingResult.Success && messageList.Count > 0) ? messageList.First() : null);
		return messagingResult;
	}

	public MessagingResult ReceiveList(TimeSpan wait, int count, out List<IMessage> messageList)
	{
		BeginTransaction();
		MessagingResult result = Destination.ReceiveList(wait, count, out messageList);
		MessagesReceived += messageList.Count;
		return result;
	}

	public MessagingResult BeginTransaction()
	{
		if (Configuration.TransactionalMessages && !m_inTransaction)
		{
			MessagingResult messagingResult = Destination.BeginTransaction();
			if (messagingResult.Success)
			{
				m_inTransaction = true;
			}
			return messagingResult;
		}
		MessagingResult messagingResult2 = new MessagingResult();
		messagingResult2.Success = true;
		return messagingResult2;
	}

	public MessagingResult Commit()
	{
		if (Configuration.TransactionalMessages && m_inTransaction)
		{
			MessagingResult result = Destination.Commit();
			m_inTransaction = false;
			return result;
		}
		MessagingResult messagingResult = new MessagingResult();
		messagingResult.Success = true;
		return messagingResult;
	}

	public MessagingResult RollBack()
	{
		if (Configuration.TransactionalMessages && m_inTransaction)
		{
			MessagingResult result = Destination.RollBack();
			m_inTransaction = false;
			return result;
		}
		MessagingResult messagingResult = new MessagingResult();
		messagingResult.Success = true;
		return messagingResult;
	}
}
