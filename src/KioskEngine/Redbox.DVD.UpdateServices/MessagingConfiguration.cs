using System.Collections.Generic;

namespace Redbox.Messaging;

internal class MessagingConfiguration
{
	private readonly List<Redbox.Messaging.ProviderConfiguration> m_providers = new List<Redbox.Messaging.ProviderConfiguration>();

	private readonly List<Redbox.Messaging.ConnectionConfiguration> m_connections = new List<Redbox.Messaging.ConnectionConfiguration>();

	private readonly List<Redbox.Messaging.ProduceConsumeConfiguration> m_producers = new List<Redbox.Messaging.ProduceConsumeConfiguration>();

	private readonly List<Redbox.Messaging.ProduceConsumeConfiguration> m_consumers = new List<Redbox.Messaging.ProduceConsumeConfiguration>();

	private readonly List<Redbox.Messaging.ConnectionGroupConfiguration> m_connectionGroups = new List<Redbox.Messaging.ConnectionGroupConfiguration>();

	public List<Redbox.Messaging.ProviderConfiguration> Providers => m_providers;

	public List<Redbox.Messaging.ConnectionConfiguration> Connections => m_connections;

	public List<Redbox.Messaging.ProduceConsumeConfiguration> Producers => m_producers;

	public List<Redbox.Messaging.ProduceConsumeConfiguration> Consumers => m_consumers;

	public List<Redbox.Messaging.ConnectionGroupConfiguration> Groups => m_connectionGroups;
}
