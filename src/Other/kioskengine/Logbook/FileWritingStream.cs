using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Belikov.GenuineChannels.Logbook;

[ComVisible(true)]
public class FileWritingStream : Stream
{
	private bool _addSuffixToBaseFileName;

	private string _baseFileName;

	private DateTime _lastDateTimeValue = DateTime.MinValue;

	private Stream _fileStream;

	private bool _isRecordStarted = false;

	private int nextTry = GenuineUtility.TickCount;

	public override bool CanRead => false;

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
			return _fileStream.Position;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public FileWritingStream(string baseFileName, bool addSuffixToBaseFileName)
	{
		if (baseFileName == null)
		{
			throw new NullReferenceException();
		}
		_addSuffixToBaseFileName = addSuffixToBaseFileName;
		_baseFileName = baseFileName;
	}

	public FileWritingStream(string baseFileName)
		: this(baseFileName, addSuffixToBaseFileName: true)
	{
	}

	private void CheckThatCorrectFileIsOpened()
	{
		try
		{
			DateTime today = DateTime.Today;
			if ((_lastDateTimeValue != today || _fileStream == null) && GenuineUtility.IsTimeoutExpired(nextTry))
			{
				if (_fileStream != null)
				{
					_fileStream.Close();
				}
				_lastDateTimeValue = DateTime.Today;
				string text = _baseFileName;
				if (_addSuffixToBaseFileName)
				{
					text = text + "." + _lastDateTimeValue.ToString("yyyy-MM-dd") + ".genchlog";
				}
				_fileStream = new FileStream(text, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
				_fileStream.Seek(0L, SeekOrigin.End);
			}
		}
		catch
		{
			_fileStream = null;
			nextTry = GenuineUtility.GetTimeout(15000);
		}
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (!_isRecordStarted)
		{
			CheckThatCorrectFileIsOpened();
			_isRecordStarted = true;
		}
		if (_fileStream != null)
		{
			_fileStream.Write(buffer, offset, count);
		}
	}

	public override void WriteByte(byte val)
	{
		if (!_isRecordStarted)
		{
			CheckThatCorrectFileIsOpened();
			_isRecordStarted = true;
		}
		if (_fileStream != null)
		{
			_fileStream.WriteByte(val);
		}
	}

	public override void Flush()
	{
		if (_fileStream != null)
		{
			_fileStream.Flush();
		}
		_isRecordStarted = false;
	}

	public override void Close()
	{
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
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
