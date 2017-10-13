using System.Collections.Generic;

namespace Redbox.Messaging;

internal class MessagingGroup
{
	private readonly List<Redbox.Messaging.IConsumeMessages> m_consumeMessagesList = new List<Redbox.Messaging.IConsumeMessages>();

	private readonly List<Redbox.Messaging.IProduceMessages> m_produceMessagesList = new List<Redbox.Messaging.IProduceMessages>();

	public List<Redbox.Messaging.IConsumeMessages> ConsumeMessagesList
	{
		get
		{
			return m_consumeMessagesList;
		}
		set
		{
			m_consumeMessagesList.Clear();
			m_consumeMessagesList.AddRange(value);
		}
	}

	public List<Redbox.Messaging.IProduceMessages> ProduceMessagesList
	{
		get
		{
			return m_produceMessagesList;
		}
		set
		{
			m_produceMessagesList.Clear();
			m_produceMessagesList.AddRange(value);
		}
	}

	public string Name { get; set; }

	internal MessagingGroup()
	{
	}
}
