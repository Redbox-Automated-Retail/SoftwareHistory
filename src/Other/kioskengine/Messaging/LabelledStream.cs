using System;
using System.IO;
using System.Runtime.InteropServices;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Messaging;

[ComVisible(true)]
public class LabelledStream : Stream, IDisposable
{
	public ITransportContext ITransportContext;

	private Stream _underlyingStream;

	private byte[] _readBuffer;

	private int _validLength;

	private int _currentPosition;

	private int _currentPacketSize;

	private int _currentPacketBytesRead;

	private bool _messageRead;

	public override bool CanRead => _underlyingStream.CanRead || _currentPosition < _validLength;

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

	public LabelledStream(ITransportContext iTransportContext, Stream underlyingStream, byte[] intermediateBuffer)
	{
		ITransportContext = iTransportContext;
		_underlyingStream = underlyingStream;
		_readBuffer = intermediateBuffer;
		_validLength = 0;
		_currentPosition = 0;
		_currentPacketSize = 0;
		_currentPacketBytesRead = 0;
		_messageRead = false;
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
			if (count <= 0 || (_currentPacketBytesRead >= _currentPacketSize && _messageRead))
			{
				break;
			}
			ReadNextPortion();
		}
		return num2;
	}

	public override int ReadByte()
	{
		if (_currentPosition < _validLength)
		{
			return _readBuffer[_currentPosition++];
		}
		ReadNextPortion();
		if (_currentPosition < _validLength)
		{
			return _readBuffer[_currentPosition++];
		}
		return -1;
	}

	private void ReadNextPortion()
	{
		int num = 0;
		int num2 = 0;
		if (_currentPacketBytesRead < _currentPacketSize)
		{
			num2 = Math.Min(_currentPacketSize - _currentPacketBytesRead, _readBuffer.Length);
			_validLength = _underlyingStream.Read(_readBuffer, 0, num2);
			if (_validLength == 0)
			{
				throw GenuineExceptions.Get_Receive_Portion();
			}
			ITransportContext.ConnectionManager.IncreaseBytesReceived(_validLength);
			_currentPacketBytesRead += _validLength;
			_currentPosition = 0;
		}
		else
		{
			if (_messageRead)
			{
				return;
			}
			num2 = 6;
			for (_currentPosition = 0; _currentPosition < num2; _currentPosition += num)
			{
				num = _underlyingStream.Read(_readBuffer, _currentPosition, num2 - _currentPosition);
				if (num == 0)
				{
					throw GenuineExceptions.Get_Receive_Portion();
				}
				ITransportContext.ConnectionManager.IncreaseBytesReceived(num);
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
				_validLength = _underlyingStream.Read(_readBuffer, 0, num2);
				if (_validLength == 0)
				{
					throw GenuineExceptions.Get_Receive_Portion();
				}
				ITransportContext.ConnectionManager.IncreaseBytesReceived(_validLength);
				_currentPacketBytesRead += _validLength;
				_currentPosition = 0;
			}
		}
	}

	public override void Close()
	{
		while (_currentPacketBytesRead < _currentPacketSize || !_messageRead)
		{
			ReadNextPortion();
		}
	}

	public override void Flush()
	{
		throw new NotSupportedException();
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

	public override void WriteByte(byte val)
	{
		throw new NotSupportedException();
	}

	public new void Dispose()
	{
		Close();
	}
}
