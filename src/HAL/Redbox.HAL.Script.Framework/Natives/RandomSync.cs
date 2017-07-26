using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "random-sync", Operand = "RANDOMSYNC", HideFromList = true)]
internal sealed class RandomSync : NativeJobAdapter
{
	private class RandomSyncDecorator : SyncDecorator
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
			}
			return SyncResult.Success;
		}
	}

	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: false, "RandomSync", flushOnWrite: true, null);
		if (!ControllerConfiguration.Instance.IsVMZMachine)
		{
			applicationLog.Write("The random sync job is only supported on VMZ kiosks. Nice try.");
			AddError("Unsupported configuration.");
			return;
		}
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		int num = base.Context.PopTop<int>();
		base.Context.AppLog.WriteFormatted("Vend time is set to {0}s", num);
		int num2 = base.Context.PopTop<int>();
		base.Context.AppLog.Write("Random sync test start.");
		List<ILocation> locations = new List<ILocation>();
		base.DecksService.ForAllDecksDo(delegate(IDeck _deck)
		{
			for (int i = 1; i <= _deck.NumberOfSlots; i++)
			{
				locations.Add(service.Get(_deck.Number, i));
			}
			return true;
		});
		int num3 = 0;
		bool flag = true;
		while (flag)
		{
			ILocation location = SelectTargetLocation(locations);
			if (location == null)
			{
				flag = false;
				break;
			}
			base.Context.AppLog.WriteFormatted("Sync {0}", location.ToString());
			num3++;
			bool vend = num3 % num2 == 0;
			RandomSyncDecorator decorator = new RandomSyncDecorator(base.Context, base.Result, location, num, vend);
			SyncResult syncResult = SyncHelper.SyncSlot(base.Result, base.Context, location, decorator);
			if (syncResult == SyncResult.HardwareError || syncResult == SyncResult.GetError)
			{
				base.Context.AppLog.WriteFormatted("Sync returned error {0}", syncResult.ToString());
				flag = false;
			}
			if (ContextSwitchRequested())
			{
				flag = false;
			}
		}
	}

	internal RandomSync(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}

	private ILocation SelectTargetLocation(List<ILocation> locations)
	{
		if (0 >= locations.Count)
		{
			return null;
		}
		int index = new Random(DateTime.Now.Millisecond).Next(0, locations.Count - 1);
		ILocation result = locations[index];
		locations.RemoveAt(index);
		return result;
	}
}
