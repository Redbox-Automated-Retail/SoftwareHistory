using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework;

internal sealed class ServerPipeChannel : BasePipeChannel
{
	public override bool Connect()
	{
		LogHelper.Instance.Log($"ServerChannel ID = {base.ID}: Connect() improperly called.");
		return false;
	}

	internal ServerPipeChannel(SafeFileHandle pipeHandle)
		: base(pipeHandle)
	{
	}

	protected override bool DisposeChannel(bool fromDispose)
	{
		if (Disposed)
		{
			return true;
		}
		BasePipeChannel.FlushFileBuffers(PipeHandle);
		DisconnectNamedPipe(PipeHandle);
		PipeHandle.Close();
		base.Connected = false;
		return Disposed = true;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool DisconnectNamedPipe(SafeFileHandle hHandle);
}
