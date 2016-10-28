using System;
using System.IO;
using System.Runtime.InteropServices;
using Belikov.GenuineChannels.Messaging;

namespace Belikov.GenuineChannels.Logbook;

[ComVisible(true)]
public class MemoryWritingStream : Stream
{
	private bool _enabled = true;

	private int _bytesWritten = 0;

	private int _maximumSize;

	private bool _isCurrentRecordIgnored = true;

	private GenuineChunkedStream _baseStream = new GenuineChunkedStream(releaseOnReadMode: true);

	private bool _isRecordStarted = false;

	private int _sizeOfRecord = 0;

	public object SyncRoot => this;

	public bool Enabled
	{
		get
		{
			lock (SyncRoot)
			{
				return _enabled;
			}
		}
		set
		{
			lock (SyncRoot)
			{
				_enabled = value;
			}
		}
	}

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => true;

	public override long Length => _bytesWritten;

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

	public MemoryWritingStream(int maximumSize)
	{
		_maximumSize = maximumSize;
	}

	public void StopLogging()
	{
		Enabled = false;
		_baseStream.WriteTo(Stream.Null);
	}

	private void CheckIfAllowedSizeExceeded()
	{
		_isCurrentRecordIgnored = !Enabled || _bytesWritten >= _maximumSize;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		lock (SyncRoot)
		{
			if (!_isRecordStarted)
			{
				CheckIfAllowedSizeExceeded();
				_isRecordStarted = true;
			}
			if (!_isCurrentRecordIgnored)
			{
				_baseStream.Write(buffer, offset, count);
				_sizeOfRecord += count;
			}
		}
	}

	public override void WriteByte(byte val)
	{
		lock (SyncRoot)
		{
			if (!_isRecordStarted)
			{
				CheckIfAllowedSizeExceeded();
				_isRecordStarted = true;
			}
			if (!_isCurrentRecordIgnored)
			{
				_baseStream.WriteByte(val);
				_sizeOfRecord++;
			}
		}
	}

	public override void Flush()
	{
		lock (SyncRoot)
		{
			_baseStream.Flush();
			_isRecordStarted = false;
			_bytesWritten += _sizeOfRecord;
			_sizeOfRecord = 0;
		}
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		lock (SyncRoot)
		{
			int num = _baseStream.Read(buffer, offset, Math.Min(_bytesWritten - _sizeOfRecord, count));
			_bytesWritten -= num;
			return num;
		}
	}

	public override void Close()
	{
	}

	public override int ReadByte()
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
}
