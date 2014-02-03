using System;
using System.Collections.Generic;
using System.Linq;

namespace Redbox.Messaging;

internal class MessageConsumer : Redbox.Messaging.IMessageConsumer, Redbox.Messaging.IMessageWorker, IDisposable
{
	private bool m_inTransaction;

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

	internal Redbox.Messaging.IDestination Destination { get; private set; }

	public MessageConsumer(Redbox.Messaging.IDestination destination)
		: this(destination, destination.Configuration)
	{
	}

	public MessageConsumer(Redbox.Messaging.IDestination destination, Redbox.Messaging.IConnectionSettings configuration)
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

	public Redbox.Messaging.MessagingResult Receive(out Redbox.Messaging.IMessage message)
	{
		return Receive(new TimeSpan(0, 0, 0, 0, 10), out message);
	}

	public Redbox.Messaging.MessagingResult Receive(TimeSpan wait, out Redbox.Messaging.IMessage message)
	{
		Redbox.Messaging.MessagingResult messagingResult = ReceiveList(wait, 1, out var messageList);
		message = ((messagingResult.Success && messageList.Count > 0) ? messageList.First() : null);
		return messagingResult;
	}

	public Redbox.Messaging.MessagingResult ReceiveList(TimeSpan wait, int count, out List<Redbox.Messaging.IMessage> messageList)
	{
		BeginTransaction();
		Redbox.Messaging.MessagingResult result = Destination.ReceiveList(wait, count, out messageList);
		MessagesReceived += messageList.Count;
		return result;
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
