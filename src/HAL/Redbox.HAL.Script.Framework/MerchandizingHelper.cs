using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class MerchandizingHelper
{
	internal static MerchandizingHelper Instance => Singleton<MerchandizingHelper>.Instance;

	internal bool Setup(ExecutionResult result, bool checkSlots, ExecutionContext context)
	{
		if (ControllerConfiguration.Instance.IsVMZMachine)
		{
			return false;
		}
		if (checkSlots)
		{
			int numberOfEmpty = 5;
			if (!InventoryService.Instance.EmptyCountExceeds(numberOfEmpty))
			{
				context.AppLog.LogMachineFull();
				context.CreateInfoResult("QlmUnloadMachineFull", "The machine is full, can't continue.");
				return false;
			}
		}
		Deck deck = QLM.Deck;
		if (null == deck)
		{
			context.AppLog.Write("There is no QLM deck on this machine.", LogEntryType.Info);
			context.CreateInfoResult("QlmNoDeck", "There is no QLM deck defined for this machine.");
			return false;
		}
		if (deck.NumberOfSlots == -1)
		{
			context.AppLog.Write("There is no QLM max slots defined for this machine.", LogEntryType.Info);
			context.CreateInfoResult("QlmNoMaxSlots", "There is no QLM max slots defined for this machine.");
			return false;
		}
		QlmStatus status = QLM.GetStatus();
		if (QlmStatus.AuxNotResponsive == status)
		{
			context.CreateInfoResult("MachineError", "The QLM status query did not get an AUX response.");
			AddError(result, "The AUX board is not responsive.");
			return false;
		}
		if (QlmStatus.Empty == status)
		{
			context.AppLog.Write("There is no QLM in the bay.");
			context.CreateInfoResult("QlmDeckEmpty", "The QLM unload request failed because there is no QLM case in the QLM bay.");
			return false;
		}
		MotionResult motionResult = ControllerService.Instance.HomeAxis(Axis.Y, context.AppLog);
		if (motionResult != MotionResult.Success)
		{
			context.CreateInfoResult(motionResult.ToString().ToUpper(), "The HOMEY instruction failed.");
			AddError(result, "The homing operation on the Y axis failed.");
			return false;
		}
		if (QlmStatus.Engaged == status)
		{
			context.AppLog.Write("The QLM is already engaged.");
			context.CreateInfoResult("QlmDeckEngaged", "The QLM deck is already engaged.");
		}
		else
		{
			if (ControllerService.Instance.MoveToVend(MoveMode.Get, context.AppLog) != MotionResult.Success)
			{
				context.CreateInfoResult("MachineError", "There was an error executing the MOVEVEND instruction.");
				AddError(result, "The MOVEVEND instruction returned an error.");
				return false;
			}
			context.AppLog.Write("Engage the QLM.");
			QlmMotionResult qlmMotionResult = QLM.Engage(home: true, context.AppLog);
			if (qlmMotionResult != QlmMotionResult.Success)
			{
				context.CreateInfoResult("QlmDeckEngageFailure", "The QLM engage failed.");
				if (QLM.Disengage(home: false, context.AppLog) != QlmMotionResult.Success)
				{
					context.CreateInfoResult("QlmDeckDisengageFailure", "The QLM disengage failed.");
				}
				AddError(result, "Engaging the QLM case timed out.");
				return false;
			}
			context.CreateInfoResult("QlmDeckEngageSuccess", "The QLM engaged successfully.");
			CorrectMotionControlCommunication(context);
		}
		context.PushTop(deck.NumberOfSlots);
		return true;
	}

	internal bool CleanupJob(ExecutionResult result, ExecutionContext context, MotionResult failCode)
	{
		if (failCode != MotionResult.Success)
		{
			context.CreateInfoResult("QlmMoveError", $"The MOVE instruction failed with code {failCode.ToString().ToUpper()}.");
		}
		if (ControllerService.Instance.MoveToVend(MoveMode.Get, context.AppLog) != MotionResult.Success)
		{
			context.CreateInfoResult("MachineError", "There was an error executing the MOVEVEND instruction.");
			if (!Gripper.IsClear())
			{
				context.CreateInfoResult("QlmDeckStillEngaged", "The QLM deck is still engaged.");
				result.Errors.Add(Error.NewError("E999", "Execution context error.", "The MOVEVEND instruction failed."));
				return false;
			}
		}
		if (!ControllerConfiguration.Instance.IsVMZMachine)
		{
			QlmMotionResult qlmMotionResult = QLM.Disengage(home: true, context.AppLog);
			if (QlmMotionResult.Success != qlmMotionResult)
			{
				context.CreateInfoResult("QlmDeckDisengageFailure", "The QLM disengage failed.");
				result.Errors.Add(Error.NewError("E999", "Execution context error.", "The QLM Case didn't disengage."));
				return false;
			}
			context.CreateInfoResult("QlmDeckDisengageSuccess", "The QLM disengaged successfully.");
		}
		return true;
	}

	internal void AddItemStuckError(ExecutionResult result)
	{
		result.Errors.Add(Error.NewError("E999", "Item stuck in gripper.", "Unable to file disc."));
	}

	internal void ExitOnMoveError(ExecutionResult result, ExecutionContext context, bool leaveResult)
	{
		if (leaveResult)
		{
			context.CreateMoveErrorResult();
		}
		result.Errors.Add(Error.NewError("E999", "Move failure", "The MOVE instruction failed."));
	}

	internal void CorrectMotionControlCommunication(ExecutionContext context)
	{
		if (ControllerMode.Emulated == ControllerConfiguration.Instance.Mode || context.IsImmediate || !ControllerConfiguration.Instance.CorrectMotionControllerCommunication)
		{
			return;
		}
		ApplicationLog appLog = context.AppLog;
		MotionController controller = MotionController.GetController();
		if (!controller.CommunicationOk())
		{
			appLog.Write("Attempt to reset Motion Controller ...", LogEntryType.Info);
			if (MotionController.Reset(appLog, quick: true))
			{
				appLog.Write(" ... inline reset of device interfaces succeeded; init axes.", LogEntryType.Info);
			}
			else
			{
				appLog.Write(" ... inline reset of device interfaces FAILED.", LogEntryType.Error);
			}
		}
	}

	internal void AddError(ExecutionResult result, string desc)
	{
		result.Errors.Add(Error.NewError("E999", desc, "There was an unrecoverable error during a merchandizing operation."));
	}

	private MerchandizingHelper()
	{
	}
}
