using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.Messaging;

namespace Redbox.IPC.Framework.Messaging;

internal class MessageQueueServerSession(Redbox.IPC.Framework.Messaging.MessageQueueServiceHost serviceHost) : Redbox.IPC.Framework.ISession, IMessageSink
{
	private Redbox.IPC.Framework.ParameterDictionary m_context;

	private List<string> m_messageSinkQueue;

	private IDictionary<string, object> m_properties;

	private readonly object m_syncLock = new object();

	public Redbox.IPC.Framework.ParameterDictionary Context
	{
		get
		{
			if (m_context == null)
			{
				m_context = new Redbox.IPC.Framework.ParameterDictionary();
			}
			return m_context;
		}
	}

	public IDictionary<string, object> Properties
	{
		get
		{
			if (m_properties == null)
			{
				m_properties = new Dictionary<string, object>();
			}
			return m_properties;
		}
	}

	private List<string> MessageSinkQueue
	{
		get
		{
			if (m_messageSinkQueue == null)
			{
				m_messageSinkQueue = new List<string>();
			}
			return m_messageSinkQueue;
		}
	}

	public event EventHandler Disconnect;

	public void Start()
	{
	}

	public Redbox.Messaging.MessagingResult ProcessRequest(Redbox.Messaging.IMessage message, Func<Redbox.Messaging.IMessage, Redbox.Messaging.MessagingResult> reply)
	{
		LogHelper.Instance.Log(message.Data, LogEntryType.Info);
		Redbox.IPC.Framework.CommandResult commandResult;
		if (message.Properties.ContainsKey("SenderIPAddress"))
		{
			string originIP = message.Properties["SenderIPAddress"];
			commandResult = Redbox.IPC.Framework.CommandService.Instance.Execute(this, originIP, message.Data);
		}
		else
		{
			commandResult = Redbox.IPC.Framework.CommandService.Instance.Execute(this, message.Data);
		}
		if (commandResult == null)
		{
			Redbox.Messaging.MessagingResult messagingResult = new Redbox.Messaging.MessagingResult();
			messagingResult.Success = false;
			Redbox.Messaging.MessagingResult messagingResult2 = messagingResult;
			messagingResult2.AddError(Redbox.Messaging.Error.NewError("MQSS0010", "MessageQueueServerSession.ProcessRequest failed.", $"  A null return result was received from CommandService.Instance.Execute. Message: {message.Data}"));
			return messagingResult2;
		}
		Redbox.IPC.Framework.ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
		FlushSinkQueue(commandResult.Messages);
		string text = commandResult.ToString();
		Redbox.Messaging.Message message2 = new Redbox.Messaging.Message();
		message2.Data = text;
		Redbox.Messaging.Message arg = message2;
		Redbox.Messaging.MessagingResult messagingResult3 = reply(arg);
		if (!messagingResult3.Success)
		{
			return messagingResult3;
		}
		LogHelper.Instance.Log(text, LogEntryType.Info);
		Redbox.Messaging.MessagingResult messagingResult4 = new Redbox.Messaging.MessagingResult();
		messagingResult4.Success = true;
		return messagingResult4;
	}

	public bool Send(string message)
	{
		string message2 = $"[MSG] {message}";
		LogHelper.Instance.Log(message2, LogEntryType.Info);
		EnqueueSinkMessage(message2);
		return true;
	}

	public bool IsConnected()
	{
		return true;
	}

	private void EnqueueSinkMessage(string message)
	{
		lock (m_syncLock)
		{
			MessageSinkQueue.Add(message);
		}
	}

	private void FlushSinkQueue(ICollection<string> messages)
	{
		lock (m_syncLock)
		{
			while (MessageSinkQueue.Count > 0)
			{
				string item = MessageSinkQueue[0];
				MessageSinkQueue.RemoveAt(0);
				messages.Add(item);
			}
		}
	}
}
