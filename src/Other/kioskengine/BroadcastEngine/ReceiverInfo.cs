using System;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Threading;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.BroadcastEngine;

[ComVisible(true)]
public class ReceiverInfo
{
	public string MbrUri;

	public bool Local = false;

	public bool LocallyCreated = false;

	public bool NeedsBroadcastSimulation = true;

	public MarshalByRefObject MbrObject;

	public GeneralBroadcastSender GeneralBroadcastSender;

	public object SerializedObjRef;

	public IClientChannelSink IClientChannelSink;

	public int NumberOfFails = 0;

	public int NumberOfMulticastFails = 0;

	private object _tag;

	private object _tagLock = new object();

	private int _dbgRecipientId = Interlocked.Increment(ref _dbgTotalRecipients);

	private static int _dbgTotalRecipients = 0;

	private HostInformation _dbgRemoteHost;

	public object Tag
	{
		get
		{
			lock (_tagLock)
			{
				return _tag;
			}
		}
		set
		{
			lock (_tagLock)
			{
				_tag = value;
			}
		}
	}

	public int DbgRecipientId => _dbgRecipientId;

	public HostInformation DbgRemoteHost
	{
		get
		{
			return _dbgRemoteHost;
		}
		set
		{
			_dbgRemoteHost = value;
		}
	}
}
