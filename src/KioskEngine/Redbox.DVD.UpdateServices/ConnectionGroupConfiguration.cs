using System.Collections.Generic;

namespace Redbox.Messaging;

internal class ConnectionGroupConfiguration
{
	private readonly List<Redbox.Messaging.ProduceConsumeConfiguration> m_consumeConfigurations = new List<Redbox.Messaging.ProduceConsumeConfiguration>();

	private readonly List<Redbox.Messaging.ProduceConsumeConfiguration> m_produceConfigurations = new List<Redbox.Messaging.ProduceConsumeConfiguration>();

	public string Name { get; set; }

	public List<Redbox.Messaging.ProduceConsumeConfiguration> ConsumeConnections => m_consumeConfigurations;

	public List<Redbox.Messaging.ProduceConsumeConfiguration> ProduceConnections => m_produceConfigurations;
}
