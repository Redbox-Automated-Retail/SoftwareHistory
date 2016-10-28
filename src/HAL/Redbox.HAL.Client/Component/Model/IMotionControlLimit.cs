namespace Redbox.HAL.Component.Model;

internal interface IMotionControlLimit
{
	MotionControlLimits Limit { get; }

	bool Blocked { get; }
}
