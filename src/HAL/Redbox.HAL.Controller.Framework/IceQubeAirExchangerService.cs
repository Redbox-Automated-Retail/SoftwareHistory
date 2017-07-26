using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal class IceQubeAirExchangerService : IAirExchangerService, IConfigurationObserver
{
	private const string ResetFailures = "IceQubeResetFailures";

	public bool Configured { get; private set; }

	public ExchangerFanStatus FanStatus { get; private set; }

	public void NotifyConfigurationChangeStart()
	{
		LogHelper.Instance.Log("[IceQubeService] Configuration change start.");
		FanStatus = ExchangerFanStatus.Unknown;
	}

	public void NotifyConfigurationChangeEnd()
	{
		LogHelper.Instance.Log("[IceQubeService] Configuration change end.");
		Configured = ControllerConfiguration.Instance.EnableIceQubePolling;
		FanStatus = ((!Configured) ? ExchangerFanStatus.NotConfigured : ExchangerFanStatus.On);
		if (!Configured)
		{
			TurnOnFan();
		}
	}

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[IceQubeService] Configuration loaded.");
		Configured = ControllerConfiguration.Instance.EnableIceQubePolling;
		FanStatus = ((!Configured) ? ExchangerFanStatus.NotConfigured : ExchangerFanStatus.On);
	}

	public AirExchangerStatus CheckStatus()
	{
		if (Configured)
		{
			return CheckStatusInner();
		}
		return AirExchangerStatus.NotConfigured;
	}

	public void TurnOnFan()
	{
		if (Configured)
		{
			if (ExchangerFanStatus.On == FanStatus)
			{
				LogHelper.Instance.Log("Fan is already on - bypass.");
				return;
			}
			ToggleLatch(on: false);
			Thread.Sleep(500);
			Reset();
		}
	}

	public void TurnOffFan()
	{
		if (Configured)
		{
			if (ExchangerFanStatus.Off == FanStatus)
			{
				LogHelper.Instance.Log("Fan is already off - bypass.");
				return;
			}
			ToggleLatch(on: true);
			Thread.Sleep(10500);
		}
	}

	public void LatchOn()
	{
		if (Configured)
		{
			ToggleLatch(on: true);
		}
	}

	public void LatchOff()
	{
		if (Configured)
		{
			ToggleLatch(on: false);
		}
	}

	public AirExchangerStatus Reset()
	{
		if (!Configured)
		{
			return AirExchangerStatus.NotConfigured;
		}
		ToggleLatch(on: true);
		Thread.Sleep(1500);
		ToggleLatch(on: false);
		Thread.Sleep(1500);
		return CheckStatusInner();
	}

	public int PersistentFailureCount()
	{
		IPersistentCounter persistentCounter = ServiceLocator.Instance.GetService<IPersistentCounterService>().Find("IceQubeResetFailures");
		if (persistentCounter == null)
		{
			LogHelper.Instance.Log("Unable to create IceQube counter.", LogEntryType.Error);
			return 0;
		}
		return persistentCounter.Value;
	}

	public bool ShouldRetry()
	{
		IPersistentCounter persistentCounter = ServiceLocator.Instance.GetService<IPersistentCounterService>().Find("IceQubeResetFailures");
		if (persistentCounter == null)
		{
			return false;
		}
		return persistentCounter.Value < 3;
	}

	public void ResetFailureCount()
	{
		LogHelper.Instance.Log("IceQubeBoard reset failure counter.");
		ServiceLocator.Instance.GetService<IPersistentCounterService>().Reset("IceQubeResetFailures");
	}

	public void IncrementResetFailureCount()
	{
		ServiceLocator.Instance.GetService<IPersistentCounterService>().Increment("IceQubeResetFailures");
	}

	internal IceQubeAirExchangerService()
	{
		FanStatus = ExchangerFanStatus.NotConfigured;
		ControllerConfiguration.Instance.AddObserver(this);
	}

	private void ToggleLatch(bool on)
	{
		ServiceLocator.Instance.GetService<ICoreCommandExecutor>().ExecuteControllerCommand(on ? CommandType.QlmDoorUnlock : CommandType.QlmDoorLock);
		FanStatus = (on ? ExchangerFanStatus.Off : ExchangerFanStatus.On);
	}

	private AirExchangerStatus CheckStatusInner()
	{
		CoreResponse coreResponse = ServiceLocator.Instance.GetService<ICoreCommandExecutor>().ExecuteControllerCommand(CommandType.AuxSensorsRead);
		if (coreResponse.CommError)
		{
			return AirExchangerStatus.Error;
		}
		if (!coreResponse.IsSensorSet(AuxSensor.QlmBayDoor))
		{
			return AirExchangerStatus.Error;
		}
		return AirExchangerStatus.Ok;
	}
}
