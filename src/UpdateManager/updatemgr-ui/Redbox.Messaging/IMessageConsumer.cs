using System;
using System.Collections.Generic;

namespace Redbox.Messaging;

internal interface IMessageConsumer : IMessageWorker, IDisposable
{
	MessagingResult Receive(out IMessage message);

	MessagingResult Receive(TimeSpan wait, out IMessage message);

	MessagingResult ReceiveList(TimeSpan wait, int count, out List<IMessage> messageList);

	MessagingResult BeginTransaction();

	MessagingResult Commit();

	MessagingResult RollBack();
}
