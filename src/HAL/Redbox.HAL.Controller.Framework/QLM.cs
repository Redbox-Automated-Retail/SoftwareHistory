using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public sealed class QLM
{
	public static QLM Instance => Singleton<QLM>.Instance;

	public Deck Deck => DecksManager.Instance.Decks.Find((Deck each) => each.IsQlmDeck);

	public bool SetDoorLock(bool locked)
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			if (!locked)
			{
				return ControllerEmulationState.Instance.ExpectedQlmUnLockResult;
			}
			return ControllerEmulationState.Instance.ExpectedQlmLockResult;
		}
		return true;
	}

	public QlmDoorStatus GetDoorStatus()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedGetQlmDoorStatusResult;
		}
		return QlmDoorStatus.Closed;
	}

	public QlmStatus GetStatus()
	{
		return StatusInner();
	}

	public QlmMotionResult Engage(bool home, IFormattedLog log)
	{
		if (ControllerConfiguration.Instance.MerchandizingMode != MerchandizingMode.QLM)
		{
			return QlmMotionResult.Timeout;
		}
		if (home && ControllerService.Instance.HomeAxis(Axis.X, log) != MotionResult.Success)
		{
			return QlmMotionResult.HomeXTimeout;
		}
		Deck?.MarkInventory("UNKNOWN");
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedEngageQlmResult;
		}
		CoreResponse coreResponse = CoreController.ExecuteSingleWaitableCommand(CommandType.QlmEngage, 120000);
		if (coreResponse.TimedOut)
		{
			CounterManager.Instance.Increment("QlmEngageTimeout");
		}
		QlmMotionResult result = (coreResponse.TimedOut ? QlmMotionResult.Timeout : QlmMotionResult.Success);
		LogResult(log, result, "ENGAGE");
		return result;
	}

	public QlmMotionResult Disengage(bool home, IFormattedLog log)
	{
		if (ControllerConfiguration.Instance.MerchandizingMode != MerchandizingMode.QLM)
		{
			return QlmMotionResult.Timeout;
		}
		if (home && ControllerService.Instance.HomeAxis(Axis.X, log) != MotionResult.Success)
		{
			return QlmMotionResult.HomeXTimeout;
		}
		Deck?.MarkInventory("UNKNOWN");
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedDisengageQlmResult;
		}
		CoreResponse coreResponse = CoreController.ExecuteSingleWaitableCommand(CommandType.QlmDisengage, 120000);
		if (coreResponse.TimedOut)
		{
			CounterManager.Instance.Increment("QlmDisengageTimeout");
		}
		QlmMotionResult result = (coreResponse.TimedOut ? QlmMotionResult.Timeout : QlmMotionResult.Success);
		LogResult(log, result, "DISENGAGE");
		return result;
	}

	public void Drop()
	{
		if (ControllerConfiguration.Instance.Mode != ControllerMode.Emulated && ControllerConfiguration.Instance.MerchandizingMode == MerchandizingMode.QLM)
		{
			CoreController.ExecuteSingleControllerCommand(CommandType.QlmDisengage);
		}
	}

	public void Lift()
	{
		if (ControllerConfiguration.Instance.Mode != ControllerMode.Emulated && ControllerConfiguration.Instance.MerchandizingMode == MerchandizingMode.QLM)
		{
			CoreController.ExecuteSingleControllerCommand(CommandType.QlmEngage);
		}
	}

	public bool HaltOperation()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedQlmHaltResult;
		}
		if (ControllerConfiguration.Instance.MerchandizingMode != MerchandizingMode.QLM)
		{
			return true;
		}
		return CoreController.ExecuteSingleControllerCommand(CommandType.QlmHalt).TimedOut;
	}

	internal bool ExtendForGet()
	{
		int qlmExtendTime = ControllerConfiguration.Instance.QlmExtendTime;
		if (ControllerConfiguration.Instance.QlmTimedExtend)
		{
			Gripper.Instance.TimedExtend(qlmExtendTime);
		}
		else
		{
			CoreController.ExecuteSingleWaitableCommand(CommandType.GripperExtend, ControllerConfiguration.Instance.QlmExtendTime);
		}
		return true;
	}

	private QlmStatus StatusInner()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedGetQlmStatusResult;
		}
		CoreResponse coreResponse = AuxBoard.Instance.ReadInputs();
		if (coreResponse.CommunicationError)
		{
			LogHelper.Instance.Log($"QLM Status - Aux board not responsive: {coreResponse.CommErrorDiagnostic}", LogEntryType.Info);
			return QlmStatus.AuxNotResponsive;
		}
		bool flag = AuxBoard.Instance.IsSensorBlocked(AuxSensor.QlmPresence, coreResponse);
		if (AuxBoard.Instance.IsSensorBlocked(AuxSensor.QlmUp, coreResponse) && flag)
		{
			return QlmStatus.Engaged;
		}
		if (!flag)
		{
			return QlmStatus.Empty;
		}
		return QlmStatus.Disengaged;
	}

	private void LogResult(IFormattedLog log, QlmMotionResult result, string state)
	{
		string msg = $"QLM {state} returned status {result.ToString().ToUpper()}.";
		log.WriteFormatted(msg, (result != QlmMotionResult.Success) ? LogEntryType.Error : LogEntryType.Info);
		if (result != QlmMotionResult.Success)
		{
			AuxBoard.Instance.LogInputs(LogEntryType.Error);
		}
	}

	private QLM()
	{
	}
}
