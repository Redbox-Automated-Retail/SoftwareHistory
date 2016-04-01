using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class QlmOperation : IQlm
{
	private const int LifterTimeout = 120000;

	public QlmStatus GetStatus()
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

	public ErrorCodes Engage(bool home, IFormattedLog log)
	{
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		if (home && service.HomeAxis(Axis.X, log) != ErrorCodes.Success)
		{
			return ErrorCodes.HomeXTimeout;
		}
		ICoreCommandExecutor service2 = ServiceLocator.Instance.GetService<ICoreCommandExecutor>();
		CoreResponse coreResponse = service2.ExecuteControllerCommand(CommandType.QlmEngage, 120000);
		if (coreResponse.TimedOut)
		{
			IPersistentCounterService service3 = ServiceLocator.Instance.GetService<IPersistentCounterService>();
			service3.Increment(TimeoutCounters.QlmEngage);
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
		ICoreCommandExecutor service2 = ServiceLocator.Instance.GetService<ICoreCommandExecutor>();
		CoreResponse coreResponse = service2.ExecuteControllerCommand(CommandType.QlmDisengage, 120000);
		IPersistentCounterService service3 = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		if (coreResponse.TimedOut)
		{
			service3.Increment(TimeoutCounters.QlmDisengage);
		}
		ErrorCodes result = ((!coreResponse.Success) ? ErrorCodes.Timeout : ErrorCodes.Success);
		LogResult(log, result, "DISENGAGE");
		return result;
	}

	public void Drop()
	{
		ICoreCommandExecutor service = ServiceLocator.Instance.GetService<ICoreCommandExecutor>();
		service.ExecuteControllerCommand(CommandType.QlmDisengage);
	}

	public void Lift()
	{
		ICoreCommandExecutor service = ServiceLocator.Instance.GetService<ICoreCommandExecutor>();
		service.ExecuteControllerCommand(CommandType.QlmEngage);
	}

	public bool HaltOperation()
	{
		ICoreCommandExecutor service = ServiceLocator.Instance.GetService<ICoreCommandExecutor>();
		return !service.ExecuteControllerCommand(CommandType.QlmHalt).Success;
	}

	private void LogResult(IFormattedLog log, ErrorCodes result, string state)
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
