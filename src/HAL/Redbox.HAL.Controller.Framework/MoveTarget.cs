namespace Redbox.HAL.Controller.Framework;

public struct MoveTarget
{
	public Axis Axis { get; set; }

	public int? XCoordinate { get; set; }

	public int? YCoordinate { get; set; }
}
