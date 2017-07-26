using Redbox.HAL.Component.Model;

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
			ServiceLocator.Instance.GetService<IRuntimeService>().SpinWait(ControllerConfiguration.Instance.FraudSensorPostWait);
		}
	}

	public void NotifyConfigurationChangeEnd()
	{
		LogHelper.Instance.Log("[DiskValidatorService] NotifyConfigurationEnd.");
		IsConfigured = ControllerConfiguration.Instance.EnableSecureDiskValidator;
		if (IsConfigured)
		{
			IFormattedLog nilLog = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>().NilLog;
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
			IFormattedLog nilLog = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>().NilLog;
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
		if (!IsConfigured)
		{
			return new FraudScanResult
			{
				ScanError = ErrorCodes.DeviceNotConfigured,
				Valid = false
			};
		}
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
		ServiceLocator.Instance.GetService<IControlSystemService>().AddHandler(this);
	}
}
