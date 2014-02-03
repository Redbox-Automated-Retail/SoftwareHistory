using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using Redbox.Core;

namespace Redbox.IPC.Framework.Remoting;

internal class RemotingServiceHost : IPCServiceHost
{
	private TcpServerChannel m_tcpChannel;

	public int MaxThreads { get; set; }

	public RemotingServiceHost(int maxThreads)
	{
		MaxThreads = maxThreads;
	}

	public override bool Start()
	{
		if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["CustomErrorsMode"]))
		{
			RemotingConfiguration.CustomErrorsMode = Enum<CustomErrorsModes>.ParseIgnoringCase(ConfigurationManager.AppSettings["CustomErrorsMode"], CustomErrorsModes.RemoteOnly);
		}
		LogHelper.Instance.Log("CustomErrors enabled = {0}", RemotingConfiguration.CustomErrorsEnabled(isLocalRequest: false));
		ListDictionary listDictionary = new ListDictionary();
		listDictionary.Add("port", base.Protocol.Port);
		ListDictionary properties = listDictionary;
		LogHelper.Instance.Log("Creating tcp channel for remoting");
		m_tcpChannel = new TcpServerChannel(properties, new RemotingServerChannelSinkProvider(this, new BinaryServerFormatterSinkProvider()));
		LogHelper.Instance.Log("Registering channel with channel services");
		ChannelServices.RegisterChannel(m_tcpChannel, base.Protocol.IsSecure);
		WellKnownServiceTypeEntry entry = new WellKnownServiceTypeEntry(typeof(RemotingHost), "CommandService.rem", WellKnownObjectMode.SingleCall);
		RemotingConfiguration.ApplicationName = "IPCFramework";
		RemotingConfiguration.RegisterWellKnownServiceType(entry);
		base.Alive = true;
		Statistics.Instance.ServerStartTime = DateTime.Now;
		LogHelper.Instance.Log("Begin processing incoming requests.");
		return true;
	}

	public override void Stop()
	{
		if (base.Alive)
		{
			m_tcpChannel.StopListening(null);
			ChannelServices.UnregisterChannel(m_tcpChannel);
			base.Alive = false;
		}
	}
}
