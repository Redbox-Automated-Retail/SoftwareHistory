using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

public sealed class PeekOperation : AbstractOperation<IPeekResult>
{
	public override IPeekResult Execute()
	{
		LogHelper.Instance.Log(">> ControllerService.GripperPeek", LogEntryType.Info);
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		ILocation currentLocation = service.CurrentLocation;
		LocationTestResult locationTestResult = new LocationTestResult(currentLocation.Deck, currentLocation.Slot);
		if (currentLocation.IsWide)
		{
			ErrorCodes errorCodes = SettleDiskInSlot();
			if (errorCodes != ErrorCodes.Success)
			{
				locationTestResult.Error = errorCodes;
				return locationTestResult;
			}
		}
		if (!Controller.SetFinger(GripperFingerState.Closed).Success)
		{
			locationTestResult.Error = ErrorCodes.GripperCloseTimeout;
			return locationTestResult;
		}
		IControlResponse controlResponse = Controller.ExtendArm(ControllerConfiguration.Instance.TestExtendTime);
		if (controlResponse.CommError)
		{
			locationTestResult.Error = ErrorCodes.CommunicationError;
			return locationTestResult;
		}
		locationTestResult.IsFull = controlResponse.TimedOut;
		locationTestResult.Error = ErrorCodes.Success;
		Controller.RetractArm();
		Controller.SetFinger(GripperFingerState.Rent);
		return locationTestResult;
	}
}
