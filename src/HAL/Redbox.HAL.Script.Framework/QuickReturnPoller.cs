using System;
using System.Threading;
using System.Timers;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Poller(Name = "QuickReturn")]
internal sealed class QuickReturnPoller : ScriptPoller, IConfigurationObserver
{
	private enum State
	{
		None,
		Busy,
		Polling
	}

	private readonly RedboxTimer PowerOnTimer;

	private readonly RedboxTimer PowerOffTimer;

	private State PollerState;

	protected override string ThreadName => "Quick Return Poller";

	protected override bool IsConfigured => QuickReturnDevice.Instance.IsConfigured;

	protected override string JobName => "quick-return";

	protected override string JobLabel => "Quick return process";

	protected override ProgramPriority Priority => ProgramPriority.Highest;

	protected override int PollSleep => 1000;

	internal static QuickReturnPoller Instance => ServiceLocator.Instance.GetService<IPollerService>().Locate("QuickReturn") as QuickReturnPoller;

	private DateRange PowerOffWindow { get; set; }

	public void NotifyConfigurationLoaded()
	{
	}

	public void NotifyConfigurationChangeStart()
	{
		LogHelper.Instance.Log("[QuickReturnPoller] ConfigurationChangeStart");
		Shutdown();
		PowerOffTimer.Disable();
		PowerOnTimer.Disable();
	}

	public void NotifyConfigurationChangeEnd()
	{
		LogHelper.Instance.Log("[QuickReturnPoller] ConfigurationChangeEnd");
		QuickReturnDevice.Instance.ConfigurationChange();
		if (EngineModes.Normal == ExecutionEngine.Instance.Mode)
		{
			Start();
			ConfigureTimers();
		}
	}

	protected override bool OnPollStart()
	{
		DateTime now = DateTime.Now;
		if (PowerOffWindow != null && PowerOffWindow.Includes(now))
		{
			LogHelper.Instance.Log("Current time is within power window of {0}; shutdown device.", PowerOffWindow.ToString());
			return false;
		}
		ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob("quick-return-setup", "Auto-scheduled setup", DateTime.Now, ProgramPriority.High);
		executionContext.Resume();
		while (!executionContext.IsComplete)
		{
			Thread.Sleep(1000);
		}
		if (executionContext.Status != ExecutionContextStatus.Completed)
		{
			LogHelper.Instance.Log("The quick-return setup failed ...", LogEntryType.Error);
			return false;
		}
		return true;
	}

	protected override void OnPollDone()
	{
		PollerState = State.None;
		QuickReturnDevice.Instance.ClosePort();
	}

	protected override bool CoreExecute()
	{
		try
		{
			if (ServiceLocator.Instance.GetService<IScannerDeviceService>().GetConfiguredDevice().InError)
			{
				LogHelper.Instance.Log("Quick return poller believes camera is down; shutting down poller.", LogEntryType.Error);
				return false;
			}
			if (ServiceLocator.Instance.GetService<IInventoryService>().MachineIsFull())
			{
				DisableUI();
				return true;
			}
			if (ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext() != null)
			{
				if (PollerState != State.Busy)
				{
					State pollerState = PollerState;
					PollerState = State.Busy;
					if (!QuickReturnDevice.Instance.DisplayState("Busy"))
					{
						LogHelper.Instance.Log("The Busy state could not be set.", LogEntryType.Error);
						return false;
					}
					if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
					{
						LogHelper.Instance.Log($"State trans: {pollerState.ToString()} -> {PollerState.ToString()}", LogEntryType.Info);
					}
				}
			}
			else
			{
				if (State.Polling != PollerState)
				{
					State pollerState2 = PollerState;
					PollerState = State.Polling;
					if (!QuickReturnDevice.Instance.DisplayState("Start"))
					{
						LogHelper.Instance.Log("The Start state could not be set.", LogEntryType.Error);
						return false;
					}
					QuickReturnDevice.Instance.ClearAttentionButtonStatus();
					if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
					{
						LogHelper.Instance.Log($"State trans: {pollerState2.ToString()} -> {PollerState.ToString()}", LogEntryType.Info);
					}
				}
				bool flag = QuickReturnDevice.Instance.CheckAttButton(out var commError);
				if (commError)
				{
					LogHelper.Instance.Log("Unable to check the button.", LogEntryType.Error);
					return false;
				}
				if (flag)
				{
					if (!QuickReturnDevice.Instance.DisplayState("Wait"))
					{
						LogHelper.Instance.Log("The Wait state could not be set.", LogEntryType.Error);
						return false;
					}
					ExecutionContext executionContext = ScheduleAndBlockOnJob();
					PollerState = State.None;
					if (ExecutionContextStatus.Errored == executionContext.Status.Value)
					{
						LogHelper.Instance.Log(LogEntryType.Error, "quick-return job ( ID {0} ) errored; halt polling.", executionContext.ID);
						return false;
					}
				}
				if (!QuickReturnDevice.Instance.ClearAttentionButtonStatus())
				{
					LogHelper.Instance.Log("Unable to clear the attention button.", LogEntryType.Error);
					return false;
				}
			}
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("QuickReturnPoller: caught an exception - shutdown.", e);
			return false;
		}
	}

