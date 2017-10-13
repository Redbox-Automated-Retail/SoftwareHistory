using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Redbox.Messaging;

internal class MessagingFactory
{
	private Redbox.Messaging.MessagingConfiguration m_messagingConfiguration;

	private readonly Dictionary<string, Assembly> m_assemblyList = new Dictionary<string, Assembly>();

	public Redbox.Messaging.MessagingConfiguration MessagingConfiguration => m_messagingConfiguration;

	private Dictionary<string, Assembly> AssemblyList => m_assemblyList;

	public MessagingFactory(Redbox.Messaging.MessagingConfiguration messagingConfiguration)
	{
		m_messagingConfiguration = messagingConfiguration;
		LoadAssemblies();
	}

	public Redbox.Messaging.MessagingGroup CreateMessagingGroup(string groupName)
	{
		CheckMessagingConfigurationLoaded();
		Redbox.Messaging.ConnectionGroupConfiguration connectionGroupConfiguration = MessagingConfiguration.Groups.Find((Redbox.Messaging.ConnectionGroupConfiguration g) => g.Name.Equals(groupName, StringComparison.CurrentCultureIgnoreCase));
		if (connectionGroupConfiguration == null)
		{
			throw new ArgumentException("GetMessagingGroup parameter groupName not found.");
		}
		Redbox.Messaging.MessagingGroup messagingGroup = new Redbox.Messaging.MessagingGroup();
		messagingGroup.Name = groupName;
		messagingGroup.ConsumeMessagesList = GetGroupConsumeMessagesList(connectionGroupConfiguration);
		messagingGroup.ProduceMessagesList = GetGroupProduceMessagesList(connectionGroupConfiguration);
		return messagingGroup;
	}

	public Redbox.Messaging.IProviderConnection CreateProviderConnection(string connectionName)
	{
		CheckMessagingConfigurationLoaded();
		Redbox.Messaging.ConnectionConfiguration connectionConfiguration = GetConnectionConfiguration(connectionName);
		Redbox.Messaging.IMessageProvider messageProvider = CreateMessageProvider(connectionConfiguration.ProviderName);
		Redbox.Messaging.IConnectionSettings configuration = CreateConnectionSettings(connectionConfiguration);
		return messageProvider.CreateConnection(configuration);
	}

	public Redbox.Messaging.IMessageProvider CreateMessageProvider(string providerName)
	{
		CheckMessagingConfigurationLoaded();
		Assembly assembly = AssemblyList[providerName];
		Type[] types = assembly.GetTypes();
		Type type = null;
		Type[] array = types;
		foreach (Type type2 in array)
		{
			Type[] source = type2.FindInterfaces((Type typeObj, object criteriaObj) => typeObj.FullName.Equals(criteriaObj.ToString(), StringComparison.CurrentCultureIgnoreCase), "Redbox.Messaging.IMessageProvider");
			if (source.Count() > 0)
			{
				type = type2;
				break;
			}
		}
		if (type == null)
		{
			throw new ApplicationException("Could not load IMessageProvider type from assembly!");
		}
		return assembly.CreateInstance(type.FullName) as Redbox.Messaging.IMessageProvider;
	}

