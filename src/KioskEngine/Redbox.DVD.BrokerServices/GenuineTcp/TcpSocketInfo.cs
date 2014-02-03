using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Belikov.GenuineChannels.Connection;
using Belikov.GenuineChannels.DotNetRemotingLayer;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.Parameters;
using Belikov.GenuineChannels.Receiving;
using Belikov.GenuineChannels.Security;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.GenuineTcp;

internal class TcpSocketInfo : Stream
{
	private object _accessToLocalMembers = new object();

	private int _connectionId = ConnectionManager.GetUniqueConnectionId();

	public Socket Socket;

	public string ConnectionName;

	public HostInformation Remote;

	public ITransportContext ITransportContext;

	public bool IsValid = true;

	public GenuineConnectionType GenuineConnectionType;

	public SecuritySession ConnectionLevelSecurity;

	public Message Message;

	public int MaxSendSize;

	public IMessageRegistrator _iMessageRegistrator = new MessageRegistratorWithLimitedTime();

	public bool LockForSending = false;

	public MessageContainer MessageContainer;

	private int _lastTimeContentWasSent = GenuineUtility.TickCount;

	private object _lastTimeContentWasSentLock = new object();

	public Message InitialMessage;

	public int CloseConnectionAfterInactivity;

	private int _shutdownTime = GenuineUtility.FurthestFuture;

	public bool IsServer;

	public TcpInvocationFiniteAutomatonState TcpInvocationFiniteAutomatonState;

	private int _syncWritingTimeout;

	internal byte[] _sendBuffer;

	internal MessageList MessagesSentSynchronously = new MessageList(1);

	public byte[] AsyncSendBuffer;

	public int AsyncSendBufferSizeOfValidContent;

	public int AsyncSendBufferCurrentPosition;

	public bool AsyncSendBufferIsLastPacket;

	public Stream AsyncSendStream;

	public byte[] ReceivingHeaderBuffer;

	public int ReceivingBufferExpectedSize;

	public int ReceivingBufferCurrentPosition;

	public bool IsHeaderIsBeingReceived;

	private ConnectionStateSignaller _connectionStateSignaller;

	private object _connectionStateSignallerLock = new object();

	public int DbgConnectionId => _connectionId;

	public string PrimaryRemoteUri => IsServer ? Remote.Uri : Remote.Url;

	public int LastTimeContentWasSent
	{
		get
		{
			lock (_lastTimeContentWasSentLock)
			{
				return _lastTimeContentWasSent;
			}
		}
		set
		{
			lock (_lastTimeContentWasSentLock)
			{
				_lastTimeContentWasSent = value;
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

	public override bool CanRead => false;

	public override bool CanSeek => false;

	public override bool CanWrite => true;

	public override long Length
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override long Position
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	internal TcpSocketInfo(Socket socket, ITransportContext iTransportContext, string connectionName)
	{
		Socket = socket;
		ITransportContext = iTransportContext;
		ConnectionName = ((connectionName == null) ? ("~" + Guid.NewGuid().ToString("N")) : connectionName);
		MaxSendSize = (int)ITransportContext.IParameterProvider[GenuineParameter.TcpMaxSendSize];
	}

	public void Renew()
	{
		lock (_accessToLocalMembers)
		{
			_shutdownTime = GenuineUtility.GetTimeout(CloseConnectionAfterInactivity);
		}
		Remote.Renew(CloseConnectionAfterInactivity, canMakeShorter: false);
	}

	public void SetupWriting(int operationTimeout)
	{
		if (_sendBuffer == null)
		{
			_sendBuffer = new byte[MaxSendSize];
		}
		_syncWritingTimeout = operationTimeout;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		BinaryLogWriter binaryLogWriter = ITransportContext.BinaryLogWriter;
		int millisecondsLeft = GenuineUtility.GetMillisecondsLeft(_syncWritingTimeout);
		if (millisecondsLeft <= 0)
		{
			throw GenuineExceptions.Get_Send_Timeout();
		}
		try
		{
			Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, millisecondsLeft);
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Transport] > 0)
			{
				binaryLogWriter.WriteTransportContentEvent(LogCategory.Transport, "TcpSocketInfo.Write", LogMessageType.SynchronousSendingStarted, null, null, (Remote == null) ? null : Remote, (binaryLogWriter[LogCategory.Transport] > 1) ? new MemoryStream(GenuineUtility.CutOutBuffer(buffer, offset, count)) : null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, DbgConnectionId, count, (Remote == null || Remote.PhysicalAddress == null) ? null : Remote.PhysicalAddress.ToString(), null, null, "The content is being sent synchronously. Size: {0}.", count);
			}
			do
			{
				int num = 0;
				try
				{
					num = Socket.Send(buffer, offset, count, SocketFlags.None);
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
					{
						binaryLogWriter.WriteTransportContentEvent(LogCategory.LowLevelTransport, "TcpSocketInfo.Write", LogMessageType.LowLevelTransport_SyncSendingCompleted, null, null, Remote, (binaryLogWriter[LogCategory.LowLevelTransport] > 1) ? new MemoryStream(GenuineUtility.CutOutBuffer(buffer, offset, num)) : null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, DbgConnectionId, num, Socket.RemoteEndPoint.ToString(), null, null, "Socket.Send(). Size: {0}. Sent: {1}.", count, num);
					}
				}
				catch (Exception exception)
				{
					if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
					{
						binaryLogWriter.WriteTransportContentEvent(LogCategory.LowLevelTransport, "TcpSocketInfo.Write", LogMessageType.LowLevelTransport_SyncSendingCompleted, exception, null, (Remote == null) ? null : Remote, (binaryLogWriter[LogCategory.LowLevelTransport] > 1) ? new MemoryStream(GenuineUtility.CutOutBuffer(buffer, offset, num)) : null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, DbgConnectionId, num, Socket.RemoteEndPoint.ToString(), null, null, "Socket.Send() failed. Size: {0}. Sent: {1}.", count, num);
					}
				}
				if (num == 0)
				{
					throw GenuineExceptions.Get_Send_TransportProblem();
				}
				offset += num;
				count -= num;
			}
			while (count > 0);
		}
		catch (Exception exception2)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.Transport] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.Transport, "TcpSocketInfo.Write", LogMessageType.SynchronousSendingStarted, exception2, null, (Remote == null) ? null : Remote, (binaryLogWriter[LogCategory.Transport] > 1) ? new MemoryStream(GenuineUtility.CutOutBuffer(buffer, offset, count)) : null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, DbgConnectionId, 0, 0, 0, count.ToString(), null, null, null, "An exception is raised during synchronous sending.");
			}
			throw GenuineExceptions.Get_Send_TransportProblem();
		}
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}

	public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		throw new NotSupportedException();
	}

	public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
	{
		throw new NotSupportedException();
	}

	public override int EndRead(IAsyncResult asyncResult)
	{
		throw new NotSupportedException();
	}

	public override void EndWrite(IAsyncResult asyncResult)
	{
		throw new NotSupportedException();
	}

	public override void Flush()
	{
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public override void SetLength(long val)
	{
		throw new NotSupportedException();
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
}
