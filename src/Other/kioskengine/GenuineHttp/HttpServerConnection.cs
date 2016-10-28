using System;
using System.Collections;
using System.IO;
using System.Threading;
using Belikov.Common.ThreadProcessing;
using Belikov.GenuineChannels.Connection;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Security;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.GenuineHttp;

internal class HttpServerConnection
{
	private object _accessToLocalMembers = new object();

	public HttpServerConnectionManager HttpServerConnectionManager;

	public byte[] HostId;

	public string HostIdAsString;

	public ITransportContext ITransportContext;

	public HostInformation Remote;

	public string ConnectionName;

	public int DbgConnectionId = ConnectionManager.GetUniqueConnectionId();

	public int Sender_SequenceNo = 0;

	public int Listener_SequenceNo = 0;

	public Stream Listener_SentStream;

	public object Listener_Lock = new object();

	public HttpServerRequestResult Listener;

	public MessageContainer Listener_MessageContainer;

	public ArrayList Listener_MessagesBeingSent = new ArrayList();

	public byte[] Listener_IntermediateBuffer = new byte[35000];

	public int Listener_Opened = GenuineUtility.TickCount;

	public SecuritySession Sender_SecuritySession;

	public SecuritySession Listener_SecuritySession;

	private bool __disposed = false;

	private object __disposedLock = new object();

	internal Exception _disposeReason = null;

	internal ReaderWriterLock _disposeLock = new ReaderWriterLock();

	private ConnectionStateSignaller _connectionStateSignaller;

	private object _connectionStateSignallerLock = new object();

	public int CloseConnectionAfterInactivity;

	private int _shutdownTime = GenuineUtility.FurthestFuture;

	internal bool _disposed
	{
		get
		{
			lock (__disposedLock)
			{
				return __disposed;
			}
		}
		set
		{
			lock (__disposedLock)
			{
				__disposed = value;
			}
		}
	}

	public int ShutdownTime
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _shutdownTime;
			}
		}
	}

	public HttpServerConnection(ITransportContext iTransportContext, Guid hostId, HostInformation remote, string connectionName, int closeConnectionAfterInactivity)
	{
		ITransportContext = iTransportContext;
		HttpServerConnectionManager = (HttpServerConnectionManager)iTransportContext.ConnectionManager;
		ConnectionName = connectionName;
		CloseConnectionAfterInactivity = closeConnectionAfterInactivity;
		HostId = hostId.ToByteArray();
		HostIdAsString = hostId.ToString("N");
		Listener_MessageContainer = new MessageContainer(iTransportContext);
		Remote = remote;
	}

	public void Dispose(Exception reason)
	{
		if (_disposed)
		{
			return;
		}
		if (reason == null)
		{
			reason = GenuineExceptions.Get_Processing_TransportConnectionFailed();
		}
		WriterAutoLocker writerAutoLocker = new WriterAutoLocker(_disposeLock);
		try
		{
			if (!_disposed)
			{
				_disposed = true;
				_disposeReason = reason;
			}
		}
		finally
		{
			writerAutoLocker.Dispose();
		}
	}

	public void SignalState(GenuineEventType genuineEventType, Exception reason, object additionalInfo)
	{
		lock (_connectionStateSignallerLock)
		{
			if (_connectionStateSignaller == null)
			{
				_connectionStateSignaller = new ConnectionStateSignaller(Remote, ITransportContext.IGenuineEventProvider);
			}
			_connectionStateSignaller.SetState(genuineEventType, reason, additionalInfo);
		}
	}

	public void Renew()
	{
		lock (_accessToLocalMembers)
		{
			_shutdownTime = GenuineUtility.GetTimeout(CloseConnectionAfterInactivity);
		}
		Remote.Renew(CloseConnectionAfterInactivity, canMakeShorter: false);
	}
}
