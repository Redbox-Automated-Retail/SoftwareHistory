using System.IO;

namespace Belikov.GenuineChannels.Messaging;

internal class NonClosableStream : Stream
{
	private Stream _underlyingStream;

	public override bool CanRead => _underlyingStream.CanRead;

	public override bool CanSeek => _underlyingStream.CanSeek;

	public override bool CanWrite => _underlyingStream.CanWrite;

	public override long Length => _underlyingStream.Length;

	public override long Position
	{
		get
		{
			return _underlyingStream.Position;
		}
		set
		{
			_underlyingStream.Position = value;
		}
	}

	public NonClosableStream(Stream underlyingStream)
	{
		_underlyingStream = underlyingStream;
	}

	public override void Close()
	{
	}

	public override void Flush()
	{
		_underlyingStream.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return _underlyingStream.Read(buffer, offset, count);
	}

	public override int ReadByte()
	{
		return _underlyingStream.ReadByte();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return _underlyingStream.Seek(offset, origin);
	}

	public override void SetLength(long val)
	{
		_underlyingStream.SetLength(val);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		_underlyingStream.Write(buffer, offset, count);
	}

	public override void WriteByte(byte val)
	{
		_underlyingStream.WriteByte(val);
	}
}
