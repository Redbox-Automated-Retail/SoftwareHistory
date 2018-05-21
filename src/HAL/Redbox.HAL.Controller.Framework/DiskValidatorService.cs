using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class DiskValidatorService : IDiskValidatorService, IConfigurationObserver
{
	internal sealed class FraudScanResult : IFraudScanResult
	{
		public bool Valid { get; internal set; }

		public ErrorCodes ScanError { get; internal set; }

		internal FraudScanResult()
		{
			Valid = false;
			ScanError = ErrorCodes.Success;
		}
	}

	private class NilFraudSensor : IFraudSensor
	{
		private readonly DiskValidatorService Validator;

		public ErrorCodes LastPostResult => ErrorCodes.DeviceNotConfigured;

		public ErrorCodes Initialize(IFormattedLog log)
		{
			return ErrorCodes.DeviceNotConfigured;
		}

		public ErrorCodes Shutdown()
		{
			return ErrorCodes.DeviceNotConfigured;
		}

		public ErrorCodes Reset(IFormattedLog log)
		{
			return ErrorCodes.DeviceNotConfigured;
		}

		public ErrorCodes StartScan()
		{
			return ErrorCodes.DeviceNotConfigured;
		}

		public ErrorCodes StopScan()
		{
			return ErrorCodes.DeviceNotConfigured;
		}

		public IFraudScanResult Validate()
		{
			return Validator.OnlyResult;
		}

		internal NilFraudSensor(DiskValidatorService s)
		{
			Validator = s;
		}
	}

	private readonly FraudScanResult OnlyResult = new FraudScanResult
	{
		ScanError = ErrorCodes.DeviceNotConfigured,
		Valid = false
	};

	private int m_persistentPostFailureCount;

	public bool IsConfigured => false;

	public IFraudSensor FraudSensor { get; private set; }

	private bool CorrectFailedFraudPost => false;

	public void NotifyConfigurationChangeStart()
	{
		LogHelper.Instance.Log("[DiskValidatorService] NotifyConfigurationStart.");
		if (IsConfigured)
		{
			FraudSensor.Shutdown();
		}
	}

	public void NotifyConfigurationChangeEnd()
	{
		LogHelper.Instance.Log("[DiskValidatorService] NotifyConfigurationEnd.");
		if (IsConfigured)
		{
			IFormattedLog nilLog = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>().NilLog;
			ErrorCodes errorCodes = FraudSensor.Initialize(nilLog);
			LogHelper.Instance.Log("[DiskValidator] Fraud sensor initialization returned {0}", errorCodes.ToString());
		}
	}

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[DiskValidatorService] ConfigurationLoad: Enabled = {0}.", IsConfigured.ToString());
	}

	public IFraudScanResult Validate(string matrix)
	{
		if (!IsConfigured)
		{
			return OnlyResult;
		}
		return FraudSensor.Validate();
	}

	public void CorrectErroredPost(IFormattedLog log)
	{
		if (!IsConfigured || FraudSensor.LastPostResult == ErrorCodes.Success)
		{
			m_persistentPostFailureCount = 0;
		}
		else if (CorrectFailedFraudPost && m_persistentPostFailureCount <= 2)
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

	internal DiskValidatorService()
	{
		ControllerConfiguration.Instance.AddObserver(this);
		FraudSensor = new NilFraudSensor(this);
	}
}
