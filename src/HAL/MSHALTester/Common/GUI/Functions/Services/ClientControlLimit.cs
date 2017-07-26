using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Common.GUI.Functions.Services;

internal sealed class ClientControlLimit : IMotionControlLimit
{
	public MotionControlLimits Limit { get; private set; }

	public bool Blocked { get; private set; }

	internal ClientControlLimit(MotionControlLimits lim, bool blocked)
	{
		Limit = lim;
		Blocked = blocked;
	}
}
