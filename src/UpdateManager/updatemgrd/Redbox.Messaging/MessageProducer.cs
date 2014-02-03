using System;
using System.Collections.Generic;

namespace Redbox.Messaging;

internal class MessageProducer : IMessageProducer, IMessageWorker, IDisposable
{
	private bool m_inTransaction;

	private IDestination m_destination;

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

	internal IDestination Destination
	{
		get
		{
			return m_destination;
		}
		private set
		{
			m_destination = value;
		}
	}

	public MessageProducer(IDestination destination)
		: this(destination, destination.Configuration)
	{
	}

	public MessageProducer(IDestination destination, IConnectionSettings configuration)
	{
		Destination = destination;
		Configuration = configuration;
	}

	public void Dispose()
	{
		if (m_destination != null)
		{
			m_destination.Close();
			m_destination = null;
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

	public MessagingResult SendMessage(IMessage message)
	{
		return SendMessageList(new List<IMessage> { message });
	}

	public MessagingResult SendMessageList(List<IMessage> messageList)
	{
		BeginTransaction();
		MessagingResult messagingResult = Destination.SendMessageList(messageList);
		if (messagingResult.Success)
		{
			MessagesSent += messageList.Count;
		}
		return messagingResult;
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
