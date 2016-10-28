using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.GenuineTcp;

internal class Async_InitiateSocketSending : IAsyncWorkItem
{
	private Socket _socket;

	private byte[] _buffer;

	private int _offset;

	private int _size;

	private AsyncCallback _asyncCallback;

	private object _state;

	public Async_InitiateSocketSending(Socket socket, byte[] buffer, int offset, int size, AsyncCallback asyncCallback, object state)
	{
		_socket = socket;
		_buffer = buffer;
		_offset = offset;
		_size = size;
		_asyncCallback = asyncCallback;
		_state = state;
	}

	public void StartAsynchronousOperation()
	{
		BinaryLogWriter binaryLogWriter = GenuineLoggingServices.BinaryLogWriter;
		try
		{
			_socket.BeginSend(_buffer, _offset, _size, SocketFlags.None, _asyncCallback, _state);
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.LowLevelTransport, "Async_InitiateSocketSending.StartAsynchronousOperation", LogMessageType.LowLevelTransport_AsyncSendingInitiated, null, null, null, new MemoryStream(_buffer, _offset, _size), GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, _buffer.GetHashCode(), _offset, _size, null, null, null, null, "Sending has been initiated. Socket: {0}.", (long)_socket.Handle);
			}
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.LowLevelTransport, "Async_InitiateSocketSending.StartAsynchronousOperation", LogMessageType.LowLevelTransport_AsyncSendingInitiated, exception, null, null, new MemoryStream(_buffer, _offset, _size), GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, _buffer.GetHashCode(), _offset, _size, null, null, null, null, "Sending cannot be initiated. Socket: {0}.", (long)_socket.Handle);
			}
		}
	}
}
