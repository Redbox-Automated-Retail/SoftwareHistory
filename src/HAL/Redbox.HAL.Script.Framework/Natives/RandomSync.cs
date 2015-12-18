using System;
using System.Collections.Generic;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "random-sync", Operand = "RANDOMSYNC", HideFromList = true)]
internal sealed class RandomSync : NativeJobAdapter
{
	private class RandomSyncDecorator : SyncDecorator
	{
		private readonly int VendTime;

		internal RandomSyncDecorator(ExecutionContext ctx, ExecutionResult r, Location loc, int vendTime)
			: base(ctx, r, loc)
		{
			VendTime = vendTime;
		}

		internal override SyncResult AfterDiskRead()
		{
			if (ControllerService.Instance.MoveToVend(MoveMode.None, Context.AppLog) != MotionResult.Success)
			{
				Context.CreateMoveErrorResult(SyncLocation.Deck, SyncLocation.Slot);
				AddError("job error.");
				return SyncResult.HardwareError;
			}
			VendItemResult vendItemResult = ControllerService.Instance.VendItem(VendTime);
			if (!vendItemResult.Presented)
			{
				Context.AppLog.Write("The disk was not presented to the user from the vend door.");
				AddError("Job error.");
				return SyncResult.HardwareError;
			}
			if (vendItemResult.PickerStatus == PickerStatus.Empty)
			{
				Context.AppLog.Write("The disk is no longer present in the picker. This is not good.");
				AddError("Job error.");
				return SyncResult.HardwareError;
			}
			PickerStatus pickerStatus = ControllerService.Instance.TakeDiskAtDoor();
			if (PickerStatus.DiscInPicker != pickerStatus)
			{
				Context.AppLog.Write("There was a problem taking the disk in.");
				AddError("Job error.");
				return SyncResult.HardwareError;
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
		int top = GetTop<int>();
		base.Context.AppLog.WriteFormatted("Vend time is set to {0}s", top);
		base.Context.AppLog.Write("Random sync test start.");
		IPersistentMap dataStore = StoreManager.Instance.GetDataStore();
		dataStore.GetValue("RandomSyncVendFrequency", 5);
		List<Location> locations = new List<Location>();
		DecksManager.Instance.ForAllDecksDo(delegate(Deck _deck)
		{
			for (int i = 1; i <= _deck.NumberOfSlots; i++)
			{
				locations.Add(new Location(_deck.Number, i));
			}
			return true;
		});
		bool flag = true;
		while (flag)
		{
			Location location = SelectTargetLocation(locations);
			if (location == null)
			{
				break;
			}
			base.Context.AppLog.WriteFormatted("Sync {0}", location.ToString());
			RandomSyncDecorator decorator = new RandomSyncDecorator(base.Context, base.Result, location, top);
			SyncResult syncResult = SyncHelper.SyncSlot(base.Result, base.Context, location, decorator);
			SyncResult syncResult2 = syncResult;
			if (syncResult2 == SyncResult.HardwareError || syncResult2 == SyncResult.GetError)
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

	private Location SelectTargetLocation(List<Location> locations)
	{
		if (0 >= locations.Count)
		{
			return null;
		}
		Random random = new Random(DateTime.Now.Millisecond);
		int index = random.Next(0, locations.Count - 1);
		Location result = locations[index];
		locations.RemoveAt(index);
		return result;
	}
}
