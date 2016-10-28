using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework;

internal sealed class IPCMessage : IIPCMessage
{
	public MessagePriority Priority
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public MessageSeverity Severity
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public string Message
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public Guid UID
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public DateTime Timestamp
	{
		get
		{
			throw new NotImplementedException();
		}
	}
}
