using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "ms-vend-disk-in-picker", Operand = "MS-VEND-DISK-IN-PICKER")]
internal sealed class VendDiskInPickerJob : NativeJobAdapter
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
			ErrorCodes errorCodes = base.MotionService.MoveVend(MoveMode.None, base.AppLog);
			if (errorCodes != ErrorCodes.Success)
			{
				base.Context.CreateInfoResult("HardwareError", $"There was an error while executing the MOVEVEND instruction: {errorCodes.ToString().ToUpper()}");
				return;
			}
			using (VendItemOperation vendItemOperation = new VendItemOperation())
			{
				switch (vendItemOperation.Execute().Status)
				{
				case ErrorCodes.TrackCloseTimeout:
				case ErrorCodes.VendDoorRentTimeout:
				case ErrorCodes.SensorReadError:
					base.Context.CreateInfoResult("HardwareError", "There was a HW error during the vend disk routine.");
					return;
				case ErrorCodes.PickerEmpty:
					base.Context.CreateInfoResult("ItemVended", "The item was successfully vended.");
					return;
				}
			}
			using AcceptDiskOperation acceptDiskOperation = new AcceptDiskOperation();
			if (ErrorCodes.PickerFull != acceptDiskOperation.Execute())
			{
				base.Context.CreateInfoResult("TakeDiskError", "Unable to get the disk.");
			}
		}
		finally
		{
			if (!base.ControlSystem.VendDoorClose().Success)
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
