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

	internal override string[] ReadLimits()
	{
		return new string[2] { "UPPER LIMIT: CLEAR", "LOWER LIMIT: BLOCKED" };
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
		return ControllerEmulationState.Instance.ExpectedMoveAbsoluteResult;
	}

	internal override ErrorCodes MoveToVend(MoveMode mode)
	{
		return ErrorCodes.Success;
	}

	internal override ErrorCodes HomeAxis(Axis axis)
	{
		if (axis != Axis.X)
		{
			return ControllerEmulationState.Instance.ExpectedHomeYResult;
		}
		return ControllerEmulationState.Instance.ExpectedHomeXResult;
	}

	internal override bool OnResetDeviceDriver()
	{
		return true;
	}

	internal EmulatedMotionController2()
	{
	}
}
