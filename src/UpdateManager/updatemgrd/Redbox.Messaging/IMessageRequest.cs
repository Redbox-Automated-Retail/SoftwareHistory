using System;

namespace Redbox.Messaging;

internal interface IMessageRequest : IMessageWorker, IDisposable
{
	MessagingResult SendRequest(TimeSpan wait, IMessage request, out IMessage reply);
}
