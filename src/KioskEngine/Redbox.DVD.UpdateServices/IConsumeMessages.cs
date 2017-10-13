using System;
using System.Collections.Generic;

namespace Redbox.Messaging;

internal interface IConsumeMessages
{
	string Name { get; set; }

	Redbox.Messaging.IMessageProvider MessageProvider { get; }

	Redbox.Messaging.MessageThroughput MessageThroughput { get; set; }

	List<Redbox.Messaging.IMessageConsumer> ConsumerList { get; }

	int MessageConsumerCount { get; set; }

	bool Throttle { get; set; }

	bool Active { get; }

	bool ShareConnection { get; set; }

	Redbox.Messaging.IConnectionSettings Configuration { get; }

	long MessagesReceived { get; }

	Action<Redbox.Messaging.MessagingResult, Redbox.Messaging.IConsumeMessages> ListenFailed { get; set; }

	bool BatchConsume { get; set; }

	int BatchCount { get; set; }

	TimeSpan ReceiveTimeout { get; set; }

	Redbox.Messaging.MessagingResult Start(Action<Redbox.Messaging.IMessageConsumer, List<Redbox.Messaging.IMessage>> receiveMessage, Action<Redbox.Messaging.IMessageConsumer, List<Redbox.Messaging.IMessage>> afterReceiveMessage);

	Redbox.Messaging.MessagingResult Stop();
}
