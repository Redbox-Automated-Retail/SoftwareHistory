using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

public sealed class AcceptDiskOperation : AbstractOperation<ErrorCodes>
{
	public override ErrorCodes Execute()
	{
		LogHelper.Instance.Log(">TakeDiskAtDoor.", LogEntryType.Info);
		Controller.StartRollerIn();
		IPickerSensorReadResult pickerSensorReadResult;
		if (PickerBoard.WaitToBlockSensor(PickerSensor.Sensor5, 15000))
		{
			Controller.StopRoller();
			pickerSensorReadResult = Controller.ReadSensors();
			if (pickerSensorReadResult.Error != ErrorCodes.Success)
			{
				return ErrorCodes.SensorReadError;
			}
			if (!pickerSensorReadResult.IsFull)
			{
				LogHelper.Instance.Log("After 15 seconds, didn't get anything in the picker; exiting.", LogEntryType.Info);
				if (!Controller.VendDoorClose().Success)
				{
					Controller.LogInputs(ControlBoards.Picker);
					Controller.LogInputs(ControlBoards.Aux);
					return ErrorCodes.VendDoorCloseTimeout;
				}
				return ErrorCodes.PickerEmpty;
			}
		}
		LogHelper.Instance.Log("Sensor 5 or 6 was tripped; pull into the picker.");
		if (!Controller.RollerToPosition(RollerPosition.Position3, 5000).Success)
		{
			LogHelper.Instance.Log("Unable to roll the disk to sensor 3.", LogEntryType.Error);
			Controller.TrackClose();
			IPickerSensorReadResult pickerSensorReadResult2 = Controller.ReadSensors();
			pickerSensorReadResult2.LogSensorState();
			if (pickerSensorReadResult2.IsFull)
			{
				return ErrorCodes.PickerFull;
			}
			return ErrorCodes.PickerEmpty;
		}
		pickerSensorReadResult = Controller.ReadSensors();
		if (pickerSensorReadResult.Error != ErrorCodes.Success)
		{
			return ErrorCodes.SensorReadError;
		}
		if (pickerSensorReadResult.BlockedCount > 3)
		{
			LogHelper.Instance.Log("TakeDiskAtDoor: the gripper is obstructed.", LogEntryType.Info);
			pickerSensorReadResult.LogSensorState();
			return ErrorCodes.PickerObstructed;
		}
		if (!Controller.VendDoorClose().Success)
		{
			return ErrorCodes.VendDoorCloseTimeout;
		}
		if (!pickerSensorReadResult.IsFull)
		{
			return ErrorCodes.PickerEmpty;
		}
		return ErrorCodes.PickerFull;
	}
}
