namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class IncompletePacket : CortexPacket
{
	internal override PacketType PacketType => PacketType.Incomplete;
}
