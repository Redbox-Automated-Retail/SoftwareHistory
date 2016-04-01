using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal class ArcusControllerPosition : IControllerPosition
{
	public Axis Axis { get; internal set; }

	public int? XCoordinate { get; internal set; }

	public int? YCoordinate { get; internal set; }

	public bool ReadOk { get; internal set; }
}
