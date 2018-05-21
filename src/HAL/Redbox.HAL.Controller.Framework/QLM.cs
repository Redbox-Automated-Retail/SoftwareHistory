using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal static class QLM
{
	private const int LifterTimeout = 120000;

	internal static QlmStatus GetStatus()
	{
		CoreResponse coreResponse = ServiceLocator.Instance.GetService<ICoreCommandExecutor>().ExecuteControllerCommand(CommandType.AuxSensorsRead);
		if (coreResponse.CommError)
		{
			return QlmStatus.AuxNotResponsive;
		}
		bool flag = coreResponse.IsSensorSet(AuxSensor.QlmPresence);
		if (coreResponse.IsSensorSet(AuxSensor.QlmUp) & flag)
		{
			return QlmStatus.Engaged;
		}
		if (!flag)
		{
			return QlmStatus.Empty;
		}
		return QlmStatus.Disengaged;
	}

	internal static ErrorCodes Engage(bool home, IFormattedLog log)
	{
		return OnLifterOperation(home, log, CommandType.QlmEngage, TimeoutCounters.QlmEngage);
	}

	internal static ErrorCodes Disengage(bool home, IFormattedLog log)
	{
		return OnLifterOperation(home, log, CommandType.QlmDisengage, TimeoutCounters.QlmDisengage);
	}

	internal static IControlResponse Drop()
	{
		return ServiceLocator.Instance.GetService<ICoreCommandExecutor>().ExecuteControllerCommand(CommandType.QlmDrop);
	}

	internal static IControlResponse Lift()
	{
		return ServiceLocator.Instance.GetService<ICoreCommandExecutor>().ExecuteControllerCommand(CommandType.QlmLift);
	}

	internal static IControlResponse HaltOperation()
	{
		return ServiceLocator.Instance.GetService<ICoreCommandExecutor>().ExecuteControllerCommand(CommandType.QlmHalt);
	}

	private static ErrorCodes OnLifterOperation(bool home, IFormattedLog log, CommandType type, TimeoutCounters counter)
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
		CoreResponse coreResponse = ServiceLocator.Instance.GetService<ICoreCommandExecutor>().ExecuteControllerCommand(type, 120000);
		IPersistentCounterService service2 = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		if (coreResponse.TimedOut)
		{
			service2.Increment(counter);
		}
		ErrorCodes errorCodes = ((!coreResponse.Success) ? ErrorCodes.Timeout : ErrorCodes.Success);
		string msg = $"{type.ToString()} returned status {errorCodes.ToString().ToUpper()}.";
		log.WriteFormatted(msg, (errorCodes != ErrorCodes.Success) ? LogEntryType.Error : LogEntryType.Info);
		if (errorCodes != ErrorCodes.Success)
		{
			ServiceLocator.Instance.GetService<IControlSystem>().LogInputs(ControlBoards.Aux, LogEntryType.Error);
		}
		return errorCodes;
	}
}
