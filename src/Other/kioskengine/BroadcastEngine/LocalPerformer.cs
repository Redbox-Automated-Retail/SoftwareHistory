using System;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;

namespace Belikov.GenuineChannels.BroadcastEngine;

[ComVisible(true)]
public class LocalPerformer
{
	private IMessage _msg;

	private ResultCollector _resultCollector;

	private MarshalByRefObject _mbr;

	private string _mbrUri;

	public LocalPerformer(IMessage msg, ResultCollector resultCollector, MarshalByRefObject mbr)
	{
		_msg = msg;
		_resultCollector = resultCollector;
		_mbr = mbr;
		_mbrUri = RemotingServices.GetObjectUri(mbr);
	}

	public void Call(object state)
	{
		try
		{
			IMethodReturnMessage returnMessage = RemotingServices.ExecuteMessage(_mbr, (IMethodCallMessage)_msg);
			_resultCollector.ParseResult(_mbrUri, returnMessage, null);
		}
		catch (Exception ex)
		{
			_resultCollector.ParseResult(_mbrUri, null, ex);
		}
	}
}
