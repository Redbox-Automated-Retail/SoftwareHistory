using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using HALUtilities.Properties;
using Redbox.HAL.Client;
using Redbox.HAL.Core;

namespace HALUtilities;

internal class TouchScreenHelper : IDisposable
{
	private readonly AutoResetEvent SleepEvent = new AutoResetEvent(initialState: false);

	private readonly Guid AppGuid = new Guid("{D42C8D24-054C-42ff-9E8F-A7A3388D2046}");

	private readonly DeviceDescriptor Descriptor;

	private readonly DriverDescriptor DriverInfo = new DriverDescriptor(new Version("7.12.0.0"), "(3M devices)", "3M");

	private readonly Guid MouseGUID = new Guid("4D36E96F-E325-11CE-BFC1-08002BE10318");

	private bool Disposed { get; set; }

	private RedboxTimer Timer { get; set; }

	private bool MonitorRunning { get; set; }

	private NamedLock InstanceLock { get; set; }

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			if (InstanceLock != null)
			{
				InstanceLock.Dispose();
			}
			if (Timer != null)
			{
				Timer.Disable();
			}
		}
	}

	internal DialogResult Reset()
	{
		if (!DeviceExists())
		{
			return DialogResult.Abort;
		}
		return ResetInner();
	}

	internal void RunMonitor(int sleepInterval)
	{
		InstanceLock = new NamedLock(AppGuid.ToString(), LockScope.Local);
		if (!InstanceLock.IsOwned)
		{
			Console.WriteLine("There is already a running instance.");
		}
		else
		{
			if (!DeviceExists())
			{
				return;
			}
			DateTime now = DateTime.Now;
			DateRange dateRange = null;
			if (now.Hour >= 7 && now.Hour < 10)
			{
				dateRange = DateRange.FromHourSpan(7, 10);
			}
			else if (now.Hour >= 15 && now.Hour < 23)
			{
				dateRange = DateRange.FromHourSpan(15, 23);
			}
			if (dateRange == null)
			{
				LogHelper.Instance.Log("Current time is outside a polling window - exit.");
				return;
			}
			LogHelper.Instance.Log("Range is {0}", dateRange.ToString());
			ClientHelper clientHelper = new ClientHelper();
			HardwareService hardwareService = clientHelper.AutoConnect();
			if (hardwareService == null)
			{
				LogHelper.Instance.Log("Cannot connect to HAL service.");
				return;
			}
			Timer = new RedboxTimer("TS Monitor", ShutdownFire);
			Timer.FireOn(dateRange.End);
			MonitorRunning = true;
			int millisecondsTimeout = sleepInterval * 60 * 1000;
			LogHelper.Instance.Log("Touch Screen monitor start.");
			while (MonitorRunning)
			{
				if (SleepEvent.WaitOne(millisecondsTimeout))
				{
					LogHelper.Instance.Log("Monitor thread was awaken.");
					MonitorRunning = false;
					break;
				}
				HardwareCommandResult hardwareCommandResult = hardwareService.ExecuteServiceCommand("SERVICE last-user-job");
				if (!hardwareCommandResult.Success)
				{
					LogHelper.Instance.Log("Call didn't succeed.");
					MonitorRunning = false;
					break;
				}
				string text = hardwareCommandResult.CommandMessages[0];
				LogHelper.Instance.Log("Service responded with {0}", text);
				if (text == "NONE")
				{
					DialogResult dialogResult = ResetInner();
					if (dialogResult != DialogResult.OK)
					{
						if (dialogResult == DialogResult.Abort)
						{
							LogHelper.Instance.Log("The driver may be locked; the form reported an abort.");
							ServiceHelper.Instance.StopHALService(check: true);
							KillKioskEngine();
						}
						else
						{
							LogHelper.Instance.Log("The reset form reported a non-abort error.");
						}
						MonitorRunning = false;
						break;
					}
					continue;
				}
				try
				{
					DateTime value = DateTime.Parse(text);
					if (DateTime.Now.Subtract(value).Minutes > sleepInterval)
					{
						LogHelper.Instance.Log("The last job {0} {1} is more than {2} minutes ago; issue reset.", now.ToShortDateString(), now.ToShortTimeString(), sleepInterval);
						switch (ResetInner())
						{
						case DialogResult.Abort:
							LogHelper.Instance.Log("The driver may be locked; the form reported an abort.");
							ServiceHelper.Instance.StopHALService(check: true);
							KillKioskEngine();
							break;
						default:
							LogHelper.Instance.Log("The reset form reported a non-abort error.");
							break;
						case DialogResult.OK:
							goto end_IL_0249;
						}
						MonitorRunning = false;
						break;
					}
					end_IL_0249:;
				}
				catch
				{
					LogHelper.Instance.Log("Failed to parse time '{0}'.", text);
				}
			}
			LogHelper.Instance.Log("Monitor thread ending.");
		}
	}

	internal TouchScreenHelper()
	{
		LogHelper.Instance.Logger = new SimpleLogHelper("TouchScreenMonitor.log");
		Descriptor = new DeviceDescriptor("0596", "0001", "3M", DriverInfo, MouseGUID);
	}

	private bool DeviceExists()
	{
		using DeviceHelper deviceHelper = new DeviceHelper(Settings.Default.DebugHardware);
		if (!deviceHelper.MatchDriver(Descriptor, DriverInfo, MouseGUID))
		{
			LogHelper.Instance.Log("Did not find an expected touch screen type; bail.");
			return false;
		}
		return true;
	}

	private DialogResult ResetInner()
	{
		ResetForm resetForm = new ResetForm(Descriptor);
		return resetForm.ShowDialog();
	}

	private void ShutdownFire(object s, ElapsedEventArgs e)
	{
		Timer.Disable();
		LogHelper.Instance.Log("Touch screen thread shutdown timer firing.");
		MonitorRunning = false;
		SleepEvent.Set();
	}

	private Process FindRunningEngine()
	{
		string processName = "kioskengine";
		Process[] processesByName = Process.GetProcessesByName(processName);
		if (processesByName.Length == 0)
		{
			LogHelper.Instance.Log("There are no running Engine processes.");
			return null;
		}
		return processesByName[0];
	}

	private void KillKioskEngine()
	{
		if (!Settings.Default.KillEngine)
		{
			return;
		}
		try
		{
			LogHelper.Instance.Log("Kill kiosk engine!");
			Process process = FindRunningEngine();
			if (process == null)
			{
				return;
			}
			int num = User32Interop.FindWindow(null, "Redbox Kiosk Engine");
			if (num > 0)
			{
				string text = "ShutdownEngine";
				LogHelper.Instance.Log("Sending message '{0}' to engine.", text);
				byte[] bytes = Encoding.ASCII.GetBytes(text);
				User32Interop.COPYDATASTRUCT lp = new User32Interop.COPYDATASTRUCT
				{
					dwData = (IntPtr)0,
					lpData = text,
					cbData = bytes.Length + 1
				};
				int num2 = User32Interop.SendMessage(num, 74, 0, ref lp);
				LogHelper.Instance.Log("SendMessage returned {0}", num2);
			}
			using ExecutionTimer executionTimer = new ExecutionTimer();
			while (true)
			{
				Thread.Sleep(1000);
				if (FindRunningEngine() == null)
				{
					break;
				}
				if (executionTimer.ElapsedMilliseconds >= 10000)
				{
					LogHelper.Instance.Log("Wait for KE shutdown has elapsed; force kill it.");
					Process process2 = FindRunningEngine();
					if (process2 == null)
					{
						break;
					}
					try
					{
						process2.Kill();
						LogHelper.Instance.Log("Sent kill to kiosk engine.");
						break;
					}
					catch (Exception)
					{
						LogHelper.Instance.Log("The Kiosk Engine kill command failed.");
						break;
					}
				}
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("SendWindowMessage: unhandled exception.", e);
		}
	}
}
