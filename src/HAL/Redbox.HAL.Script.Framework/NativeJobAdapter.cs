using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal abstract class NativeJobAdapter
{
	protected ApplicationLog AppLog => Context.AppLog;

	protected ControllerService Controller => ControllerService.Instance;

	protected ExecutionResult Result { get; private set; }

	protected ExecutionContext Context { get; private set; }

	protected abstract void ExecuteInner();

	protected void AddError(string errMsg)
	{
		Result.Errors.Add(Error.NewError("E999", "Execution context error.", errMsg));
	}

	protected bool ResetMotionController()
	{
		bool flag = MotionController.Reset(AppLog, quick: true);
		HardwareCorrectionStats.InsertStat(CorrectionType.Arcus, flag, Context.ID);
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
		if (ControllerService.Instance.MoveTo(deck, slot, MoveMode.Get, AppLog) != MotionResult.Success)
		{
			return false;
		}
		return ControllerService.Instance.Get(AppLog).Success;
	}

	protected bool MoveAndPut(int deck, int slot, string id)
	{
		if (ControllerService.Instance.MoveTo(deck, slot, MoveMode.Put, AppLog) != MotionResult.Success)
		{
			return false;
		}
		return ControllerService.Instance.Put(id, AppLog).Success;
	}

	protected MotionResult CorrectiveMoveVend(MoveMode mode)
	{
		MotionResult motionResult = ControllerService.Instance.MoveToVend(mode, AppLog);
		if (MotionResult.ArcusNotResponsive == motionResult)
		{
			if (ResetMotionControllerChecked())
			{
				return ControllerService.Instance.MoveToVend(mode, AppLog);
			}
			return MotionResult.ArcusNotResponsive;
		}
		return motionResult;
	}

	protected MotionResult CorrectiveMove(int deck, int slot, MoveMode mode)
	{
		MotionResult motionResult = ControllerService.Instance.MoveTo(deck, slot, mode, AppLog);
		if (MotionResult.ArcusNotResponsive == motionResult)
		{
			if (ResetMotionControllerChecked())
			{
				return ControllerService.Instance.MoveTo(deck, slot, mode, AppLog);
			}
			return MotionResult.ArcusNotResponsive;
		}
		return motionResult;
	}

	protected MotionResult CorrectiveMove(Location loc, MoveMode mode)
	{
		return CorrectiveMove(loc.Deck, loc.Slot, mode);
	}

	protected T GetTop<T>()
	{
		return (T)ConversionHelper.ChangeType(Context.PopTop(), typeof(T));
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
