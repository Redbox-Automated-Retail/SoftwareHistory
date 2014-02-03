using System;
using System.IO;
using System.Security.Cryptography;

namespace Belikov.GenuineChannels.Security;

internal class ResettableCryptoStream : Stream
{
	private Stream _underlyingStream;

	private ICryptoTransform _encryptor;

	private Stream _currentStream;

	public override bool CanRead => _currentStream.CanRead;

	public override bool CanSeek => false;

	public override bool CanWrite => _currentStream.CanWrite;

	public override long Length => _currentStream.Length;

	public override long Position
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			if (value != 0)
			{
				throw new NotSupportedException("Only setting position to zero is supported.");
			}
			Seek(0L, SeekOrigin.Begin);
		}
	}

	public ResettableCryptoStream(Stream underlyingStream, ICryptoTransform encryptor)
	{
		_underlyingStream = underlyingStream;
		_encryptor = encryptor;
		_currentStream = new CryptoStream(new FinishReadingStream(underlyingStream), _encryptor, CryptoStreamMode.Read);
	}

	public override void Close()
	{
		_currentStream.Close();
		_underlyingStream.Close();
	}

	public override void Flush()
	{
		_currentStream.Close();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return _currentStream.Read(buffer, offset, count);
	}

	public override int ReadByte()
	{
		return _currentStream.ReadByte();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		if (offset != 0 || origin != SeekOrigin.Begin)
		{
			throw new NotSupportedException("Only setting position to the very beginning is supported.");
		}
		_underlyingStream.Position = 0L;
		_currentStream = new CryptoStream(new FinishReadingStream(_underlyingStream), _encryptor, CryptoStreamMode.Read);
		return 0L;
	}

	public override void SetLength(long val)
	{
		throw new NotSupportedException();
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
