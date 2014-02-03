using System;
using System.IO;
using Belikov.GenuineChannels.BufferPooling;

namespace Belikov.GenuineChannels.Messaging;

internal class DelimiterStream : Stream, IDisposable
{
	private Stream _underlyingStream;

	private long _assumedLength;

	public override bool CanRead => _underlyingStream.CanRead;

	public override bool CanSeek => _underlyingStream.CanSeek;

	public override bool CanWrite => _underlyingStream.CanWrite;

	public override long Length => _assumedLength;

	public override long Position
	{
		get
		{
			return _underlyingStream.Position;
		}
		set
		{
			throw new NotSupportedException("DelimiterStream does not support Position property.");
		}
	}

	public DelimiterStream(Stream underlyingStream, long assumedLength)
	{
		_underlyingStream = underlyingStream;
		_assumedLength = assumedLength;
	}

	public new void Dispose()
	{
		Close();
	}

	public override void Close()
	{
		if (_assumedLength <= 0)
		{
			return;
		}
		BufferKeeper bufferKeeper = new BufferKeeper(0);
		try
		{
			while (_assumedLength > 0)
			{
				int num = _underlyingStream.Read(bufferKeeper.Buffer, 0, (int)_assumedLength);
				_assumedLength -= num;
				if (num == 0)
				{
					throw GenuineExceptions.Get_Receive_IncorrectData();
				}
			}
		}
		finally
		{
			bufferKeeper.Dispose();
		}
	}

	public override void Flush()
	{
		_underlyingStream.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		count = Math.Min(count, (int)_assumedLength);
		if (count <= 0)
		{
			return 0;
		}
		int num = _underlyingStream.Read(buffer, offset, count);
		_assumedLength -= num;
		return num;
	}

	public override int ReadByte()
	{
		int num = ((_assumedLength > 0) ? _underlyingStream.ReadByte() : (-1));
		if (num != -1)
		{
			_assumedLength--;
		}
		return num;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException("DelimiterStream does not support Seek method.");
	}

	public override void SetLength(long val)
	{
		throw new NotSupportedException("DelimiterStream does not support SetLength method.");
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException("DelimiterStream does not support Write method.");
	}

	public override void WriteByte(byte val)
	{
		throw new NotSupportedException("DelimiterStream does not support WriteByte method.");
	}
}
