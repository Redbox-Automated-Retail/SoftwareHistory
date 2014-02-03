using System;

namespace Redbox.HAL.Controller.Framework;

public class GetResult
{
	public bool Success
	{
		get
		{
			if (HardwareError == PickerHardwareError.Success)
			{
				return Code == GetResultCode.Success;
			}
			return false;
		}
	}

	public bool EmptyOrStuck
	{
		get
		{
			if (Code != GetResultCode.SlotEmpty)
			{
				return Code == GetResultCode.ItemStuck;
			}
			return true;
		}
	}

	public bool IsSlotEmpty => Code == GetResultCode.SlotEmpty;

	public bool ItemStuck => Code == GetResultCode.ItemStuck;

	public LocationTestResult PeekResult { get; internal set; }

	public string Previous { get; internal set; }

	public DateTime? ReturnTime { get; internal set; }

	public MerchFlags Flags { get; internal set; }

	internal GetResultCode Code { get; set; }

	internal PickerHardwareError HardwareError { get; set; }

	public static GetResult MakeEmulatedSlotEmpty()
	{
		if (ControllerMode.Emulated != ControllerConfiguration.Instance.Mode)
		{
			return null;
		}
		GetResult getResult = new GetResult();
		getResult.Code = GetResultCode.SlotEmpty;
		return getResult;
	}

	public override string ToString()
	{
		if (HardwareError != PickerHardwareError.Success)
		{
			return HardwareError.ToString();
		}
		return Code.ToString();
	}

	internal GetResult()
	{
		Code = GetResultCode.Success;
		HardwareError = PickerHardwareError.Success;
		Previous = "UNKNOWN";
	}
}
