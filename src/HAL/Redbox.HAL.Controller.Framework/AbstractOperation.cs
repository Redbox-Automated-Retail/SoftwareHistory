using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public abstract class AbstractOperation<T> : IDisposable
{
	protected readonly IInventoryService InventoryService;

	protected readonly IRuntimeService RuntimeService;

	protected readonly IControlSystem Controller;

	protected readonly IMotionControlService MotionService;

	protected readonly IDecksService DeckService;

	protected readonly IPersistentCounterService PersistentCounterService;

	private bool Disposed;

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public abstract T Execute();

	protected bool WaitToClear(PickerSensor highSensor, PickerSensor lowSensor, int timeout)
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		IControlSystem service2 = ServiceLocator.Instance.GetService<IControlSystem>();
		int num = (int)(lowSensor - 1);
		int num2 = (int)(highSensor - 1);
		if (highSensor < lowSensor)
		{
			int num3 = num;
			num = num2;
			num2 = num3;
		}
		bool result = false;
		int clearSensorPauseDelay = ControllerConfiguration.Instance.ClearSensorPauseDelay;
		using (ExecutionTimer executionTimer = new ExecutionTimer())
		{
			while (true)
			{
				service.SpinWait(clearSensorPauseDelay);
				IPickerSensorReadResult pickerSensorReadResult = service2.ReadSensors();
				if (pickerSensorReadResult.Error != ErrorCodes.Success)
				{
					LogHelper.Instance.Log("WaittoClear: sensor read timed out.", LogEntryType.Error);
					return true;
				}
				int num4 = 0;
				for (int i = num; i <= num2; i++)
				{
					if (pickerSensorReadResult.Sensors[i])
					{
						num4++;
					}
				}
				if (num4 == 0)
				{
					result = false;
					break;
				}
				if (executionTimer.ElapsedMilliseconds > timeout)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	protected bool ClearDiskFromPicker()
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		int clearSensorPauseDelay = ControllerConfiguration.Instance.ClearSensorPauseDelay;
		bool flag = WaitToClear(PickerSensor.Sensor2, PickerSensor.Sensor5, 8000);
		if (!flag)
		{
			service.SpinWait(clearSensorPauseDelay);
			bool[] array = null;
			bool flag2 = true;
			for (int i = 0; (i < 3) & flag2; i++)
			{
				if (flag2)
				{
					if (i > 0)
					{
						LogHelper.Instance.Log("2-5 didn't timeout; sensors show something in picker.", LogEntryType.Info);
					}
					WaitToClear(PickerSensor.Sensor2, PickerSensor.Sensor5, 3000);
				}
				array = Controller.ReadSensors().Sensors;
				flag2 = array[1] || array[2] || array[3] || array[4];
				service.SpinWait(clearSensorPauseDelay);
			}
			array = Controller.ReadSensors().Sensors;
			flag = array[1] || array[2] || array[3] || array[4];
		}
		return flag;
	}

	protected ErrorCodes SettleDiskInSlot()
	{
		try
		{
			if (!Controller.SetFinger(GripperFingerState.Open).Success)
			{
				return ErrorCodes.GripperOpenTimeout;
			}
			if (Controller.ExtendArm().TimedOut)
			{
				Controller.RetractArm();
				Controller.SetFinger(GripperFingerState.Closed);
				LogHelper.Instance.Log("[SettleDisk] Timeout for GripperArmExtend; attempt to set fingers to Open");
				Controller.SetFinger(GripperFingerState.Open);
				if (!Controller.ExtendArm().Success)
				{
					Controller.RetractArm();
					return ErrorCodes.GripperExtendTimeout;
				}
			}
			if (!Controller.SetFinger(GripperFingerState.Closed).Success)
			{
				return ErrorCodes.GripperCloseTimeout;
			}
			if (!Controller.SetFinger(GripperFingerState.Open).Success)
			{
				return ErrorCodes.GripperOpenTimeout;
			}
			return (!Controller.RetractArm().Success) ? ErrorCodes.GripperRetractTimeout : ErrorCodes.Success;
		}
		finally
		{
			Controller.SetFinger(GripperFingerState.Open);
			Controller.RetractArm();
		}
	}

	protected ErrorCodes PullFrom(ILocation location)
	{
		return PullFrom(location, ControllerConfiguration.Instance.NumberOfPulls);
	}

	protected ErrorCodes PullFrom(ILocation location, int pulls)
	{
		GripperFingerState gripperFingerState = ((!location.IsWide) ? GripperFingerState.Rent : GripperFingerState.Open);
		for (int i = 0; i < pulls; i++)
		{
			if (!Controller.SetFinger(gripperFingerState).Success)
			{
				if (gripperFingerState != GripperFingerState.Open)
				{
					return ErrorCodes.GripperRentTimeout;
				}
				return ErrorCodes.GripperOpenTimeout;
			}
			if (DeckService.GetByNumber(MotionService.CurrentLocation.Deck).IsQlm)
			{
				int qlmExtendTime = ControllerConfiguration.Instance.QlmExtendTime;
				if (ControllerConfiguration.Instance.QlmTimedExtend)
				{
					Controller.TimedExtend(qlmExtendTime);
				}
				else
				{
					Controller.ExtendArm(ControllerConfiguration.Instance.QlmExtendTime);
				}
			}
			else if (Controller.ExtendArm().TimedOut)
			{
				Controller.RetractArm();
				Controller.SetFinger(GripperFingerState.Closed);
				LogHelper.Instance.Log($"Timeout for GripperArmExtend; attempt to set fingers to {gripperFingerState.ToString()}", LogEntryType.Info);
				Controller.SetFinger(gripperFingerState);
				if (!Controller.ExtendArm().Success)
				{
					Controller.RetractArm();
					return ErrorCodes.GripperExtendTimeout;
				}
			}
			if (!Controller.SetFinger(GripperFingerState.Closed).Success)
			{
				return ErrorCodes.GripperCloseTimeout;
			}
			if (!Controller.RetractArm().Success)
			{
				return ErrorCodes.GripperRetractTimeout;
			}
		}
		if (!Controller.SetFinger(GripperFingerState.Rent).Success)
		{
			return ErrorCodes.GripperRentTimeout;
		}
		return ErrorCodes.Success;
	}

	protected ErrorCodes PushIntoSlot()
	{
		ErrorCodes errorCodes = PushWithArm(ControllerConfiguration.Instance.RollInExtendTime);
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		errorCodes = PushWithArm(ControllerConfiguration.Instance.PushTime);
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		if (ControllerConfiguration.Instance.AdditionalPutPush)
		{
			PushWithArm(ControllerConfiguration.Instance.PushTime);
		}
		return ErrorCodes.Success;
	}

	protected virtual void OnDispose()
	{
	}

	protected AbstractOperation()
	{
		DeckService = ServiceLocator.Instance.GetService<IDecksService>();
		MotionService = ServiceLocator.Instance.GetService<IMotionControlService>();
		Controller = ServiceLocator.Instance.GetService<IControlSystem>();
		RuntimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
		InventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
		PersistentCounterService = ServiceLocator.Instance.GetService<IPersistentCounterService>();
	}

	private ErrorCodes PushWithArm(int timeout)
	{
		if (!Controller.SetFinger(GripperFingerState.Closed).Success)
		{
			return ErrorCodes.GripperCloseTimeout;
		}
		Controller.TimedExtend(timeout);
		if (!Controller.SetFinger(GripperFingerState.Rent).Success)
		{
			return ErrorCodes.GripperRentTimeout;
		}
		if (Controller.RetractArm().Success)
		{
			return ErrorCodes.Success;
		}
		return ErrorCodes.GripperRetractTimeout;
	}

	private void Dispose(bool disposing)
	{
		if (!Disposed)
		{
			Disposed = true;
			if (disposing)
			{
				OnDispose();
			}
		}
	}
}
