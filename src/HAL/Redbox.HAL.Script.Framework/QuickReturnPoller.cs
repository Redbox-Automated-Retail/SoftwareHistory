using System;
using System.Threading;
using System.Timers;
using Redbox.HAL.Camera.Framework;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class QuickReturnPoller
{
	private enum State
	{
		None,
		Busy,
		Polling
	}

	private object m_pollerStateBarrier = new object();

	private bool m_pollerEnabled;

	internal static QuickReturnPoller Instance => Singleton<QuickReturnPoller>.Instance;

	internal bool QuickReturnPollerEnabled
	{
		get
		{
			lock (m_pollerStateBarrier)
			{
				return m_pollerEnabled;
			}
		}
		private set
		{
			lock (m_pollerStateBarrier)
			{
				m_pollerEnabled = value;
			}
		}
	}

	private bool QuickReturnConfigured => QuickReturnDevice.Instance.IsConfigured;

	private RedboxTimer PowerOnTimer { get; set; }

	private RedboxTimer PowerOffTimer { get; set; }

	private bool PowerOffConfigured { get; set; }

	private int QRPollInterval { get; set; }

	private Thread PollerThread { get; set; }

	private ManualResetEvent WaitEvent { get; set; }

	private ManualResetEvent JobCompletionHandle { get; set; }

	private State PollState { get; set; }

	private DateRange PowerOffWindow { get; set; }

	internal void Startup()
	{
		if (QuickReturnConfigured && !QuickReturnPollerEnabled)
		{
			PollState = State.None;
			ConfigureWindow();
			CreatePollerThread();
		}
	}

	internal void Shutdown()
	{
		if (QuickReturnConfigured && QuickReturnPollerEnabled)
		{
			QuickReturnPollerEnabled = false;
			if (PollerThread != null && !PollerThread.Join(5000))
			{
				LogHelper.Instance.Log("QuickReturn poller thread did not join after wait.");
			}
			LogHelper.Instance.Log("Shutdown QR device; disabling timer.");
			PowerOnTimer.Disable();
			PowerOffTimer.Disable();
		}
	}

	private void ProcessQRTerminal()
	{
		if (PowerOffConfigured)
		{
			DateTime now = DateTime.Now;
			if (PowerOffWindow.Includes(now))
			{
				LogHelper.Instance.Log("Current time is within power window of {0}; shutdown device.", PowerOffWindow.ToString());
				PowerOnTimer.FireOn(PowerOffWindow.End);
				QuickReturnPollerEnabled = false;
				return;
			}
			PowerOffTimer.FireOn(PowerOffWindow.Start);
		}
		try
		{
			ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob("quick-return-setup", "Auto-scheduled setup", DateTime.Now, ProgramPriority.High);
			executionContext.Resume();
			while (!executionContext.IsComplete)
			{
				Thread.Sleep(1000);
			}
			if (executionContext.Status != ExecutionContextStatus.Completed)
			{
				LogHelper.Instance.Log("The quick-return setup failed ...", LogEntryType.Error);
				return;
			}
			while (QuickReturnPollerEnabled)
			{
				if (CameraService.Instance.CameraInErroredState)
				{
					LogHelper.Instance.Log("Quick return poller believes camera is down; shutting down poller.", LogEntryType.Error);
					QuickReturnPollerEnabled = false;
				}
				if (ControllerService.Instance.MachineIsFull())
				{
					DisableUI();
					Thread.Sleep(QRPollInterval);
					continue;
				}
				if (ExecutionEngine.Instance.ActiveContext != null)
				{
					if (PollState != State.Busy)
					{
						State pollState = PollState;
						PollState = State.Busy;
						if (!QuickReturnDevice.Instance.DisplayState("Busy"))
						{
							LogHelper.Instance.Log("The Busy state could not be set.", LogEntryType.Error);
							QuickReturnPollerEnabled = false;
							break;
						}
						if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
						{
							LogHelper.Instance.Log($"State trans: {pollState.ToString()} -> {PollState.ToString()}", LogEntryType.Info);
						}
					}
				}
				else
				{
					if (State.Polling != PollState)
					{
						State pollState2 = PollState;
						PollState = State.Polling;
						if (!QuickReturnDevice.Instance.DisplayState("Start"))
						{
							LogHelper.Instance.Log("The Start state could not be set.", LogEntryType.Error);
							break;
						}
						QuickReturnDevice.Instance.ClearAttentionButtonStatus();
						if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
						{
							LogHelper.Instance.Log($"State trans: {pollState2.ToString()} -> {PollState.ToString()}", LogEntryType.Info);
						}
					}
					bool flag = QuickReturnDevice.Instance.CheckAttButton(out var commError);
					if (commError)
					{
						LogHelper.Instance.Log("Unable to check the button.", LogEntryType.Error);
						QuickReturnPollerEnabled = false;
						break;
					}
					if (flag)
					{
						if (!QuickReturnDevice.Instance.DisplayState("Wait"))
						{
							LogHelper.Instance.Log("The Wait state could not be set.", LogEntryType.Error);
							QuickReturnPollerEnabled = false;
							break;
						}
						ScheduleAndBlockOnJob();
						PollState = State.None;
					}
					if (!QuickReturnDevice.Instance.ClearAttentionButtonStatus())
					{
						LogHelper.Instance.Log("Unable to clear the attention button.", LogEntryType.Error);
						QuickReturnPollerEnabled = false;
						break;
					}
				}
				Thread.Sleep(QRPollInterval);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("QuickReturnPoller: caught an exception - shutdown.", e);
		}
		finally
		{
			QuickReturnDevice.Instance.ClosePort();
			LogHelper.Instance.Log("Quick Return Poller thread exiting ...", LogEntryType.Info);
			PowerOnTimer.FireOn(PowerOffWindow.End);
		}
	}

	private void ConfigureWindow()
	{
		PowerOffConfigured = false;
		if (!QuickReturnConfigured)
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
			PowerOffConfigured = true;
			PowerOffWindow = DateRange.FromHourSpan(result, result2);
			LogHelper.Instance.Log("QuickReturnPoller: Power off window {0}", PowerOffWindow.ToString());
		}
	}

	private void PowerOnDevice(object source, ElapsedEventArgs e)
	{
		PowerOnTimer.Disable();
		PowerOffWindow.ShiftOneDay();
		LogHelper.Instance.Log("Quick return timer - restart device: new window is {0}", PowerOffWindow.ToString());
		CreatePollerThread();
	}

	private void PowerOffDevice(object source, ElapsedEventArgs e)
	{
		LogHelper.Instance.Log("Quick return timer: shut down device.");
		PowerOffTimer.Disable();
		QuickReturnPollerEnabled = false;
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

	private void HandleStatusChange(ExecutionContextStatus status)
	{
		switch (status)
		{
		case ExecutionContextStatus.Errored:
			QuickReturnPollerEnabled = false;
			JobCompletionHandle.Set();
			break;
		case ExecutionContextStatus.Completed:
			JobCompletionHandle.Set();
			break;
		}
	}

	private void ScheduleAndBlockOnJob()
	{
		ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob("quick-return", "Quick return process", DateTime.Now, ProgramPriority.Highest);
		executionContext.StatusChangeCallback = HandleStatusChange;
		executionContext.Resume();
		JobCompletionHandle.WaitOne();
		executionContext.StatusChangeCallback = null;
		JobCompletionHandle.Reset();
	}

	private void CreatePollerThread()
	{
		JobCompletionHandle.Reset();
		if (!QuickReturnPollerEnabled)
		{
			PollState = State.None;
			QuickReturnPollerEnabled = true;
			PollerThread = new Thread(ProcessQRTerminal)
			{
				Name = "Quick Return Poller",
				IsBackground = true
			};
			PollerThread.SetApartmentState(ApartmentState.STA);
			PollerThread.Start();
		}
	}

	private QuickReturnPoller()
	{
		QRPollInterval = 1000;
		WaitEvent = new ManualResetEvent(initialState: false);
		JobCompletionHandle = new ManualResetEvent(initialState: false);
		PollState = State.None;
		PowerOnTimer = new RedboxTimer("Power On QR", PowerOnDevice);
		PowerOffTimer = new RedboxTimer("Power Off QR", PowerOffDevice);
	}
}
