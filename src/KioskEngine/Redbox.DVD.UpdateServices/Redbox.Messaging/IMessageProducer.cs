using System;
using System.Collections.Generic;

namespace Redbox.Messaging;

internal interface IMessageProducer : Redbox.Messaging.IMessageWorker, IDisposable
{
	Redbox.Messaging.MessagingResult SendMessage(Redbox.Messaging.IMessage message);

	Redbox.Messaging.MessagingResult SendMessageList(List<Redbox.Messaging.IMessage> messageList);

	Redbox.Messaging.MessagingResult BeginTransaction();

	Redbox.Messaging.MessagingResult Commit();

	Redbox.Messaging.MessagingResult RollBack();
}
