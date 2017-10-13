using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "games-sync")]
internal sealed class GamesSync : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog.ConfigureLog(base.Context, logAppends: false, "GamesSync", flushOnWrite: true, null);
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		base.Context.AppLog.Write("Games sync test start.");
		List<ILocation> list = new List<ILocation>();
		for (int i = 1; i <= 4; i++)
		{
			list.Add(service.Get(i, 2));
		}
		bool flag = true;
		for (int j = 0; (j < 150) & flag; j++)
		{
			foreach (ILocation item in list)
			{
				base.Context.AppLog.WriteFormatted("Sync {0}", item.ToString());
				RandomSyncDecorator decorator = new RandomSyncDecorator(base.Context, base.Result, item, 1, vend: true);
				SyncResult syncResult = SyncHelper.SyncSlot(base.Result, base.Context, item, decorator);
				if (syncResult != SyncResult.Success)
				{
					base.Context.AppLog.WriteFormatted("Sync returned error {0}", syncResult.ToString());
					flag = false;
					AddError("Sync encountered an error.");
					break;
				}
				if (ContextSwitchRequested())
				{
					flag = false;
				}
			}
		}
	}

	internal GamesSync(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}
}
