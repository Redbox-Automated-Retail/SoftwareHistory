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
		if (ServiceLocator.Instance.GetService<IMoveDecoratorService>().ShowEmptyStuck(Context, location) != ErrorCodes.Success)
		{
			AddError("There was an error moving to the offset position.");
		}
	}

	protected bool MoveAndGet(int deck, int slot)
	{
		if (MotionService.MoveTo(deck, slot, MoveMode.Get, AppLog) != ErrorCodes.Success)
		{
			return false;
		}
		return ServiceLocator.Instance.GetService<IControllerService>().Get(AppLog).Success;
	}

	protected bool MoveAndPut(int deck, int slot, string id)
	{
		if (MotionService.MoveTo(deck, slot, MoveMode.Put, AppLog) != ErrorCodes.Success)
		{
			return false;
		}
		ControlSystem.TrackCycle();
		return ServiceLocator.Instance.GetService<IControllerService>().Put(id, AppLog).Success;
	}

	protected ErrorCodes CorrectiveMoveVend(MoveMode mode)
	{
		return ServiceLocator.Instance.GetService<IMotionControlService>().MoveVend(mode, Context.AppLog);
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
