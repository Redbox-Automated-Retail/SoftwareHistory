using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal abstract class NativeJobAdapter
{
	protected ApplicationLog AppLog => Context.AppLog;

	protected IInventoryService InventoryService => ServiceLocator.Instance.GetService<IInventoryService>();

	protected IDumpbinService DumpbinService => ServiceLocator.Instance.GetService<IDumpbinService>();

	protected IMotionControlService MotionService => ServiceLocator.Instance.GetService<IMotionControlService>();

	protected IDecksService DecksService => ServiceLocator.Instance.GetService<IDecksService>();

	protected IControlSystem ControlSystem => ServiceLocator.Instance.GetService<IControlSystem>();

	protected ExecutionResult Result { get; private set; }

	protected ExecutionContext Context { get; private set; }

	internal string Name { get; set; }

	protected abstract void ExecuteInner();

	protected void AddError(string errMsg)
	{
		Result.Errors.Add(Error.NewError("E999", "Execution context error.", errMsg));
	}

	protected void ShowEmptyStuck(ILocation location)
	{
		IMoveDecoratorService service = ServiceLocator.Instance.GetService<IMoveDecoratorService>();
		if (service.ShowEmptyStuck(Context, location) != ErrorCodes.Success)
		{
			AddError("There was an error moving to the offset position.");
		}
	}

	protected bool ResetMotionControllerChecked()
	{
		IMoveDecoratorService service = ServiceLocator.Instance.GetService<IMoveDecoratorService>();
		return service.ResetMotionControllerChecked(Context);
	}

	protected bool MoveAndGet(int deck, int slot)
	{
		if (MotionService.MoveTo(deck, slot, MoveMode.Get, AppLog) != ErrorCodes.Success)
		{
			return false;
		}
		IControllerService service = ServiceLocator.Instance.GetService<IControllerService>();
		return service.Get(AppLog).Success;
	}

	protected bool MoveAndPut(int deck, int slot, string id)
	{
		if (MotionService.MoveTo(deck, slot, MoveMode.Put, AppLog) != ErrorCodes.Success)
		{
			return false;
		}
		ControlSystem.TrackCycle();
		IControllerService service = ServiceLocator.Instance.GetService<IControllerService>();
		return service.Put(id, AppLog).Success;
	}

	protected ErrorCodes CorrectiveMoveVend(MoveMode mode)
	{
		IMoveDecoratorService service = ServiceLocator.Instance.GetService<IMoveDecoratorService>();
		return service.CorrectiveMoveVend(Context, mode);
	}

	protected ErrorCodes CorrectiveMove(int deck, int slot, MoveMode mode)
	{
		IMoveDecoratorService service = ServiceLocator.Instance.GetService<IMoveDecoratorService>();
		return service.CorrectiveMove(Context, deck, slot, mode);
	}

	protected ErrorCodes CorrectiveMove(ILocation loc, MoveMode mode)
	{
		return CorrectiveMove(loc.Deck, loc.Slot, mode);
	}

	protected bool ContextSwitchRequested()
	{
		if (Context.DeferredStatusChangeRequested.HasValue)
		{
			LogHelper.Instance.Log($"SwitchRequested: deferred change to {Context.DeferredStatusChangeRequested.Value.ToString()}", LogEntryType.Info);
			Context.PromoteDeferredStatusChange();
			return true;
		}
		Context.FlushMessages();
		if (Context.Messages.Count != 0)
		{
			LogHelper.Instance.Log($"Context {Context.ID} {Context.ProgramName}: msg count = {Context.Messages.Count}", LogEntryType.Info);
		}
		return false;
	}

	protected NativeJobAdapter(ExecutionResult result, ExecutionContext context)
	{
		Result = result;
		Context = context;
	}

	internal void Execute()
	{
		ExecuteInner();
	}
}
