using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "ms-vend-disk-in-picker", Operand = "MS-VEND-DISK-IN-PICKER")]
internal class VendDiskInPickerJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		try
		{
			if (!PickerBoard.IsFull())
			{
				base.Context.CreateInfoResult("EmptyPicker", "No disk in picker.");
				return;
			}
			MotionResult motionResult = ControllerService.Instance.MoveToVend(MoveMode.None, base.AppLog);
			if (motionResult != MotionResult.Success)
			{
				base.Context.CreateInfoResult("HardwareError", $"There was an error while executing the MOVEVEND instruction: {motionResult.ToString().ToUpper()}");
				return;
			}
			VendItemResult vendItemResult = ControllerService.Instance.VendItem();
			if (vendItemResult.PickerStatus == PickerStatus.HardwareError)
			{
				base.Context.CreateInfoResult("HardwareError", "There was a HW error during the vend disk routine.");
				return;
			}
			if (vendItemResult.PickerStatus == PickerStatus.Empty)
			{
				base.Context.CreateInfoResult("ItemVended", "The item was successfully vended.");
				return;
			}
			PickerStatus pickerStatus = ControllerService.Instance.TakeDiskAtDoor();
			if (PickerStatus.DiscInPicker != pickerStatus)
			{
				base.Context.CreateInfoResult("TakeDiskError", "Unable to get the disk.");
			}
		}
		finally
		{
			if (VendDoor.Close())
			{
				base.Context.CreateInfoResult("HardwareError", "Unable to close the vend door.");
			}
		}
	}

	internal VendDiskInPickerJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
