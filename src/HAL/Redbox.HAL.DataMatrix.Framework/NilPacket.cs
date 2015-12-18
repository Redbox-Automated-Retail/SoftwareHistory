namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class NilPacket : CortexPacket
{
	internal override PacketType PacketType => PacketType.Invalid;

	internal NilPacket(byte[] packet)
	{
	}
}
