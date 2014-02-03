using System;

namespace Redbox.Messaging;

internal interface IMessageWorker : IDisposable
{
	bool IsOpen { get; }

	long MessagesSent { get; set; }

	long MessagesReceived { get; set; }

	MessagingResult Open();

	MessagingResult Close();
}
