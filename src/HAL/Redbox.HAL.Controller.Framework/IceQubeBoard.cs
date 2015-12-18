using System.Threading;
using Redbox.HAL.Core;
using Redbox.HAL.DataStorage;

namespace Redbox.HAL.Controller.Framework;

public sealed class IceQubeBoard
{
	public static IceQubeBoard Instance => Singleton<IceQubeBoard>.Instance;

	public FanStatus FanState { get; private set; }

	public bool Configured
	{
		get
		{
			return ControllerConfiguration.Instance.EnableIceQubePolling;
		}
		set
		{
			ControllerConfiguration.Instance.SetPropertyByName("EnableIceQubePolling", new object[1] { value });
		}
	}

	public IceQubeBoardStatus CheckStatus()
	{
		if (Configured)
		{
			return CheckStatusInner();
		}
		return IceQubeBoardStatus.NotConfigured;
	}

	public void TurnOnFan()
	{
		if (Configured)
		{
			if (FanStatus.On == FanState)
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
			if (FanStatus.Off == FanState)
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

	public IceQubeBoardStatus Reset()
	{
		if (ControllerMode.Emulated == ControllerConfiguration.Instance.Mode)
		{
			return IceQubeBoardStatus.Error;
		}
		if (!Configured)
		{
			return IceQubeBoardStatus.NotConfigured;
		}
		int millisecondsTimeout = 1500;
		ToggleLatch(on: true);
		Thread.Sleep(millisecondsTimeout);
		ToggleLatch(on: false);
		Thread.Sleep(millisecondsTimeout);
		return CheckStatusInner();
	}

	public int PersistentFailureCount()
	{
		Counter counter = CounterManager.Instance.Find("IceQubeResetFailures");
		if (counter == null)
		{
			LogHelper.Instance.Log("Unable to create IceQube counter.", LogEntryType.Error);
			return 0;
		}
		return counter.Value;
	}

	public bool ShouldRetry()
	{
		Counter counter = CounterManager.Instance.Find("IceQubeResetFailures");
		if (counter == null)
		{
			return false;
		}
		return counter.Value < 3;
	}

	public void ResetFailureCount()
	{
		LogHelper.Instance.Log("IceQubeBoard reset failure counter.", LogEntryType.Info);
		CounterManager.Instance.Reset("IceQubeResetFailures");
	}

	public void IncrementResetFailureCount()
	{
		CounterManager.Instance.Increment("IceQubeResetFailures");
	}

	public void ConfigurationUpdateFinish()
	{
		FanState = ((!Configured) ? FanStatus.NotConfigured : FanStatus.On);
	}

	public void ConfigurationChangeStart()
	{
		FanState = FanStatus.Unknown;
	}

	private void ToggleLatch(bool on)
	{
		ControlSystem.Instance.ExecuteControllerCommand(on ? CommandType.QlmDoorUnlock : CommandType.QlmDoorLock);
		FanState = (on ? FanStatus.Off : FanStatus.On);
	}

	private IceQubeBoardStatus CheckStatusInner()
	{
		CoreResponse coreResponse = ControlSystem.Instance.ExecuteControllerCommand(CommandType.AuxSensorsRead);
		if (coreResponse.CommunicationError)
		{
			return IceQubeBoardStatus.Error;
		}
		if (!coreResponse.IsSensorSet(AuxSensor.QlmBayDoor))
		{
			return IceQubeBoardStatus.Error;
		}
		return IceQubeBoardStatus.Ok;
	}

	private IceQubeBoard()
	{
		FanState = ((!ControllerConfiguration.Instance.EnableIceQubePolling) ? FanStatus.NotConfigured : FanStatus.On);
	}
}
