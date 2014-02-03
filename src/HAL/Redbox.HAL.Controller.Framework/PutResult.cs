namespace Redbox.HAL.Controller.Framework;

public sealed class PutResult
{
	public bool Success
	{
		get
		{
			if (HardwareError == PickerHardwareError.Success)
			{
				return Code == PutResultCode.Success;
			}
			return false;
		}
	}

	public bool IsSlotInUse => Code == PutResultCode.SlotInUse;

	public bool PickerEmpty => Code == PutResultCode.PickerEmpty;

	public bool PickerObstructed => Code == PutResultCode.PickerObstructed;

	public bool IsDuplicate { get; internal set; }

	internal PutResultCode Code { get; set; }

	internal PickerHardwareError HardwareError { get; set; }

	public static PutResult MakeEmulatedSlotInUse()
	{
		if (ControllerMode.Emulated != ControllerConfiguration.Instance.Mode)
		{
			return null;
		}
		PutResult putResult = new PutResult();
		putResult.Code = PutResultCode.SlotInUse;
		return putResult;
	}

	public static PutResult MakeEmulatedLocationOutOfRange()
	{
		if (ControllerMode.Emulated != ControllerConfiguration.Instance.Mode)
		{
			return null;
		}
		PutResult putResult = new PutResult();
		putResult.Code = PutResultCode.LocationOutOfRange;
		return putResult;
	}

	public override string ToString()
	{
		if (HardwareError != PickerHardwareError.Success)
		{
			return HardwareError.ToString();
		}
		return Code.ToString();
	}

	internal PutResult()
	{
		Code = PutResultCode.Success;
		HardwareError = PickerHardwareError.Success;
		IsDuplicate = false;
	}
}
