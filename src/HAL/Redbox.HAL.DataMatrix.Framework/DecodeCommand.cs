using System;
using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Core;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class DecodeCommand : AbstractCortexCommand
{
	internal readonly List<string> FoundCodes = new List<string>();

	private readonly List<int> DataPackets = new List<int>();

	private readonly byte[] m_rawCommand = new byte[4] { 36, 1, 5, 0 };

	private readonly CortexDecodeVersion Version;

	private readonly char[] Separator = new char[1] { ' ' };

	internal bool DecodeFailure { get; private set; }

	internal string FailedImage { get; private set; }

	internal ImagePacket ImagePacket { get; private set; }

	protected override byte[] CoreCommand => m_rawCommand;

	protected override int ReadTimeout => BarcodeConfiguration.Instance.ScanTimeout;

	internal DecodeCommand(CortexDecodeVersion version)
	{
		Version = version;
	}

	protected override bool CoreValidate()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (CortexPacket packet in Packets)
		{
			if (packet.RawPacketType == 'd')
			{
				num++;
			}
			else if (packet.RawPacketType == 'r')
			{
				num2++;
			}
			else if (packet.PacketType == PacketType.Data)
			{
				num3++;
			}
		}
		if (num == 0)
		{
			return false;
		}
		if (num2 <= 0)
		{
			return num3 > 0;
		}
		return true;
	}

	protected override bool CoreProcess()
	{
		bool logDetailedScan = BarcodeConfiguration.Instance.LogDetailedScan;
		ImagePacket = null;
		bool flag = false;
		if (logDetailedScan)
		{
			LogHelper.Instance.Log("[Decode Command] start.");
		}
		try
		{
			CortexPacket packet;
			foreach (CortexPacket packet2 in Packets)
			{
				packet = packet2;
				if (packet.PacketType == PacketType.Incomplete)
				{
					if (logDetailedScan)
					{
						LogHelper.Instance.Log(" >> Reponse packet: incomplete.");
					}
					return false;
				}
				if (packet.PacketType == PacketType.Data)
				{
					string text = Encoding.ASCII.GetString(packet.PayloadData);
					if (logDetailedScan)
					{
						LogHelper.Instance.Log(" >> Reponse packet: data (barcode = {0} Number = {1}).", text, packet.PacketNumber);
					}
					List<int> list = DataPackets.FindAll((int each) => each == packet.PacketNumber);
					if (list.Count != 0)
					{
						continue;
					}
					DataPackets.Add(packet.PacketNumber);
					if (CortexDecodeVersion.Version2 == Version)
					{
						string[] array = text.Split(Separator);
						if ((int)Convert.ChangeType(array[2], typeof(int)) == 0)
						{
							DecodeFailure = true;
							continue;
						}
						for (int num = 3; num < array.Length; num++)
						{
							FoundCodes.Add(array[num]);
						}
					}
					else
					{
						FoundCodes.Add(text);
					}
				}
				else
				{
					if (logDetailedScan)
					{
						LogHelper.Instance.Log(" >> Reponse packet '{0}'", packet.RawPacketType);
					}
					switch (packet.RawPacketType)
					{
					case 'e':
					case 'r':
						DecodeFailure = true;
						break;
					case 'd':
						flag = true;
						break;
					case 'm':
						ImagePacket = new ImagePacket(packet);
						break;
					}
				}
			}
			if (!flag)
			{
				return false;
			}
			if (DataPackets.Count == 0)
			{
				return false;
			}
			if (DecodeFailure)
			{
				return true;
			}
			if (logDetailedScan)
			{
				LogHelper.Instance.Log(" >> Packet received count: {0}", DataPackets.Count);
				foreach (string foundCode in FoundCodes)
				{
					LogHelper.Instance.Log(" >>  Scanned code {0}", foundCode);
				}
			}
			return (Version == CortexDecodeVersion.Version1) ? (DataPackets.Count > 1) : (DataPackets.Count > 0);
		}
		finally
		{
			if (logDetailedScan)
			{
				LogHelper.Instance.Log("[Decode Command] End.");
			}
		}
	}
}
