using System;
using Redbox.Core;
using Redbox.Messaging;

namespace Redbox.IPC.Framework.Messaging;

internal class MessageQueueConfiguration
{
	private static Redbox.Messaging.MessagingFactory m_messagingFactory;

	public static Redbox.IPC.Framework.Messaging.MessageQueueConfiguration Instance => Singleton<Redbox.IPC.Framework.Messaging.MessageQueueConfiguration>.Instance;

	public Redbox.Messaging.IMessageProvider GetProvider(IPCProtocol protocol)
	{
		return m_messagingFactory.CreateMessageProvider("Redbox.Messaging." + protocol.Channel);
	}

	public static void Initialize(Redbox.Messaging.MessagingConfiguration messagingConfiguration)
	{
		m_messagingFactory = new Redbox.Messaging.MessagingFactory(messagingConfiguration);
	}

	public bool GetConnectionSettings(IPCProtocol protocol, out Redbox.Messaging.IConnectionSettings configIn, out Redbox.Messaging.IConnectionSettings configOut)
	{
		configIn = new Redbox.Messaging.ConnectionSettings
		{
			ConnectionString = protocol.Host + (string.IsNullOrEmpty(protocol.Port) ? string.Empty : (":" + protocol.Port)),
			DestinationType = Redbox.Messaging.DestinationType.Queue,
			UserName = protocol.UserName,
			Password = protocol.Password,
			TransactionalMessages = true
		};
		configOut = new Redbox.Messaging.ConnectionSettings
		{
			ConnectionString = protocol.Host + (string.IsNullOrEmpty(protocol.Port) ? string.Empty : (":" + protocol.Port)),
			DestinationType = Redbox.Messaging.DestinationType.Queue,
			UserName = protocol.UserName,
			Password = protocol.Password,
			TransactionalMessages = true
		};
		if (!protocol.Query.ContainsKey("QUEUEIN"))
		{
			LogHelper.Instance.Log("MessageQueueConfiguration.GetConnectionSettings failed.  QUEUEIN does not exist.");
			return false;
		}
		if (!protocol.Query.ContainsKey("QUEUEOUT"))
		{
			LogHelper.Instance.Log("MessageQueueConfiguration.GetConnectionSettings failed.  QUEUEOUT does not exist.");
			return false;
		}
		if (protocol.Query.ContainsKey("MINCONNECTIONPOOLSIZE"))
		{
			configIn.MinConnectionPoolSize = Convert.ToInt32(protocol.Query["MINCONNECTIONPOOLSIZE"]);
			configOut.MinConnectionPoolSize = Convert.ToInt32(protocol.Query["MINCONNECTIONPOOLSIZE"]);
		}
		if (protocol.Query.ContainsKey("MAXCONNECTIONPOOLSIZE"))
		{
			configIn.MaxConnectionPoolSize = Convert.ToInt32(protocol.Query["MAXCONNECTIONPOOLSIZE"]);
			configOut.MaxConnectionPoolSize = Convert.ToInt32(protocol.Query["MAXCONNECTIONPOOLSIZE"]);
		}
		configIn.DestinationName = protocol.Query["QUEUEIN"];
		configOut.DestinationName = protocol.Query["QUEUEOUT"];
		configIn.Properties = protocol.Query;
		configOut.Properties = protocol.Query;
		if (!SetProviderSettings(protocol, configIn))
		{
			LogHelper.Instance.Log("MessageQueueConfiguration.SetProviderSettings failed.  Problem exists with configIn.");
			return false;
		}
		if (!SetProviderSettings(protocol, configOut))
		{
			LogHelper.Instance.Log("MessageQueueConfiguration.SetProviderSettings failed.  Problem exists with configOut.");
			return false;
		}
		return true;
	}

	private MessageQueueConfiguration()
	{
	}

	private static bool SetProviderSettings(IPCProtocol protocol, Redbox.Messaging.IConnectionSettings config)
	{
		switch (protocol.Channel)
		{
		case Redbox.IPC.Framework.ChannelType.MSMQ:
		{
			bool flag = false;
			if (config.Properties.ContainsKey("PRIVATE"))
			{
				flag = Convert.ToBoolean(config.Properties["PRIVATE"]);
			}
			config.ConnectionString = string.Format("{0}{1}\\{2}", protocol.Host, flag ? "\\private$" : "", config.DestinationName);
			break;
		}
		case Redbox.IPC.Framework.ChannelType.ActiveMQ:
			config.ConnectionString = string.Format("tcp://{0}:{1}", protocol.Host, string.IsNullOrEmpty(protocol.Port) ? "61616" : protocol.Port);
			break;
		case Redbox.IPC.Framework.ChannelType.OracleMQ:
			if (!config.Properties.ContainsKey("SERVICENAME"))
			{
				return false;
			}
			config.ConnectionString = string.Format("user id={0};password={1};data source={2}", config.UserName, config.Password, config.Properties["SERVICENAME"]);
			break;
		}
		return true;
	}
}
