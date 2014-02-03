using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Redbox.Messaging;

internal class MessagingFactory
{
	private MessagingConfiguration m_messagingConfiguration;

	private readonly Dictionary<string, Assembly> m_assemblyList = new Dictionary<string, Assembly>();

	public MessagingConfiguration MessagingConfiguration => m_messagingConfiguration;

	private Dictionary<string, Assembly> AssemblyList => m_assemblyList;

	public MessagingFactory(MessagingConfiguration messagingConfiguration)
	{
		m_messagingConfiguration = messagingConfiguration;
		LoadAssemblies();
	}

	public MessagingGroup CreateMessagingGroup(string groupName)
	{
		CheckMessagingConfigurationLoaded();
		ConnectionGroupConfiguration connectionGroupConfiguration = MessagingConfiguration.Groups.Find((ConnectionGroupConfiguration g) => g.Name.Equals(groupName, StringComparison.CurrentCultureIgnoreCase));
		if (connectionGroupConfiguration == null)
		{
			throw new ArgumentException("GetMessagingGroup parameter groupName not found.");
		}
		MessagingGroup messagingGroup = new MessagingGroup();
		messagingGroup.Name = groupName;
		messagingGroup.ConsumeMessagesList = GetGroupConsumeMessagesList(connectionGroupConfiguration);
		messagingGroup.ProduceMessagesList = GetGroupProduceMessagesList(connectionGroupConfiguration);
		return messagingGroup;
	}

	public IProviderConnection CreateProviderConnection(string connectionName)
	{
		CheckMessagingConfigurationLoaded();
		ConnectionConfiguration connectionConfiguration = GetConnectionConfiguration(connectionName);
		IMessageProvider messageProvider = CreateMessageProvider(connectionConfiguration.ProviderName);
		IConnectionSettings configuration = CreateConnectionSettings(connectionConfiguration);
		return messageProvider.CreateConnection(configuration);
	}

	public IMessageProvider CreateMessageProvider(string providerName)
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
		return assembly.CreateInstance(type.FullName) as IMessageProvider;
	}

	public IConsumeMessages CreateConsumeMessages(string consumeMessagesName)
	{
		CheckMessagingConfigurationLoaded();
		ProduceConsumeConfiguration produceConsumeConfiguration = MessagingConfiguration.Consumers.Find((ProduceConsumeConfiguration c) => c.Name.Equals(consumeMessagesName, StringComparison.CurrentCultureIgnoreCase));
		ConnectionConfiguration connectionConfiguration = GetConnectionConfiguration(produceConsumeConfiguration.ConnectionConfigurationName);
		IConnectionSettings configuration = CreateConnectionSettings(connectionConfiguration);
		IMessageProvider messageProvider = CreateMessageProvider(connectionConfiguration.ProviderName);
		IConsumeMessages consumeMessages = messageProvider.CreateConsumeMessages(configuration);
		consumeMessages.Name = produceConsumeConfiguration.Name;
		consumeMessages.Throttle = produceConsumeConfiguration.Throttle;
		consumeMessages.MessageConsumerCount = produceConsumeConfiguration.InstanceCount;
		consumeMessages.ShareConnection = produceConsumeConfiguration.ShareConnection;
		consumeMessages.MessageThroughput = produceConsumeConfiguration.MaxThroughput;
		consumeMessages.ReceiveTimeout = produceConsumeConfiguration.ReceiveTimeout;
		return consumeMessages;
	}

	public IProduceMessages CreateProduceMessages(string produceMessagesName)
	{
		CheckMessagingConfigurationLoaded();
		ProduceConsumeConfiguration produceConsumeConfiguration = MessagingConfiguration.Producers.Find((ProduceConsumeConfiguration c) => c.Name.Equals(produceMessagesName, StringComparison.CurrentCultureIgnoreCase));
		ConnectionConfiguration connectionConfiguration = GetConnectionConfiguration(produceConsumeConfiguration.ConnectionConfigurationName);
		IConnectionSettings configuration = CreateConnectionSettings(connectionConfiguration);
		IMessageProvider messageProvider = CreateMessageProvider(connectionConfiguration.ProviderName);
		IProduceMessages produceMessages = messageProvider.CreateProduceMessages(configuration);
		produceMessages.Name = produceConsumeConfiguration.Name;
		produceMessages.MessageProducerCount = produceConsumeConfiguration.InstanceCount;
		produceMessages.ShareConnection = produceConsumeConfiguration.ShareConnection;
		produceMessages.Throttle = produceConsumeConfiguration.Throttle;
		produceMessages.MessageThroughput = produceConsumeConfiguration.MaxThroughput;
		return produceMessages;
	}

	private IConnectionSettings CreateConnectionSettings(ConnectionConfiguration connection)
	{
		IMessageProvider messageProvider = CreateMessageProvider(connection.ProviderName);
		IConnectionSettings connectionSettings = messageProvider.CreateConfiguration();
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

	private ConnectionConfiguration GetConnectionConfiguration(string connectionName)
	{
		CheckMessagingConfigurationLoaded();
		ConnectionConfiguration connectionConfiguration = MessagingConfiguration.Connections.Find((ConnectionConfiguration c) => c.Name.Equals(connectionName, StringComparison.CurrentCultureIgnoreCase));
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
		MessagingConfiguration.Providers.ForEach(delegate(ProviderConfiguration p)
		{
			if (!m_assemblyList.ContainsKey(p.Name))
			{
				Assembly value = Assembly.Load(p.Type);
				m_assemblyList.Add(p.Name, value);
			}
		});
	}

	private List<IConsumeMessages> GetGroupConsumeMessagesList(ConnectionGroupConfiguration group)
	{
		List<IConsumeMessages> list = new List<IConsumeMessages>();
		if (group != null)
		{
			for (int i = 0; i < group.ConsumeConnections.Count; i++)
			{
				ProduceConsumeConfiguration produceConsumeConfiguration = group.ConsumeConnections[i];
				ConnectionConfiguration connectionConfiguration = GetConnectionConfiguration(produceConsumeConfiguration.ConnectionConfigurationName);
				IMessageProvider messageProvider = CreateMessageProvider(connectionConfiguration.ProviderName);
				IConnectionSettings configuration = CreateConnectionSettings(connectionConfiguration);
				IConsumeMessages consumeMessages = messageProvider.CreateConsumeMessages(configuration);
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

	private List<IProduceMessages> GetGroupProduceMessagesList(ConnectionGroupConfiguration group)
	{
		List<IProduceMessages> list = new List<IProduceMessages>();
		if (group != null)
		{
			for (int i = 0; i < group.ProduceConnections.Count; i++)
			{
				ProduceConsumeConfiguration produceConsumeConfiguration = group.ProduceConnections[i];
				ConnectionConfiguration connectionConfiguration = GetConnectionConfiguration(produceConsumeConfiguration.ConnectionConfigurationName);
				IMessageProvider messageProvider = CreateMessageProvider(connectionConfiguration.ProviderName);
				IConnectionSettings configuration = CreateConnectionSettings(connectionConfiguration);
				IProduceMessages produceMessages = messageProvider.CreateProduceMessages(configuration);
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
