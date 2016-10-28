namespace Redbox.HAL.Component.Model;

internal struct OffsetMoveData
{
	public int? XOffset { get; private set; }

	public int? YOffset { get; private set; }

	public OffsetMoveData(int? xoff, int? yoff)
	{
		this = default(OffsetMoveData);
		XOffset = xoff;
		YOffset = yoff;
	}
}
