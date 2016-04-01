using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class EmulatedGetResult : IGetResult
{
	public bool Success => ErrorCodes.Success == HardwareError;

	public bool EmptyOrStuck
	{
		get
		{
			if (!IsSlotEmpty)
			{
				return ItemStuck;
			}
			return true;
		}
	}

	public bool IsSlotEmpty => ErrorCodes.SlotEmpty == HardwareError;

	public bool ItemStuck => ErrorCodes.ItemStuck == HardwareError;

	public ErrorCodes HardwareError { get; private set; }

	public ILocation Location { get; private set; }

	public string Previous { get; internal set; }

	public DateTime? ReturnTime { get; internal set; }

	public MerchFlags Flags { get; internal set; }

	public void Update(ErrorCodes newError)
	{
	}

	internal EmulatedGetResult(ErrorCodes error)
	{
		HardwareError = error;
	}
}
