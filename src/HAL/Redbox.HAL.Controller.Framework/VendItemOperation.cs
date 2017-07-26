using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

public sealed class VendItemOperation : AbstractOperation<VendItemResult>
{
	private readonly int PollCount;

	public VendItemOperation()
		: this(ControllerConfiguration.Instance.VendDiskPollCount)
	{
	}

	public VendItemOperation(int tries)
	{
		PollCount = tries;
	}

	public override VendItemResult Execute()
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		VendItemResult vendItemResult = new VendItemResult
		{
			Presented = true,
			Status = ErrorCodes.PickerEmpty
		};
		Controller.RollerToPosition(RollerPosition.Position5, 3000);
		if (!Controller.VendDoorRent().Success)
		{
			vendItemResult.Status = ErrorCodes.VendDoorRentTimeout;
			return vendItemResult;
		}
		LogHelper.Instance.Log(">> VendDiskAtDoor...", LogEntryType.Info);
		int ms = 1000;
		ErrorCodes errorCodes = PickerBoard.PushOut();
		if (ErrorCodes.TrackCloseTimeout == errorCodes || ErrorCodes.SensorReadError == errorCodes)
		{
			vendItemResult.Status = errorCodes;
			vendItemResult.Presented = false;
			return vendItemResult;
		}
		Controller.TrackCycle();
		int num = 0;
		IPickerSensorReadResult pickerSensorReadResult;
		for (int i = 0; i <= PollCount; i++)
		{
			pickerSensorReadResult = Controller.ReadSensors();
			if (pickerSensorReadResult.Error != ErrorCodes.Success)
			{
				vendItemResult.Status = ErrorCodes.SensorReadError;
				return vendItemResult;
			}
			bool[] sensors = pickerSensorReadResult.Sensors;
			if (sensors[0] || sensors[1] || sensors[2] || sensors[3])
			{
				pickerSensorReadResult.LogSensorState(LogEntryType.Error);
				if (num == 1 && ControllerConfiguration.Instance.TrackPushOutFailures)
				{
					vendItemResult.Presented = false;
					LogHelper.Instance.Log("After 2 pushes, the disk has not cleared sensor 4.", LogEntryType.Info);
					vendItemResult.Status = ErrorCodes.PickerFull;
					return vendItemResult;
				}
				num++;
				LogHelper.Instance.Log("Disc was pushed to the drum - push it back out.", LogEntryType.Error);
				Controller.TrackCycle();
				PickerBoard.PushOut();
				service.Wait(ms);
				continue;
			}
			num = 0;
			if (sensors[4])
			{
				if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
				{
					LogHelper.Instance.Log("The item is still blocking sensor 5.", LogEntryType.Info);
				}
				service.Wait(ms);
				continue;
			}
			if (!sensors[5])
			{
				vendItemResult.Status = ErrorCodes.PickerEmpty;
				return vendItemResult;
			}
			service.Wait(ms);
		}
		pickerSensorReadResult = Controller.ReadSensors();
		if (pickerSensorReadResult.Error != ErrorCodes.Success)
		{
			vendItemResult.Status = ErrorCodes.SensorReadError;
			return vendItemResult;
		}
		vendItemResult.Status = (pickerSensorReadResult.IsFull ? ErrorCodes.PickerFull : ErrorCodes.PickerEmpty);
		return vendItemResult;
	}
}
