using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

public static class QLM
{
	private const int LifterTimeout = 120000;

	public static IDeck Deck
	{
		get
		{
			IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
			return service.QlmDeck;
		}
	}

	public static QlmStatus GetStatus()
	{
		ICoreCommandExecutor service = ServiceLocator.Instance.GetService<ICoreCommandExecutor>();
		CoreResponse coreResponse = service.ExecuteControllerCommand(CommandType.AuxSensorsRead);
		if (coreResponse.CommError)
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

	public static ErrorCodes Engage(bool home, IFormattedLog log)
	{
		if (ControllerConfiguration.Instance.IsVMZMachine)
		{
			return ErrorCodes.Timeout;
		}
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		if (home && service.HomeAxis(Axis.X, log) != ErrorCodes.Success)
		{
			return ErrorCodes.HomeXTimeout;
		}
		IDeck deck = Deck;
		if (deck != null)
		{
			IInventoryService service2 = ServiceLocator.Instance.GetService<IInventoryService>();
			service2.MarkDeckInventory(deck, "UNKNOWN");
		}
		ICoreCommandExecutor service3 = ServiceLocator.Instance.GetService<ICoreCommandExecutor>();
		CoreResponse coreResponse = service3.ExecuteControllerCommand(CommandType.QlmEngage, 120000);
		if (coreResponse.TimedOut)
		{
			IPersistentCounterService service4 = ServiceLocator.Instance.GetService<IPersistentCounterService>();
			service4.Increment(TimeoutCounters.QlmEngage);
		}
		ErrorCodes result = ((!coreResponse.Success) ? ErrorCodes.Timeout : ErrorCodes.Success);
		LogResult(log, result, "ENGAGE");
		return result;
	}

	public static ErrorCodes Disengage(bool home, IFormattedLog log)
	{
		if (ControllerConfiguration.Instance.IsVMZMachine)
		{
			return ErrorCodes.Timeout;
		}
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		if (home && service.HomeAxis(Axis.X, log) != ErrorCodes.Success)
		{
			return ErrorCodes.HomeXTimeout;
		}
		IDeck deck = Deck;
		if (deck != null)
		{
			IInventoryService service2 = ServiceLocator.Instance.GetService<IInventoryService>();
			service2.MarkDeckInventory(deck, "UNKNOWN");
		}
		ICoreCommandExecutor service3 = ServiceLocator.Instance.GetService<ICoreCommandExecutor>();
		CoreResponse coreResponse = service3.ExecuteControllerCommand(CommandType.QlmDisengage, 120000);
		IPersistentCounterService service4 = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		if (coreResponse.TimedOut)
		{
			service4.Increment(TimeoutCounters.QlmDisengage);
		}
		ErrorCodes result = ((!coreResponse.Success) ? ErrorCodes.Timeout : ErrorCodes.Success);
		LogResult(log, result, "DISENGAGE");
		return result;
	}

	public static void Drop()
	{
		if (ControllerConfiguration.Instance.Mode != ControllerMode.Emulated && !ControllerConfiguration.Instance.IsVMZMachine)
		{
			ICoreCommandExecutor service = ServiceLocator.Instance.GetService<ICoreCommandExecutor>();
			service.ExecuteControllerCommand(CommandType.QlmDisengage);
		}
	}

	public static void Lift()
	{
		if (ControllerConfiguration.Instance.Mode != ControllerMode.Emulated && !ControllerConfiguration.Instance.IsVMZMachine)
		{
			ICoreCommandExecutor service = ServiceLocator.Instance.GetService<ICoreCommandExecutor>();
			service.ExecuteControllerCommand(CommandType.QlmEngage);
		}
	}

	public static bool HaltOperation()
	{
		if (ControllerConfiguration.Instance.IsVMZMachine)
		{
			return true;
		}
		ICoreCommandExecutor service = ServiceLocator.Instance.GetService<ICoreCommandExecutor>();
		return !service.ExecuteControllerCommand(CommandType.QlmHalt).Success;
	}

	private static void LogResult(IFormattedLog log, ErrorCodes result, string state)
	{
		string msg = $"QLM {state} returned status {result.ToString().ToUpper()}.";
		log.WriteFormatted(msg, (result != ErrorCodes.Success) ? LogEntryType.Error : LogEntryType.Info);
		if (result != ErrorCodes.Success)
		{
			IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
			service.LogInputs(ControlBoards.Aux, LogEntryType.Error);
		}
	}
}
