using System;
using System.Collections.Generic;

namespace Redbox.Messaging;

internal class MessageProducer : Redbox.Messaging.IMessageProducer, Redbox.Messaging.IMessageWorker, IDisposable
{
	private bool m_inTransaction;

	private Redbox.Messaging.IDestination m_destination;

	private Redbox.Messaging.IConnectionSettings m_configuration = new Redbox.Messaging.ConnectionSettings();

	public bool IsOpen { get; private set; }

	public long MessagesSent { get; set; }

	public long MessagesReceived { get; set; }

	internal Redbox.Messaging.IConnectionSettings Configuration
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

	internal Redbox.Messaging.IDestination Destination
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

	public MessageProducer(Redbox.Messaging.IDestination destination)
		: this(destination, destination.Configuration)
	{
	}

	public MessageProducer(Redbox.Messaging.IDestination destination, Redbox.Messaging.IConnectionSettings configuration)
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

	public Redbox.Messaging.MessagingResult Open()
	{
		Redbox.Messaging.MessagingResult messagingResult = Destination.Open();
		if (messagingResult.Success)
		{
			IsOpen = true;
		}
		return messagingResult;
	}

	public Redbox.Messaging.MessagingResult Close()
	{
		Redbox.Messaging.MessagingResult messagingResult = Destination.Close();
		if (messagingResult.Success)
		{
			IsOpen = false;
		}
		return messagingResult;
	}

	public Redbox.Messaging.MessagingResult SendMessage(Redbox.Messaging.IMessage message)
	{
		return SendMessageList(new List<Redbox.Messaging.IMessage> { message });
	}

	public Redbox.Messaging.MessagingResult SendMessageList(List<Redbox.Messaging.IMessage> messageList)
	{
		BeginTransaction();
		Redbox.Messaging.MessagingResult messagingResult = Destination.SendMessageList(messageList);
		if (messagingResult.Success)
		{
			MessagesSent += messageList.Count;
		}
		return messagingResult;
	}

	public Redbox.Messaging.MessagingResult BeginTransaction()
	{
		if (Configuration.TransactionalMessages && !m_inTransaction)
		{
			Redbox.Messaging.MessagingResult messagingResult = Destination.BeginTransaction();
			if (messagingResult.Success)
			{
				m_inTransaction = true;
			}
			return messagingResult;
		}
		Redbox.Messaging.MessagingResult messagingResult2 = new Redbox.Messaging.MessagingResult();
		messagingResult2.Success = true;
		return messagingResult2;
	}

	public Redbox.Messaging.MessagingResult Commit()
	{
		if (Configuration.TransactionalMessages && m_inTransaction)
		{
			Redbox.Messaging.MessagingResult result = Destination.Commit();
			m_inTransaction = false;
			return result;
		}
		Redbox.Messaging.MessagingResult messagingResult = new Redbox.Messaging.MessagingResult();
		messagingResult.Success = true;
		return messagingResult;
	}

	public Redbox.Messaging.MessagingResult RollBack()
	{
		if (Configuration.TransactionalMessages && m_inTransaction)
		{
			Redbox.Messaging.MessagingResult result = Destination.RollBack();
			m_inTransaction = false;
			return result;
		}
		Redbox.Messaging.MessagingResult messagingResult = new Redbox.Messaging.MessagingResult();
		messagingResult.Success = true;
		return messagingResult;
	}
}
