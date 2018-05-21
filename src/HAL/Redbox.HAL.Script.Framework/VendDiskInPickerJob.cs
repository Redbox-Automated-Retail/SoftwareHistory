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
				base.Context.CreateInfoResult("HardwareError", $"MOVEVEND returned error {errorCodes.ToString().ToUpper()}");
				return;
			}
			using (VendItemOperation vendItemOperation = new VendItemOperation())
			{
				VendItemResult vendItemResult = vendItemOperation.Execute();
				switch (vendItemResult.Status)
				{
				case ErrorCodes.TrackCloseTimeout:
				case ErrorCodes.VendDoorRentTimeout:
				case ErrorCodes.SensorReadError:
					base.Context.CreateInfoResult("HardwareError", $"VendItem returned {vendItemResult.Status.ToString().ToUpper()}");
					return;
				case ErrorCodes.PickerEmpty:
					base.Context.CreateInfoResult("ItemVended", "The item was vended.");
					return;
				}
			}
			using AcceptDiskOperation acceptDiskOperation = new AcceptDiskOperation();
			ErrorCodes errorCodes2 = acceptDiskOperation.Execute();
			if (ErrorCodes.PickerFull != errorCodes2)
			{
				base.Context.CreateInfoResult("TakeDiskError", $"Accept disk returned error {errorCodes2.ToString().ToUpper()}");
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
