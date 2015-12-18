using System.Collections.Generic;
using System.Text;

namespace Redbox.HAL.DataMatrix.Framework;

internal class ImageDescriptorPacket : ImagePacket
{
	internal int ImageSize { get; private set; }

	internal ImageDescriptorPacket(CortexPacket packet)
		: base(packet)
	{
	}

	protected override void ParseData()
	{
		List<byte> list = new List<byte>();
		int num = 22;
		while (Packet[num] != 9)
		{
			list.Add(Packet[num++]);
		}
		base.ImageName = Encoding.ASCII.GetString(list.ToArray());
		num += 2;
		StringBuilder stringBuilder = new StringBuilder();
		for (; Packet[num] != 41; num++)
		{
			stringBuilder.Append((byte)(Packet[num] - 48));
		}
		ImageSize = int.Parse(stringBuilder.ToString());
		stringBuilder = null;
	}
}
