using System;

namespace Redbox.Messaging;

internal interface IMessageRequest : Redbox.Messaging.IMessageWorker, IDisposable
{
	Redbox.Messaging.MessagingResult SendRequest(TimeSpan wait, Redbox.Messaging.IMessage request, out Redbox.Messaging.IMessage reply);
}
