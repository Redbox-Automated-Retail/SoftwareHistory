using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class VendItemOperation : AbstractOperation<VendItemResult>
{
	private readonly int PollCount;

	public override VendItemResult Execute()
	{
		VendItemResult vendItemResult = OnExecute();
		if (ErrorCodes.PickerEmpty == vendItemResult.Status && !Controller.VendDoorClose().Success)
		{
			vendItemResult.Status = ErrorCodes.VendDoorCloseTimeout;
		}
		return vendItemResult;
	}

	internal VendItemOperation(int tries)
	{
		PollCount = tries;
	}

	private VendItemResult OnExecute()
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		VendItemResult vendItemResult = new VendItemResult();
		vendItemResult.Presented = true;
		vendItemResult.Status = ErrorCodes.PickerEmpty;
		VendItemResult vendItemResult2 = vendItemResult;
		LogHelper.Instance.WithContext(toFormattedLog: false, LogEntryType.Info, "VendDiskAtDoor...");
		Controller.RollerToPosition(RollerPosition.Position5, 3000);
		ErrorCodes errorCodes = OpenDoorChecked();
		if (errorCodes != ErrorCodes.Success)
		{
			vendItemResult2.Status = errorCodes;
			return vendItemResult2;
		}
		int ms = 1000;
		ErrorCodes errorCodes2 = ControllerService.PushOut();
		if (ErrorCodes.TrackCloseTimeout == errorCodes2 || ErrorCodes.SensorReadError == errorCodes2)
		{
			vendItemResult2.Status = errorCodes2;
			vendItemResult2.Presented = false;
			return vendItemResult2;
		}
		Controller.TrackCycle();
		int num = 0;
		IPickerSensorReadResult pickerSensorReadResult;
		for (int i = 0; i <= PollCount; i++)
		{
			pickerSensorReadResult = Controller.ReadPickerSensors();
			if (!pickerSensorReadResult.Success)
			{
				vendItemResult2.Status = ErrorCodes.SensorReadError;
				return vendItemResult2;
			}
			if (pickerSensorReadResult.IsInputActive(PickerInputs.Sensor1) || pickerSensorReadResult.IsInputActive(PickerInputs.Sensor2) || pickerSensorReadResult.IsInputActive(PickerInputs.Sensor3) || pickerSensorReadResult.IsInputActive(PickerInputs.Sensor4))
			{
				pickerSensorReadResult.Log(LogEntryType.Error);
				if (num == 1 && ControllerConfiguration.Instance.TrackPushOutFailures)
				{
					vendItemResult2.Presented = false;
					LogHelper.Instance.WithContext(toFormattedLog: false, LogEntryType.Error, "[VendItemInPicker] After 2 pushes, the disk has not cleared sensor 4.");
					vendItemResult2.Status = ErrorCodes.PickerFull;
					return vendItemResult2;
				}
				num++;
				LogHelper.Instance.WithContext(toFormattedLog: false, LogEntryType.Error, "[VendItemInPicker] Disc was pushed to the drum - push it back out.");
				Controller.TrackCycle();
				ControllerService.PushOut();
				service.Wait(ms);
				continue;
			}
			num = 0;
			if (pickerSensorReadResult.IsInputActive(PickerInputs.Sensor5))
			{
				if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
				{
					LogHelper.Instance.WithContext(toFormattedLog: false, LogEntryType.Info, "[VendItemInPicker] The item is still blocking sensor 5.");
				}
				service.Wait(ms);
				continue;
			}
			if (!pickerSensorReadResult.IsInputActive(PickerInputs.Sensor6))
			{
				vendItemResult2.Status = ErrorCodes.PickerEmpty;
				return vendItemResult2;
			}
			service.Wait(ms);
		}
		pickerSensorReadResult = Controller.ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			vendItemResult2.Status = ErrorCodes.SensorReadError;
		}
		else
		{
			vendItemResult2.Status = (pickerSensorReadResult.IsFull ? ErrorCodes.PickerFull : ErrorCodes.PickerEmpty);
		}
		return vendItemResult2;
	}
}
