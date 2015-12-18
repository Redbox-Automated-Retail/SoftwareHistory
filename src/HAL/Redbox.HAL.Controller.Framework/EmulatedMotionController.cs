using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal sealed class EmulatedMotionController : MotionController
{
	public override bool CommunicationOk()
	{
		return true;
	}

	public override bool IsMotorErrored(Axis axis, out MotionResult status)
	{
		status = MotionResult.Success;
		return false;
	}

	public override bool GetCurrentLocation(Axis axis, ref MoveTarget target)
	{
		return true;
	}

	protected override bool ClosePortInner()
	{
		return true;
	}

	protected override MotionResult HomeAxisInner(Axis axis)
	{
		if (axis != Axis.X)
		{
			return ControllerEmulationState.Instance.ExpectedHomeYResult;
		}
		return ControllerEmulationState.Instance.ExpectedHomeXResult;
	}

	protected override HomeAxesResult InitAxesInner(IFormattedLog log, bool notUsed)
	{
		HomeAxesResult homeAxesResult = new HomeAxesResult();
		homeAxesResult.Succeded = true;
		return homeAxesResult;
	}

	protected override bool IsInitializedInner()
	{
		return true;
	}

	protected override MotionResult MoveToTargetInner(MoveTarget target)
	{
		return ControllerEmulationState.Instance.ExpectedMoveAbsoluteResult;
	}

	protected override MotionResult MoveToVendInner(MoveMode mode)
	{
		return MotionResult.Success;
	}

	protected override string[] ReadLimitsInner()
	{
		return new string[2] { "UPPER LIMIT: CLEAR", "LOWER LIMIT: BLOCKED" };
	}

	internal override bool ResetDeviceDriver()
	{
		return true;
	}

	internal override void InitializeCommunication()
	{
	}
}