	internal override void Initialize()
	{
		ServiceLocator.Instance.GetService<IConfigurationService>().FindConfiguration(Configurations.Controller).AddObserver(this);
		ServiceLocator.Instance.GetService<IExecutionService>().AddObserver(this);
		ConfigureTimers();
	}

	private void PowerOnDevice(object source, ElapsedEventArgs e)
	{
		LogHelper.Instance.Log("Quick return timer - restart device.");
		PowerOnTimer.Disable();
		PowerOnTimer.ScheduleAtNext(PowerOffWindow.End.Hour, 0);
		Start();
	}

	private void PowerOffDevice(object source, ElapsedEventArgs e)
	{
		LogHelper.Instance.Log("Quick return timer: shut down device.");
		Shutdown();
		PowerOffTimer.Disable();
		PowerOffTimer.ScheduleAtNext(PowerOffWindow.Start.Hour, 0);
	}

	private int DisableUI()
	{
		int num = 0;
		try
		{
			if (!QuickReturnDevice.Instance.TurnOffLED(QuickReturnDevice.LED.RedButton))
			{
				num++;
			}
			if (!QuickReturnDevice.Instance.TurnOffLED(QuickReturnDevice.LED.RedArrow))
			{
				num++;
			}
			if (!QuickReturnDevice.Instance.TurnOffLED(QuickReturnDevice.LED.GreenButton))
			{
				num++;
			}
			if (!QuickReturnDevice.Instance.TurnOffLED(QuickReturnDevice.LED.GreenArrow))
			{
				num++;
			}
			if (!QuickReturnDevice.Instance.TurnOffLED(QuickReturnDevice.LED.Background))
			{
				num++;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("QuickReturnPoller:DisableQRUI - unhandled exception.", e);
		}
		return num;
	}

	private void ConfigureTimers()
	{
		if (!IsConfigured)
		{
			return;
		}
		string quickReturnPowerOffWindow = ControllerConfiguration.Instance.QuickReturnPowerOffWindow;
		if (!string.IsNullOrEmpty(quickReturnPowerOffWindow) && !(quickReturnPowerOffWindow == "NONE"))
		{
			string[] array = quickReturnPowerOffWindow.Split(':');
			if (array.Length != 2)
			{
				LogHelper.Instance.Log("The power configuration option is malformed.");
				return;
			}
			if (!int.TryParse(array[0], out var result) || !int.TryParse(array[1], out var result2))
			{
				LogHelper.Instance.Log("The power configuration option is malformed.");
				return;
			}
			if (result == result2)
			{
				LogHelper.Instance.Log("The power configuration option is malformed; start matches end.");
				return;
			}
			PowerOffWindow = DateRange.FromHourSpan(result, result2);
			LogHelper.Instance.Log("QuickReturnPoller: Power off window {0}", PowerOffWindow.ToString());
			PowerOnTimer.ScheduleAtNext(PowerOffWindow.End.Hour, 0);
			PowerOffTimer.ScheduleAtNext(PowerOffWindow.Start.Hour, 0);
		}
	}

	private QuickReturnPoller()
	{
		PowerOnTimer = new RedboxTimer("Power On QR", PowerOnDevice);
		PowerOffTimer = new RedboxTimer("Power Off QR", PowerOffDevice);
	}
}
