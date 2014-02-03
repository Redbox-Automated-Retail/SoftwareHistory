namespace Belikov.GenuineChannels.Receiving;

internal interface IMessageRegistrator
{
	bool WasRegistered(string uri, int messageId, int replyId);
}
