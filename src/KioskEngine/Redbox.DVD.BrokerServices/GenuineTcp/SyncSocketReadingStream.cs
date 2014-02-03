using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Belikov.GenuineChannels.BufferPooling;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Messaging;

namespace Belikov.GenuineChannels.GenuineTcp;

internal class SyncSocketReadingStream : Stream, IDisposable
{
	private TcpConnectionManager _tcpConnectionManager;

	private TcpSocketInfo _tcpSocketInfo;

	private int _receiveTimeout;

	private byte[] _readBuffer;

	private bool _automaticallyContinueReading;

	private int _validLength;

	private int _currentPosition;

	private int _currentPacketSize;

	private int _currentPacketBytesRead;

	private bool _messageRead;

	private int _dbgStreamId = Interlocked.Increment(ref _totalDbgStreamId);

	private static int _totalDbgStreamId = 0;

	public int DbgStreamId => _dbgStreamId;

	public bool IsReadingFinished => _currentPacketBytesRead >= _currentPacketSize && _messageRead;

	public bool IsMessageProcessed => _currentPacketBytesRead >= _currentPacketSize && _messageRead && _currentPosition >= _validLength;

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

	public SyncSocketReadingStream(TcpConnectionManager tcpConnectionManager, TcpSocketInfo tcpSocketInfo, int receiveTimeout, bool automaticallyContinueReading)
	{
		_readBuffer = BufferPool.ObtainBuffer();
		_tcpConnectionManager = tcpConnectionManager;
		_tcpSocketInfo = tcpSocketInfo;
		_receiveTimeout = receiveTimeout;
		_automaticallyContinueReading = automaticallyContinueReading;
		ReadNextPortion(deriveHeader: true);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int num = 0;
		int num2 = 0;
		while (true)
		{
			if (_currentPosition < _validLength)
			{
				num = Math.Min(_validLength - _currentPosition, count);
				Buffer.BlockCopy(_readBuffer, _currentPosition, buffer, offset, num);
				_currentPosition += num;
				count -= num;
				num2 += num;
				offset += num;
			}
			if (_readBuffer != null && _messageRead && _currentPacketBytesRead >= _currentPacketSize && _currentPosition >= _validLength)
			{
				BufferPool.RecycleBuffer(_readBuffer);
				_readBuffer = null;
			}
			if (count <= 0 || (_messageRead && _currentPacketBytesRead >= _currentPacketSize))
			{
				break;
			}
			ReadNextPortion(deriveHeader: false);
		}
		return num2;
	}

	public override int ReadByte()
	{
		try
		{
			if (_currentPosition < _validLength)
			{
				return _readBuffer[_currentPosition++];
			}
			ReadNextPortion(deriveHeader: false);
			if (_currentPosition < _validLength)
			{
				return _readBuffer[_currentPosition++];
			}
			return -1;
		}
		finally
		{
			if (_readBuffer != null && _messageRead && _currentPacketBytesRead >= _currentPacketSize && _currentPosition >= _validLength)
			{
				BufferPool.RecycleBuffer(_readBuffer);
				_readBuffer = null;
			}
		}
	}

