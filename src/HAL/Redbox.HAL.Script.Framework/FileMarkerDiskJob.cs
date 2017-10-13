using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "file-marker-disk", Operand = "FILE-MARKER-DISK")]
internal sealed class FileMarkerDiskJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int num = base.Context.PopTop<int>();
		int num2 = base.Context.PopTop<int>();
		ErrorCodes errorCodes = base.MotionService.MoveVend(MoveMode.Get, base.Context.AppLog);
		if (errorCodes != ErrorCodes.Success)
		{
			HandleHWError(errorCodes.ToString());
			return;
		}
		if (!base.ControlSystem.VendDoorRent().Success)
		{
			HandleHWError("Venddoor Rent Timeout.");
			return;
		}
		using (AcceptDiskOperation acceptDiskOperation = new AcceptDiskOperation())
		{
			ErrorCodes errorCodes2 = acceptDiskOperation.Execute();
			switch (errorCodes2)
			{
			case ErrorCodes.PickerEmpty:
				base.Context.CreateInfoResult("NoReceivedItem", "No disk was placed in the picker.");
				return;
			default:
				HandleHWError(errorCodes2.ToString());
				return;
			case ErrorCodes.PickerFull:
				break;
			}
		}
		errorCodes = ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(num, num2, MoveMode.Put, base.Context.AppLog);
		if (errorCodes != ErrorCodes.Success)
		{
			HandleHWError(errorCodes.ToString());
			return;
		}
		base.ControlSystem.TrackCycle();
		if (!ServiceLocator.Instance.GetService<IControllerService>().Put("UNKNOWN", base.AppLog).Success)
		{
			errorCodes = base.MotionService.MoveVend(MoveMode.None, base.Context.AppLog);
			if (errorCodes == ErrorCodes.Success)
			{
				using (VendItemOperation vendItemOperation = new VendItemOperation())
				{
					VendItemResult vendItemResult = vendItemOperation.Execute();
					if (vendItemResult.Status == ErrorCodes.PickerEmpty)
					{
						base.Context.CreateInfoResult("ItemTakenByMerchandizer", "The disk was taken by the merchandizer.");
						AddError("Disk not filed.");
					}
					else if (vendItemResult.Status == ErrorCodes.PickerFull)
					{
						base.Context.CreateInfoResult("ItemNotTaken", "The disk was taken by the merchandizer.");
						AddError("Disk not filed.");
					}
					else
					{
						HandleHWError(vendItemResult.Status.ToString());
					}
					return;
				}
			}
			HandleHWError(errorCodes.ToString());
		}
		else
		{
			base.Context.CreateResult("ItemReturned", "The marker disk was filed.", num, num2, "UNKNOWN", null, null);
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
