using System;

namespace Redbox.Messaging;

internal interface IMessageReply : Redbox.Messaging.IMessageWorker, IDisposable
{
	Redbox.Messaging.MessagingResult ReceiveRequest(TimeSpan wait, out Redbox.Messaging.IMessage message);

	Redbox.Messaging.MessagingResult SendReply(Redbox.Messaging.IMessage requestMessage, Redbox.Messaging.IMessage replyMessage);
}
