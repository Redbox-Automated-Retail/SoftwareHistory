using System.Collections.Generic;
using System.Text;

namespace Redbox.HAL.DataMatrix.Framework;

internal class ImagePacket : CortexPacket
{
	protected const int DataStart = 22;

	internal string ImageName { get; set; }

	internal ImagePacket(CortexPacket packet)
		: base(packet)
	{
	}

	protected override void ParseData()
	{
		List<byte> list = new List<byte>();
		int num = 22;
		while (Packet[num] != 4)
		{
			list.Add(Packet[num++]);
		}
		ImageName = Encoding.ASCII.GetString(list.ToArray());
	}
}
