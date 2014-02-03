using System;

namespace Redbox.Messaging;

internal interface IMessageReply : IMessageWorker, IDisposable
{
	MessagingResult ReceiveRequest(TimeSpan wait, out IMessage message);

	MessagingResult SendReply(IMessage requestMessage, IMessage replyMessage);
}
