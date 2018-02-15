using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.ControlPanel.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "ControlPanelViewUserControl")]
public partial class ControlPanelViewUserControl : System.Windows.Controls.UserControl, IComponentConnector
{
	private bool _maintenancemodestate;

	private DateTimeFormatInfo _fmt = new CultureInfo("hr-HR").DateTimeFormat;

	private DispatcherTimer _idleTimer = new DispatcherTimer(DispatcherPriority.Background);

	public ControlPanelViewUserControl()
	{
		InitializeComponent();
		UpdateEloPcapStatus();
		UpdateWatchDogStatus();
		SetInfoLabel();
		GetMaintenanceMode();
		UpdateCloseKioskStatus();
		UpdateTouchScreenStatus();
		SetOutOfOrderMode();
		UpdateHalServiceStatus();
		if (Environment.OSVersion.Version.Major >= 6)
		{
			hyper_terminal_button.Visibility = Visibility.Hidden;
		}
		_idleTimer.Interval = TimeSpan.FromSeconds(1.0);
		_idleTimer.IsEnabled = true;
		close_button.Focusable = false;
		base.Loaded += delegate
		{
			start_kiosk_button.Focus();
		};
		ProcessFunctions.timeOfCPStart = DateTime.Now;
		_idleTimer.Tick += delegate
		{
			UpdateTime();
		};
	}

	private void SetInfoLabel()
	{
		long? kioskId = ProcessFunctions.kioskId;
		lblKioskId.Content = kioskId.ToString();
		lblDate.Content = DateTime.Today.ToShortDateString();
		string timeZoneName = (TimeZone.CurrentTimeZone.IsDaylightSavingTime(DateTime.Now) ? TimeZone.CurrentTimeZone.DaylightName : TimeZone.CurrentTimeZone.StandardName);
		timeZoneName = GetTzAbbreviation(timeZoneName);
		lblTimeZone.Content = timeZoneName;
		lblTime.Content = DateTime.Now.ToString("T", _fmt);
	}

	private void UpdateTime()
	{
		if (DateTime.Now.Subtract(ProcessFunctions.timeOfCPStart).TotalMinutes > 1.0)
		{
			if (!ProcessFunctions.ListRunningProcesses().Any((KeyValuePair<int, ProcessFunctions.ProcessDetails> x) => string.Compare(x.Value.Name, "MSHALTESTER", ignoreCase: true) == 0))
			{
				CloseControlPanel();
			}
			else
			{
				ProcessFunctions.timeOfCPStart = DateTime.Now;
			}
		}
		lblTime.Content = DateTime.Now.ToString("T", _fmt);
	}

	private void CloseControlPanel()
	{
		System.Windows.Forms.Application.OpenForms["SecureBrowser"]?.Close();
		List<int> list = (from x in ProcessFunctions.ListRunningProcesses()
			where string.Compare(x.Value.Name, "MSHALTESTER", ignoreCase: true) == 0
			select x)?.Select((KeyValuePair<int, ProcessFunctions.ProcessDetails> x) => x.Value.Pid).ToList();
		if (list != null && list.Any())
		{
			ProcessFunctions.KillProcesses(list);
		}
		try
		{
			CloseKiosk.Visibility = Visibility.Hidden;
			IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
			if (_idleTimer != null)
			{
				_idleTimer.Stop();
			}
			service.ActivateDefaultBundle();
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("Unexpected exception closing Control Panel window. ", ex.Message);
		}
	}

	private void GetMaintenanceMode()
	{
		IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
		string path = "Field Maintenance";
		string key = "MaintenanceMode";
		bool.TryParse(service.GetValue<string>(path, key), out _maintenancemodestate);
		string text = toggle_maintenance_mode_button.Content.ToString();
		text = ((!_maintenancemodestate) ? text.Replace("Off", "On") : text.Replace("On", "Off"));
		toggle_maintenance_mode_button.Content = text;
	}

	private void SetMaintenanceMode()
	{
		IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
		string path = "Field Maintenance";
		string key = "MaintenanceMode";
		service.SetValue(path, key, _maintenancemodestate.ToString().ToLower());
		string text = toggle_maintenance_mode_button.Content.ToString();
		text = ((!_maintenancemodestate) ? text.Replace("Off", "On") : text.Replace("On", "Off"));
		toggle_maintenance_mode_button.Content = text;
	}

