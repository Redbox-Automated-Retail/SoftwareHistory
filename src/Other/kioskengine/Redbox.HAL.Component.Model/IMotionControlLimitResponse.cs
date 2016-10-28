namespace Redbox.HAL.Component.Model;

internal interface IMotionControlLimitResponse
{
	bool ReadOk { get; }

	IMotionControlLimit[] Limits { get; }

	bool IsLimitBlocked(MotionControlLimits limit);
}
