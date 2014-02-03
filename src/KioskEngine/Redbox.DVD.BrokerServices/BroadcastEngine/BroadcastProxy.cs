using System;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using Belikov.Common.ThreadProcessing;

namespace Belikov.GenuineChannels.BroadcastEngine;

internal class BroadcastProxy : RealProxy
{
	private Dispatcher _dispatcher;

	public BroadcastProxy(Type interfaceToSupport, Dispatcher dispatcher)
		: base(interfaceToSupport)
	{
		_dispatcher = dispatcher;
	}

	public override IMessage Invoke(IMessage msg)
	{
		ResultCollector resultCollector = null;
		ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(_dispatcher._readerWriterLock);
		try
		{
			resultCollector = new ResultCollector(_dispatcher, _dispatcher._receivers.Count + 3, msg);
		}
		finally
		{
			readerAutoLocker.Dispose();
		}
		resultCollector.PerformBroadcasting(msg);
		return new ReturnMessage(resultCollector, null, 0, (LogicalCallContext)msg.Properties["__CallContext"], (IMethodCallMessage)msg);
	}
}
