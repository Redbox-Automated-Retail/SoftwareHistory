using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class TakeDiskAtDoorJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		try
		{
			if (VendDoor.Instance.Rent())
			{
				ResultHelper.Instance.CreateInfoResult("HardwareError", "Vend rent timed out.");
				return;
			}
			PickerStatus pickerStatus = ControllerService.Instance.TakeDiskAtDoor();
			if (PickerStatus.DiscInPicker != pickerStatus)
			{
				ResultHelper.Instance.CreateInfoResult("ReceiveFailure", pickerStatus.ToString().ToUpper());
			}
			else
			{
				ResultHelper.Instance.CreateInfoResult("ItemReceived", "Item received in picker.");
			}
		}
		finally
		{
			if (VendDoor.Instance.Close())
			{
				ResultHelper.Instance.CreateInfoResult("HardwareError", "Vend close timed out.");
			}
		}
	}
}
