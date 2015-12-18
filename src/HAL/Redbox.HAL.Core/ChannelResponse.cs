using System;
using System.Collections.Generic;
using System.Threading;

namespace Redbox.HAL.Core;

internal sealed class ChannelResponse : IChannelResponse, IDisposable
{
	internal readonly byte[] Buffer;

	private bool Disposed;

	private readonly List<byte> PortResponse = new List<byte>();

	private readonly EventWaitHandle ReadEvent = new EventWaitHandle(initialState: false, EventResetMode.ManualReset);

	public bool CommOk { get; private set; }

	public byte[] RawResponse => PortResponse.ToArray();

	public string Error { get; private set; }

	public bool ReponseValid { get; internal set; }

	public bool PortTimeout { get; private set; }

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			ReadEvent.Close();
			PortResponse.Clear();
			GC.SuppressFinalize(this);
		}
	}

	public int GetIndex(byte b)
	{
		byte[] array = PortResponse.ToArray();
		if (array == null || array.Length == 0)
		{
			return -1;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == b)
			{
				return i;
			}
		}
		return -1;
	}

	internal void SetCommunicationError()
	{
		Error = "CommunicationError";
		CommOk = false;
	}

	internal void SetClosedError()
	{
		Error = "ChannelClosed";
		CommOk = false;
	}

	internal void Accumulate(int len)
	{
		Accumulate(Buffer, len);
	}

	internal void Accumulate(byte[] bytes, int len)
	{
		for (int i = 0; i < len; i++)
		{
			PortResponse.Add(bytes[i]);
		}
	}

	internal bool Wait(int timeout)
	{
		return ReadEvent.WaitOne(timeout);
	}

	internal void ReadEnd()
	{
		if (!Disposed)
		{
			ReadEvent.Set();
		}
	}

	internal ChannelResponse()
		: this(0)
	{
	}

	internal ChannelResponse(int bufferSize)
	{
		CommOk = true;
		Buffer = ((bufferSize <= 0) ? null : new byte[bufferSize]);
	}
}
