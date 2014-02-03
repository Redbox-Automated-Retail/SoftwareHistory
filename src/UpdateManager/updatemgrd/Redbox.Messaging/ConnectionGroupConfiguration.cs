using System.Collections.Generic;

namespace Redbox.Messaging;

internal class ConnectionGroupConfiguration
{
	private readonly List<ProduceConsumeConfiguration> m_consumeConfigurations = new List<ProduceConsumeConfiguration>();

	private readonly List<ProduceConsumeConfiguration> m_produceConfigurations = new List<ProduceConsumeConfiguration>();

	public string Name { get; set; }

	public List<ProduceConsumeConfiguration> ConsumeConnections => m_consumeConfigurations;

	public List<ProduceConsumeConfiguration> ProduceConnections => m_produceConfigurations;
}
