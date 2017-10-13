using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

public sealed class ClearGripperOperation : AbstractOperation<ErrorCodes>
{
	public override ErrorCodes Execute()
	{
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		if (Controller.TrackState != TrackState.Closed && !Controller.TrackClose().Success)
		{
			LogHelper.Instance.Log("[ClearGripper] Track closed failed.");
			return ErrorCodes.TrackCloseTimeout;
		}
		for (int i = 0; i < 2; i++)
		{
			IPickerSensorReadResult pickerSensorReadResult = Controller.ReadSensors();
			if (pickerSensorReadResult.Error != ErrorCodes.Success)
			{
				return pickerSensorReadResult.Error;
			}
			if (!pickerSensorReadResult.IsFull)
			{
				return ErrorCodes.Success;
			}
			bool[] sensors = pickerSensorReadResult.Sensors;
			if (!sensors[0])
			{
				if (!sensors[5])
				{
					return ErrorCodes.Success;
				}
				Controller.SetFinger(GripperFingerState.Rent);
				try
				{
					if (service.AtVendDoor && VendDoorState.Closed != Controller.VendDoorState)
					{
						Controller.VendDoorRent();
					}
					Controller.RollerToPosition(RollerPosition.Position3, 3000);
				}
				finally
				{
					if (VendDoorState.Closed != Controller.VendDoorState)
					{
						Controller.VendDoorClose();
					}
				}
				continue;
			}
			if (sensors[5])
			{
				return ErrorCodes.PickerObstructed;
			}
			if (!sensors[1] && !sensors[2] && !sensors[3] && !sensors[4])
			{
				try
				{
					Controller.StartRollerIn();
					PushIntoSlot();
				}
				finally
				{
					Controller.StopRoller();
				}
			}
			else
			{
				Controller.SetFinger(GripperFingerState.Rent);
				Controller.RollerToPosition(RollerPosition.Position5, 8000);
			}
		}
		return ErrorCodes.PickerObstructed;
	}

	public string GetPrintableLocation()
	{
		string result = "Location unknown";
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		if (service.CurrentLocation != null)
		{
			result = service.CurrentLocation.ToString();
		}
		else if (service.AtVendDoor)
		{
			result = "Vend Door";
		}
		return result;
	}
}
