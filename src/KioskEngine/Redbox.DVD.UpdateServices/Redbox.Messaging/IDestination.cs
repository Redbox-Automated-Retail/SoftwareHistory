using System;
using System.Collections.Generic;

namespace Redbox.Messaging;

internal interface IDestination : IDisposable
{
	Redbox.Messaging.IConnectionSettings Configuration { get; set; }

	Redbox.Messaging.DestinationConnectionType DestinationConnectionType { get; set; }

	bool IsOpen { get; }

	Redbox.Messaging.MessagingResult Open();

	Redbox.Messaging.MessagingResult Close();

	Redbox.Messaging.MessagingResult Receive(TimeSpan wait, out Redbox.Messaging.IMessage message);

	Redbox.Messaging.MessagingResult ReceiveList(TimeSpan wait, int count, out List<Redbox.Messaging.IMessage> messageList);

	Redbox.Messaging.MessagingResult ReceiveReply(TimeSpan wait, Redbox.Messaging.IMessage requestMessage, out Redbox.Messaging.IMessage message);

	Redbox.Messaging.MessagingResult SendMessage(Redbox.Messaging.IMessage message);

	Redbox.Messaging.MessagingResult SendMessageList(List<Redbox.Messaging.IMessage> messageList);

	Redbox.Messaging.MessagingResult Peek(TimeSpan wait, out Redbox.Messaging.IMessage message);

	Redbox.Messaging.MessagingResult PeekList(TimeSpan wait, int count, out List<Redbox.Messaging.IMessage> messageList);

	Redbox.Messaging.MessagingResult BeginTransaction();

	Redbox.Messaging.MessagingResult Commit();

	Redbox.Messaging.MessagingResult RollBack();

	Redbox.Messaging.MessagingResult GetMessageCount(out long count);
}
