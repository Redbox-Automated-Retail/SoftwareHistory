using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class SyncInstruction : Instruction
{
	private enum SyncType
	{
		None,
		Standard,
		Location
	}

	public override string Mnemonic => "SYNC";

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		if (!GetLocation(result, context, out var location))
		{
			return;
		}
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("TYPE");
		SyncType syncType = SyncType.Standard;
		if (keyValuePair != null)
		{
			syncType = Enum<SyncType>.ParseIgnoringCase(keyValuePair.Value, SyncType.None);
			if (syncType == SyncType.None)
			{
				result.Errors.Add(Error.NewError("E777", "Unrecognized sync type.", $"Unrecognized sync type {keyValuePair}"));
				return;
			}
		}
		SyncDecorator syncDecorator = null;
		SyncResult syncResult = SyncHelper.SyncSlot(decorator: (SyncType.Location != syncType) ? ((SyncDecorator)new StandardSyncDecorator(context, result, location)) : ((SyncDecorator)new SyncLocationDecorator(context, result, location)), result: result, context: context, loc: location);
		context.PushTop(syncResult.ToString().ToUpper());
	}
}
