using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using Belikov.GenuineChannels.BufferPooling;

namespace Belikov.GenuineChannels.Messaging;

[ComVisible(true)]
public class GenuineChunkedStream : Stream, ICloneable, IDisposable
{
	private bool _releaseOnReadMode = false;

	protected bool _disposed = false;

	protected bool _cloned = false;

	private ArrayList _chunksAndStreams = new ArrayList(10);

	private long _length = 0L;

	private int _readBlockNumber = 0;

	private int _readPosition = 0;

	private int _writePosition = 0;

	private byte[] _currentWriteBlock = null;

	public bool ReleaseOnReadMode
	{
		get
		{
			return _releaseOnReadMode;
		}
		set
		{
			if (!value)
			{
				throw new NotSupportedException("Only setting to true is supported.");
			}
			_releaseOnReadMode = true;
		}
	}

	public override bool CanRead => true;

	public override bool CanSeek => !_releaseOnReadMode && _length != -1;

	public override bool CanWrite => true;

	public override long Length
	{
		get
		{
			if (_length >= 0)
			{
				return _length;
			}
			throw new NotSupportedException();
		}
	}

	public override long Position
	{
		get
		{
			return Length;
		}
		set
		{
			if (value != 0)
			{
				throw new NotSupportedException();
			}
			Seek(0L, SeekOrigin.Begin);
		}
	}

	public GenuineChunkedStream()
		: this(releaseOnReadMode: false)
	{
	}

	public GenuineChunkedStream(bool releaseOnReadMode)
	{
		_releaseOnReadMode = releaseOnReadMode;
	}

	~GenuineChunkedStream()
	{
		Close();
	}

	public override void Close()
	{
		if (_disposed)
		{
			return;
		}
		for (int i = 0; i < _chunksAndStreams.Count; i += 2)
		{
			if (_chunksAndStreams[i] is Stream stream)
			{
				stream.Close();
				continue;
			}
			byte[] buf = (byte[])_chunksAndStreams[i];
			if (!_cloned)
			{
				BufferPool.RecycleBuffer(buf);
			}
		}
		_disposed = true;
	}

	public override void Flush()
	{
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		if (offset != 0 || origin != SeekOrigin.Begin || _releaseOnReadMode)
		{
			throw new NotSupportedException();
		}
		_readBlockNumber = 0;
		_readPosition = 0;
		for (int i = 0; i < _chunksAndStreams.Count; i += 2)
		{
			if (_chunksAndStreams[i] is Stream stream)
			{
				stream.Seek(0L, SeekOrigin.Begin);
			}
		}
		return 0L;
	}

