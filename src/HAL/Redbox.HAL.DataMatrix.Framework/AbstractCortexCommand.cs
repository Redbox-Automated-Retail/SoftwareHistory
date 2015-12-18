using System;
using System.Collections.Generic;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataMatrix.Framework;

internal abstract class AbstractCortexCommand
{
	protected readonly List<CortexPacket> Packets = new List<CortexPacket>();

	private readonly byte[] Prefix = new byte[4] { 238, 238, 238, 238 };

	private byte[] m_preppedCommand;

	public byte[] Command
	{
		get
		{
			if (m_preppedCommand == null)
			{
				m_preppedCommand = PrepCommand(CoreCommand);
			}
			return m_preppedCommand;
		}
	}

	public CortexPacket[] ParsedPackets => Packets.ToArray();

	internal bool PortError { get; private set; }

	internal TimeSpan ExecutionTime { get; private set; }

	protected virtual int ReadTimeout => 2000;

	protected abstract byte[] CoreCommand { get; }

	public bool Process(byte[] b)
	{
		Packets.Clear();
		List<CortexPacket> list = PacketFactory.Instance.Parse(b);
		if (list.Count == 0)
		{
			return false;
		}
		Packets.AddRange(list);
		return CoreProcess();
	}

	public void Send(RedboxSerialPort port)
	{
		port.Validate = Test;
		if (port == null || !port.IsOpen)
		{
			LogHelper.Instance.Log("[Cortex Service] The port is not open sending a command.");
			return;
		}
		try
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			IChannelResponse channelResponse = port.SendRecv(Command, ReadTimeout);
			if (!channelResponse.CommOk)
			{
				LogHelper.Instance.Log("[Cortex Service] Communication error with the device.");
				PortError = true;
			}
			if (!ValidateResponse())
			{
				PortError = true;
			}
			executionTimer.Stop();
			ExecutionTime = executionTimer.Elapsed;
		}
		finally
		{
			port.ResetBuffers();
		}
	}

	protected virtual bool CoreValidate()
	{
		return true;
	}

	protected virtual bool CoreProcess()
	{
		foreach (CortexPacket packet in Packets)
		{
			if (packet.PacketType == PacketType.Incomplete)
			{
				return false;
			}
			if (packet.RawPacketType == 'd')
			{
				return true;
			}
		}
		return false;
	}

	private bool Test(IChannelResponse r)
	{
		return Process(r.RawResponse);
	}

	private bool ValidateResponse()
	{
		if (Packets.Count == 0)
		{
			LogHelper.Instance.Log("[Cortex Commands] Validate response found no response packets.");
			return false;
		}
		foreach (CortexPacket packet in Packets)
		{
			if (packet.PacketType == PacketType.Incomplete)
			{
				LogHelper.Instance.Log("[Cortex Commands] Validate response found an incomplete packet.");
				return false;
			}
		}
		return CoreValidate();
	}

	private byte[] PrepCommand(byte[] baseCommand)
	{
		List<byte> list = new List<byte>();
		try
		{
			list.AddRange(Prefix);
			ushort num = 0;
			for (int i = 0; i < baseCommand.Length; i++)
			{
				num = CRC16.CRC(num, baseCommand[i]);
				list.Add(baseCommand[i]);
			}
			byte item = (byte)((num >> 8) & 0x7F);
			list.Add(item);
			byte item2 = (byte)(num & 0x7F);
			list.Add(item2);
			return list.ToArray();
		}
		finally
		{
			list.Clear();
		}
	}
}
