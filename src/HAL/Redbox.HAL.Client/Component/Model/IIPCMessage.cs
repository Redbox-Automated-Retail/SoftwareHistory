using System;

namespace Redbox.HAL.Component.Model;

internal interface IIPCMessage
{
	MessagePriority Priority { get; }

	MessageSeverity Severity { get; }

	string Message { get; }

	Guid UID { get; }

	DateTime Timestamp { get; }
}