	public Redbox.Messaging.IConsumeMessages CreateConsumeMessages(string consumeMessagesName)
	{
		CheckMessagingConfigurationLoaded();
		Redbox.Messaging.ProduceConsumeConfiguration produceConsumeConfiguration = MessagingConfiguration.Consumers.Find((Redbox.Messaging.ProduceConsumeConfiguration c) => c.Name.Equals(consumeMessagesName, StringComparison.CurrentCultureIgnoreCase));
		Redbox.Messaging.ConnectionConfiguration connectionConfiguration = GetConnectionConfiguration(produceConsumeConfiguration.ConnectionConfigurationName);
		Redbox.Messaging.IConnectionSettings configuration = CreateConnectionSettings(connectionConfiguration);
		Redbox.Messaging.IMessageProvider messageProvider = CreateMessageProvider(connectionConfiguration.ProviderName);
		Redbox.Messaging.IConsumeMessages consumeMessages = messageProvider.CreateConsumeMessages(configuration);
		consumeMessages.Name = produceConsumeConfiguration.Name;
		consumeMessages.Throttle = produceConsumeConfiguration.Throttle;
		consumeMessages.MessageConsumerCount = produceConsumeConfiguration.InstanceCount;
		consumeMessages.ShareConnection = produceConsumeConfiguration.ShareConnection;
		consumeMessages.MessageThroughput = produceConsumeConfiguration.MaxThroughput;
		consumeMessages.ReceiveTimeout = produceConsumeConfiguration.ReceiveTimeout;
		return consumeMessages;
	}

	public Redbox.Messaging.IProduceMessages CreateProduceMessages(string produceMessagesName)
	{
		CheckMessagingConfigurationLoaded();
		Redbox.Messaging.ProduceConsumeConfiguration produceConsumeConfiguration = MessagingConfiguration.Producers.Find((Redbox.Messaging.ProduceConsumeConfiguration c) => c.Name.Equals(produceMessagesName, StringComparison.CurrentCultureIgnoreCase));
		Redbox.Messaging.ConnectionConfiguration connectionConfiguration = GetConnectionConfiguration(produceConsumeConfiguration.ConnectionConfigurationName);
		Redbox.Messaging.IConnectionSettings configuration = CreateConnectionSettings(connectionConfiguration);
		Redbox.Messaging.IMessageProvider messageProvider = CreateMessageProvider(connectionConfiguration.ProviderName);
		Redbox.Messaging.IProduceMessages produceMessages = messageProvider.CreateProduceMessages(configuration);
		produceMessages.Name = produceConsumeConfiguration.Name;
		produceMessages.MessageProducerCount = produceConsumeConfiguration.InstanceCount;
		produceMessages.ShareConnection = produceConsumeConfiguration.ShareConnection;
		produceMessages.Throttle = produceConsumeConfiguration.Throttle;
		produceMessages.MessageThroughput = produceConsumeConfiguration.MaxThroughput;
		return produceMessages;
	}

	private Redbox.Messaging.IConnectionSettings CreateConnectionSettings(Redbox.Messaging.ConnectionConfiguration connection)
	{
		Redbox.Messaging.IMessageProvider messageProvider = CreateMessageProvider(connection.ProviderName);
		Redbox.Messaging.IConnectionSettings connectionSettings = messageProvider.CreateConfiguration();
		connectionSettings.Name = connection.Name;
		connectionSettings.ConnectionString = connection.ConnectionString;
		connectionSettings.DestinationName = connection.DestinationName;
		connectionSettings.TransactionalMessages = connection.TransactionalMessages;
		connectionSettings.DestinationType = connection.DestinationType;
		connectionSettings.UserName = connection.UserName;
		connectionSettings.Password = connection.Password;
		connectionSettings.Properties = connection.Properties;
		connectionSettings.MaxConnectionPoolSize = connection.MaxConnectionPoolSize;
		connectionSettings.MinConnectionPoolSize = connection.MinConnectionPoolSize;
		return connectionSettings;
	}

	private Redbox.Messaging.ConnectionConfiguration GetConnectionConfiguration(string connectionName)
	{
		CheckMessagingConfigurationLoaded();
		Redbox.Messaging.ConnectionConfiguration connectionConfiguration = MessagingConfiguration.Connections.Find((Redbox.Messaging.ConnectionConfiguration c) => c.Name.Equals(connectionName, StringComparison.CurrentCultureIgnoreCase));
		if (connectionConfiguration == null)
		{
			throw new ArgumentException("GetProviderConnection parameter connectionName not found.");
		}
		return connectionConfiguration;
	}

