using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "ms-take-disk-at-door", Operand = "MS-TAKE-DISK-AT-DOOR")]
internal sealed class TakeDiskAtDoorJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		try
		{
			ErrorCodes errorCodes = ServiceLocator.Instance.GetService<IMotionControlService>().MoveVend(MoveMode.None, base.Context.AppLog);
			if (errorCodes != ErrorCodes.Success)
			{
				base.Context.CreateInfoResult("HardwareError", $"MOVE failed with error {errorCodes.ToString()}");
				return;
			}
			if (!base.ControlSystem.VendDoorRent().Success)
			{
				base.Context.CreateInfoResult("HardwareError", "Vend rent timed out.");
				return;
			}
			using AcceptDiskOperation acceptDiskOperation = new AcceptDiskOperation();
			ErrorCodes errorCodes2 = acceptDiskOperation.Execute();
			if (ErrorCodes.PickerFull != errorCodes2)
			{
				base.Context.CreateInfoResult("ReceiveFailure", errorCodes2.ToString().ToUpper());
			}
			else
			{
				base.Context.CreateInfoResult("ItemReceived", "Item received in picker.");
			}
		}
		finally
		{
			if (!base.ControlSystem.VendDoorClose().Success)
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
