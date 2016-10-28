using Microsoft.Win32.SafeHandles;

namespace Redbox.HAL.IPC.Framework;

internal interface PipeSessionFactory
{
	AbstractNamedPipeServerSession CreateSession(SafeFileHandle pipeHandle);
}
