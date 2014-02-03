using System;
using System.IO;
using System.Threading;
using Belikov.Common.ThreadProcessing;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Security;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Connection;

internal abstract class GeneralConnection
{
	public MessageContainer MessageContainer;

	public string HostIdAsString;

	public HostInformation Remote;

	public ITransportContext ITransportContext;

	public GenuineConnectionType GenuineConnectionType;

	protected object _accessToLocalMembers = new object();

	public object ReconnectionLock = new object();

	private int _lastTimeContentWasSent = GenuineUtility.TickCount;

	private bool __disposed = false;

	internal Exception _disposeReason = null;

	internal ReaderWriterLock DisposeLock = new ReaderWriterLock();

	private ConnectionStateSignaller _connectionStateSignaller;

	private object _connectionStateSignallerLock = new object();

	public int CloseConnectionAfterInactivity;

	private int _shutdownTime = GenuineUtility.FurthestFuture;

	public int LastTimeContentWasSent
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _lastTimeContentWasSent;
			}
		}
		set
		{
			lock (_accessToLocalMembers)
			{
				_lastTimeContentWasSent = value;
			}
		}
	}

	internal bool IsDisposed
	{
		get
		{
			ReaderAutoLocker readerAutoLocker = new ReaderAutoLocker(DisposeLock);
			try
			{
				return __disposed;
			}
			finally
			{
				readerAutoLocker.Dispose();
			}
		}
		set
		{
			WriterAutoLocker writerAutoLocker = new WriterAutoLocker(DisposeLock);
			try
			{
				__disposed = value;
			}
			finally
			{
				writerAutoLocker.Dispose();
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

	public GeneralConnection(ITransportContext iTransportContext)
	{
		ITransportContext = iTransportContext;
		HostIdAsString = iTransportContext.HostIdentifier;
	}

	public void Dispose(Exception reason)
	{
		if (IsDisposed)
		{
			return;
		}
		if (reason == null)
		{
			reason = GenuineExceptions.Get_Processing_TransportConnectionFailed();
		}
		WriterAutoLocker writerAutoLocker = new WriterAutoLocker(DisposeLock);
		try
		{
			if (IsDisposed)
			{
				return;
			}
			IsDisposed = true;
			_disposeReason = reason;
		}
		finally
		{
			writerAutoLocker.Dispose();
		}
		InternalDispose(reason);
	}

	public abstract void InternalDispose(Exception reason);

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

	public bool GatherContentOfConnectionLevelSecuritySessions(Stream senderInput, Stream listenerInput, GenuineChunkedStream output, SecuritySession sender, SecuritySession listener)
	{
		bool result = false;
		GenuineChunkedStreamSizeLabel genuineChunkedStreamSizeLabel = new GenuineChunkedStreamSizeLabel(output);
		try
		{
			if (sender != null && !sender.IsEstablished)
			{
				Stream stream = sender.EstablishSession(senderInput, connectionLevel: true);
				if (stream != null)
				{
					result = true;
					GenuineUtility.CopyStreamToStream(stream, output);
				}
			}
		}
		finally
		{
			genuineChunkedStreamSizeLabel.Dispose();
		}
		genuineChunkedStreamSizeLabel = new GenuineChunkedStreamSizeLabel(output);
		try
		{
			if (listener != null && !listener.IsEstablished)
			{
				Stream stream = listener.EstablishSession(listenerInput, connectionLevel: true);
				if (stream != null)
				{
					result = true;
					GenuineUtility.CopyStreamToStream(stream, output);
				}
			}
		}
		finally
		{
			genuineChunkedStreamSizeLabel.Dispose();
		}
		return result;
	}
}
