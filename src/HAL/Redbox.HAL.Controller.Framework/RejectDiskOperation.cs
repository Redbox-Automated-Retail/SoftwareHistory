using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

public class RejectDiskOperation : AbstractOperation<ErrorCodes>
{
	private readonly int Attempts;

	public override ErrorCodes Execute()
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		int attempts = Attempts;
		LogHelper.Instance.Log("[RejectDisk]: called with {0} tries", Attempts);
		int ms = 1000;
		IControlResponse controlResponse = Controller.VendDoorRent();
		if (!controlResponse.Success)
		{
			LogHelper.Instance.Log(LogEntryType.Error, "[RejectDisk]: VENDDOOR RENT failed with error {0}", controlResponse.Diagnostic);
			return ErrorCodes.VendDoorRentTimeout;
		}
		IPickerSensorReadResult pickerSensorReadResult = null;
		bool[] array = null;
		while (attempts-- >= 0)
		{
			Controller.TrackCycle();
			pickerSensorReadResult = Controller.ReadSensors();
			if (pickerSensorReadResult.Error != ErrorCodes.Success)
			{
				return ErrorCodes.SensorReadError;
			}
			if (pickerSensorReadResult.BlockedCount == 0)
			{
				if (!Controller.VendDoorClose().Success)
				{
					return ErrorCodes.PickerObstructed;
				}
				return ErrorCodes.PickerEmpty;
			}
			LogHelper.Instance.Log("[RejectDisk] The picker is not clear.");
			pickerSensorReadResult.LogSensorState();
			array = pickerSensorReadResult.Sensors;
			bool flag = array[3] || array[2] || array[1] || array[0];
			if (array[5])
			{
				if (flag)
				{
					PickerBoard.PushOut();
					service.Wait(ms);
					continue;
				}
				if (Controller.RollerToPosition(RollerPosition.Position3, 8000).Success)
				{
					pickerSensorReadResult = Controller.ReadSensors();
					if (pickerSensorReadResult.Error != ErrorCodes.Success)
					{
						return ErrorCodes.SensorReadError;
					}
					if (!pickerSensorReadResult.IsFull)
					{
						return ErrorCodes.PickerEmpty;
					}
					if (pickerSensorReadResult.BlockedCount <= 3)
					{
						if (!Controller.VendDoorClose().Success)
						{
							return ErrorCodes.PickerObstructed;
						}
						return ErrorCodes.PickerFull;
					}
				}
				service.Wait(ms);
			}
			else if (flag)
			{
				PickerBoard.PushOut();
				service.Wait(1500);
			}
		}
		pickerSensorReadResult = Controller.ReadSensors();
		if (pickerSensorReadResult.Error != ErrorCodes.Success)
		{
			return ErrorCodes.SensorReadError;
		}
		if (!pickerSensorReadResult.IsFull)
		{
			return ErrorCodes.PickerEmpty;
		}
		if (pickerSensorReadResult.BlockedCount > 3)
		{
			return ErrorCodes.PickerObstructed;
		}
		if (!Controller.VendDoorClose().Success)
		{
			return ErrorCodes.PickerObstructed;
		}
		return ErrorCodes.PickerFull;
	}

	public RejectDiskOperation(int attempts)
	{
		Attempts = attempts;
	}
}
