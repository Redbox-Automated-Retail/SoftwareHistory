using System;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework;

namespace HALUtilities;

public sealed class PipeMessageServer
{
	private class F : IIPCMessage
	{
		public MessagePriority Priority { get; internal set; }

		public MessageSeverity Severity { get; internal set; }

		public string Message { get; internal set; }

		public Guid UID { get; internal set; }

		public DateTime Timestamp { get; internal set; }

		public bool Write()
		{
			throw new NotImplementedException();
		}

		internal F()
		{
		}
	}

	private volatile bool IsAlive;

	private readonly string Name;

	public void Run()
	{
		IsAlive = true;
		using NamedPipeServer namedPipeServer = NamedPipeServer.CreateServer(Name);
		while (IsAlive)
		{
			BasePipeChannel channel = namedPipeServer.WaitForClientConnect();
			if (channel != null)
			{
				ThreadPool.QueueUserWorkItem(delegate
				{
					ProcessMessage(channel);
				});
			}
		}
	}

	internal PipeMessageServer(string name)
	{
		Name = name;
	}

	private void ProcessMessage(BasePipeChannel channel)
	{
	}
}
