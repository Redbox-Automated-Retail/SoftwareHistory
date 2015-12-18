using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class DumpBinThinHelper : ThinHelper
{
	internal DumpBinThinHelper(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}

	internal override MerchandizeResult ThinDisk(string id, MerchFlags flags)
	{
		if (DumpBin.Instance.IsFull())
		{
			base.Context.CreateInfoResult("DumpBinIsFull", "The dump bin is filled to capacity.");
			base.Context.AppLog.Write("The dump bin is full.");
			return MerchandizeResult.DumpBinFull;
		}
		base.Context.AppLog.Write($"Attempt thin of item {id}");
		Location location = InventoryService.Instance.Lookup(id);
		if (location == null)
		{
			base.Context.AppLog.LogLookupFailure(id);
			base.Context.CreateInfoResult("ThinLookupFailed", "The LOOKUP request failed because the item was not found.", id);
			return MerchandizeResult.LookupFailure;
		}
		MerchandizeResult merchandizeResult = FetchThin(location, id);
		switch (merchandizeResult)
		{
		case MerchandizeResult.EmptyStuck:
			return merchandizeResult;
		case MerchandizeResult.Success:
			return ThinDiskToLocation(location, DumpBin.Instance.MoveableLocation(), id, MerchFlags.Thin);
		default:
		{
			MerchandizeResult merchandizeResult2 = ReturnToSource(location, id);
			if (merchandizeResult2 != MerchandizeResult.Success)
			{
				return merchandizeResult2;
			}
			return merchandizeResult;
		}
		}
	}

	protected override Location FindEmptyTarget(MerchFlags flags, out MerchandizeResult result)
	{
		if (DumpBin.Instance.IsFull())
		{
			result = MerchandizeResult.DumpBinFull;
			return null;
		}
		result = MerchandizeResult.Success;
		return DumpBin.Instance.MoveableLocation();
	}
}
