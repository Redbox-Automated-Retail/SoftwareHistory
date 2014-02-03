using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class FileMarkerDiskJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int top = GetTop<int>();
		int top2 = GetTop<int>();
		MotionResult motionResult = ControllerService.Instance.MoveToVend(MoveMode.Get, base.AppLog);
		if (motionResult != MotionResult.Success)
		{
			HandleHWError(motionResult.ToString());
			return;
		}
		if (VendDoor.Instance.Rent())
		{
			HandleHWError("Venddoor Rent Timeout.");
			return;
		}
		PickerStatus pickerStatus = ControllerService.Instance.TakeDiskAtDoor();
		switch (pickerStatus)
		{
		case PickerStatus.Empty:
			ResultHelper.Instance.CreateInfoResult("NoReceivedItem", "No disk was placed in the picker.");
			break;
		default:
			HandleHWError(pickerStatus.ToString());
			break;
		case PickerStatus.DiscInPicker:
		{
			motionResult = ControllerService.Instance.MoveTo(top, top2, MoveMode.Put, base.AppLog);
			if (motionResult != MotionResult.Success)
			{
				HandleHWError(motionResult.ToString());
				break;
			}
			PutResult putResult = ControllerService.Instance.Put("UNKNOWN", base.AppLog);
			if (!putResult.Success)
			{
				motionResult = ControllerService.Instance.MoveToVend(MoveMode.None, base.AppLog);
				if (motionResult != MotionResult.Success)
				{
					HandleHWError(motionResult.ToString());
					break;
				}
				VendItemResult vendItemResult = ControllerService.Instance.VendItem();
				if (vendItemResult.PickerStatus == PickerStatus.Empty)
				{
					ResultHelper.Instance.CreateInfoResult("ItemTakenByMerchandizer", "The disk was taken by the merchandizer.");
					AddError("Disk not filed.");
				}
				else if (vendItemResult.PickerStatus == PickerStatus.DiscInPicker)
				{
					ResultHelper.Instance.CreateInfoResult("ItemNotTaken", "The disk was taken by the merchandizer.");
					AddError("Disk not filed.");
				}
				else
				{
					HandleHWError(vendItemResult.PickerStatus.ToString());
				}
			}
			else
			{
				ResultHelper.Instance.CreateResult("ItemReturned", "The marker disk was filed.", top, top2, "UNKNOWN", null, null);
			}
			break;
		}
		}
	}

	private void HandleHWError(string detail)
	{
		base.Context.Send("MachineError");
		ResultHelper.Instance.CreateInfoResult("MachineError", detail);
		AddError("Hardware Error during job.");
	}
}
