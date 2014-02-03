using System;
using System.Collections.Generic;

namespace Redbox.Messaging;

internal interface IMessageProducer : IMessageWorker, IDisposable
{
	MessagingResult SendMessage(IMessage message);

	MessagingResult SendMessageList(List<IMessage> messageList);

	MessagingResult BeginTransaction();

	MessagingResult Commit();

	MessagingResult RollBack();
}
