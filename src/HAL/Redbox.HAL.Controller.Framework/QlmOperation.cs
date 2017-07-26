using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class QlmOperation
{
	private const int LifterTimeout = 120000;

	public QlmStatus GetStatus()
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

	public ErrorCodes Engage(bool home, IFormattedLog log)
	{
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		if (home && service.HomeAxis(Axis.X, log) != ErrorCodes.Success)
		{
			return ErrorCodes.HomeXTimeout;
		}
		CoreResponse coreResponse = ServiceLocator.Instance.GetService<ICoreCommandExecutor>().ExecuteControllerCommand(CommandType.QlmEngage, 120000);
		if (coreResponse.TimedOut)
		{
			ServiceLocator.Instance.GetService<IPersistentCounterService>().Increment(TimeoutCounters.QlmEngage);
		}
		ErrorCodes result = ((!coreResponse.Success) ? ErrorCodes.Timeout : ErrorCodes.Success);
		LogResult(log, result, "ENGAGE");
		return result;
	}

	public ErrorCodes Disengage(bool home, IFormattedLog log)
	{
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		if (home && service.HomeAxis(Axis.X, log) != ErrorCodes.Success)
		{
			return ErrorCodes.HomeXTimeout;
		}
		CoreResponse coreResponse = ServiceLocator.Instance.GetService<ICoreCommandExecutor>().ExecuteControllerCommand(CommandType.QlmDisengage, 120000);
		IPersistentCounterService service2 = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		if (coreResponse.TimedOut)
		{
			service2.Increment(TimeoutCounters.QlmDisengage);
		}
		ErrorCodes result = ((!coreResponse.Success) ? ErrorCodes.Timeout : ErrorCodes.Success);
		LogResult(log, result, "DISENGAGE");
		return result;
	}

	public void Drop()
	{
		ServiceLocator.Instance.GetService<ICoreCommandExecutor>().ExecuteControllerCommand(CommandType.QlmDisengage);
	}

	public void Lift()
	{
		ServiceLocator.Instance.GetService<ICoreCommandExecutor>().ExecuteControllerCommand(CommandType.QlmEngage);
	}

	public bool HaltOperation()
	{
		return !ServiceLocator.Instance.GetService<ICoreCommandExecutor>().ExecuteControllerCommand(CommandType.QlmHalt).Success;
	}

	private void LogResult(IFormattedLog log, ErrorCodes result, string state)
	{
		string msg = $"QLM {state} returned status {result.ToString().ToUpper()}.";
		log.WriteFormatted(msg, (result != ErrorCodes.Success) ? LogEntryType.Error : LogEntryType.Info);
		if (result != ErrorCodes.Success)
		{
			ServiceLocator.Instance.GetService<IControlSystem>().LogInputs(ControlBoards.Aux, LogEntryType.Error);
		}
	}
}
