using System;
using System.Collections.Generic;

namespace Redbox.Messaging;

internal interface IProduceMessages
{
	string Name { get; set; }

	IMessageProvider MessageProvider { get; }

	MessageThroughput MessageThroughput { get; set; }

	List<IMessageProducer> ProducerList { get; }

	int MessageProducerCount { get; set; }

	bool Throttle { get; set; }

	bool Active { get; }

	bool ShareConnection { get; set; }

	IConnectionSettings Configuration { get; }

	long MessagesSent { get; }

	MessagingResult Start(Action<IMessageProducer, List<IMessage>> afterSendMesage);

	MessagingResult Stop();

	MessagingResult SendMessage(IMessage message);

	MessagingResult SendMessage(List<IMessage> messageList);
}
