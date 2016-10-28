using System;

namespace Belikov.GenuineChannels.GenuineXHttp;

internal class GXHTTPHeaderParser
{
	public enum HeaderFields
	{
		ContentLength,
		Expect100Continue,
		FirstLine,
		OtherField
	}

	private bool _http11_100continueIsParsed;

	private bool _firstLineIsParsed;

	private int _lineNumber;

	private bool _client;

	private long _contentLength = -1L;

	private bool _http11_100Continue;

	private bool _isHttp11;

	private bool _contentLengthIsParsed => _contentLength != -1;

	public long ContentLength => _contentLength;

	public bool Http11_100Continue => _http11_100Continue;

	public bool IsHttp11 => _isHttp11;

	public GXHTTPHeaderParser(bool client)
	{
		_contentLength = -1L;
		_http11_100continueIsParsed = false;
		_isHttp11 = false;
		_firstLineIsParsed = false;
		_lineNumber = -1;
		_client = client;
	}

	public HeaderFields ParseHeader(string line, int indexOfFirstDigit, int indexOfLastDigit)
	{
		_lineNumber++;
		if (!_firstLineIsParsed)
		{
			int num = line.IndexOf("TTP/");
			if (num < 0)
			{
				throw GenuineExceptions.Get_Receive_IncorrectData();
			}
			_firstLineIsParsed = true;
			_isHttp11 = line.Substring(num + 4, 3).CompareTo("1.1") == 0;
			if (line.IndexOf("409", num + 1) >= 0)
			{
				throw GenuineExceptions.Get_Receive_ConflictOfConnections();
			}
			return HeaderFields.FirstLine;
		}
		if (_client && !_firstLineIsParsed)
		{
			throw GenuineExceptions.Get_Receive_IncorrectData();
		}
		if (!_contentLengthIsParsed && line.IndexOf("CONTENT-LENGTH") >= 0)
		{
			_contentLength = Convert.ToInt64(line.Substring(indexOfFirstDigit, indexOfLastDigit - indexOfFirstDigit + 1));
			return HeaderFields.ContentLength;
		}
		int num2;
		if (!_client && !_http11_100continueIsParsed && (num2 = line.IndexOf("EXPECT")) >= 0 && line.IndexOf("100", num2 + 1) >= 0)
		{
			_http11_100continueIsParsed = true;
			_http11_100Continue = true;
			return HeaderFields.Expect100Continue;
		}
		if (_lineNumber > 30)
		{
			throw GenuineExceptions.Get_Receive_IncorrectData();
		}
		return HeaderFields.OtherField;
	}
}
