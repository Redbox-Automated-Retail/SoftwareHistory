using System;
using System.Collections.Generic;

namespace Redbox.Messaging;

internal interface IConsumeMessages
{
	string Name { get; set; }

	IMessageProvider MessageProvider { get; }

	MessageThroughput MessageThroughput { get; set; }

	List<IMessageConsumer> ConsumerList { get; }

	int MessageConsumerCount { get; set; }

	bool Throttle { get; set; }

	bool Active { get; }

	bool ShareConnection { get; set; }

	IConnectionSettings Configuration { get; }

	long MessagesReceived { get; }

	Action<MessagingResult, IConsumeMessages> ListenFailed { get; set; }

	bool BatchConsume { get; set; }

	int BatchCount { get; set; }

	TimeSpan ReceiveTimeout { get; set; }

	MessagingResult Start(Action<IMessageConsumer, List<IMessage>> receiveMessage, Action<IMessageConsumer, List<IMessage>> afterReceiveMessage);

	MessagingResult Stop();
}
