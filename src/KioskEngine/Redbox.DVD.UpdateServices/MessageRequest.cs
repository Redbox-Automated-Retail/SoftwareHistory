#define DEBUG
using System;
using System.Diagnostics;

namespace Redbox.Messaging;

internal class MessageRequest : Redbox.Messaging.IMessageRequest, Redbox.Messaging.IMessageWorker, IDisposable
{
	private Redbox.Messaging.IDestination m_requestDestination;

	private Redbox.Messaging.IDestination m_replyDestination;

	public bool IsOpen { get; private set; }

	public long MessagesSent { get; set; }

	public long MessagesReceived { get; set; }

	public MessageRequest(Redbox.Messaging.IDestination requestDestination, Redbox.Messaging.IDestination replyDestination)
	{
		Debug.Assert(requestDestination != null, "MessageRequest constructor request destination must not be null.");
		Debug.Assert(replyDestination != null, "MessageRequest constructor reply destination must not be null.");
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

	public Redbox.Messaging.MessagingResult Open()
	{
		Redbox.Messaging.MessagingResult messagingResult = m_requestDestination.Open();
		if (!messagingResult.Success)
		{
			return messagingResult;
		}
		Redbox.Messaging.MessagingResult messagingResult2 = m_replyDestination.Open();
		if (!messagingResult2.Success)
		{
			Redbox.Messaging.MessagingResult messagingResult3 = m_requestDestination.Close();
			if (!messagingResult3.Success)
			{
				messagingResult2.AddErrorList(messagingResult3.Errors);
			}
			return messagingResult2;
		}
		IsOpen = true;
		Redbox.Messaging.MessagingResult messagingResult4 = new Redbox.Messaging.MessagingResult();
		messagingResult4.Success = true;
		return messagingResult4;
	}

	public Redbox.Messaging.MessagingResult Close()
	{
		Redbox.Messaging.MessagingResult messagingResult = m_requestDestination.Close();
		Redbox.Messaging.MessagingResult messagingResult2 = m_replyDestination.Close();
		Redbox.Messaging.MessagingResult messagingResult3 = new Redbox.Messaging.MessagingResult();
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

	public Redbox.Messaging.MessagingResult SendRequest(TimeSpan wait, Redbox.Messaging.IMessage request, out Redbox.Messaging.IMessage reply)
	{
		reply = null;
		request.MessageType = Redbox.Messaging.MessageType.Request;
		Redbox.Messaging.MessagingResult messagingResult;
		if (m_requestDestination.Configuration.TransactionalMessages)
		{
			messagingResult = m_requestDestination.BeginTransaction();
			if (!messagingResult.Success)
			{
				return messagingResult;
			}
		}
		messagingResult = m_requestDestination.SendMessage(request);
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
		if (m_replyDestination.Configuration.TransactionalMessages)
		{
			messagingResult = m_replyDestination.BeginTransaction();
			if (!messagingResult.Success)
			{
				return messagingResult;
			}
		}
		messagingResult = m_replyDestination.ReceiveReply(wait, request, out reply);
		if (!messagingResult.Success)
		{
			return messagingResult;
		}
		if (reply == null)
		{
			messagingResult.Success = false;
			messagingResult.AddError(Redbox.Messaging.Error.NewError("MR0010", "MessageRequest.SendRequest did not receive a reply before the wait timespan elapsed.", ""));
			return messagingResult;
		}
		if (m_replyDestination.Configuration.TransactionalMessages)
		{
			messagingResult = m_replyDestination.Commit();
			if (!messagingResult.Success)
			{
				reply = null;
				return messagingResult;
			}
		}
		MessagesSent++;
		Redbox.Messaging.MessagingResult messagingResult2 = new Redbox.Messaging.MessagingResult();
		messagingResult2.Success = true;
		return messagingResult2;
	}
}
