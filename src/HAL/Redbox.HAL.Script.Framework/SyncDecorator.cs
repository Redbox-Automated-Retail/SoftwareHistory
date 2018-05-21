using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal abstract class SyncDecorator
{
	protected readonly ExecutionResult Result;

	protected readonly ExecutionContext Context;

	protected readonly ILocation SyncLocation;

	private static readonly CenterDiskMethod[] m_readMethods = new CenterDiskMethod[2]
	{
		CenterDiskMethod.VendDoorAndBack,
		CenterDiskMethod.DrumAndBack
	};

	protected void UpdateSymbol(string name, object value)
	{
		if (Context.SymbolTable.ContainsKey(name))
		{
			Context.SymbolTable[name] = value;
		}
		else
		{
			Context.SymbolTable.Add(name, value);
		}
	}

	protected virtual void OnMoveErrorInner(ErrorCodes result)
	{
	}

	protected virtual void OnReadDisk(ScanResult result)
	{
	}

	protected SyncDecorator(ExecutionContext ctx, ExecutionResult result, ILocation loc)
	{
		SyncLocation = loc;
		Result = result;
		Context = ctx;
	}

	internal virtual SyncResult AfterDiskRead()
	{
		return SyncResult.Success;
	}

	internal virtual IGetResult GetDisk(IFormattedLog applog)
	{
		return ServiceLocator.Instance.GetService<IControllerService>().Get(applog);
	}

	internal virtual void OnGetFailure(IGetResult gr)
	{
	}

	internal virtual void OnPutError(string id, IPutResult putResult)
	{
	}

	internal virtual void OnExcludedLocation()
	{
	}

	internal ScanResult ReadDisk()
	{
		ScanResult result = Scanner.SmartReadDisk(m_readMethods);
		OnReadDisk(result);
		return result;
	}

	internal void AddError(string msg)
	{
		Result.Errors.Add(Error.NewError("E999", "Execution context error.", msg));
	}

	internal void OnMoveError(ErrorCodes err)
	{
		Context.CreateMoveErrorResult(SyncLocation.Deck, SyncLocation.Slot);
		AddError("There was a MOVE error.");
		OnMoveErrorInner(err);
	}
}