	private void SetOutOfOrderMode()
	{
		string text = toggle_out_of_order_button.Content.ToString();
		if (ProcessFunctions.GetMachineSettingValue("Store", "OutOfOrder", "false") == "true")
		{
			toggle_out_of_order_button.Content = text.Replace("On", "Off");
			out_of_order_message.Visibility = Visibility.Visible;
		}
		else
		{
			toggle_out_of_order_button.Content = text.Replace("Off", "On");
			out_of_order_message.Visibility = Visibility.Hidden;
		}
	}

	private void ToggleOutOforder()
	{
		if (ProcessFunctions.GetMachineSettingValue("Store", "OutOfOrder", "false") == "true")
		{
			ProcessFunctions.SetMachineSettingValue("Store", "OutOfOrder", "false");
			ProcessFunctions.HardwareNotifyMaintModeState(goingIntoMM: false);
		}
		else
		{
			ProcessFunctions.EnqueueStatistic("MinutesInMaintenance", "0");
			ProcessFunctions.SetMachineSettingValue("Store", "OutOfOrder", "true");
			ProcessFunctions.HardwareNotifyMaintModeState(goingIntoMM: true);
		}
		SetOutOfOrderMode();
	}

	private void UpdateCloseKioskStatus()
	{
		if (ProcessFunctions.ListRunningProcesses().Any((KeyValuePair<int, ProcessFunctions.ProcessDetails> rp) => rp.Value.Name == "halsvc-win32"))
		{
			close_kiosk_button.Foreground = Brushes.White;
			close_kiosk_button.IsEnabled = true;
		}
		else
		{
			close_kiosk_button.Foreground = Brushes.Gray;
			close_kiosk_button.IsEnabled = false;
		}
	}

	private void UpdateTouchScreenStatus()
	{
		switch (ProcessFunctions.TouchScreenStatus())
		{
		case "Enabled":
			toggle_touch_screen_button.Content = "Turn Off\nTouchscreen";
			break;
		case "Disabled":
			toggle_touch_screen_button.Content = "Turn On\nTouchscreen";
			break;
		case "Unavailable":
			toggle_touch_screen_button.Content = "Touchscreen\nUnavailable";
			break;
		}
	}

	private void UpdateHalServiceStatus()
	{
		switch (ProcessFunctions.GetServiceStatus("halsvc$default"))
		{
		case "Running":
			ProcessFunctions.StopTimer("hal_status_timer");
			ProcessFunctions.RemoveTimer("hal_status_timer");
			toggle_hal_service_button.Content = "Stop HAL\nService";
			toggle_hal_service_button.IsEnabled = true;
			break;
		case "Stopped":
			ProcessFunctions.StopTimer("hal_status_timer");
			ProcessFunctions.RemoveTimer("hal_status_timer");
			toggle_hal_service_button.Content = "Start HAL\nService";
			toggle_hal_service_button.IsEnabled = true;
			break;
		case "StartPending":
			toggle_hal_service_button.Content = "Starting HAL\nService";
			toggle_hal_service_button.IsEnabled = false;
			break;
		case "StopPending":
			toggle_hal_service_button.Content = "Stopping HAL\nService";
			toggle_hal_service_button.IsEnabled = false;
			break;
		default:
			toggle_hal_service_button.IsEnabled = false;
			break;
		}
	}

	private void UpdateEloPcapStatus()
	{
		elo_pcap_control_button.Visibility = Visibility.Hidden;
		string text = ProcessFunctions.TouchScreenDriver();
		if (!string.IsNullOrEmpty(text) && ProcessFunctions.DoesFileExist(text))
		{
			elo_pcap_control_button.Visibility = Visibility.Visible;
		}
	}

	private void UpdateWatchDogStatus()
	{
		string text = watch_dog_toggle_button.Content.ToString();
		bool.TryParse(ProcessFunctions.GetMachineSettingValue("Store", "WatchDogEnabled", "true"), out var result);
		if (result)
		{
			watch_dog_toggle_button.Content = text.Replace("On", "Off");
		}
		else
		{
			watch_dog_toggle_button.Content = text.Replace("Off", "On");
		}
	}

	private string GetTzAbbreviation(string timeZoneName)
	{
		string text = string.Empty;
		string[] array = timeZoneName.Split(' ');
		foreach (string text2 in array)
		{
			text = ((text2[0] == '(') ? (text + text2) : (text + text2[0]));
		}
		return text;
	}

