using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Redbox.Core;

namespace Redbox.IPC.Framework.Remoting;

internal class RemotingServerSession : Redbox.IPC.Framework.ISession, Redbox.Core.IMessageSink
{
	private Redbox.IPC.Framework.ParameterDictionary m_context;

	private Dictionary<string, object> m_properties;

	public Redbox.IPC.Framework.ParameterDictionary Context => m_context ?? (m_context = new Redbox.IPC.Framework.ParameterDictionary());

	public IDictionary<string, object> Properties => m_properties ?? (m_properties = new Dictionary<string, object>());

	public event EventHandler Disconnect;

	public bool Send(string message)
	{
		throw new NotImplementedException("Not valid for RemotingChannel");
	}

	public void Start()
	{
		throw new NotImplementedException("Not valid for RemotingChannel");
	}

	public string ProcessRequest(string commandString)
	{
		string originIP = CallContext.GetData("RemoteHostIP") as string;
		Redbox.IPC.Framework.CommandResult commandResult = Redbox.IPC.Framework.CommandService.Instance.Execute(this, originIP, commandString);
		if (commandResult == null)
		{
			return null;
		}
		Redbox.IPC.Framework.ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
		return commandResult.ToString();
	}

	public bool IsConnected()
	{
		return true;
	}
}
