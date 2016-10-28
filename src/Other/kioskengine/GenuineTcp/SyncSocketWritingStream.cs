using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Belikov.GenuineChannels.Connection;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.GenuineTcp;

internal class SyncSocketWritingStream : Stream
{
	private ConnectionManager _connectionManager;

	private Socket _socket;

	private int _writeTimeout;

	private int _dbgConnectionId;

	private HostInformation _remote;

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

	public SyncSocketWritingStream(ConnectionManager connectionManager, Socket socket, int writeTimeout, int dbgConnectionId, HostInformation remote)
	{
		_connectionManager = connectionManager;
		_socket = socket;
		_writeTimeout = writeTimeout;
		_dbgConnectionId = dbgConnectionId;
		_remote = remote;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		BinaryLogWriter binaryLogWriter = _remote.ITransportContext.BinaryLogWriter;
		try
		{
			while (count > 0)
			{
				int millisecondsLeft = GenuineUtility.GetMillisecondsLeft(_writeTimeout);
				if (millisecondsLeft <= 0)
				{
					throw GenuineExceptions.Get_Send_Timeout();
				}
				_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, millisecondsLeft);
				int num = _socket.Send(buffer, offset, count, SocketFlags.None);
				if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
				{
					binaryLogWriter.WriteTransportContentEvent(LogCategory.LowLevelTransport, "SyncSocketWritingStream.Write", LogMessageType.LowLevelTransport_SyncSendingCompleted, null, null, _remote, (binaryLogWriter[LogCategory.LowLevelTransport] > 1) ? new MemoryStream(GenuineUtility.CutOutBuffer(buffer, offset, count)) : null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, _dbgConnectionId, count, _socket.RemoteEndPoint.ToString(), null, null, "Immediately after Socket.Send(). Size: {0}. Sent: {1}.", count, num);
				}
				if (num == 0)
				{
					throw GenuineExceptions.Get_Send_TransportProblem();
				}
				offset += num;
				count -= num;
				_connectionManager.IncreaseBytesSent(num);
			}
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
			{
				binaryLogWriter.WriteTransportContentEvent(LogCategory.LowLevelTransport, "SyncSocketWritingStream.Write", LogMessageType.LowLevelTransport_SyncSendingCompleted, exception, null, _remote, (binaryLogWriter[LogCategory.LowLevelTransport] > 1) ? new MemoryStream(GenuineUtility.CutOutBuffer(buffer, offset, count)) : null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, _dbgConnectionId, count, null, null, null, "Socket.Send(); ERROR.");
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
}
