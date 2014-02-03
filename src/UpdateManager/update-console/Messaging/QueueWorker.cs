using System;
using Redbox.Messaging;

namespace Redbox.IPC.Framework.Messaging;

internal class QueueWorker
{
	private TimeSpan m_timeout = new TimeSpan(0, 0, 0, 0, 100);

	private bool m_isOpen;

	private IMessageReply m_messageReply;

	private IProviderConnection m_connectionIn;

	private IProviderConnection m_connectionOut;

	private readonly IMessageProvider m_provider;

	private readonly object m_lock = new object();

	private readonly IConnectionSettings m_configIn;

	private readonly IConnectionSettings m_configOut;

	public TimeSpan DefaultTimeOut
	{
		get
		{
			return m_timeout;
		}
		set
		{
			m_timeout = value;
		}
	}

	public bool IsOpen => m_isOpen;

	public QueueWorker(IMessageProvider provider, IConnectionSettings configIn, IConnectionSettings configOut)
	{
		m_provider = provider;
		m_configIn = configIn;
		m_configOut = configOut;
	}

	public MessagingResult Open()
	{
		m_connectionIn = m_provider.CreateConnection(m_configIn);
		m_connectionOut = m_provider.CreateConnection(m_configOut);
		MessagingResult messagingResult = m_connectionIn.Open();
		if (!messagingResult.Success)
		{
			Close(destroyConnections: true);
			return messagingResult;
		}
		messagingResult = m_connectionOut.Open();
		if (!messagingResult.Success)
		{
			Close(destroyConnections: true);
			return messagingResult;
		}
		m_messageReply = new MessageReply(m_connectionIn.CreateDestination(), m_connectionOut.CreateDestination());
		messagingResult = m_messageReply.Open();
		if (!messagingResult.Success)
		{
			Close(destroyConnections: true);
			return messagingResult;
		}
		m_isOpen = true;
		MessagingResult messagingResult2 = new MessagingResult();
		messagingResult2.Success = true;
		return messagingResult2;
	}

	public void Close()
	{
		Close(destroyConnections: false);
	}

	public void Close(bool destroyConnections)
	{
		lock (m_lock)
		{
			if (m_messageReply != null)
			{
				m_messageReply.Close();
				m_messageReply = null;
			}
			if (m_connectionIn != null)
			{
				m_connectionIn.DestroyConnectionOnClose = destroyConnections;
				m_connectionIn.Close();
				m_connectionIn = null;
			}
			if (m_connectionOut != null)
			{
				m_connectionOut.DestroyConnectionOnClose = destroyConnections;
				m_connectionOut.Close();
				m_connectionOut = null;
			}
			m_isOpen = false;
		}
	}

	public MessagingResult ReceiveRequest(out IMessage message)
	{
		lock (m_lock)
		{
			if (m_isOpen)
			{
				return m_messageReply.ReceiveRequest(DefaultTimeOut, out message);
			}
			message = null;
			MessagingResult messagingResult = new MessagingResult();
			messagingResult.Success = false;
			return messagingResult;
		}
	}

	public MessagingResult SendReply(IMessage request, IMessage reply)
	{
		lock (m_lock)
		{
			MessagingResult result;
			if (!m_isOpen)
			{
				MessagingResult messagingResult = new MessagingResult();
				messagingResult.Success = false;
				result = messagingResult;
			}
			else
			{
				result = m_messageReply.SendReply(request, reply);
			}
			return result;
		}
	}
}