	private void ReadNextPortion(bool deriveHeader)
	{
		int num = 0;
		int num2 = 0;
		if (!deriveHeader)
		{
			if (_currentPacketBytesRead < _currentPacketSize)
			{
				num2 = Math.Min(_currentPacketSize - _currentPacketBytesRead, _readBuffer.Length);
				_validLength = ReadFromSocket(_readBuffer, 0, num2);
				if (_validLength == 0)
				{
					throw GenuineExceptions.Get_Receive_Portion();
				}
				_tcpSocketInfo.ITransportContext.ConnectionManager.IncreaseBytesReceived(_validLength);
				_currentPacketBytesRead += _validLength;
				_currentPosition = 0;
				if (_currentPacketBytesRead == _currentPacketSize && _messageRead)
				{
					ReadingCompleted();
				}
				return;
			}
			if (_messageRead)
			{
				return;
			}
			_currentPosition = 0;
		}
		num2 = 6;
		if (deriveHeader)
		{
			if (_tcpSocketInfo.ReceivingBufferCurrentPosition > 0)
			{
				Buffer.BlockCopy(_tcpSocketInfo.ReceivingHeaderBuffer, 0, _readBuffer, 0, _tcpSocketInfo.ReceivingBufferCurrentPosition);
			}
			_currentPosition = _tcpSocketInfo.ReceivingBufferCurrentPosition;
		}
		while (_currentPosition < num2)
		{
			num = ReadFromSocket(_readBuffer, _currentPosition, num2 - _currentPosition);
			if (num == 0)
			{
				throw GenuineExceptions.Get_Receive_Portion();
			}
			_tcpSocketInfo.ITransportContext.ConnectionManager.IncreaseBytesReceived(num);
			_currentPosition += num;
		}
		if (_readBuffer[0] != 55)
		{
			throw GenuineExceptions.Get_Receive_IncorrectData();
		}
		_currentPacketSize = MessageCoder.ReadInt32(_readBuffer, 1);
		_messageRead = _readBuffer[5] != 0;
		_currentPacketBytesRead = 0;
		if (_currentPacketBytesRead < _currentPacketSize)
		{
			num2 = Math.Min(_currentPacketSize - _currentPacketBytesRead, _readBuffer.Length);
			_validLength = ReadFromSocket(_readBuffer, 0, num2);
			if (_validLength == 0)
			{
				throw GenuineExceptions.Get_Receive_Portion();
			}
			_tcpSocketInfo.ITransportContext.ConnectionManager.IncreaseBytesReceived(_validLength);
			_currentPacketBytesRead += _validLength;
			_currentPosition = 0;
		}
		if (_currentPacketBytesRead == _currentPacketSize && _messageRead)
		{
			ReadingCompleted();
		}
	}

	private void ReadingCompleted()
	{
		if (_automaticallyContinueReading)
		{
			_tcpConnectionManager.LowLevel_HalfSync_StartReceiving(_tcpSocketInfo);
		}
	}

	public int ReadFromSocket(byte[] buffer, int offset, int count)
	{
		BinaryLogWriter binaryLogWriter = _tcpConnectionManager.ITransportContext.BinaryLogWriter;
		try
		{
			int millisecondsLeft = GenuineUtility.GetMillisecondsLeft(_receiveTimeout);
			if (millisecondsLeft <= 0)
			{
				throw GenuineExceptions.Get_Send_ServerDidNotReply();
			}
			_tcpSocketInfo.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, millisecondsLeft);
			int num = _tcpSocketInfo.Socket.Receive(buffer, offset, count, SocketFlags.None);
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
			{
				binaryLogWriter.WriteTransportContentEvent(LogCategory.LowLevelTransport, "SyncSocketReadingStream.ReadFromSocket", LogMessageType.LowLevelTransport_SyncReceivingCompleted, null, null, _tcpSocketInfo.Remote, (binaryLogWriter[LogCategory.LowLevelTransport] > 1) ? new MemoryStream(buffer, offset, num) : null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, _tcpSocketInfo.DbgConnectionId, num, _tcpSocketInfo.Socket.RemoteEndPoint.ToString(), null, null, "Socket.Receive(). Bytes received: {0}.", num);
			}
			_tcpConnectionManager.IncreaseBytesReceived(num);
			return num;
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.LowLevelTransport, "SyncSocketReadingStream.ReadFromSocket", LogMessageType.LowLevelTransport_SyncReceivingCompleted, exception, null, _tcpSocketInfo.Remote, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, _tcpSocketInfo.DbgConnectionId, 0, 0, 0, null, null, null, null, "Socket.Receive() failed.");
			}
			throw;
		}
	}

	public new void Dispose()
	{
		Close();
	}

	public override void Close()
	{
		SkipMessage();
	}

	public void SkipMessage()
	{
		while (!IsReadingFinished)
		{
			ReadNextPortion(deriveHeader: false);
		}
	}

	public override void Write(byte[] buffer, int offset, int count)
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
