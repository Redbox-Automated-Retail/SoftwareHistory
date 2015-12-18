using System;
using System.IO.Ports;
using System.Threading;

namespace Redbox.HAL.Core;

internal sealed class CallbackReadPort : RedboxSerialPort
{
	private const int Reading = 1;

	private const int NotReading = 0;

	private int ReadState;

	private byte[] ReadBuffer;

	private ChannelResponse Current;

	protected override PortModes Mode => PortModes.Callback;

	public override bool Open()
	{
		if (Port == null)
		{
			return false;
		}
		Port.DataReceived += Port_DataReceived;
		if (!OpenInner())
		{
			return false;
		}
		ReadBuffer = new byte[Port.ReadBufferSize];
		return true;
	}

	public override ChannelResponse SendRecv(byte[] command, int readTimeout)
	{
		ChannelResponse channelResponse = (Current = ChannelResponse.NewChannel(createBuffer: false));
		try
		{
			try
			{
				if (!Port.IsOpen)
				{
					LogHelper.Instance.Log("Port.WriteCommand: the port was closed.", LogEntryType.Error);
					channelResponse.SetClosedError();
					return channelResponse;
				}
				Interlocked.Exchange(ref ReadState, 1);
				Port.Write(command, 0, command.Length);
				if (base.WriteTerminator != null)
				{
					Port.Write(base.WriteTerminator, 0, base.WriteTerminator.Length);
				}
				PerfFunctions.SpinWait(WritePause);
			}
			catch (Exception e)
			{
				Interlocked.Exchange(ref ReadState, 0);
				LogHelper.Instance.Log("Port.WriteCommand() caught an exception.", e);
				channelResponse.SetCommunicationError();
				return channelResponse;
			}
			channelResponse.ReadEvent.WaitOne(readTimeout);
			Interlocked.Exchange(ref ReadState, 0);
			return channelResponse;
		}
		finally
		{
			Current = null;
		}
	}

	protected override void CloseInner()
	{
		ReadBuffer = null;
	}

	internal CallbackReadPort(RedboxSerialPortConfiguration conf)
		: base(conf)
	{
	}

	private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
	{
		int bytesToRead = Port.BytesToRead;
		if (bytesToRead == 0)
		{
			return;
		}
		if (bytesToRead > ReadBuffer.Length)
		{
			LogHelper.Instance.Log("[DataReceived ERROR] {0} The bytes to read exceeds the read buffer length!", string.IsNullOrEmpty(FriendlyName) ? base.Name : FriendlyName);
		}
		try
		{
			int num = Port.Read(ReadBuffer, 0, bytesToRead);
			if (num != bytesToRead)
			{
				LogHelper.Instance.Log("[DataReceived ERROR] {0} The bytes to read {1} doesn't match the read count {2}", string.IsNullOrEmpty(FriendlyName) ? base.Name : FriendlyName, num, bytesToRead);
			}
			if (1 != ReadState)
			{
				return;
			}
			if (Current == null)
			{
				LogHelper.Instance.Log("[DataReceived] {0} called outside of a send; {1} bytes read.", string.IsNullOrEmpty(FriendlyName) ? base.Name : FriendlyName, num);
				return;
			}
			Current.Accumulate(ReadBuffer, bytesToRead);
			if (base.Validate(Current))
			{
				Current.ReadEvent.Set();
			}
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log($"DataReceived {base.Name} handler caught an exception.", e2);
		}
	}
}
