using System;
using System.IO.Ports;
using System.Text;
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

	public override IChannelResponse SendRecv(byte[] command, int readTimeout)
	{
		ChannelResponse channelResponse = (Current = new ChannelResponse());
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
				LogHelper.Instance.Log("Port.WriteCommand() caught an exception.", e);
				channelResponse.SetCommunicationError();
				return channelResponse;
			}
			if (!channelResponse.Wait(readTimeout))
			{
				channelResponse.SetCommunicationError();
				if (string.IsNullOrEmpty(FriendlyName))
				{
					LogHelper.Instance.Log("Communication ERROR: port read timed out", LogEntryType.Error);
				}
				else
				{
					LogHelper.Instance.Log(LogEntryType.Error, "Communication ERROR ({0}): port read timed out", FriendlyName);
				}
			}
			return channelResponse;
		}
		finally
		{
			Interlocked.Exchange(ref ReadState, 0);
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
		if (base.EnableDebugging)
		{
			LogHelper.Instance.Log("[Port_DataReceived] There are {0} bytes to read.", bytesToRead);
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
				Current.ReadEnd();
			}
			if (base.EnableDebugging)
			{
				LogHelper.Instance.Log("[Port_DataReceived] response currently: {0}", Encoding.ASCII.GetString(Current.RawResponse));
			}
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log($"DataReceived {base.Name} handler caught an exception.", e2);
		}
	}
}
