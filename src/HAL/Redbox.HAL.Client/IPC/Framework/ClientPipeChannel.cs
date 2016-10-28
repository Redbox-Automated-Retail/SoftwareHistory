using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework;

internal class ClientPipeChannel : BasePipeChannel
{
	private readonly string PipeName;

	public override bool Connect()
	{
		if (base.Connected)
		{
			return false;
		}
		PipeHandle = BasePipeChannel.CreateFile(PipeName, 3221225472u, 0u, IntPtr.Zero, 3u, 0u, IntPtr.Zero);
		if (PipeHandle.IsInvalid)
		{
			return false;
		}
		return base.Connected = true;
	}

	public ClientPipeChannel(string pipeName)
	{
		PipeName = "\\\\.\\PIPE\\" + pipeName;
	}

	protected override bool DisposeChannel(bool fromDispose)
	{
		if (Disposed)
		{
			return true;
		}
		if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
		{
			LogHelper.Instance.Log("Client channel: dispose()");
		}
		PipeHandle.Close();
		base.Connected = false;
		return Disposed = true;
	}
}
