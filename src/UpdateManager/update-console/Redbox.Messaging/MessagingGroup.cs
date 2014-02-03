using System.Collections.Generic;

namespace Redbox.Messaging;

internal class MessagingGroup
{
	private readonly List<IConsumeMessages> m_consumeMessagesList = new List<IConsumeMessages>();

	private readonly List<IProduceMessages> m_produceMessagesList = new List<IProduceMessages>();

	public List<IConsumeMessages> ConsumeMessagesList
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

	public List<IProduceMessages> ProduceMessagesList
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
