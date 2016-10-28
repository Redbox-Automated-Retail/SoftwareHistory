using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal sealed class FraudSensor : IFraudSensor
{
	private const string PostFailureCounter = "FraudPostFailure";

	private const string PostOkCounter = "FraudPostSuccess";

	private DateTime LastSampleDate = DateTime.Now;

	private bool SensorPowered;

	public ErrorCodes LastPostResult { get; private set; }

	public ErrorCodes Initialize(IFormattedLog log)
	{
		try
		{
			LastSampleDate = DateTime.Now;
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
			IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
			PerfFunctions.SpinWait(fraudSensorPostWait);
			string[] array = service.ReadInputs(ControlBoards.Picker);
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
			PerfFunctions.SpinWait(fraudSensorPostWait);
			if (ControllerConfiguration.Instance.PauseBeforePostReadEnd)
			{
				StopScan();
			}
			else
			{
				StopScan(0);
			}
			array = service.ReadInputs(ControlBoards.Picker);
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
		PerfFunctions.Wait(ControllerConfiguration.Instance.FraudResetPause);
		return Initialize(log);
	}

	public ErrorCodes StartScan()
	{
		if (!SensorPowered)
		{
			LogHelper.Instance.Log("[FraudSensor] Scan called when unit is not under power.");
			return ErrorCodes.InvalidDeviceState;
		}
		return ExecuteCommand(CommandType.FraudSensorEnableTransistor);
	}

	public ErrorCodes StopScan()
	{
		return StopScan(ControllerConfiguration.Instance.FraudStopScanWait);
	}

	public IFraudScanResult Validate()
	{
		FraudScanResult fraudScanResult = new FraudScanResult();
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		string[] array = service.ReadInputs(ControlBoards.Picker);
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

	public IFraudPostStatistics GetStatistics()
	{
		IPersistentCounterService service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		IPersistentCounter failures = service.Find("FraudPostFailure");
		IPersistentCounter success = service.Find("FraudPostSuccess");
		return new PostStatistics(LastSampleDate, failures, success);
	}

	public bool ResetPostStatistics()
	{
		IPersistentCounterService service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		service.Reset("FraudPostFailure");
		service.Reset("FraudPostSuccess");
		return true;
	}

	internal FraudSensor()
	{
		IPersistentCounterService service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		IPersistentCounter counter = service.Find("FraudPostFailure");
		service.AddWeeklyResettable(counter);
		IPersistentCounter counter2 = service.Find("FraudPostSuccess");
		service.AddWeeklyResettable(counter2);
	}

	private ErrorCodes StopScan(int pause)
	{
		ErrorCodes result = ExecuteCommand(CommandType.FraudSensorDisableTransistor);
		if (pause > 0)
		{
			PerfFunctions.Wait(pause);
		}
		return result;
	}

	private void OnAfterPost()
	{
		if (ControllerConfiguration.Instance.EnableFraudPostTracking)
		{
			IPersistentCounterService service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
			if (LastPostResult != ErrorCodes.Success)
			{
				service.Increment("FraudPostFailure");
			}
			else
			{
				service.Increment("FraudPostSuccess");
			}
		}
	}

	private ErrorCodes ExecuteCommand(CommandType type)
	{
		ICoreCommandExecutor service = ServiceLocator.Instance.GetService<ICoreCommandExecutor>();
		return service.ExecuteControllerCommand(type).Error;
	}
}
