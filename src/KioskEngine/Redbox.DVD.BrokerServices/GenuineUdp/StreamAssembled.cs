using System;
using System.Collections;
using System.IO;
using System.Net;
using Belikov.GenuineChannels.BufferPooling;

namespace Belikov.GenuineChannels.GenuineUdp;

internal class StreamAssembled : Stream, IDisposable
{
	private object _accessToLocalMembers = new object();

	private int _started;

	public ArrayList _chunks = new ArrayList();

	public IPEndPoint RemoteIPEndPoint;

	private bool _isProcessed;

	private int _headerSize;

	private bool _wasLastPacketReceived;

	private bool _disposed = false;

	private int _readBlockNumber = 0;

	private int _readPosition;

	public int Started
	{
		get
		{
			lock (_accessToLocalMembers)
			{
				return _started;
			}
		}
	}

	public bool IsProcessed
	{
		get
		{
			lock (this)
			{
				return _isProcessed;
			}
		}
		set
		{
			lock (this)
			{
				_isProcessed = value;
			}
		}
	}

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

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

	public StreamAssembled(IPEndPoint remoteIPEndPoint, int headerSize)
	{
		_started = GenuineUtility.TickCount;
		RemoteIPEndPoint = remoteIPEndPoint;
		_headerSize = headerSize;
		_readPosition = headerSize;
	}

	public bool BufferReceived(int chunkNumber, byte[] buffer, int size, bool isLast)
	{
		if (isLast)
		{
			_wasLastPacketReceived = true;
		}
		while (_chunks.Count < chunkNumber * 2 + 1)
		{
			_chunks.Add(null);
			_chunks.Add(0);
		}
		if ((int)_chunks[chunkNumber * 2 + 1] == 0)
		{
			_chunks[chunkNumber * 2] = buffer;
			_chunks[chunkNumber * 2 + 1] = size;
		}
		if (!_wasLastPacketReceived)
		{
			return false;
		}
		for (int i = 0; i < _chunks.Count / 2; i++)
		{
			if ((int)_chunks[i * 2 + 1] == 0)
			{
				return false;
			}
		}
		return true;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("GenuineChunkedStream");
		}
		int num = 0;
		while (_readBlockNumber < _chunks.Count && count > 0)
		{
			byte[] array = (byte[])_chunks[_readBlockNumber];
			int num2 = (int)_chunks[_readBlockNumber + 1];
			if (num2 <= _readPosition)
			{
				BufferPool.RecycleBuffer(array);
				_chunks.RemoveRange(0, 2);
				_readPosition = _headerSize;
				continue;
			}
			int num3 = Math.Min(count, num2 - _readPosition);
			Buffer.BlockCopy(array, _readPosition, buffer, offset, num3);
			offset += num3;
			count -= num3;
			num += num3;
			_readPosition += num3;
		}
		return num;
	}

	public override int ReadByte()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("GenuineChunkedStream");
		}
		byte[] array;
		while (true)
		{
			if (_readBlockNumber >= _chunks.Count)
			{
				return -1;
			}
			array = (byte[])_chunks[_readBlockNumber];
			int num = (int)_chunks[_readBlockNumber + 1];
			if (num > _readPosition)
			{
				break;
			}
			BufferPool.RecycleBuffer(array);
			_chunks.RemoveRange(0, 2);
			_readPosition = _headerSize;
		}
		return array[_readPosition++];
	}

	public new void Dispose()
	{
		Close();
	}

	public override void Close()
	{
		if (!_disposed)
		{
			for (int i = 0; i < _chunks.Count; i += 2)
			{
				byte[] buf = (byte[])_chunks[i];
				BufferPool.RecycleBuffer(buf);
			}
			_chunks.Clear();
			_disposed = true;
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

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}
}
