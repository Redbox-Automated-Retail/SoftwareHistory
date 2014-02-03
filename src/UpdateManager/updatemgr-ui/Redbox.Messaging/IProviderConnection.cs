using System;

namespace Redbox.Messaging;

internal interface IProviderConnection : IDisposable
{
	bool Active { get; }

	IConnectionSettings Configuration { get; set; }

	bool DestroyConnectionOnClose { get; set; }

	IMessageConsumer CreateConsumer();

	IMessageProducer CreateProducer();

	IDestination CreateDestination();

	IMessage CreateMessage();

	MessagingResult Open();

	MessagingResult Close();
}
