using System;
using System.Collections.Generic;

namespace Redbox.Messaging;

internal interface IDestination : IDisposable
{
	IConnectionSettings Configuration { get; set; }

	DestinationConnectionType DestinationConnectionType { get; set; }

	bool IsOpen { get; }

	MessagingResult Open();

	MessagingResult Close();

	MessagingResult Receive(TimeSpan wait, out IMessage message);

	MessagingResult ReceiveList(TimeSpan wait, int count, out List<IMessage> messageList);

	MessagingResult ReceiveReply(TimeSpan wait, IMessage requestMessage, out IMessage message);

	MessagingResult SendMessage(IMessage message);

	MessagingResult SendMessageList(List<IMessage> messageList);

	MessagingResult Peek(TimeSpan wait, out IMessage message);

	MessagingResult PeekList(TimeSpan wait, int count, out List<IMessage> messageList);

	MessagingResult BeginTransaction();

	MessagingResult Commit();

	MessagingResult RollBack();

	MessagingResult GetMessageCount(out long count);
}
