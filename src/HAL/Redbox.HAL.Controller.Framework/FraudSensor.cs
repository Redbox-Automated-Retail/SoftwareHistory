using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class FraudSensor : IFraudSensor
{
	private bool SensorPowered;

	public ErrorCodes LastPostResult { get; private set; }

	public ErrorCodes Initialize(IFormattedLog log)
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		try
		{
			bool enableFraudSensorPostTrace = ControllerConfiguration.Instance.EnableFraudSensorPostTrace;
			int fraudSensorPostWait = ControllerConfiguration.Instance.FraudSensorPostWait;
			ErrorCodes errorCodes = ExecuteCommand(CommandType.FraudSensorEnablePowerTransistor);
			if (errorCodes != ErrorCodes.Success)
			{
				LogHelper.Instance.Log("[FraudSensor] Initialize: Power on returned error {0}.", errorCodes.ToString());
				LastPostResult = ErrorCodes.CommunicationError;
				return errorCodes;
			}
			SensorPowered = true;
			IControlSystem service2 = ServiceLocator.Instance.GetService<IControlSystem>();
			service.SpinWait(fraudSensorPostWait);
			string[] array = service2.ReadInputs(ControlBoards.Picker);
			if (array.Length == 0)
			{
				log.WriteFormatted("[FraudSensor] POST: unable to read picker inputs.", LogEntryType.Error);
				return LastPostResult = ErrorCodes.CommunicationError;
			}
			if (enableFraudSensorPostTrace)
			{
				log.WriteFormatted("[FraudSensor] POST startup: inputs are {0} and {1}.", array[6], array[7]);
			}
			if (array[6] == "0" || array[7] == "0")
			{
				log.WriteFormatted("[FraudSensor] POST startup error: inputs are {0} and {1}.", array[6], array[7]);
				return LastPostResult = ErrorCodes.FraudPostFailure;
			}
			ErrorCodes errorCodes4 = StartScan();
			if (errorCodes4 != ErrorCodes.Success)
			{
				log.WriteFormatted("[FraudSensor] POST startup error: cannot send reset signal error = {0}.", errorCodes4.ToString());
				return LastPostResult = errorCodes4;
			}
			service.SpinWait(fraudSensorPostWait);
			StopScan();
			array = service2.ReadInputs(ControlBoards.Picker);
			if (array.Length == 0)
			{
				log.WriteFormatted("[FraudSensor] POST: unable to read picker inputs.", LogEntryType.Error);
				return LastPostResult = ErrorCodes.CommunicationError;
			}
			if (enableFraudSensorPostTrace)
			{
				log.WriteFormatted("[FraudSensor] POST after reset signal: inputs are {0} and {1}.", array[6], array[7]);
			}
			if (array[6] == "0" && array[7] == "0")
			{
				log.WriteFormatted("[FraudSensor] POST: OK.");
				return LastPostResult = ErrorCodes.Success;
			}
			log.WriteFormatted("[FraudSensor] POST completion error: inputs are {0} and {1}.", array[6], array[7]);
			return LastPostResult = ErrorCodes.FraudPostFailure;
		}
		finally
		{
			OnAfterPost();
		}
	}

	public ErrorCodes Shutdown()
	{
		SensorPowered = false;
		ErrorCodes errorCodes = StopScan();
		if (errorCodes != ErrorCodes.Success)
		{
			LogHelper.Instance.Log("[Fraud Sensor] Shutdown: unable to stop scan - error = {0}.", errorCodes.ToString());
			return errorCodes;
		}
		ErrorCodes errorCodes2 = ExecuteCommand(CommandType.FraudSensorDisablePowerTransistor);
		if (errorCodes2 != ErrorCodes.Success)
		{
			LogHelper.Instance.Log("[FraudSensor] Shutdown: Power off returned error {0}.", errorCodes2.ToString());
			return errorCodes2;
		}
		return ErrorCodes.Success;
	}

	public ErrorCodes Reset(IFormattedLog log)
	{
		ErrorCodes errorCodes = Shutdown();
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		ServiceLocator.Instance.GetService<IRuntimeService>().Wait(ControllerConfiguration.Instance.FraudResetPause);
		return Initialize(log);
	}

	public ErrorCodes StartScan()
	{
		if (!SensorPowered)
		{
			LogHelper.Instance.Log("[FraudSensor] Scan called when unit is not under power.");
			return ErrorCodes.InvalidDeviceState;
		}
		ErrorCodes result = ExecuteCommand(CommandType.FraudSensorEnableTransistor);
		int fraudStartScanWait = ControllerConfiguration.Instance.FraudStartScanWait;
		if (fraudStartScanWait > 0)
		{
			ServiceLocator.Instance.GetService<IRuntimeService>().Wait(fraudStartScanWait);
		}
		return result;
	}

	public ErrorCodes StopScan()
	{
		int fraudStopScanWait = ControllerConfiguration.Instance.FraudStopScanWait;
		ErrorCodes result = ExecuteCommand(CommandType.FraudSensorDisableTransistor);
		if (fraudStopScanWait > 0)
		{
			ServiceLocator.Instance.GetService<IRuntimeService>().Wait(fraudStopScanWait);
		}
		return result;
	}

	public IFraudScanResult Validate()
	{
		ServiceLocator.Instance.GetService<IRuntimeService>().Wait(ControllerConfiguration.Instance.FraudScanValidateDelay);
		FraudScanResult fraudScanResult = new FraudScanResult();
		string[] array = ServiceLocator.Instance.GetService<IControlSystem>().ReadInputs(ControlBoards.Picker);
		if (array.Length == 0)
		{
			LogHelper.Instance.Log("[FraudSensor.Validate] Read inputs failed with {0}.", ErrorCodes.CommunicationError.ToString());
			fraudScanResult.ScanError = ErrorCodes.CommunicationError;
			return fraudScanResult;
		}
		fraudScanResult.Valid = array[6] == "1" || array[7] == "1";
		if (ControllerConfiguration.Instance.EnableFraudSensorTrace)
		{
			LogHelper.Instance.Log("[Fraud Sensor.Validate] IsValid = {0} inputs = {1} {2}", fraudScanResult.Valid.ToString(), array[6], array[7]);
		}
		return fraudScanResult;
	}

	internal FraudSensor()
	{
	}

	private void OnAfterPost()
	{
	}

	private ErrorCodes ExecuteCommand(CommandType type)
	{
		return ServiceLocator.Instance.GetService<ICoreCommandExecutor>().ExecuteControllerCommand(type).Error;
	}
}
