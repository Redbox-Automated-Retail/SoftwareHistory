using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class HardwareStatusJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			ResultHelper.Instance.CreateInfoResult("HardwareStatusOk", "The three tests passed!");
			return;
		}
		int num = 0;
		using (MotionController motionController = MotionController.GetController())
		{
			if (!motionController.IsInitialized())
			{
				ResultHelper.Instance.CreateInfoResult("MotionControllerCommunicationError", "Could not talk with the motion controller.");
				num++;
			}
			if (ControllerConfiguration.Instance.EnableDoorCheckInStatus && DoorSensors.Instance.Query() != DoorSensorResult.Ok)
			{
				ResultHelper.Instance.CreateInfoResult("MotionControllerCommunicationError", "The door sensors are not in a safe mode.");
				num++;
			}
		}
		if (!ControllerService.Instance.IsGripperClear())
		{
			num++;
			ResultHelper.Instance.CreateInfoResult("GripperObstructed", "The gripper is obstructed.");
		}
		if (PickerBoard.Instance.IsFull())
		{
			num++;
			ResultHelper.Instance.CreateInfoResult("GripperObstructed", "The picker is full.");
		}
		if (base.VendDoor.Close())
		{
			num++;
			ResultHelper.Instance.CreateInfoResult("VendDoorOpen", "Unable to close the vend door.");
		}
		if (num == 0)
		{
			ResultHelper.Instance.CreateInfoResult("HardwareStatusOk", "The three tests passed!");
		}
		else
		{
			ResultHelper.Instance.CreateInfoResult("HardwareStatusInError", "One or more ops failed.");
		}
	}
}
