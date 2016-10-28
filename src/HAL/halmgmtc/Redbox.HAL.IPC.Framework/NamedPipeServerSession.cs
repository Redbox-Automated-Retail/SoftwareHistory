using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework;

internal class NamedPipeServerSession : AbstractNamedPipeServerSession, ISession, IMessageSink
{
	private List<string> m_messageSinkQueue;

	private readonly object m_syncLock = new object();

	public bool LogDetailedMessages { get; set; }

	internal IPCServiceHost ServiceHost { get; private set; }

	protected override string WelcomeMessage => $"Welcome! {ServiceHost.Name}, Version {ServiceHost.Version}, {ServiceHost.Copyright}";

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

	public bool Send(string message)
	{
		string msg = string.Format("{0} {1}", "[MSG]", message);
		LogHelper.Instance.Log(msg);
		lock (m_syncLock)
		{
			MessageSinkQueue.Add(message);
		}
		return true;
	}

	internal NamedPipeServerSession(BasePipeChannel channel, IPCServiceHost host)
		: base(channel)
	{
		ServiceHost = host;
	}

	protected override void CloseSessionInner()
	{
		RaiseDisconnect();
		try
		{
			ServiceHost.UnregisterSession(this);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("QuitSession: unable to unregister session.", e);
		}
	}

	protected override string ProcessClientCommand(string clientCommand)
	{
		try
		{
			CommandResult commandResult = CommandService.Instance.Execute(this, clientCommand);
			if (commandResult == null)
			{
				return null;
			}
			ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
			FlushSinkQueue(commandResult.Messages);
			return commandResult.ToString();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ProcessCommand.", e);
			return null;
		}
	}

	private void RaiseDisconnect()
	{
		if (Disconnect != null)
		{
			Disconnect(this, EventArgs.Empty);
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
