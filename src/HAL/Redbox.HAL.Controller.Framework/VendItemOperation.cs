using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

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
		VendItemResult vendItemResult = new VendItemResult();
		vendItemResult.Presented = true;
		vendItemResult.Status = ErrorCodes.PickerEmpty;
		VendItemResult vendItemResult2 = vendItemResult;
		Controller.RollerToPosition(RollerPosition.Position5, 3000);
		if (!Controller.VendDoorRent().Success)
		{
			vendItemResult2.Status = ErrorCodes.VendDoorRentTimeout;
			return vendItemResult2;
		}
		LogHelper.Instance.Log(">> VendDiskAtDoor...", LogEntryType.Info);
		int ms = 1000;
		ErrorCodes errorCodes = PickerBoard.PushOut();
		if (ErrorCodes.TrackCloseTimeout == errorCodes || ErrorCodes.SensorReadError == errorCodes)
		{
			vendItemResult2.Status = errorCodes;
			vendItemResult2.Presented = false;
			return vendItemResult2;
		}
		Controller.TrackCycle();
		int num = 0;
		IPickerSensorReadResult pickerSensorReadResult;
		for (int i = 0; i <= PollCount; i++)
		{
			pickerSensorReadResult = Controller.ReadSensors();
			if (pickerSensorReadResult.Error != ErrorCodes.Success)
			{
				vendItemResult2.Status = ErrorCodes.SensorReadError;
				return vendItemResult2;
			}
			bool[] sensors = pickerSensorReadResult.Sensors;
			if (sensors[0] || sensors[1] || sensors[2] || sensors[3])
			{
				pickerSensorReadResult.LogSensorState(LogEntryType.Error);
				if (num == 1 && ControllerConfiguration.Instance.TrackPushOutFailures)
				{
					vendItemResult2.Presented = false;
					LogHelper.Instance.Log("After 2 pushes, the disk has not cleared sensor 4.", LogEntryType.Info);
					vendItemResult2.Status = ErrorCodes.PickerFull;
					return vendItemResult2;
				}
				num++;
				LogHelper.Instance.Log("Disc was pushed to the drum - push it back out.", LogEntryType.Error);
				Controller.TrackCycle();
				PickerBoard.PushOut();
				PerfFunctions.Wait(ms);
				continue;
			}
			num = 0;
			if (sensors[4])
			{
				if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
				{
					LogHelper.Instance.Log("The item is still blocking sensor 5.", LogEntryType.Info);
				}
				PerfFunctions.Wait(ms);
				continue;
			}
			if (!sensors[5])
			{
				vendItemResult2.Status = ErrorCodes.PickerEmpty;
				return vendItemResult2;
			}
			PerfFunctions.Wait(ms);
		}
		pickerSensorReadResult = Controller.ReadSensors();
		if (pickerSensorReadResult.Error != ErrorCodes.Success)
		{
			vendItemResult2.Status = ErrorCodes.SensorReadError;
			return vendItemResult2;
		}
		vendItemResult2.Status = (pickerSensorReadResult.IsFull ? ErrorCodes.PickerFull : ErrorCodes.PickerEmpty);
		return vendItemResult2;
	}
}
