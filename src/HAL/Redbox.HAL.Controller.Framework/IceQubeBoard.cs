using System.Threading;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public class IceQubeBoard
{
	private const string ConfigurationKey = "IceQubeExchanger";

	private bool? m_isActive = null;

	public static IceQubeBoard Instance => Singleton<IceQubeBoard>.Instance;

	public FanStatus FanState { get; private set; }

	public bool Configured
	{
		get
		{
			if (m_isActive.HasValue)
			{
				return m_isActive.Value;
			}
			m_isActive = SecretOptions.Instance.GetValue("IceQubeExchanger", defValue: false);
			if (FanState == FanStatus.Unknown)
			{
				FanState = ((!m_isActive.Value) ? FanStatus.NotConfigured : FanStatus.On);
			}
			return m_isActive.Value;
		}
		set
		{
			m_isActive = value;
			FanState = ((!m_isActive.Value) ? FanStatus.NotConfigured : FanStatus.On);
			SecretOptions.Instance.SetValue("IceQubeExchanger", m_isActive.Value);
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
			LatchOff();
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
			LatchOn();
			Thread.Sleep(10500);
		}
	}

	public void LatchOn()
	{
		if (Configured)
		{
			CoreController.ExecuteSingleControllerCommand(CommandType.IceQubeAlarmResetOn);
			FanState = FanStatus.Off;
		}
	}

	public void LatchOff()
	{
		if (Configured)
		{
			CoreController.ExecuteSingleControllerCommand(CommandType.IceQubeAlarmResetOff);
			FanState = FanStatus.On;
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
		LatchOn();
		Thread.Sleep(millisecondsTimeout);
		LatchOff();
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

	private IceQubeBoardStatus CheckStatusInner()
	{
		if (!AuxBoard.Instance.IsSensorBlocked(AuxSensor.QlmBayDoor))
		{
			return IceQubeBoardStatus.Error;
		}
		return IceQubeBoardStatus.Ok;
	}

	private IceQubeBoard()
	{
		FanState = FanStatus.Unknown;
	}
}
