using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Controller.Framework;

public static class QLM
{
	private const int LifterTimeout = 120000;

	public static Deck Deck
	{
		get
		{
			Deck rv = null;
			DecksManager.Instance.ForAllDecksDo(delegate(Deck d)
			{
				if (d.IsQlmDeck)
				{
					rv = d;
					return false;
				}
				return true;
			});
			return rv;
		}
	}

	public static bool SetDoorLock(bool locked)
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

	public static QlmDoorStatus GetDoorStatus()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedGetQlmDoorStatusResult;
		}
		return QlmDoorStatus.Closed;
	}

	public static QlmStatus GetStatus()
	{
		return StatusInner();
	}

	public static QlmMotionResult Engage(bool home, IFormattedLog log)
	{
		if (ControllerConfiguration.Instance.IsVMZMachine)
		{
			return QlmMotionResult.Timeout;
		}
		if (home && ControllerService.Instance.HomeAxis(Axis.X, log) != MotionResult.Success)
		{
			return QlmMotionResult.HomeXTimeout;
		}
		Deck deck = Deck;
		if (deck != null)
		{
			deck.MarkInventory("UNKNOWN");
		}
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedEngageQlmResult;
		}
		CoreResponse coreResponse = ControlSystem.Instance.ExecuteControllerCommand(CommandType.QlmEngage, 120000);
		if (coreResponse.TimedOut)
		{
			CounterManager.Instance.Increment("QlmEngageTimeout");
		}
		QlmMotionResult result = (coreResponse.TimedOut ? QlmMotionResult.Timeout : QlmMotionResult.Success);
		LogResult(log, result, "ENGAGE");
		return result;
	}

	public static QlmMotionResult Disengage(bool home, IFormattedLog log)
	{
		if (ControllerConfiguration.Instance.IsVMZMachine)
		{
			return QlmMotionResult.Timeout;
		}
		if (home && ControllerService.Instance.HomeAxis(Axis.X, log) != MotionResult.Success)
		{
			return QlmMotionResult.HomeXTimeout;
		}
		Deck deck = Deck;
		if (deck != null)
		{
			deck.MarkInventory("UNKNOWN");
		}
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedDisengageQlmResult;
		}
		CoreResponse coreResponse = ControlSystem.Instance.ExecuteControllerCommand(CommandType.QlmDisengage, 120000);
		if (coreResponse.TimedOut)
		{
			CounterManager.Instance.Increment("QlmDisengageTimeout");
		}
		QlmMotionResult result = (coreResponse.TimedOut ? QlmMotionResult.Timeout : QlmMotionResult.Success);
		LogResult(log, result, "DISENGAGE");
		return result;
	}

	public static void Drop()
	{
		if (ControllerConfiguration.Instance.Mode != ControllerMode.Emulated && !ControllerConfiguration.Instance.IsVMZMachine)
		{
			ControlSystem.Instance.ExecuteControllerCommand(CommandType.QlmDisengage);
		}
	}

	public static void Lift()
	{
		if (ControllerConfiguration.Instance.Mode != ControllerMode.Emulated && !ControllerConfiguration.Instance.IsVMZMachine)
		{
			ControlSystem.Instance.ExecuteControllerCommand(CommandType.QlmEngage);
		}
	}

	public static bool HaltOperation()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedQlmHaltResult;
		}
		if (ControllerConfiguration.Instance.IsVMZMachine)
		{
			return true;
		}
		return ControlSystem.Instance.ExecuteControllerCommand(CommandType.QlmHalt).TimedOut;
	}

	internal static bool ExtendForGet()
	{
		int qlmExtendTime = ControllerConfiguration.Instance.QlmExtendTime;
		if (ControllerConfiguration.Instance.QlmTimedExtend)
		{
			Gripper.TimedExtend(qlmExtendTime);
		}
		else
		{
			ControlSystem.Instance.ExecuteControllerCommand(CommandType.GripperExtend, ControllerConfiguration.Instance.QlmExtendTime);
		}
		return true;
	}

	private static QlmStatus StatusInner()
	{
		if (ControllerConfiguration.Instance.Mode == ControllerMode.Emulated)
		{
			return ControllerEmulationState.Instance.ExpectedGetQlmStatusResult;
		}
		CoreResponse coreResponse = ControlSystem.Instance.ExecuteControllerCommand(CommandType.AuxSensorsRead);
		if (coreResponse.CommunicationError)
		{
			return QlmStatus.AuxNotResponsive;
		}
		bool flag = coreResponse.IsSensorSet(AuxSensor.QlmPresence);
		if (coreResponse.IsSensorSet(AuxSensor.QlmUp) && flag)
		{
			return QlmStatus.Engaged;
		}
		if (!flag)
		{
			return QlmStatus.Empty;
		}
		return QlmStatus.Disengaged;
	}

	private static void LogResult(IFormattedLog log, QlmMotionResult result, string state)
	{
		string msg = $"QLM {state} returned status {result.ToString().ToUpper()}.";
		log.WriteFormatted(msg, (result != QlmMotionResult.Success) ? LogEntryType.Error : LogEntryType.Info);
		if (result != QlmMotionResult.Success)
		{
			ControlSystem.Instance.LogInputs(ControlBoards.Aux, LogEntryType.Error);
		}
	}
}