	public override void SetLength(long val)
	{
		throw new NotSupportedException("GenuineChunkedStream does not support SetLength method.");
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("GenuineChunkedStream");
		}
		int num = 0;
		while (_readBlockNumber < _chunksAndStreams.Count && count > 0)
		{
			if (_chunksAndStreams[_readBlockNumber] is Stream stream)
			{
				int num2 = stream.Read(buffer, offset, count);
				offset += num2;
				count -= num2;
				num += num2;
				if (num2 <= 0)
				{
					AdvanceReadPointer();
				}
				continue;
			}
			byte[] src = (byte[])_chunksAndStreams[_readBlockNumber];
			int num3 = (int)_chunksAndStreams[_readBlockNumber + 1];
			if (num3 <= _readPosition)
			{
				AdvanceReadPointer();
				continue;
			}
			int num4 = Math.Min(count, num3 - _readPosition);
			Buffer.BlockCopy(src, _readPosition, buffer, offset, num4);
			offset += num4;
			count -= num4;
			num += num4;
			_readPosition += num4;
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
			if (_readBlockNumber >= _chunksAndStreams.Count)
			{
				return -1;
			}
			if (_chunksAndStreams[_readBlockNumber] is Stream stream)
			{
				int num = stream.ReadByte();
				if (num >= 0)
				{
					return num;
				}
				AdvanceReadPointer();
				continue;
			}
			array = (byte[])_chunksAndStreams[_readBlockNumber];
			int num2 = (int)_chunksAndStreams[_readBlockNumber + 1];
			if (num2 > _readPosition)
			{
				break;
			}
			AdvanceReadPointer();
		}
		return array[_readPosition++];
	}

	private void AdvanceReadPointer()
	{
		if (_releaseOnReadMode)
		{
			if (_chunksAndStreams[_readBlockNumber] is byte[] buf)
			{
				BufferPool.RecycleBuffer(buf);
			}
			else
			{
				Stream stream = (Stream)_chunksAndStreams[_readBlockNumber];
				stream.Close();
			}
			_chunksAndStreams.RemoveRange(0, 2);
			_readPosition = 0;
		}
		else
		{
			_readBlockNumber += 2;
			_readPosition = 0;
		}
	}

	public void WriteTo(Stream outputStream)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("GenuineChunkedStream");
		}
		while (_readBlockNumber < _chunksAndStreams.Count)
		{
			if (_chunksAndStreams[_readBlockNumber] is Stream inputStream)
			{
				GenuineUtility.CopyStreamToStream(inputStream, outputStream);
				_readBlockNumber += 2;
				_readPosition = 0;
				continue;
			}
			byte[] buffer = (byte[])_chunksAndStreams[_readBlockNumber];
			int num = (int)_chunksAndStreams[_readBlockNumber + 1];
			if (num > _readPosition)
			{
				int count = num - _readPosition;
				outputStream.Write(buffer, _readPosition, count);
			}
			_readBlockNumber += 2;
			_readPosition = 0;
		}
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("GenuineChunkedStream");
		}
		if (_length >= 0)
		{
			_length += count;
		}
		while (count > 0)
		{
			if (_currentWriteBlock == null || _writePosition >= _currentWriteBlock.Length - 1)
			{
				ObtainNextBuffer();
			}
			int num = Math.Min(count, _currentWriteBlock.Length - _writePosition);
			Buffer.BlockCopy(buffer, offset, _currentWriteBlock, _writePosition, num);
			count -= num;
			offset += num;
			_writePosition += num;
			_chunksAndStreams[_chunksAndStreams.Count - 1] = _writePosition;
		}
	}

	private void ObtainNextBuffer()
	{
		_currentWriteBlock = BufferPool.ObtainBuffer();
		_writePosition = 0;
		_chunksAndStreams.Add(_currentWriteBlock);
		_chunksAndStreams.Add(0);
	}

	public override void WriteByte(byte val)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("GenuineChunkedStream");
		}
		if (_currentWriteBlock == null || _writePosition >= _currentWriteBlock.Length - 1)
		{
			ObtainNextBuffer();
		}
		_currentWriteBlock[_writePosition] = val;
		_writePosition++;
		_chunksAndStreams[_chunksAndStreams.Count - 1] = _writePosition;
		if (_length >= 0)
		{
			_length++;
		}
	}

	public void WriteStream(Stream stream)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("GenuineChunkedStream");
		}
		if (stream.CanSeek)
		{
			if (stream.Length <= 0)
			{
				return;
			}
			if (_length >= 0)
			{
				_length += stream.Length;
			}
		}
		else
		{
			_length = -1L;
		}
		_currentWriteBlock = null;
		_writePosition = 0;
		_chunksAndStreams.Add(stream);
		_chunksAndStreams.Add(0);
	}

	public void WriteBuffer(byte[] buffer, int validContentSize)
	{
		if (validContentSize == -1)
		{
			validContentSize = buffer.Length;
		}
		_currentWriteBlock = buffer;
		_writePosition = validContentSize;
		_chunksAndStreams.Add(buffer);
		_chunksAndStreams.Add(validContentSize);
		if (_length >= 0)
		{
			_length += validContentSize;
		}
	}

	public void WriteInt32AndRememberItsLocation(int number, out byte[] chunk, out int position)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("GenuineChunkedStream");
		}
		if (_currentWriteBlock == null || _writePosition >= _currentWriteBlock.Length - 4)
		{
			ObtainNextBuffer();
		}
		chunk = _currentWriteBlock;
		position = _writePosition;
		MessageCoder.WriteInt32(_currentWriteBlock, _writePosition, number);
		_writePosition += 4;
		_chunksAndStreams[_chunksAndStreams.Count - 1] = _writePosition;
		if (_length >= 0)
		{
			_length += 4L;
		}
	}

	public new void Dispose()
	{
		Close();
	}

	public object Clone()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("GenuineChunkedStream");
		}
		if (_releaseOnReadMode)
		{
			throw new InvalidOperationException();
		}
		GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: false);
		genuineChunkedStream._chunksAndStreams = _chunksAndStreams;
		genuineChunkedStream._currentWriteBlock = _currentWriteBlock;
		genuineChunkedStream._length = _length;
		genuineChunkedStream._writePosition = _writePosition;
		_cloned = true;
		genuineChunkedStream._cloned = true;
		return genuineChunkedStream;
	}
}
