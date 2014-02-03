#define DEBUG
using System;
using System.Diagnostics;

namespace Redbox.Messaging;

internal class MessageReply : IMessageReply, IMessageWorker, IDisposable
{
	private IDestination m_requestDestination;

	private IDestination m_replyDestination;

	public bool IsOpen { get; private set; }

	public long MessagesSent { get; set; }

	public long MessagesReceived { get; set; }

	public MessageReply(IDestination requestDestination, IDestination replyDestination)
	{
		Debug.Assert(requestDestination != null, "MessageReply constructor request destination must not be null.");
		Debug.Assert(requestDestination != null, "MessageReply constructor reply destination must not be null.");
		m_requestDestination = requestDestination;
		m_replyDestination = replyDestination;
	}

	public void Dispose()
	{
		if (m_requestDestination != null)
		{
			m_requestDestination.Close();
			m_requestDestination = null;
		}
		if (m_replyDestination != null)
		{
			m_replyDestination.Close();
			m_replyDestination = null;
		}
	}

	public MessagingResult Open()
	{
		MessagingResult messagingResult = m_requestDestination.Open();
		if (!messagingResult.Success)
		{
			return messagingResult;
		}
		MessagingResult messagingResult2 = m_replyDestination.Open();
		if (!messagingResult2.Success)
		{
			MessagingResult messagingResult3 = m_requestDestination.Close();
			if (!messagingResult3.Success)
			{
				messagingResult2.AddErrorList(messagingResult3.Errors);
			}
			return messagingResult2;
		}
		IsOpen = true;
		MessagingResult messagingResult4 = new MessagingResult();
		messagingResult4.Success = true;
		return messagingResult4;
	}

	public MessagingResult Close()
	{
		MessagingResult messagingResult = m_requestDestination.Close();
		MessagingResult messagingResult2 = m_replyDestination.Close();
		MessagingResult messagingResult3 = new MessagingResult();
		if (messagingResult.Success && messagingResult2.Success)
		{
			messagingResult3.Success = true;
		}
		else
		{
			messagingResult3.AddErrorList(messagingResult.Errors);
			messagingResult3.AddErrorList(messagingResult2.Errors);
		}
		IsOpen = false;
		return messagingResult3;
	}

	public MessagingResult ReceiveRequest(TimeSpan wait, out IMessage message)
	{
		message = null;
		MessagingResult messagingResult;
		if (m_requestDestination.Configuration.TransactionalMessages)
		{
			messagingResult = m_requestDestination.BeginTransaction();
			if (!messagingResult.Success)
			{
				return messagingResult;
			}
		}
		messagingResult = m_requestDestination.Receive(wait, out message);
		if (!messagingResult.Success)
		{
			return messagingResult;
		}
		if (m_requestDestination.Configuration.TransactionalMessages)
		{
			messagingResult = m_requestDestination.Commit();
			if (!messagingResult.Success)
			{
				return messagingResult;
			}
		}
		MessagesReceived++;
		return messagingResult;
	}

	public MessagingResult SendReply(IMessage requestMessage, IMessage replyMessage)
	{
		replyMessage.MessageType = MessageType.Reply;
		replyMessage.CorrelationId = requestMessage.MessageId;
		MessagingResult messagingResult;
		if (m_replyDestination.Configuration.TransactionalMessages)
		{
			messagingResult = m_replyDestination.BeginTransaction();
			if (!messagingResult.Success)
			{
				return messagingResult;
			}
		}
		messagingResult = m_replyDestination.SendMessage(replyMessage);
		if (!messagingResult.Success)
		{
			return messagingResult;
		}
		if (m_replyDestination.Configuration.TransactionalMessages)
		{
			messagingResult = m_replyDestination.Commit();
			if (!messagingResult.Success)
			{
				return messagingResult;
			}
		}
		MessagesSent++;
		return messagingResult;
	}
}