	private void LogErrorsFromList(ErrorList errors)
	{
		if (errors == null)
		{
			return;
		}
		foreach (Redbox.KioskEngine.ComponentModel.Error error in errors)
		{
			LogHelper.Instance.Log(error.Description);
		}
	}

	private void RebootButtonClick(object sender, RoutedEventArgs e)
	{
		try
		{
			if (System.Windows.MessageBox.Show("Are you sure you want to reboot the kiosk?", "Redbox Control Panel", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
			{
				ProcessFunctions.LaunchProcess("shutdown.exe", "-r -t 0", hiddenProcess: false);
			}
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("Unexpected exception in Control Panel, Reboot Kiosk Button click. ", e2);
		}
	}

	private void OpenCdriveButtonClick(object sender, RoutedEventArgs e)
	{
		open_c_drive_button.Focus();
		ProcessFunctions.LaunchProcess("explorer.exe", "C:\\", hiddenProcess: false);
	}

	private void OpenCommandPromptButtonClick(object sender, RoutedEventArgs e)
	{
		ProcessFunctions.LaunchProcess("cmd.exe", "/K cd c:\\program files", hiddenProcess: false);
	}

	private void RedboxFolderButtonClick(object sender, RoutedEventArgs e)
	{
		ProcessFunctions.LaunchProcess("explorer.exe", "C:\\Program Files\\Redbox", hiddenProcess: false);
	}

	private void NotepadButtonClick(object sender, RoutedEventArgs e)
	{
		ProcessFunctions.LaunchProcess("notepad.exe", null, hiddenProcess: false);
	}

	private void OpenHalServiceLogsButtonClick(object sender, RoutedEventArgs e)
	{
		ProcessFunctions.LaunchProcess("explorer.exe", "C:\\Program Files\\Redbox\\KioskLogs", hiddenProcess: false);
	}

	private void ComputerManagementButtonClick(object sender, RoutedEventArgs e)
	{
		ProcessFunctions.LaunchProcess("mmc.exe", Environment.SystemDirectory + "\\compmgmt.msc", hiddenProcess: false);
	}

	private void DeviceManagementButtonClick(object sender, RoutedEventArgs e)
	{
		ProcessFunctions.LaunchProcess("mmc.exe", Environment.SystemDirectory + "\\devmgmt.msc", hiddenProcess: false);
	}

	private void KioskEngineLogsFolderButtonClick(object sender, RoutedEventArgs e)
	{
		ProcessFunctions.LaunchProcess("explorer.exe", "C:\\Program Files\\Redbox\\REDS\\Kiosk Engine\\logs", hiddenProcess: false);
	}

	private void DateTimeButtonClick(object sender, RoutedEventArgs e)
	{
		ProcessFunctions.LaunchProcess("control.exe", "timedate.cpl", hiddenProcess: false);
	}

	private void ControlPanelButtonClick(object sender, RoutedEventArgs e)
	{
		ProcessFunctions.LaunchProcess("control.exe", "", hiddenProcess: false);
	}

	public void SetConfigureButtonFocus(object sender, RoutedEventArgs e)
	{
		configure_kiosk_button.Focus();
	}

	private void ConfigureKioskButtonClick(object sender, RoutedEventArgs e)
	{
		try
		{
			ConfigureKiosk.Visibility = Visibility.Visible;
			ConfigureKiosk.ResetConfigureKiosk();
			ConfigureKiosk.cancel_button.Click += SetConfigureButtonFocus;
			ConfigureKiosk.close_button.Click += SetConfigureButtonFocus;
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("Unexpected Error starting Configure Kiosk module. ", ex.Message);
			System.Windows.MessageBox.Show("Unexpected Error starting Configure Kiosk module. " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	public void SetCloseButtonFocus(object sender, RoutedEventArgs e)
	{
		close_kiosk_button.Focus();
	}

	private void CloseKioskButtonClick(object sender, RoutedEventArgs e)
	{
		try
		{
			CloseKiosk.Visibility = Visibility.Visible;
			CloseKiosk.button_no.Click += SetCloseButtonFocus;
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("Unexpected Error starting Close Kiosk module. ", ex.Message);
			System.Windows.MessageBox.Show("Unexpected Error starting Close Kiosk module. " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	private void ToggleMaintenanceModeButtonClick(object sender, RoutedEventArgs e)
	{
		ServiceLocator.Instance.GetService<IResourceBundleService>();
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			LogHelper.Instance.Log("Unexpected Error toggling maintenance mode. Unable to instantiate Hardware Service!");
			System.Windows.MessageBox.Show("Error toggling maintenance mode. Unable to instantiate Hardware Service!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
		if (_maintenancemodestate)
		{
			ErrorList errorList = service.SuspendAllJobsForMaintenance();
			if (errorList.Count > 0)
			{
				LogErrorsFromList(errorList);
				System.Windows.MessageBox.Show("Error turning off maintenance mode, Suspending jobs, Please check the log file.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
		}
		else
		{
			ErrorList errorList2 = service.ResumeAllMaintenanceSuspendedJobs();
			if (errorList2.Count > 0)
			{
				LogErrorsFromList(errorList2);
				System.Windows.MessageBox.Show("Error resuming maintenance mode, Resuming jobs, Please check the log file.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
		}
		_maintenancemodestate = !_maintenancemodestate;
		SetMaintenanceMode();
	}

	private void StartKioskButtonClick(object sender, RoutedEventArgs e)
	{
		CloseControlPanel();
	}

	private void CameraFolderButtonClick(object sender, RoutedEventArgs e)
	{
		try
		{
			object servicePath = ProcessFunctions.GetServicePath("HalSvc$Default");
			if (servicePath != null)
			{
				string fullPath = ProcessFunctions.GetFullPath(string.Concat(servicePath, "\\..\\video"));
				if (!string.IsNullOrEmpty(fullPath))
				{
					ProcessFunctions.LaunchProcess("explorer.exe", fullPath.ToString(), hiddenProcess: false);
					return;
				}
				LogHelper.Instance.Log("Unexpected error: Could not find path to video directory: " + servicePath);
				System.Windows.MessageBox.Show("Could not build path to video directory.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
			else
			{
				LogHelper.Instance.Log("Unexpected error: Unable to locate HAL install path.");
				System.Windows.MessageBox.Show("Unable to locate HAL install path.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("Unexpected error In Control Panel, Camera Folder Button Click: " + ex.Message);
		}
	}

	private void StartSecureBrowserButtonClick(object sender, RoutedEventArgs e)
	{
		try
		{
			Dictionary<string, object> table = ProcessFunctions.GetTable("approved_urls");
			NavigateUrlCallback navigateUrlCallback = ProcessFunctions.SendAudit;
			ProcessFunctions.LaunchSecureBrowser(table, navigateUrlCallback);
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("Unexpected Error  starting Secure Browser. ", ex.Message);
			System.Windows.MessageBox.Show("Unexpected error starting Secure Browser:" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	private void HyperTerminalButtonClick(object sender, RoutedEventArgs e)
	{
		ProcessFunctions.LaunchProcess("c:\\program files\\Windows NT\\hypertrm.exe", "", hiddenProcess: false);
	}

	private void OpenKaseyaConfigButtonClick(object sender, RoutedEventArgs e)
	{
		string kaseyainiPath = ProcessFunctions.GetKaseyainiPath();
		if (!string.IsNullOrEmpty(kaseyainiPath))
		{
			ProcessFunctions.LaunchProcess("notepad.exe", kaseyainiPath, hiddenProcess: false);
			return;
		}
		LogHelper.Instance.Log("Unexpected Error:  Unable to locate Kaseya directory.");
		System.Windows.MessageBox.Show("Unable to locate Kaseya directory. Please also check the log file for errors.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
	}

	private void DisplayButtonClick(object sender, RoutedEventArgs e)
	{
		ProcessFunctions.LaunchProcess("control.exe", "desk.cpl", hiddenProcess: false);
	}

	private void CloseButtonClick(object sender, RoutedEventArgs e)
	{
		CloseControlPanel();
	}

	private void NetworkButtonClick(object sender, RoutedEventArgs e)
	{
		ProcessFunctions.LaunchProcess("control.exe", "ncpa.cpl", hiddenProcess: false);
	}

	private void AddRemoveProgramsButtonClick(object sender, RoutedEventArgs e)
	{
		ProcessFunctions.LaunchProcess("control.exe", "appwiz.cpl", hiddenProcess: false);
	}

	private void EventViewerButtonClick(object sender, RoutedEventArgs e)
	{
		ProcessFunctions.LaunchProcess("mmc.exe", Environment.SystemDirectory + "\\eventvwr.msc", hiddenProcess: false);
	}

	private void KillProcessButtonClick(object sender, RoutedEventArgs e)
	{
		List<string> processNamesToIgnore = new List<string> { "halsvc-win32" };
		KillHandler killHandler = ProcessFunctions.KillProcesses;
		ProcessFunctions.ShowTaskManager(processNamesToIgnore, killHandler);
	}

	private void ToggleOutOfOrderButtonClick(object sender, RoutedEventArgs e)
	{
		ToggleOutOforder();
	}

	private void EloPcapControlButtonClick(object sender, RoutedEventArgs e)
	{
		string text = ProcessFunctions.TouchScreenDriver();
		if (!string.IsNullOrEmpty(text))
		{
			ProcessFunctions.LaunchProcess(ProcessFunctions.GetFileName(text), text, hiddenProcess: false);
		}
	}

	private void WatchDogToggleButtonClick(object sender, RoutedEventArgs e)
	{
		try
		{
			bool flag = ProcessFunctions.GetMachineSettingValue("Store", "WatchDogEnabled", "true").ToUpper() == "true".ToUpper();
			ProcessFunctions.SetMachineSettingValue("Store", "WatchDogEnabled", (!flag).ToString());
			UpdateWatchDogStatus();
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("Control Panel, Toggle WatchDog  button click has unexpected exception.", ex);
			System.Windows.MessageBox.Show("Unexpected exception on Watch Dog Toggle. " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	private void StartFieldMaintenanceButtonClick(object sender, RoutedEventArgs e)
	{
		while (System.Windows.Forms.Application.OpenForms["SecureBrowser"] != null)
		{
			System.Windows.Forms.Application.OpenForms["SecureBrowser"].Close();
		}
		if (_idleTimer != null)
		{
			_idleTimer.Stop();
		}
		try
		{
			ServiceLocator.Instance.GetService<IResourceBundleService>().Activate("Field Maintenance Application", "*");
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("Control Panel, Start Field Maintenance button click has unexpected exception.", ex);
			System.Windows.MessageBox.Show("Unexpected exception starting Field Maintenance. " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	private void ToggleHalServiceButtonClick(object sender, RoutedEventArgs e)
	{
		try
		{
			string serviceStatus = ProcessFunctions.GetServiceStatus("halsvc$default");
			if (serviceStatus == "Running")
			{
				ProcessFunctions.StopService("halsvc$default");
			}
			else if (serviceStatus == "Stopped")
			{
				ProcessFunctions.StartService("halsvc$default");
			}
			ProcessFunctions.TimerFiredHandler timerHandler = UpdateHalServiceStatus;
			ProcessFunctions.CreateTimer("hal_status_timer", 100, 100, timerHandler);
			ProcessFunctions.StartTimer("hal_status_timer");
		}
		catch (Exception ex)
		{
			System.Windows.MessageBox.Show("Unexpected exception on HAL Service Toggle. " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}

	private void MsHalTesterButtonClick(object sender, RoutedEventArgs e)
	{
		string text = "C:\\Program Files\\Redbox\\MS HAL Tester\\bin";
		if (!ProcessFunctions.DoesDirectoryExist(text))
		{
			LogHelper.Instance.Log("Unexpected error: Could not find MS HAL Tester directory!");
			System.Windows.MessageBox.Show("Could not find MSHALTester directory.", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
		else
		{
			string processName = ProcessFunctions.CombinePaths(text, "mshaltester.exe");
			string text2 = ServiceLocator.Instance.GetService<IControlPanelService>()?.LoggedInUser ?? "N/A";
			ProcessFunctions.LaunchProcess(processName, "--secure --username:" + text2, hiddenProcess: false);
		}
	}

	private void ToggleTouchScreenButtonClick(object sender, RoutedEventArgs e)
	{
		try
		{
			ProcessFunctions.ToggleTouchScreen();
			UpdateTouchScreenStatus();
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log("Unexpected exception on Touch Screen Toggle. " + ex.Message);
			System.Windows.MessageBox.Show("Unexpected exception on Touch Screen Toggle. " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
		}
	}
}
