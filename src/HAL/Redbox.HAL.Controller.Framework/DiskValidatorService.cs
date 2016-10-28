using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

internal sealed class DiskValidatorService : IDiskValidatorService, IConfigurationObserver, IControlSystemObserver
{
	private int m_persistentPostFailureCount;

	public bool IsConfigured { get; private set; }

	public IFraudSensor FraudSensor { get; private set; }

	public void NotifyConfigurationChangeStart()
	{
		LogHelper.Instance.Log("[DiskValidatorService] NotifyConfigurationStart.");
		if (IsConfigured)
		{
			FraudSensor.Shutdown();
			PerfFunctions.SpinWait(ControllerConfiguration.Instance.FraudSensorPostWait);
		}
	}

	public void NotifyConfigurationChangeEnd()
	{
		LogHelper.Instance.Log("[DiskValidatorService] NotifyConfigurationEnd.");
		IsConfigured = ControllerConfiguration.Instance.EnableSecureDiskValidator;
		if (IsConfigured)
		{
			IFormattedLogFactoryService service = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
			IFormattedLog nilLog = service.NilLog;
			ErrorCodes errorCodes = FraudSensor.Initialize(nilLog);
			LogHelper.Instance.Log("[DiskValidator] Fraud sensor initialization returned {0}", errorCodes.ToString());
		}
	}

	public void NotifyConfigurationLoaded()
	{
		IsConfigured = ControllerConfiguration.Instance.EnableSecureDiskValidator;
		LogHelper.Instance.Log("[DiskValidatorService] ConfigurationLoad: Enabled = {0}.", IsConfigured.ToString());
	}

	public void OnSystemInitialize(ErrorCodes initError)
	{
		LogHelper.Instance.Log("[DiskValidatorService] On initialize.");
		if (initError == ErrorCodes.Success && IsConfigured)
		{
			IFormattedLogFactoryService service = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
			IFormattedLog nilLog = service.NilLog;
			ErrorCodes errorCodes = FraudSensor.Initialize(nilLog);
			LogHelper.Instance.Log("[DiskValidator] Fraud sensor initialization returned {0}", errorCodes.ToString());
		}
	}

	public void OnSystemShutdown()
	{
		LogHelper.Instance.Log("[DiskValidatorService] On shutdown.");
		if (IsConfigured)
		{
			FraudSensor.Shutdown();
		}
	}

	public IFraudScanResult Validate(string matrix)
	{
		return Validate();
	}

	public IFraudScanResult Validate()
	{
		if (!IsConfigured)
		{
			FraudScanResult fraudScanResult = new FraudScanResult();
			fraudScanResult.ScanError = ErrorCodes.DeviceNotConfigured;
			fraudScanResult.Valid = false;
			return fraudScanResult;
		}
		PerfFunctions.Wait(ControllerConfiguration.Instance.FraudCameraScanPause);
		return FraudSensor.Validate();
	}

	public void CorrectErroredPost(IFormattedLog log)
	{
		if (!IsConfigured || FraudSensor.LastPostResult == ErrorCodes.Success)
		{
			m_persistentPostFailureCount = 0;
		}
		else if (ControllerConfiguration.Instance.CorrectFailedFraudPost && m_persistentPostFailureCount <= 2)
		{
			ErrorCodes errorCodes = FraudSensor.Reset(log);
			log.WriteFormatted($"[CorrectErroredPost] POST returned {errorCodes.ToString()}");
			if (errorCodes == ErrorCodes.Success)
			{
				m_persistentPostFailureCount = 0;
			}
			else
			{
				m_persistentPostFailureCount++;
			}
		}
	}

	internal DiskValidatorService(IFraudSensor sensor)
	{
		FraudSensor = sensor;
		ControllerConfiguration.Instance.AddObserver(this);
		IControlSystemService service = ServiceLocator.Instance.GetService<IControlSystemService>();
		service.AddHandler(this);
	}
}
