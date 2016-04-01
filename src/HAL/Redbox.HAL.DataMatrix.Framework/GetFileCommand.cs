using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class GetFileCommand : TextCommand
{
	private readonly string ImageName;

	private readonly List<byte> ImageData = new List<byte>();

	private readonly List<int> ProcessedPackets = new List<int>();

	internal byte[] ImageBytes => ImageData.ToArray();

	internal ImageDescriptorPacket Descriptor { get; private set; }

	protected override string Prefix => "^";

	protected override string Data => ImageName;

	protected override int ReadTimeout => 10000;

	internal GetFileCommand(ImagePacket packet)
	{
		ImageName = packet.ImageName;
	}

	protected override bool CoreValidate()
	{
		if (Descriptor == null)
		{
			return false;
		}
		return ImageBytes.Length == Descriptor.ImageSize;
	}

	protected override bool CoreProcess()
	{
		bool logDetailedScan = BarcodeConfiguration.Instance.LogDetailedScan;
		if (logDetailedScan)
		{
			LogHelper.Instance.Log("[GetFileCommand] Start.");
		}
		try
		{
			CortexPacket packet;
			foreach (CortexPacket packet2 in Packets)
			{
				packet = packet2;
				if (packet.PacketType == PacketType.Data)
				{
					if (-1 == ProcessedPackets.FindIndex((int each) => each == packet.PacketNumber))
					{
						if (logDetailedScan)
						{
							LogHelper.Instance.Log(" [GetFileCommand] Process data packet ( data length = {0} )", packet.PayloadData.Length);
						}
						ImageData.AddRange(packet.PayloadData);
						ProcessedPackets.Add(packet.PacketNumber);
					}
					continue;
				}
				if (packet.PacketType == PacketType.Incomplete)
				{
					if (logDetailedScan)
					{
						LogHelper.Instance.Log(" [GetFileCommand] Process incomplete packet.");
					}
					return false;
				}
				if (packet.RawPacketType == 'g')
				{
					if (Descriptor == null)
					{
						Descriptor = new ImageDescriptorPacket(packet);
					}
					if (logDetailedScan)
					{
						LogHelper.Instance.Log(" [GetFileCommand] Process 'g' packet ( ImageSize = {0} ).", Descriptor.ImageSize);
					}
				}
			}
			if (Descriptor == null)
			{
				return false;
			}
			return ImageData.Count == Descriptor.ImageSize;
		}
		finally
		{
			if (logDetailedScan)
			{
				LogHelper.Instance.Log("[GetFileCommand] End.");
			}
		}
	}
}
