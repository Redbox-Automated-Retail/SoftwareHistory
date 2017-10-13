using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "random-sync", Operand = "RANDOMSYNC", HideFromList = true)]
internal sealed class RandomSync : NativeJobAdapter
{
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
		int num2 = base.Context.PopTop<int>();
		base.Context.AppLog.WriteFormatted("Vend time is set to {0}s, vend frequency = {1}", num2, num);
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
			bool vend = false;
			if (num != 0 && num3 % num == 0)
			{
				vend = true;
			}
			RandomSyncDecorator decorator = new RandomSyncDecorator(base.Context, base.Result, location, num2, vend);
			SyncResult syncResult = SyncHelper.SyncSlot(base.Result, base.Context, location, decorator);
			if (syncResult == SyncResult.HardwareError || syncResult == SyncResult.GetError)
			{
				base.Context.AppLog.WriteFormatted("Sync returned error {0}", syncResult.ToString());
				flag = false;
				AddError("Sync encountered an error.");
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
