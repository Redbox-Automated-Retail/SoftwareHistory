using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class EmulatedMotionController2 : AbstractMotionController
{
	internal override bool OnStartup()
	{
		return true;
	}

	internal override bool OnShutdown()
	{
		return true;
	}

	internal override IMotionControlLimitResponse ReadLimits()
	{
		return new EmulatedArcusLimitResponse();
	}

	internal override bool CommunicationOk()
	{
		return true;
	}

	internal override IControllerPosition ReadPositions()
	{
		return EmulatedControllerPosition.DefaultInstance;
	}

	internal override ErrorCodes MoveToTarget(ref MoveTarget target)
	{
		return ErrorCodes.Success;
	}

	internal override ErrorCodes MoveToVend(MoveMode mode)
	{
		return ErrorCodes.Success;
	}

	internal override ErrorCodes HomeAxis(Axis axis)
	{
		return ErrorCodes.Success;
	}

	internal override bool OnResetDeviceDriver()
	{
		return true;
	}

	internal EmulatedMotionController2()
	{
	}
}
