using System;

namespace Redbox.Messaging;

internal interface IProviderConnection : IDisposable
{
	bool Active { get; }

	Redbox.Messaging.IConnectionSettings Configuration { get; set; }

	bool DestroyConnectionOnClose { get; set; }

	Redbox.Messaging.IMessageConsumer CreateConsumer();

	Redbox.Messaging.IMessageProducer CreateProducer();

	Redbox.Messaging.IDestination CreateDestination();

	Redbox.Messaging.IMessage CreateMessage();

	Redbox.Messaging.MessagingResult Open();

	Redbox.Messaging.MessagingResult Close();
}
