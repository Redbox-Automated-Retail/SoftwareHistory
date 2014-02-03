using System.Collections.Generic;

namespace Redbox.Messaging;

internal class MessagingConfiguration
{
	private readonly List<ProviderConfiguration> m_providers = new List<ProviderConfiguration>();

	private readonly List<ConnectionConfiguration> m_connections = new List<ConnectionConfiguration>();

	private readonly List<ProduceConsumeConfiguration> m_producers = new List<ProduceConsumeConfiguration>();

	private readonly List<ProduceConsumeConfiguration> m_consumers = new List<ProduceConsumeConfiguration>();

	private readonly List<ConnectionGroupConfiguration> m_connectionGroups = new List<ConnectionGroupConfiguration>();

	public List<ProviderConfiguration> Providers => m_providers;

	public List<ConnectionConfiguration> Connections => m_connections;

	public List<ProduceConsumeConfiguration> Producers => m_producers;

	public List<ProduceConsumeConfiguration> Consumers => m_consumers;

	public List<ConnectionGroupConfiguration> Groups => m_connectionGroups;
}
