using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using Redbox.Core;

namespace Redbox.IPC.Framework.Remoting;

internal class RemotingClientSession : ClientSession
{
	private static readonly object m_lockObject = new object();

	protected override Stream Stream
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	private string Url { get; set; }

	public RemotingClientSession(IPCProtocol protocol, int timeout)
		: base(protocol)
	{
		base.Timeout = timeout;
		Url = $"tcp://{base.Protocol.Host}:{base.Protocol.Port}/IPCFramework/CommandService.rem";
	}

	public override bool Connect()
	{
		base.IsConnected = true;
		return true;
	}

	public override void ConnectThrowOnError()
	{
		base.IsConnected = true;
	}

	public override void Close()
	{
	}

	public override void Dispose()
	{
		if (base.OwningPool != null)
		{
			base.OwningPool.ReturnSession(this);
		}
		else
		{
			base.Dispose();
		}
	}

	public override List<string> ExecuteCommand(string command)
	{
		if (IsRegistrationRequired())
		{
			RegisterChannel();
		}
		Redbox.IPC.Framework.Remoting.IRemotingHost remotingHost = (Redbox.IPC.Framework.Remoting.IRemotingHost)new Redbox.IPC.Framework.Remoting.RemotingClientProxy(typeof(Redbox.IPC.Framework.Remoting.IRemotingHost), Url, base.Timeout).GetTransparentProxy();
		string text = remotingHost.ExecuteCommand(command);
		if (string.IsNullOrEmpty(text))
		{
			return new List<string>();
		}
		return new List<string>(text.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
	}

	protected internal override bool IsConnectionAvailable()
	{
		return true;
	}

	protected internal override void CustomClose()
	{
	}

	protected override int GetAvailableData()
	{
		throw new NotImplementedException();
	}

	protected override bool CanRead()
	{
		throw new NotImplementedException();
	}

	private void RegisterChannel()
	{
		lock (m_lockObject)
		{
			if (IsRegistrationRequired())
			{
				if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["CustomErrorsMode"]))
				{
					RemotingConfiguration.CustomErrorsMode = Enum<CustomErrorsModes>.ParseIgnoringCase(ConfigurationManager.AppSettings["CustomErrorsMode"], CustomErrorsModes.RemoteOnly);
				}
				LogHelper.Instance.Log("CustomErrors enabled = {0}", RemotingConfiguration.CustomErrorsEnabled(isLocalRequest: false));
				BinaryClientFormatterSinkProvider binaryClientFormatterSinkProvider = new BinaryClientFormatterSinkProvider();
				Redbox.IPC.Framework.Remoting.RemotingClientChannelSinkProvider next = new Redbox.IPC.Framework.Remoting.RemotingClientChannelSinkProvider(binaryClientFormatterSinkProvider.Next);
				binaryClientFormatterSinkProvider.Next = next;
				TcpClientChannel chnl = new TcpClientChannel(GetProperties(), binaryClientFormatterSinkProvider);
				ChannelServices.RegisterChannel(chnl, base.Protocol.IsSecure);
			}
		}
	}

	private static bool IsRegistrationRequired()
	{
		return ChannelServices.GetChannel("tcp") == null;
	}

	private ListDictionary GetProperties()
	{
		ListDictionary listDictionary = new ListDictionary();
		listDictionary.Add("port", "0");
		listDictionary.Add("timeout", base.Timeout);
		return listDictionary;
	}
}
