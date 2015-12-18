using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal sealed class StandardSyncDecorator : SyncDecorator
{
	internal override void OnGetFailure(GetResult notUsed)
	{
		SyncLocation.MarkUnknown();
	}

	internal StandardSyncDecorator(ExecutionContext ctx, ExecutionResult r, Location l)
		: base(ctx, r, l)
	{
	}
}
