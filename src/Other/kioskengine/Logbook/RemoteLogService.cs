using System;
using System.IO;
using System.Runtime.InteropServices;
using Belikov.GenuineChannels.DirectExchange;
using Belikov.GenuineChannels.Messaging;
using Belikov.GenuineChannels.TransportContext;

namespace Belikov.GenuineChannels.Logbook;

[ComVisible(true)]
public class RemoteLogService : MarshalByRefObject, IServerServiceEntry
{
	private MemoryWritingStream _memoryWritingStream;

	private bool _stopped = false;

	public bool Enabled
	{
		get
		{
			return _memoryWritingStream.Enabled;
		}
		set
		{
			if (!_stopped)
			{
				_memoryWritingStream.Enabled = value;
			}
		}
	}

	public RemoteLogService(MemoryWritingStream memoryWritingStream)
	{
		_memoryWritingStream = memoryWritingStream;
	}

	public void StopLogging()
	{
		_stopped = true;
		_memoryWritingStream.StopLogging();
	}

	public Stream HandleMessage(Stream stream, HostInformation sender)
	{
		if (_stopped)
		{
			return Stream.Null;
		}
		int size = 640000;
		GenuineChunkedStream genuineChunkedStream = new GenuineChunkedStream(releaseOnReadMode: true);
		GenuineUtility.CopyStreamToStream(_memoryWritingStream, genuineChunkedStream, size);
		return genuineChunkedStream;
	}

	public override object InitializeLifetimeService()
	{
		return null;
	}
}
