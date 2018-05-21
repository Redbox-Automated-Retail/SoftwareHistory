using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework.Natives;

internal class RandomSyncDecorator : SyncDecorator
{
	private readonly bool VendDisk;

	private readonly int VendTime;

	internal RandomSyncDecorator(ExecutionContext ctx, ExecutionResult r, ILocation loc, int vendtime, bool vend)
		: base(ctx, r, loc)
	{
		VendDisk = vend;
		VendTime = vendtime;
	}

	internal override SyncResult AfterDiskRead()
	{
		if (!VendDisk)
		{
			return SyncResult.Success;
		}
		if (ServiceLocator.Instance.GetService<IMotionControlService>().MoveVend(MoveMode.None, Context.AppLog) != ErrorCodes.Success)
		{
			Context.CreateMoveErrorResult(SyncLocation.Deck, SyncLocation.Slot);
			AddError("job error.");
			return SyncResult.HardwareError;
		}
		using (VendItemOperation vendItemOperation = new VendItemOperation(VendTime))
		{
			VendItemResult vendItemResult = vendItemOperation.Execute();
			if (!vendItemResult.Presented)
			{
				Context.AppLog.Write("The disk was not presented to the user from the vend door.");
				AddError("Job error.");
				return SyncResult.HardwareError;
			}
			if (vendItemResult.Status == ErrorCodes.PickerEmpty)
			{
				Context.AppLog.Write("The disk is no longer present in the picker. This is not good.");
				AddError("Job error.");
				return SyncResult.HardwareError;
			}
		}
		using (AcceptDiskOperation acceptDiskOperation = new AcceptDiskOperation())
		{
			ErrorCodes errorCodes = acceptDiskOperation.Execute();
			if (ErrorCodes.PickerFull != errorCodes)
			{
				Context.AppLog.Write("There was a problem taking the disk in.");
				AddError("Job error.");
				return SyncResult.HardwareError;
			}
			IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
			service.TrackCycle();
			using Scanner scanner = new Scanner(service);
			if (!scanner.Read().SnapOk)
			{
				Context.CreateResult("MachineError", "The CAMERA CAPTURE request timed out.", null, null, null, null, null);
				return SyncResult.HardwareError;
			}
		}
		return SyncResult.Success;
	}
}
