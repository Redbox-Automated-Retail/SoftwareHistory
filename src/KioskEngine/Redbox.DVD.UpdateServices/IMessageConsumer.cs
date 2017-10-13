using System;
using System.Collections.Generic;

namespace Redbox.Messaging;

internal interface IMessageConsumer : Redbox.Messaging.IMessageWorker, IDisposable
{
	Redbox.Messaging.MessagingResult Receive(out Redbox.Messaging.IMessage message);

	Redbox.Messaging.MessagingResult Receive(TimeSpan wait, out Redbox.Messaging.IMessage message);

	Redbox.Messaging.MessagingResult ReceiveList(TimeSpan wait, int count, out List<Redbox.Messaging.IMessage> messageList);

	Redbox.Messaging.MessagingResult BeginTransaction();

	Redbox.Messaging.MessagingResult Commit();

	Redbox.Messaging.MessagingResult RollBack();
}
