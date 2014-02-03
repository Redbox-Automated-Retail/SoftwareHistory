using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal class DumpBinThinHelper : ThinHelper
{
	internal DumpBinThinHelper(ExecutionResult result)
	{
		base.Result = result;
	}

	internal override MerchandizeResult ThinDisk(string id, MerchFlags flags)
	{
		ExecutionContext activeContext = ExecutionEngine.Instance.ActiveContext;
		if (DumpBin.Instance.IsFull())
		{
			ResultHelper.Instance.CreateInfoResult("DumpBinIsFull", "The dump bin is filled to capacity.");
			activeContext.AppLog.Write("The dump bin is full.");
			return MerchandizeResult.DumpBinFull;
		}
		activeContext.AppLog.Write($"Attempt thin of item {id}");
		Location location = ControllerService.Instance.Lookup(id);
		if (location == null)
		{
			activeContext.AppLog.LogLookupFailure(id);
			ResultHelper.Instance.CreateInfoResult("ThinLookupFailed", "The LOOKUP request failed because the item was not found.", id);
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

	protected override MerchandizeResult ProcessSuccessfulThin(Location loc, string id, MerchFlags flags)
	{
		ResultHelper.Instance.CreateResult("ThinTransferSuccessful", "The item was transferred from storage to the dump bin.", null, null, id, null, null);
		return MerchandizeResult.Success;
	}

	protected override Location PrepTarget(MerchFlags flags, out MerchandizeResult result)
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
