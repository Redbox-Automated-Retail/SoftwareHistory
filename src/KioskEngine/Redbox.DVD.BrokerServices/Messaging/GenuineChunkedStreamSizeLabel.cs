using System;

namespace Belikov.GenuineChannels.Messaging;

internal struct GenuineChunkedStreamSizeLabel : IDisposable
{
	private GenuineChunkedStream _outputStream;

	private byte[] _chunk;

	private int _position;

	private long _initialLength;

	public GenuineChunkedStreamSizeLabel(GenuineChunkedStream outputStream)
	{
		_outputStream = outputStream;
		_outputStream.WriteInt32AndRememberItsLocation(0, out _chunk, out _position);
		_initialLength = _outputStream.Length;
	}

	public void Dispose()
	{
		MessageCoder.WriteInt32(_chunk, _position, (int)(_outputStream.Length - _initialLength));
	}
}
