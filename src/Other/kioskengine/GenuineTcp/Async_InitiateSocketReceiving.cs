using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using Belikov.GenuineChannels.Logbook;
using Belikov.GenuineChannels.Utilities;

namespace Belikov.GenuineChannels.GenuineTcp;

[ComVisible(true)]
public class Async_InitiateSocketReceiving : IAsyncWorkItem
{
	private Socket _socket;

	private byte[] _buffer;

	private int _offset;

	private int _size;

	private AsyncCallback _asyncCallback;

	private object _state;

	public Async_InitiateSocketReceiving(Socket socket, byte[] buffer, int offset, int size, AsyncCallback asyncCallback, object state)
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
			_socket.BeginReceive(_buffer, _offset, _size, SocketFlags.None, _asyncCallback, _state);
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.LowLevelTransport, "Async_InitiateSocketReceiving.StartAsynchronousOperation", LogMessageType.LowLevelTransport_AsyncReceivingInitiated, null, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, 0, 0, 0, null, null, null, null, "Socket.BeginReceive(). Socket: {0}. Buffer: {1}. Offset: {2}. Size: {3}.", (long)_socket.Handle, _buffer.GetHashCode(), _offset, _size);
			}
		}
		catch (Exception exception)
		{
			if (binaryLogWriter != null && binaryLogWriter[LogCategory.LowLevelTransport] > 0)
			{
				binaryLogWriter.WriteEvent(LogCategory.LowLevelTransport, "Async_InitiateSocketReceiving.StartAsynchronousOperation", LogMessageType.LowLevelTransport_AsyncReceivingInitiated, exception, null, null, null, GenuineUtility.CurrentThreadId, Thread.CurrentThread.Name, null, null, -1, _buffer.GetHashCode(), _offset, _size, null, null, null, null, "Socket.BeginReceive(). Socket: {0}. Buffer: {1}. Offset: {2}. Size: {3}.", (long)_socket.Handle, _buffer.GetHashCode(), _offset, _size);
			}
		}
	}
}
