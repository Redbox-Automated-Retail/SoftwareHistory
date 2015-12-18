using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "ms-take-disk-at-door", Operand = "MS-TAKE-DISK-AT-DOOR")]
internal sealed class TakeDiskAtDoorJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		try
		{
			if (VendDoor.Rent())
			{
				base.Context.CreateInfoResult("HardwareError", "Vend rent timed out.");
				return;
			}
			PickerStatus pickerStatus = ControllerService.Instance.TakeDiskAtDoor();
			if (PickerStatus.DiscInPicker != pickerStatus)
			{
				base.Context.CreateInfoResult("ReceiveFailure", pickerStatus.ToString().ToUpper());
			}
			else
			{
				base.Context.CreateInfoResult("ItemReceived", "Item received in picker.");
			}
		}
		finally
		{
			if (VendDoor.Close())
			{
				base.Context.CreateInfoResult("HardwareError", "Vend close timed out.");
			}
		}
	}

	internal TakeDiskAtDoorJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
