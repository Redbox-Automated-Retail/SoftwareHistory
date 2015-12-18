using System.Text;

namespace Redbox.HAL.DataMatrix.Framework;

internal class ImageInfoPacket : CortexPacket
{
	internal override PacketType PacketType => PacketType.Response;

	protected override void ParseData()
	{
		base.RawPacketType = Encoding.ASCII.GetChars(Packet, 21, 1)[0];
		if (base.DataSize != 0)
		{
			for (int i = 22; Packet[i] != 4; i++)
			{
				PacketData.Add(Packet[i]);
			}
		}
	}
}
