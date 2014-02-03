using System;
using System.IO;
using System.Net.Sockets;
using Belikov.GenuineChannels.Connection;

namespace Belikov.GenuineChannels.GenuineTcp;

internal class SyncSocketStream : Stream
{
	private Socket _socket;

	private ConnectionManager _connectionManager;

	private int _timeout;

	public override bool CanRead => true;

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

	public SyncSocketStream(Socket socket, ConnectionManager connectionManager, int timeout)
	{
		_socket = socket;
		_connectionManager = connectionManager;
		_timeout = timeout;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int num = _socket.Receive(buffer, offset, count, SocketFlags.None);
		_connectionManager.IncreaseBytesReceived(num);
		return num;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		while (count > 0)
		{
			int num = _socket.Send(buffer, offset, count, SocketFlags.None);
			if (num == 0)
			{
				throw GenuineExceptions.Get_Send_TransportProblem();
			}
			_connectionManager.IncreaseBytesSent(num);
			count -= num;
			offset += num;
		}
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