	private void CheckMessagingConfigurationLoaded()
	{
		if (MessagingConfiguration == null)
		{
			throw new ApplicationException("MessagingFactory.MessagingConfiguration is null.  MessagingFactory must be initialized with a MessagingConfiguration.");
		}
	}

	private void LoadAssemblies()
	{
		MessagingConfiguration.Providers.ForEach(delegate(Redbox.Messaging.ProviderConfiguration p)
		{
			if (!m_assemblyList.ContainsKey(p.Name))
			{
				Assembly value = Assembly.Load(p.Type);
				m_assemblyList.Add(p.Name, value);
			}
		});
	}

	private List<Redbox.Messaging.IConsumeMessages> GetGroupConsumeMessagesList(Redbox.Messaging.ConnectionGroupConfiguration group)
	{
		List<Redbox.Messaging.IConsumeMessages> list = new List<Redbox.Messaging.IConsumeMessages>();
		if (group != null)
		{
			for (int i = 0; i < group.ConsumeConnections.Count; i++)
			{
				Redbox.Messaging.ProduceConsumeConfiguration produceConsumeConfiguration = group.ConsumeConnections[i];
				Redbox.Messaging.ConnectionConfiguration connectionConfiguration = GetConnectionConfiguration(produceConsumeConfiguration.ConnectionConfigurationName);
				Redbox.Messaging.IMessageProvider messageProvider = CreateMessageProvider(connectionConfiguration.ProviderName);
				Redbox.Messaging.IConnectionSettings configuration = CreateConnectionSettings(connectionConfiguration);
				Redbox.Messaging.IConsumeMessages consumeMessages = messageProvider.CreateConsumeMessages(configuration);
				consumeMessages.Name = produceConsumeConfiguration.Name;
				consumeMessages.MessageConsumerCount = produceConsumeConfiguration.InstanceCount;
				consumeMessages.ShareConnection = produceConsumeConfiguration.ShareConnection;
				consumeMessages.Throttle = produceConsumeConfiguration.Throttle;
				consumeMessages.MessageThroughput = produceConsumeConfiguration.MaxThroughput;
				consumeMessages.BatchConsume = produceConsumeConfiguration.BatchConsume;
				consumeMessages.BatchCount = produceConsumeConfiguration.BatchCount;
				consumeMessages.ReceiveTimeout = produceConsumeConfiguration.ReceiveTimeout;
				list.Add(consumeMessages);
			}
		}
		return list;
	}

	private List<Redbox.Messaging.IProduceMessages> GetGroupProduceMessagesList(Redbox.Messaging.ConnectionGroupConfiguration group)
	{
		List<Redbox.Messaging.IProduceMessages> list = new List<Redbox.Messaging.IProduceMessages>();
		if (group != null)
		{
			for (int i = 0; i < group.ProduceConnections.Count; i++)
			{
				Redbox.Messaging.ProduceConsumeConfiguration produceConsumeConfiguration = group.ProduceConnections[i];
				Redbox.Messaging.ConnectionConfiguration connectionConfiguration = GetConnectionConfiguration(produceConsumeConfiguration.ConnectionConfigurationName);
				Redbox.Messaging.IMessageProvider messageProvider = CreateMessageProvider(connectionConfiguration.ProviderName);
				Redbox.Messaging.IConnectionSettings configuration = CreateConnectionSettings(connectionConfiguration);
				Redbox.Messaging.IProduceMessages produceMessages = messageProvider.CreateProduceMessages(configuration);
				produceMessages.Name = produceConsumeConfiguration.Name;
				produceMessages.MessageProducerCount = produceConsumeConfiguration.InstanceCount;
				produceMessages.ShareConnection = produceConsumeConfiguration.ShareConnection;
				produceMessages.Throttle = produceConsumeConfiguration.Throttle;
				produceMessages.MessageThroughput = produceConsumeConfiguration.MaxThroughput;
				list.Add(produceMessages);
			}
		}
		return list;
	}
}
