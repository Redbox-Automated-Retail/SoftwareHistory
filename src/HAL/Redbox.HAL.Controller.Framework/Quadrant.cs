using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class Quadrant
{
	public int Offset { get; private set; }

	public Range Slots { get; private set; }

	public bool IsExcluded { get; set; }

	public Quadrant(int offset)
		: this(offset, null)
	{
	}

	public Quadrant(int offset, Range slots)
	{
		Offset = offset;
		Slots = slots;
		IsExcluded = false;
	}
}
