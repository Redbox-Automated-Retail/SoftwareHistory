using System;
using System.Collections.Generic;
using System.Threading;

namespace Redbox.HAL.Core;

internal sealed class ChannelResponse : IDisposable
{
	private enum ChannelError
	{
		Success,
		ChannelClosed,
		CommunicationError
	}

	private readonly List<byte> PortResponse = new List<byte>();

	public bool CommOk => Error == ChannelError.Success;

	public byte[] RawResponse => PortResponse.ToArray();

	public string CommError => Error.ToString();

	public bool ReponseValid { get; internal set; }

	public bool PortTimeout { get; private set; }

	internal AutoResetEvent ReadEvent { get; private set; }

	internal byte[] ReadBuffer { get; private set; }

	private ChannelError Error { get; set; }

	private bool Disposed { get; set; }

	public static bool EndsWithEOT(ChannelResponse response)
	{
		return -1 != response.IndexOf(4);
	}

	public int IndexOf(byte b)
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

	internal static ChannelResponse NewChannel()
	{
		return new ChannelResponse(createBuffer: true);
	}

	internal static ChannelResponse NewChannel(bool createBuffer)
	{
		return new ChannelResponse(createBuffer);
	}

	internal void SetCommunicationError()
	{
		UpdateErrorState(ChannelError.CommunicationError);
	}

	internal void SetClosedError()
	{
		UpdateErrorState(ChannelError.ChannelClosed);
	}

	internal void Accumulate(byte[] bytes, int len)
	{
		for (int i = 0; i < len; i++)
		{
			PortResponse.Add(bytes[i]);
		}
	}

	internal void Accumulate(int count)
	{
		for (int i = 0; i < count; i++)
		{
			PortResponse.Add(ReadBuffer[i]);
		}
	}

	private void UpdateErrorState(ChannelError err)
	{
		Error = err;
	}

	private ChannelResponse(bool createBuffer)
	{
		Error = ChannelError.Success;
		ReadEvent = new AutoResetEvent(initialState: false);
		if (createBuffer)
		{
			ReadBuffer = new byte[256];
		}
	}
}
