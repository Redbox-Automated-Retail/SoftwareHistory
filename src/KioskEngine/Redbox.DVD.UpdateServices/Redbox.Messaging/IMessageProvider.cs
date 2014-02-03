namespace Redbox.Messaging;

internal interface IMessageProvider
{
	Redbox.Messaging.IProviderConnection CreateConnection(Redbox.Messaging.IConnectionSettings configuration);

	Redbox.Messaging.IConsumeMessages CreateConsumeMessages(Redbox.Messaging.IConnectionSettings configuration);

	Redbox.Messaging.IProduceMessages CreateProduceMessages(Redbox.Messaging.IConnectionSettings configuration);

	Redbox.Messaging.IConnectionSettings CreateConfiguration();
}
