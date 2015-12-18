using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "file-marker-disk", Operand = "FILE-MARKER-DISK")]
internal sealed class FileMarkerDiskJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int top = GetTop<int>();
		int top2 = GetTop<int>();
		MotionResult motionResult = CorrectiveMoveVend(MoveMode.Get);
		if (motionResult != MotionResult.Success)
		{
			HandleHWError(motionResult.ToString());
			return;
		}
		if (VendDoor.Rent())
		{
			HandleHWError("Venddoor Rent Timeout.");
			return;
		}
		PickerStatus pickerStatus = ControllerService.Instance.TakeDiskAtDoor();
		switch (pickerStatus)
		{
		case PickerStatus.Empty:
			base.Context.CreateInfoResult("NoReceivedItem", "No disk was placed in the picker.");
			break;
		default:
			HandleHWError(pickerStatus.ToString());
			break;
		case PickerStatus.DiscInPicker:
		{
			motionResult = CorrectiveMove(top, top2, MoveMode.Put);
			if (motionResult != MotionResult.Success)
			{
				HandleHWError(motionResult.ToString());
				break;
			}
			PutResult putResult = ControllerService.Instance.Put("UNKNOWN", base.AppLog);
			if (!putResult.Success)
			{
				motionResult = CorrectiveMoveVend(MoveMode.None);
				if (motionResult != MotionResult.Success)
				{
					HandleHWError(motionResult.ToString());
					break;
				}
				VendItemResult vendItemResult = ControllerService.Instance.VendItem();
				if (vendItemResult.PickerStatus == PickerStatus.Empty)
				{
					base.Context.CreateInfoResult("ItemTakenByMerchandizer", "The disk was taken by the merchandizer.");
					AddError("Disk not filed.");
				}
				else if (vendItemResult.PickerStatus == PickerStatus.DiscInPicker)
				{
					base.Context.CreateInfoResult("ItemNotTaken", "The disk was taken by the merchandizer.");
					AddError("Disk not filed.");
				}
				else
				{
					HandleHWError(vendItemResult.PickerStatus.ToString());
				}
			}
			else
			{
				base.Context.CreateResult("ItemReturned", "The marker disk was filed.", top, top2, "UNKNOWN", null, null);
			}
			break;
		}
		}
	}

	internal FileMarkerDiskJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}

	private void HandleHWError(string detail)
	{
		base.Context.Send("MachineError");
		base.Context.CreateInfoResult("MachineError", detail);
		AddError("Hardware Error during job.");
	}
}
