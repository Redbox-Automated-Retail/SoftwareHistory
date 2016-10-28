using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class EmulatedArcusLimitResponse : IMotionControlLimitResponse
{
	public bool ReadOk => true;

	public IMotionControlLimit[] Limits { get; private set; }

	public bool IsLimitBlocked(MotionControlLimits limit)
	{
		return false;
	}

	internal EmulatedArcusLimitResponse()
	{
		Limits = new IMotionControlLimit[2];
		Limits[0] = new ArcusControllerLimit(MotionControlLimits.Upper, b: false);
		Limits[1] = new ArcusControllerLimit(MotionControlLimits.Lower, b: true);
	}
}
