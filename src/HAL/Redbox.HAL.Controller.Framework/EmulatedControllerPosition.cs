using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class EmulatedControllerPosition : IControllerPosition
{
	internal static IControllerPosition DefaultInstance;

	public Axis Axis => Axis.XY;

	public int? XCoordinate => 0;

	public int? YCoordinate => 0;

	public bool ReadOk => true;

	private EmulatedControllerPosition()
	{
	}

	static EmulatedControllerPosition()
	{
		DefaultInstance = new EmulatedControllerPosition();
	}
}
