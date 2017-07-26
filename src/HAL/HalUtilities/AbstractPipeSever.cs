using System;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.IPC.Framework.Pipes;

namespace HALUtilities;

internal abstract class AbstractPipeSever
{
	protected readonly string PipeName;

	protected volatile bool IsAlive;

	protected abstract void OnStop();

	protected abstract void Process(BasePipeChannel channel);

	protected AbstractPipeSever(string pipeName)
	{
		PipeName = pipeName;
	}

	internal virtual void Run()
	{
		LogHelper.Instance.Log("Abstract[PipeServer] {0} Server running on {1}", DateTime.Now, PipeName);
		IsAlive = true;
		using (NamedPipeServer namedPipeServer = NamedPipeServer.Create(PipeName))
		{
			while (IsAlive)
			{
				BasePipeChannel channel = namedPipeServer.WaitForClientConnect();
				if (channel != null)
				{
					ThreadPool.QueueUserWorkItem(delegate
					{
						Process(channel);
					});
				}
			}
		}
		LogHelper.Instance.Log("[AbstractPipeServer] {0} Exiting.", DateTime.Now);
	}

	internal void Stop()
	{
		IsAlive = false;
		OnStop();
	}
}
