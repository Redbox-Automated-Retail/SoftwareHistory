using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

internal abstract class NativeJobAdapter
{
	internal string Name { get; set; }

	protected ApplicationLog AppLog => Context.AppLog;

	protected ControllerService Controller => ControllerService.Instance;

	protected IInventoryService InventoryService => ServiceLocator.Instance.GetService<IInventoryService>();

	protected IDumpbinService DumpbinService => ServiceLocator.Instance.GetService<IDumpbinService>();

	protected IMotionControlService MotionService => ServiceLocator.Instance.GetService<IMotionControlService>();

	protected IDecksService DecksService => ServiceLocator.Instance.GetService<IDecksService>();

	protected IControlSystem ControlSystem => ServiceLocator.Instance.GetService<IControlSystem>();

	protected ExecutionResult Result { get; private set; }

	protected ExecutionContext Context { get; private set; }

	internal void Execute()
	{
		ExecuteInner();
	}

	protected abstract void ExecuteInner();

	protected void AddError(string errMsg)
	{
		Result.Errors.Add(Error.NewError("E999", "Execution context error.", errMsg));
	}

	protected bool ResetMotionController()
	{
		bool flag = MotionService.Reset(AppLog, quick: true);
		IPersistentCounterService service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		service.Insert(HardwareCorrectionStatistic.Arcus, Context, flag);
		return flag;
	}

	protected bool ResetMotionControllerChecked()
	{
		if (ControllerConfiguration.Instance.RestartControllerDuringUserJobs)
		{
			return ResetMotionController();
		}
		return false;
	}

	protected bool MoveAndGet(int deck, int slot)
	{
		if (MotionService.MoveTo(deck, slot, MoveMode.Get, AppLog) != ErrorCodes.Success)
		{
			return false;
		}
		return ControllerService.Instance.Get(AppLog).Success;
	}

	protected bool MoveAndPut(int deck, int slot, string id)
	{
		if (MotionService.MoveTo(deck, slot, MoveMode.Put, AppLog) != ErrorCodes.Success)
		{
			return false;
		}
		ControlSystem.TrackCycle();
		return ControllerService.Instance.Put(id, AppLog).Success;
	}

	protected ErrorCodes CorrectiveMoveVend(MoveMode mode)
	{
		ErrorCodes errorCodes = MotionService.MoveVend(mode, AppLog);
		if (ErrorCodes.ArcusNotResponsive == errorCodes)
		{
			if (ResetMotionControllerChecked())
			{
				return MotionService.MoveVend(mode, AppLog);
			}
			return ErrorCodes.ArcusNotResponsive;
		}
		return errorCodes;
	}

	protected ErrorCodes CorrectiveMove(int deck, int slot, MoveMode mode)
	{
		ErrorCodes errorCodes = MotionService.MoveTo(deck, slot, mode, AppLog);
		if (ErrorCodes.LowerLimitError == errorCodes && SessionErrorState.Instance.LowerLimitConfigured(Context))
		{
			SessionErrorState.Instance.IncrementLowerLimit();
			ErrorCodes errorCodes2 = MotionService.HomeAxis(Axis.Y, AppLog);
			AppLog.WriteFormatted("Move returned {0}; HOME Y returned {1}; return TIMEOUT", errorCodes.ToString(), errorCodes2.ToString());
			return ErrorCodes.Timeout;
		}
		if (ErrorCodes.ArcusNotResponsive == errorCodes)
		{
			if (ResetMotionControllerChecked())
			{
				return MotionService.MoveTo(deck, slot, mode, AppLog);
			}
			return ErrorCodes.ArcusNotResponsive;
		}
		return errorCodes;
	}

	protected ErrorCodes CorrectiveMove(ILocation loc, MoveMode mode)
	{
		return CorrectiveMove(loc.Deck, loc.Slot, mode);
	}

	protected T GetTop<T>()
	{
		return Context.PopTop<T>();
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
}
