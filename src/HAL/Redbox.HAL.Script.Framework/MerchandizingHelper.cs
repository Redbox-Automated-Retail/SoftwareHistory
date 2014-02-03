using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class MerchandizingHelper
{
	internal static MerchandizingHelper Instance => Singleton<MerchandizingHelper>.Instance;

	private QLM QLM => QLM.Instance;

	internal bool Setup(ExecutionResult result, bool checkSlots)
	{
		if (ControllerConfiguration.Instance.MerchandizingMode != MerchandizingMode.QLM)
		{
			return false;
		}
		ExecutionContext activeContext = ExecutionEngine.Instance.ActiveContext;
		if (checkSlots)
		{
			int numberOfEmpty = 5;
			if (!ControllerService.Instance.EmptyCountExceeds(numberOfEmpty))
			{
				ExecutionEngine.Instance.ActiveContext.AppLog.LogMachineFull();
				ResultHelper.Instance.CreateInfoResult("QlmUnloadMachineFull", "The machine is full, can't continue.");
				return false;
			}
		}
		Deck deck = QLM.Deck;
		if (deck == null)
		{
			activeContext.AppLog.Write("There is no QLM deck on this machine.", LogEntryType.Info);
			ResultHelper.Instance.CreateInfoResult("QlmNoDeck", "There is no QLM deck defined for this machine.");
			return false;
		}
		if (deck.NumberOfSlots == -1)
		{
			activeContext.AppLog.Write("There is no QLM max slots defined for this machine.", LogEntryType.Info);
			ResultHelper.Instance.CreateInfoResult("QlmNoMaxSlots", "There is no QLM max slots defined for this machine.");
			return false;
		}
		QlmStatus status = QLM.GetStatus();
		if (QlmStatus.AuxNotResponsive == status)
		{
			ResultHelper.Instance.CreateInfoResult("MachineError", "The QLM status query did not get an AUX response.");
			AddError(result, "The AUX board is not responsive.");
			return false;
		}
		if (QlmStatus.Empty == status)
		{
			activeContext.AppLog.Write("There is no QLM in the bay.");
			ResultHelper.Instance.CreateInfoResult("QlmDeckEmpty", "The QLM unload request failed because there is no QLM case in the QLM bay.");
			return false;
		}
		MotionResult motionResult = ControllerService.Instance.HomeAxis(Axis.Y, activeContext.AppLog);
		if (motionResult != MotionResult.Success)
		{
			ResultHelper.Instance.CreateInfoResult(motionResult.ToString().ToUpper(), "The HOMEY instruction failed.");
			AddError(result, "The homing operation on the Y axis failed.");
			return false;
		}
		if (QlmStatus.Engaged == status)
		{
			activeContext.AppLog.Write("The QLM is already engaged.");
			ResultHelper.Instance.CreateInfoResult("QlmDeckEngaged", "The QLM deck is already engaged.");
		}
		else
		{
			if (ControllerService.Instance.MoveToVend(MoveMode.Get, activeContext.AppLog) != MotionResult.Success)
			{
				ResultHelper.Instance.CreateInfoResult("MachineError", "There was an error executing the MOVEVEND instruction.");
				AddError(result, "The MOVEVEND instruction returned an error.");
				return false;
			}
			activeContext.AppLog.Write("Engage the QLM.");
			QlmMotionResult qlmMotionResult = QLM.Engage(home: true, activeContext.AppLog);
			if (qlmMotionResult != QlmMotionResult.Success)
			{
				ResultHelper.Instance.CreateInfoResult("QlmDeckEngageFailure", "The QLM engage failed.");
				if (QLM.Disengage(home: false, activeContext.AppLog) != QlmMotionResult.Success)
				{
					ResultHelper.Instance.CreateInfoResult("QlmDeckDisengageFailure", "The QLM disengage failed.");
				}
				AddError(result, "Engaging the QLM case timed out.");
				return false;
			}
			ResultHelper.Instance.CreateInfoResult("QlmDeckEngageSuccess", "The QLM engaged successfully.");
			CorrectMotionControlCommunication();
		}
		activeContext.PushTop(deck.NumberOfSlots);
		return true;
	}

	internal bool CleanupJob(ExecutionResult result, MotionResult failCode)
	{
		if (failCode != MotionResult.Success)
		{
			ResultHelper.Instance.CreateInfoResult("QlmMoveError", $"The MOVE instruction failed with code {failCode.ToString().ToUpper()}.");
		}
		ExecutionContext activeContext = ExecutionEngine.Instance.ActiveContext;
		if (ControllerService.Instance.MoveToVend(MoveMode.Get, activeContext.AppLog) != MotionResult.Success)
		{
			ResultHelper.Instance.CreateInfoResult("MachineError", "There was an error executing the MOVEVEND instruction.");
			if (!ControllerService.Instance.IsGripperClear())
			{
				ResultHelper.Instance.CreateInfoResult("QlmDeckStillEngaged", "The QLM deck is still engaged.");
				result.Errors.Add(Error.NewError("E999", "Execution context error.", "The MOVEVEND instruction failed."));
				return false;
			}
		}
		if (ControllerConfiguration.Instance.MerchandizingMode == MerchandizingMode.QLM)
		{
			ServiceLogHelper.Instance.WriteInfoMessage("Disengage the QLM.");
			QlmMotionResult qlmMotionResult = QLM.Disengage(home: true, activeContext.AppLog);
			if (QlmMotionResult.Success != qlmMotionResult)
			{
				ResultHelper.Instance.CreateInfoResult("QlmDeckDisengageFailure", "The QLM disengage failed.");
				result.Errors.Add(Error.NewError("E999", "Execution context error.", "The QLM Case didn't disengage."));
				return false;
			}
			ResultHelper.Instance.CreateInfoResult("QlmDeckDisengageSuccess", "The QLM disengaged successfully.");
		}
		return true;
	}

	internal void AddItemStuckError(ExecutionResult result)
	{
		result.Errors.Add(Error.NewError("E999", "Item stuck in gripper.", "Unable to file disc."));
	}

	internal void ExitOnMoveError(ExecutionResult result, bool leaveResult)
	{
		if (leaveResult)
		{
			ResultHelper.Instance.CreateMoveErrorResult();
		}
		result.Errors.Add(Error.NewError("E999", "Move failure", "The MOVE instruction failed."));
	}

	internal void CorrectMotionControlCommunication()
	{
		if (ControllerMode.Emulated == ControllerConfiguration.Instance.Mode || ExecutionEngine.Instance.ActiveContext.IsImmediate || !ControllerConfiguration.Instance.CorrectMotionControllerCommunication)
		{
			return;
		}
		ApplicationLog appLog = ExecutionEngine.Instance.ActiveContext.AppLog;
		MotionController controller = MotionController.GetController();
		if (!controller.CommunicationOk())
		{
			appLog.Write("Attempt to reset Motion Controller ...", LogEntryType.Info);
			if (controller.ResetDeviceDriver())
			{
				appLog.Write(" ... inline reset of device interfaces succeeded; init axes.", LogEntryType.Info);
				controller.Restart(appLog);
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
