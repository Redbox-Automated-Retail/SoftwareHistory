using System;

namespace Redbox.IPC.Framework;

[Obsolete("Use ClientSessionPool instead")]
public class TcpClientSessionPool : ClientSessionPool
{
	public TcpClientSessionPool(Redbox_002EIPC_002EFramework1769326_002EIPCProtocol protocol, int poolsize)
		: base(protocol, poolsize)
	{
	}

	public TcpClientSessionPool(Redbox_002EIPC_002EFramework1769326_002EIPCProtocol protocol, int poolsize, int? timeout)
		: base(protocol, poolsize, timeout)
	{
	}
}
