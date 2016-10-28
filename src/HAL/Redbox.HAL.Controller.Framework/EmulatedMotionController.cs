using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class EmulatedMotionController : MotionController
{
	protected override void OnConfigurationChangeEnd()
	{
	}

	protected override void OnConfigurationChangeStart()
	{
	}

	protected override void OnConfigurationLoad()
	{
	}

	protected override ErrorCodes HomeAxisInner(Axis axis)
	{
		return ErrorCodes.Success;
	}

	protected override ErrorCodes MoveToTargetInner(ref MoveTarget target)
	{
		return ErrorCodes.Success;
	}

	protected override ErrorCodes MoveToVendInner(MoveMode mode)
	{
		return ErrorCodes.Success;
	}

	protected override bool IsInitializedInner()
	{
		return true;
	}

	internal override bool CommunicationOk()
	{
		return true;
	}

	internal override IControllerPosition ReadPositions()
	{
		return EmulatedControllerPosition.DefaultInstance;
	}

	internal override bool ClosePort()
	{
		return true;
	}

	internal override IMotionControlLimitResponse ReadLimits()
	{
		return new EmulatedArcusLimitResponse();
	}

	internal override bool ResetDeviceDriver()
	{
		return true;
	}

	internal override bool InitializeCommunication()
	{
		return true;
	}

	internal EmulatedMotionController(MotionControlService service)
		: base(service)
	{
	}
}
