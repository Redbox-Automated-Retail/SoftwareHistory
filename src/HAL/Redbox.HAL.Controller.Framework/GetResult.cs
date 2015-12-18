using System;

namespace Redbox.HAL.Controller.Framework;

public sealed class GetResult
{
	private GetResultCode Code;

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

	public PickerHardwareError HardwareError { get; private set; }

	public string Previous { get; internal set; }

	public DateTime? ReturnTime { get; internal set; }

	public MerchFlags Flags { get; internal set; }

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
		if (HardwareError == PickerHardwareError.Success)
		{
			return Code.ToString();
		}
		return HardwareError.ToString();
	}

	public void UpdateCode(GetResultCode newCode)
	{
		Code = newCode;
	}

	public void SetHardwareError(PickerHardwareError error)
	{
		HardwareError = error;
	}

	internal GetResult()
	{
		Previous = "UNKNOWN";
		HardwareError = PickerHardwareError.Success;
	}
}
