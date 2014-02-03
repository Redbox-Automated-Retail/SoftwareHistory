using System;
using System.Collections.Generic;

namespace Redbox.Messaging;

internal interface IProduceMessages
{
	string Name { get; set; }

	Redbox.Messaging.IMessageProvider MessageProvider { get; }

	Redbox.Messaging.MessageThroughput MessageThroughput { get; set; }

	List<Redbox.Messaging.IMessageProducer> ProducerList { get; }

	int MessageProducerCount { get; set; }

	bool Throttle { get; set; }

	bool Active { get; }

	bool ShareConnection { get; set; }

	Redbox.Messaging.IConnectionSettings Configuration { get; }

	long MessagesSent { get; }

	Redbox.Messaging.MessagingResult Start(Action<Redbox.Messaging.IMessageProducer, List<Redbox.Messaging.IMessage>> afterSendMesage);

	Redbox.Messaging.MessagingResult Stop();

	Redbox.Messaging.MessagingResult SendMessage(Redbox.Messaging.IMessage message);

	Redbox.Messaging.MessagingResult SendMessage(List<Redbox.Messaging.IMessage> messageList);
}
