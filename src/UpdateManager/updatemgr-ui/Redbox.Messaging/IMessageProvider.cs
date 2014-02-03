namespace Redbox.Messaging;

internal interface IMessageProvider
{
	IProviderConnection CreateConnection(IConnectionSettings configuration);

	IConsumeMessages CreateConsumeMessages(IConnectionSettings configuration);

	IProduceMessages CreateProduceMessages(IConnectionSettings configuration);

	IConnectionSettings CreateConfiguration();
}
