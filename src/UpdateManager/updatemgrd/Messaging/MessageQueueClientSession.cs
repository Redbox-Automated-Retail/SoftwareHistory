using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Redbox.Messaging;

namespace Redbox.IPC.Framework.Messaging;

internal class MessageQueueClientSession : ClientSession
{
	internal IProviderConnection ConnectionIn;

	internal IProviderConnection ConnectionOut;

	internal IMessageRequest MessageRequest;

	private readonly TimeSpan m_timeoutSpan;

	protected override Stream Stream => null;

	public MessageQueueClientSession(IPCProtocol protocol, int? timeout)
		: base(protocol)
	{
		m_timeoutSpan = new TimeSpan(0, 0, 0, 0, timeout ?? 5000);
	}

	public override bool Connect()
	{
		IMessageProvider provider = MessageQueueConfiguration.Instance.GetProvider(base.Protocol);
		if (!MessageQueueConfiguration.Instance.GetConnectionSettings(base.Protocol, out var configIn, out var configOut))
		{
			return false;
		}
		ConnectionIn = provider.CreateConnection(configIn);
		ConnectionOut = provider.CreateConnection(configOut);
		MessagingResult messagingResult = ConnectionIn.Open();
		if (!messagingResult.Success)
		{
			return false;
		}
		messagingResult = ConnectionOut.Open();
		if (!messagingResult.Success)
		{
			return false;
		}
		MessageRequest = new MessageRequest(ConnectionIn.CreateDestination(), ConnectionOut.CreateDestination());
		messagingResult = MessageRequest.Open();
		if (!messagingResult.Success)
		{
			return false;
		}
		base.IsConnected = true;
		return true;
	}

	public override void ConnectThrowOnError()
	{
	}

	public override List<string> ExecuteCommand(string command)
	{
		Message message = new Message();
		message.Data = command;
		Message request = message;
		MessagingResult messagingResult = MessageRequest.SendRequest(m_timeoutSpan, request, out var reply);
		if (!messagingResult.Success)
		{
			return null;
		}
		if (reply != null)
		{
			ProcessResult(reply.Data, out var messages);
			return messages;
		}
		return null;
	}

	protected internal override void CustomClose()
	{
		if (MessageRequest != null)
		{
			MessageRequest.Close();
			MessageRequest = null;
		}
		if (ConnectionIn != null)
		{
			ConnectionIn.Close();
			ConnectionIn = null;
		}
		if (ConnectionOut != null)
		{
			ConnectionOut.Close();
			ConnectionOut = null;
		}
	}

	protected override int GetAvailableData()
	{
		return 0;
	}

	protected override bool CanRead()
	{
		return true;
	}

	protected internal override bool IsConnectionAvailable()
	{
		return true;
	}

	private static string GetNextBufferLine(StringBuilder builder)
	{
		string text = builder.ToString();
		int num = text.IndexOf(Environment.NewLine);
		if (num != -1)
		{
			string text2 = text.Substring(0, num + Environment.NewLine.Length);
			builder.Remove(0, num + Environment.NewLine.Length);
			return text2.Trim();
		}
		return string.Empty;
	}

	private static bool BufferHasMoreLines(StringBuilder builder)
	{
		return builder.ToString().IndexOf(Environment.NewLine) != -1;
	}

	private bool ProcessResult(string data, out List<string> messages)
	{
		messages = new List<string>();
		StringBuilder builder = new StringBuilder(data);
		string nextBufferLine;
		do
		{
			nextBufferLine = GetNextBufferLine(builder);
			if (nextBufferLine == null || (nextBufferLine == string.Empty && !BufferHasMoreLines(builder)))
			{
				return false;
			}
			if (nextBufferLine.StartsWith("[MSG]"))
			{
				ProcessServerEvent(nextBufferLine);
			}
			else
			{
				messages.Add(nextBufferLine);
			}
		}
		while (!nextBufferLine.StartsWith("Welcome!") && !nextBufferLine.StartsWith("Goodbye!") && !nextBufferLine.StartsWith("545 Command") && !nextBufferLine.StartsWith("203 Command"));
		return true;
	}
}
