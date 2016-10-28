using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using Redbox.HAL.Client;
using Redbox.HAL.Common.GUI.Functions;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using Redbox.HAL.MSHALTester.Properties;
using log4net;

namespace Redbox.HAL.MSHALTester;

public class Form1 : Form, IDisposable, IClientOutputSink, ILogger
{
	private delegate void OnFinishInitDelegate();

	internal enum Direction
	{
		Unknown,
		Left,
		Right,
		Up,
		Down
	}

	private enum FanState
	{
		On,
		Off
	}

	private enum CameraState
	{
		Unknown,
		Started,
		Stopped
	}

	private const int MinimumSensor = 1;

	private const int MaximumSensor = 6;

	private const string TesterKey = "SOFTWARE\\Redbox\\MS_HALTester";

	private const int ImmediateTimeout = 90000;

	private const string ExchangerMnemonic = "AIRXCHGR";

	private OnFinishInitDelegate FinishInitDelegate;

	private ImageViewer ImageViewer;

	private Color m_sensorEnabledColor = Color.Green;

	private Color m_runningCommandButtonColor;

	private Dictionary<int, Panel> m_sensors = new Dictionary<int, Panel>();

	private List<HardwareJob> m_stoppedJobs = new List<HardwareJob>();

	private HardwareService m_hardwareService;

	private string m_servicePath;

	private string CounterFile = "c:\\Program Files\\Redbox\\KioskLogs\\ErrorLogs\\Timeouts.log";

	private bool RinglightOn;

	private CameraState StateOfCamera;

	private bool FanConfigured;

	private int? m_returnErrorCount = null;

	private bool LegacyCamera;

	private bool FormDisposed;

	private ClientHelper Helper;

	private ButtonAspectsManager Manager;

	private bool HasFraudSensor;

	private bool VMZConfigured;

	private bool DoorSensorsConfigured;

	private readonly OutputBox OutputBox;

	private readonly string Version;

	private readonly byte[] CameraSnapImmediate;

	private RadioButton m_offsetGetRadioButton;

	private readonly ILog m_log;

	private readonly bool m_secure;

	private IContainer components;

	private GroupBox groupBox2;

	private GroupBox groupBox3;

	private GroupBox groupBox4;

	private GroupBox groupBox5;

	private GroupBox groupBox6;

	private GroupBox groupBox7;

	private GroupBox groupBox8;

	private GroupBox m_qlmGroupBox;

	private GroupBox groupBox10;

	private GroupBox groupBox11;

	private GroupBox groupBox12;

	private TextBox m_encoderUnitsTextBox;

	private Button button1;

	private TextBox m_sourceSlotTextBox;

	private TextBox m_sourceDeckTextBox;

	private Button m_readPosButton;

	private Button button4;

	private Button button3;

	private Button button2;

	private Button button18;

	private Button button17;

	private Button button16;

	private Button button14;

	private Button button13;

	private Button button12;

	private Button button11;

	private Button button10;

	private Button button8;

	private Button button7;

	private Button m_qlmDoorStatus;

	private Button m_qlmCaseStatusButton;

	private Button m_qlmStopButton;

	private Button m_qlmDisengageButton;

	private Button m_qlmEngageButton;

	private Button button39;

	private Button button38;

	private Button button37;

	private Button button36;

	private Button button35;

	private Button button34;

	private Button m_gripperSellButton;

	private Button button32;

	private Button button31;

	private Button button30;

	private Button button29;

	private Button button28;

	private Button m_cameraPreview;

	private Button button26;

	private Button m_startButtonCamera;

	private Button m_snapImageButton;

	private Button m_homeYButton;

	private Button m_homeXButton;

	private Button button44;

	private Button m_runInitButton;

	private Button button50;

	private Button button47;

	private Button button54;

	private Button button53;

	private Button button51;

	private TextBox m_rollToSensorText;

	private ListBox m_outputBox;

	private Button m_clearOutputButton;

	private Label label11;

	private Label label10;

	private TextBox m_destinationSlotText;

	private TextBox m_destinationDeckText;

	private Label label14;

	private Button m_qlmDownButton;

	private Button m_qlmUpButton;

	private Button m_advancedMode;

	private Label label16;

	private Label label15;

	private TextBox m_rollerTimeoutTextBox;

	private Label label17;

	private TextBox m_versionTextBox;

	private Label label19;

	private Label label20;

	private TextBox m_kioskIDTextBox;

	private GroupBox groupBox1;

	private Button button57;

	private Button button5;

	private ErrorProvider m_errorProvider;

	private Button m_deckConfigurationButton;

	private Button button63;

	private Button button62;

	private Button button61;

	private Button button60;

	private Button button64;

	private Button m_takeDiskButton;

	private CheckBox m_sensorCheckBox;

	private Button m_openLogsButton;

	private Button m_configureDevicesButton;

	private Button button66;

	private Button m_resetProteusButton;

	private Button button19;

	private Button button20;

	private Button m_cameraWorkingButton;

	private Button m_readLimitsButton;

	private Button m_showTimeoutLogButton;

	private Button button22;

	private Button m_qlmTestButton;

	private Button m_closeSerialButton;

	private Button m_fanButton;

	private Button m_testArcusCommButton;

	private Button m_getWithOffset;

	private RadioButton m_getOffXRadio;

	private RadioButton m_getOffYRadio;

	private TextBox m_getOffsetTB;

	public bool CheckForHAL { get; set; }

	private string ServicePath
	{
		get
		{
			if (string.IsNullOrEmpty(m_servicePath))
			{
				RegistryKey registryKey = null;
				try
				{
					registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\HalSvc$default");
					if (registryKey != null)
					{
						string text = (string)registryKey.GetValue("ImagePath");
						if (!string.IsNullOrEmpty(text))
						{
							if (text.StartsWith("\""))
							{
								text = text.Remove(0, 1);
							}
							if (text.EndsWith("\""))
							{
								text = text.Remove(text.Length - 1, 1);
							}
							m_servicePath = Path.GetDirectoryName(text);
						}
					}
				}
				catch (Exception ex)
				{
					OutputBox.Write("Unable to locate HAL service path " + ex.Message);
					m_servicePath = null;
				}
			}
			return m_servicePath;
		}
	}

	private int SourceDeck => m_sourceDeckTextBox.GetInteger("Source Deck", OutputBox);

	private int SourceSlot => m_sourceSlotTextBox.GetInteger("Source Slot", OutputBox);

	private string DeckConfigPassword
	{
		get
		{
			RegistryKey registryKey = null;
			try
			{
				registryKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Redbox\\MS_HALTester\\Security");
				if (registryKey != null)
				{
					string text = (string)registryKey.GetValue("Password");
					if (string.IsNullOrEmpty(text))
					{
						registryKey.SetValue("Password", "aH/ag3NGv+Sr6gB8MT4/Dg==");
					}
					return text;
				}
				return string.Empty;
			}
			catch (Exception)
			{
				OutputBox.Write("Unable to open MS HAL Tester store.");
				return string.Empty;
			}
			finally
			{
				if (registryKey != null)
				{
					registryKey.Close();
					registryKey = null;
				}
			}
		}
	}

	private int CameraInError
	{
		get
		{
			if (!m_returnErrorCount.HasValue)
			{
				HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(m_hardwareService, "CAMERA RETURNERRORCOUNT");
				if (hardwareJob == null)
				{
					m_returnErrorCount = 0;
				}
				else
				{
					try
					{
						m_returnErrorCount = int.Parse(hardwareJob.GetTopOfStack());
					}
					catch
					{
						m_returnErrorCount = 0;
					}
				}
			}
			return m_returnErrorCount.Value;
		}
	}

	public void WriteMessage(string msg)
	{
		OutputBox.Write(msg);
	}

	public void WriteMessage(string fmt, params object[] stuff)
	{
		OutputBox.Write(fmt, stuff);
	}

	public void Log(string message, Exception e)
	{
		m_log.Error(message, e);
	}

	public void Log(string message, LogEntryType type)
	{
		switch (type)
		{
		case LogEntryType.Info:
			m_log.Info(message);
			break;
		case LogEntryType.Debug:
			m_log.Debug(message);
			break;
		case LogEntryType.Error:
			m_log.Error(message);
			break;
		case LogEntryType.Fatal:
			m_log.Fatal(message);
			break;
		}
	}

	public void Log(string message, Exception e, LogEntryType type)
	{
		switch (type)
		{
		case LogEntryType.Info:
			m_log.Info(message, e);
			break;
		case LogEntryType.Debug:
			m_log.Debug(message, e);
			break;
		case LogEntryType.Error:
			m_log.Error(message, e);
			break;
		case LogEntryType.Fatal:
			m_log.Fatal(message, e);
			break;
		}
	}

	public bool IsLevelEnabled(LogEntryType entryLogLevel)
	{
		return entryLogLevel switch
		{
			LogEntryType.Info => m_log.IsInfoEnabled, 
			LogEntryType.Debug => m_log.IsDebugEnabled, 
			LogEntryType.Error => m_log.IsErrorEnabled, 
			LogEntryType.Fatal => m_log.IsFatalEnabled, 
			_ => false, 
		};
	}

	public Form1(bool secure, string user)
	{
		InitializeComponent();
		Version = typeof(Form1).Assembly.GetName().Version.ToString();
		TesterSession testerSession = new TesterSession(user);
		TesterSessionImplemtation instance = new TesterSessionImplemtation(testerSession);
		ServiceLocator.Instance.AddService(typeof(ISessionUserService), instance);
		ServiceLocator.Instance.AddService(typeof(IUsbDeviceService), new UsbDeviceService(Settings.Default.UsbServiceDebug));
		ServiceLocator.Instance.AddService(typeof(IDeviceSetupClassFactory), new DeviceSetupClassFactory());
		ServiceLocator.Instance.AddService(typeof(ITouchscreenService), new TouchscreenService());
		FinishInitDelegate = AfterInit;
		OutputBox = new TesterOutputBox(m_outputBox, this);
		m_log = LogManager.GetLogger("TesterLog");
		m_secure = secure;
		if (ShouldStart())
		{
			m_offsetGetRadioButton = m_getOffXRadio;
			m_offsetGetRadioButton.Checked = true;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("CLEAR");
			stringBuilder.AppendLine(" RINGLIGHT ON");
			stringBuilder.AppendLine(" CAMERA SNAP");
			stringBuilder.AppendLine(" RINGLIGHT OFF");
			CameraSnapImmediate = Encoding.ASCII.GetBytes(stringBuilder.ToString());
			ServiceLocator.Instance.AddService(typeof(IRuntimeService), new RuntimeService());
			ServiceLocator.Instance.AddService(typeof(ILogger), this);
			LogHelper.Instance.Log("Tester startup; form {0} in secure mode, user = {1}.", m_secure ? "is" : "isn't", testerSession.User);
			ConfigureChannel(_test: false);
			m_runningCommandButtonColor = Color.Red;
			base.FormClosing += OnFormClosing;
			PickerSensorsBar pickerSensorsBar = new PickerSensorsBar(m_hardwareService)
			{
				Dock = DockStyle.Bottom
			};
			groupBox5.Controls.Add(pickerSensorsBar);
			pickerSensorsBar.ReadEvents += OnSensorReadDone;
			pickerSensorsBar.BarEvents += OnSensorOperation;
			if (!string.IsNullOrEmpty(Settings.Default.SensorColor))
			{
				m_sensorEnabledColor = Color.FromName(Settings.Default.SensorColor);
			}
			if (!string.IsNullOrEmpty(Settings.Default.ButtonRunningColor))
			{
				m_runningCommandButtonColor = Color.FromName(Settings.Default.ButtonRunningColor);
			}
			if (!string.IsNullOrEmpty(Settings.Default.FormBackgroundColor))
			{
				Color backColor = Color.FromName(Settings.Default.FormBackgroundColor);
				BackColor = backColor;
			}
			Manager = new ButtonAspectsManager();
			m_versionTextBox.Text = Version;
			string id = "UNKNOWN";
			if (!m_hardwareService.GetKioskID(out id).Success)
			{
				id = "UNKNOWN";
			}
			m_kioskIDTextBox.Text = id;
			ThreadPool.QueueUserWorkItem(delegate
			{
				Helper.WaitforInit(FinishInit);
			});
		}
	}

	private bool ShouldStart()
	{
		if (!Settings.Default.CheckForRunningHAL)
		{
			return true;
		}
		if (!ProcessIsRunning("halsvc-win32.exe") && !ProcessIsRunning("halsvc-console.exe"))
		{
			OutputBox.Write("The HAL process is not running.");
			return false;
		}
		return true;
	}

	private bool ProcessIsRunning(string name)
	{
		int num = name.LastIndexOf(".exe");
		name = ((-1 == num) ? name : name.Substring(0, num));
		Process[] processesByName = Process.GetProcessesByName(name);
		if (processesByName.Length != 0)
		{
			return true;
		}
		return false;
	}

	private void AfterInit()
	{
		m_takeDiskButton.Enabled = m_secure;
		if (m_secure)
		{
			m_takeDiskButton.Text = (Settings.Default.DeveloperMode ? "Engineering Tests" : "Unknown Count");
		}
		else
		{
			m_deckConfigurationButton.Visible = false;
			m_advancedMode.Visible = false;
			m_sensorCheckBox.Visible = false;
		}
		ChangeUIAfterconfig();
	}

	private void FinishInit()
	{
		StopJobs();
		CompileScripts();
		Invoke(FinishInitDelegate);
	}

	internal void LogMessage(string msg)
	{
		if (!string.IsNullOrEmpty(msg))
		{
			WriteToLogFile(msg);
		}
	}

	internal void LogFormattedMessage(string format, params object[] parms)
	{
		string msg = string.Format(format, parms);
		WriteToLogFile(msg);
	}

	private void OnSensorReadDone(bool readError)
	{
		OutputBox.Write(readError ? "Sensor Read Failure: PCB Not Responsive" : "Sensors read.");
	}

	private void OnSensorOperation(string msg, string response)
	{
		OutputBox.Write($"{msg}: {response}");
	}

	private void ChangeUIAfterconfig()
	{
		Button[] array = FormButtons();
		foreach (Button button in array)
		{
			button.Enabled = true;
		}
		using MachineConfiguration machineConfiguration = new MachineConfiguration(m_hardwareService);
		machineConfiguration.Run();
		VMZConfigured = machineConfiguration.VMZConfigured;
		DoorSensorsConfigured = machineConfiguration.DoorSensorsConfigured;
		LegacyCamera = machineConfiguration.LegacyCamera;
		FanConfigured = machineConfiguration.AirExchangerConfigured;
		HasFraudSensor = machineConfiguration.HasFraudDevice;
		if (VMZConfigured)
		{
			OutputBox.Write("The kiosk is configured for a VMZ.");
		}
		if (DoorSensorsConfigured)
		{
			OutputBox.Write("The kiosk is configured for door sensors.");
			if (machineConfiguration.DoorSensorStatus != "OK")
			{
				OutputBox.Write($"WARNING: DoorSensor status returned {machineConfiguration.DoorSensorStatus} (door may be open!)");
			}
		}
		else
		{
			OutputBox.Write($"Door sensor configuration shows: NOT CONFIGURED.");
		}
		if (machineConfiguration.HasQuickReturn)
		{
			OutputBox.Write("The kiosk is configured for a quick return device.");
		}
		if (HasFraudSensor)
		{
			m_gripperSellButton.Enabled = false;
			OutputBox.Write("The kiosk is configured with a fraud sensor.");
		}
		if (!LegacyCamera)
		{
			m_cameraPreview.Enabled = false;
			m_startButtonCamera.Enabled = true;
			ToggleNonLegacyStart();
		}
		ChangeQlmPanelState();
		if (FanConfigured)
		{
			m_fanButton.Enabled = true;
			SetFanButtonState();
		}
		DisplayInventoryStats(first: true);
		m_cameraWorkingButton.Enabled = CameraInError > 0;
	}

	private void ToggleNonLegacyStart()
	{
		if (StateOfCamera == CameraState.Unknown)
		{
			string text = ExecuteInstructionWithResult("CAMERA STATUS", logResult: false);
			if (string.IsNullOrEmpty(text))
			{
				OutputBox.Write("Could not find current camera status.");
				return;
			}
			StateOfCamera = ((text == "RUNNING") ? CameraState.Started : CameraState.Stopped);
		}
		m_startButtonCamera.Text = ((StateOfCamera == CameraState.Stopped) ? "Start Camera" : "Stop Camera");
	}

	private void DisplayInventoryStats(bool first)
	{
		InventoryStatsJob inventoryStatsJob = new InventoryStatsJob(m_hardwareService);
		inventoryStatsJob.Run();
		foreach (ProgramResult result in inventoryStatsJob.Results)
		{
			if (result.Code == "TotalEmptyCount")
			{
				OutputBox.Write($"  EMPTY slots: {result.Message}");
			}
			else if (result.Code == "UnknownCount")
			{
				OutputBox.Write($"  UNKNOWN slots: {result.Message}");
			}
		}
		OutputBox.Write("Inventory Stats: ");
	}

	private void CompileScripts()
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		string path = service.RuntimePath("Scripts");
		if (!Directory.Exists(path))
		{
			return;
		}
		string[] files = Directory.GetFiles(path, "*.hs");
		string[] array = files;
		foreach (string path2 in array)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path2);
			HardwareCommandResult hardwareCommandResult = m_hardwareService.CompileProgram(path2, fileNameWithoutExtension, requiresClientConnection: false);
			if (!hardwareCommandResult.Success)
			{
				LogHelper.Instance.Log("Failed to compile script {0} ", fileNameWithoutExtension);
			}
		}
	}

	private void ConfigureChannel(bool _test)
	{
		Helper = new ClientHelper(this);
		m_hardwareService = Helper.AutoConnect();
		if (m_hardwareService == null)
		{
			OutputBox.Write("Unable to create a hardware service.");
		}
		else if (_test)
		{
			OutputBox.Write(Helper.TestCommunication() ? "Hardware service initialized." : "Failed to communicate with HAL.");
		}
	}

	private string GetInstructionFromButton(object sender)
	{
		if (!(sender is Button button))
		{
			return string.Empty;
		}
		if (button.Tag is string result)
		{
			return result;
		}
		return string.Empty;
	}

	private void OnDropQLM(object sender, EventArgs args)
	{
		using (Manager.MakeAspect(sender))
		{
			string text = ExecuteInstructionWithResult("HOMEY CLEARGRIPPER=TRUE", logResult: false);
			if (!string.IsNullOrEmpty(text))
			{
				if (text != "SUCCESS")
				{
					OutputBox.Write($"The HOMEY function failed with error {text}");
				}
				else
				{
					CommonFunctions.ExecuteInstruction(m_hardwareService, "QLM DROP", 120000);
				}
			}
		}
	}

	private void OnDoSnap(object sender, EventArgs args)
	{
		using (Manager.MakeAspect(sender))
		{
			if (!m_hardwareService.ExecuteImmediateProgram(CameraSnapImmediate, out var job).Success)
			{
				OutputBox.Write("Command error.");
				return;
			}
			string topOfStack = job.GetTopOfStack();
			if ("SUCCESS" == topOfStack)
			{
				string[] stackEntries = job.GetStackEntries(2);
				if (ImageViewer == null)
				{
					ImageViewer = new ImageViewer();
					ImageViewer.FormClosing += OnImageViewClosing;
				}
				else
				{
					ImageViewer.Hide();
				}
				ImageViewer.DisplayFile(stackEntries[1]);
				ImageViewer.Show();
			}
			else
			{
				OutputBox.Write("CAMERA CAPTURE error. I'm sorry!");
			}
		}
	}

	private void OnImageViewClosing(object sender, FormClosingEventArgs args)
	{
		ImageViewer = null;
	}

	private void ToggleRinglight()
	{
		string instruction = (RinglightOn ? "RINGLIGHT ON" : "RINGLIGHT OFF");
		CommonFunctions.ExecuteInstruction(m_hardwareService, instruction);
	}

	private void OnStartCamera(object sender, EventArgs args)
	{
		ButtonAspects buttonAspects = Manager.MakeAspect(sender);
		try
		{
			if (LegacyCamera)
			{
				RinglightOn = !RinglightOn;
				ToggleRinglight();
				buttonAspects.Button.Text = (RinglightOn ? "Turn Ringlight Off" : "Turn Ringlight On");
			}
			else
			{
				if (StateOfCamera == CameraState.Unknown)
				{
					return;
				}
				string instruction = ((CameraState.Started == StateOfCamera) ? "CAMERA STOP FORCE=TRUE" : "CAMERA START");
				CameraState cameraState = ((CameraState.Started != StateOfCamera) ? CameraState.Started : CameraState.Stopped);
				HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(m_hardwareService, instruction);
				if (hardwareJob == null)
				{
					OutputBox.Write("Unable to execute instruction.");
					return;
				}
				string topOfStack = hardwareJob.GetTopOfStack();
				if ("TRUE".Equals(topOfStack, StringComparison.CurrentCultureIgnoreCase))
				{
					OutputBox.Write($"Changed the camera state to {cameraState.ToString()}.");
					StateOfCamera = cameraState;
					ToggleNonLegacyStart();
				}
				else
				{
					OutputBox.Write($"Couldn't change camera state to {cameraState.ToString()}.I'm sorry!");
				}
			}
		}
		finally
		{
			((IDisposable)buttonAspects/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private void OnExecuteInstruction(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			string instructionFromButton = GetInstructionFromButton(sender);
			if (!string.IsNullOrEmpty(instructionFromButton))
			{
				if (CommonFunctions.ExecuteInstruction(m_hardwareService, instructionFromButton) != null)
				{
					OutputBox.Write("Sent instruction " + instructionFromButton);
				}
				else
				{
					OutputBox.Write("Sending instruction " + instructionFromButton + " failed.");
				}
			}
			else
			{
				OutputBox.Write($"{instructionFromButton} failed.");
			}
		}
	}

	private void OnExecuteInstructionWithResult(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			string instructionFromButton = GetInstructionFromButton(sender);
			ExecuteInstructionWithResult(instructionFromButton);
		}
	}

	private void OnRunQlmDisengage(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			if (!VMZConfigured)
			{
				ExecuteInstructionWithResult("QLM DISENGAGE");
				return;
			}
			ConfirmationDialog confirmationDialog = new ConfirmationDialog();
			if (confirmationDialog.ShowDialog() != DialogResult.OK)
			{
				return;
			}
			using TestRetrofitDeck testRetrofitDeck = new TestRetrofitDeck(m_hardwareService);
			testRetrofitDeck.Run();
			if (HardwareJobStatus.Completed == testRetrofitDeck.EndStatus)
			{
				OutputBox.Write("Test succeeded.");
			}
			else
			{
				OutputBox.Write("The test failed.");
			}
		}
	}

	private void OnTestBoards(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			OutputBox.Write("Test boards:");
			if (TestBoards())
			{
				OutputBox.Write("Board test ok.");
				return;
			}
			OutputBox.Write(" One or more boards not responsive - issue RESET.");
			using (ResetControlSystemJob resetControlSystemJob = new ResetControlSystemJob(m_hardwareService))
			{
				resetControlSystemJob.Run();
				foreach (ProgramResult result in resetControlSystemJob.Results)
				{
					OutputBox.Write(result.Message);
				}
				if (resetControlSystemJob.EndStatus == HardwareJobStatus.Errored)
				{
					return;
				}
			}
			TestBoards();
		}
	}

	private bool TestBoards()
	{
		using BoardTestJob boardTestJob = new BoardTestJob(m_hardwareService);
		boardTestJob.Run();
		foreach (ProgramResult result in boardTestJob.Results)
		{
			if (result.Code == "VersionResult")
			{
				OutputBox.Write(result.Message);
			}
		}
		return boardTestJob.EndStatus == HardwareJobStatus.Completed;
	}

	private void WriteToLogFile(string msg)
	{
		try
		{
			LogHelper.Instance.Log(msg);
		}
		catch (Exception)
		{
		}
	}

	private void OnMove(object sender, EventArgs ea)
	{
		using (Manager.MakeAspect(sender))
		{
			int sourceDeck = SourceDeck;
			int sourceSlot = SourceSlot;
			if (-1 == sourceDeck || -1 == sourceSlot)
			{
				OutputBox.Write(Resources.SourceDeckSlotInvalid);
				return;
			}
			string text = $"MOVE DECK={sourceDeck} SLOT={sourceSlot}";
			HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(m_hardwareService, text);
			if (hardwareJob != null)
			{
				string topOfStack = hardwareJob.GetTopOfStack();
				OutputBox.Write(text + " " + topOfStack);
			}
		}
	}

	private Direction DirectionFromButton(object sender)
	{
		Direction result = Direction.Unknown;
		if (!(sender is Button button))
		{
			return result;
		}
		if (!(button.Tag is string value))
		{
			return result;
		}
		try
		{
			return (Direction)Enum.Parse(typeof(Direction), value, ignoreCase: true);
		}
		catch (Exception)
		{
			return Direction.Unknown;
		}
	}

	private void OnDirectionMove(object sender, EventArgs args)
	{
		using (Manager.MakeAspect(sender))
		{
			Direction direction = DirectionFromButton(sender);
			if (direction == Direction.Unknown)
			{
				return;
			}
			bool flag = false;
			if (m_secure && m_sensorCheckBox.Checked)
			{
				flag = true;
				m_sensorCheckBox.Checked = false;
			}
			m_errorProvider.SetError(m_encoderUnitsTextBox, string.Empty);
			Application.DoEvents();
			string s = m_encoderUnitsTextBox.Text;
			int result = 0;
			if (int.TryParse(s, out result))
			{
				switch (direction)
				{
				case Direction.Left:
				case Direction.Right:
					if (Math.Abs(result) > 50)
					{
						m_errorProvider.SetError(m_encoderUnitsTextBox, "Cannot exceed 50 units.");
						OutputBox.Write("ERROR: move cannot exceed 50 units on X axis.");
						Application.DoEvents();
						return;
					}
					break;
				case Direction.Up:
				case Direction.Down:
					if (Math.Abs(result) > 200)
					{
						m_errorProvider.SetError(m_encoderUnitsTextBox, "Cannot exceed 200 units.");
						OutputBox.Write("ERROR: move cannot exceed 200 units on Y axis.");
						Application.DoEvents();
						return;
					}
					break;
				}
				if (direction == Direction.Left || direction == Direction.Down)
				{
					result = -result;
					OutputBox.Write($"{direction.ToString()} moving {result} eu");
				}
				if (GetArcusPosition("MOTIONCONTROL XPOS", out var position))
				{
					int num = position;
					if (GetArcusPosition("MOTIONCONTROL YPOS", out position))
					{
						int num2 = position;
						bool flag2 = direction == Direction.Up || direction == Direction.Down;
						int units = (flag2 ? (num2 + result) : (num + result));
						MoveHelper moveHelper = new MoveHelper(m_hardwareService);
						MoveHelperResult moveHelperResult = moveHelper.MoveAbs(flag2 ? MoveAxis.Y : MoveAxis.X, units, !m_secure || !flag);
						OutputBox.Write(moveHelperResult.Status);
					}
					else
					{
						OutputBox.Write("Cannot get current Y position.");
					}
				}
				else
				{
					OutputBox.Write("Cannot get current X position.");
				}
			}
			else
			{
				m_errorProvider.SetError(m_encoderUnitsTextBox, "Must specify an integer!");
			}
		}
	}

	private bool GetArcusPosition(string instruction, out int position)
	{
		HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(m_hardwareService, instruction);
		if (hardwareJob == null)
		{
			position = 0;
			return false;
		}
		HardwareCommandResult stack = hardwareJob.GetStack(out var stack2);
		position = 0;
		bool flag = false;
		string text = null;
		if (stack.Success)
		{
			if (stack2.Count >= 1)
			{
				text = stack2.Pop();
				flag = int.TryParse(text, out position);
				if (!flag)
				{
					OutputBox.Write($"Failed to decode {text}");
				}
			}
		}
		else
		{
			OutputBox.Write($"MOTIONCONTROL instruction failed: {stack.ToString()}");
		}
		if (stack.Success)
		{
			return flag;
		}
		return false;
	}

	private bool ReadXPosition(out int position)
	{
		position = 0;
		if (GetArcusPosition("MOTIONCONTROL XPOS", out position))
		{
			return true;
		}
		OutputBox.Write("Unable to get position from MOTIONCONTROL.");
		return false;
	}

	private bool ReadYPosition(out int position)
	{
		position = 0;
		if (GetArcusPosition("MOTIONCONTROL YPOS", out position))
		{
			return true;
		}
		OutputBox.Write("Unable to get position from MOTIONCONTROL.");
		return false;
	}

	private void OnReadPositions(object sender, EventArgs ea)
	{
		using (Manager.MakeAspect(sender))
		{
			string instruction = "LOC";
			string format = "Deck = {0}, Slot = {1}";
			HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(m_hardwareService, instruction);
			if (hardwareJob == null)
			{
				OutputBox.Write("Unable to execute immediate instruction.");
				return;
			}
			if (hardwareJob.GetStack(out var stack).Success)
			{
				string arg = stack.Pop();
				string arg2 = stack.Pop();
				OutputBox.Write(string.Format(format, arg, arg2));
			}
			else
			{
				OutputBox.Write(string.Format(format, "UNDEFINED", "UNDEFINED"));
			}
			if (ReadXPosition(out var position) && ReadYPosition(out var position2))
			{
				OutputBox.Write($"X = {position}, Y = {position2}");
			}
			else
			{
				OutputBox.Write("Unable to obtain position information.");
			}
		}
		Application.DoEvents();
	}

	private List<HardwareJob> GetJobs()
	{
		List<HardwareJob> list = new List<HardwareJob>();
		HardwareCommandResult jobs = m_hardwareService.GetJobs(out var jobs2);
		if (!jobs.Success)
		{
			OutputBox.Write("Unable to read job list - JOBS MAY BE ACTIVE!");
			return list;
		}
		list.AddRange(jobs2);
		return list;
	}

	private void StopJobs()
	{
		int num = 0;
		List<HardwareJob> jobs = GetJobs();
		using (new DisposeableList<HardwareJob>(jobs))
		{
			foreach (HardwareJob item in jobs)
			{
				if (item.ProgramName.Equals("self-healer", StringComparison.CurrentCultureIgnoreCase))
				{
					if (item.Suspend().Success)
					{
						m_stoppedJobs.Add(item);
						OutputBox.Write($"Suspended job {item.ProgramName} ID = {item.ID}");
					}
					else
					{
						OutputBox.Write("Unable to stop job " + item.ProgramName);
					}
				}
				else if ((item.Status == HardwareJobStatus.Pending || item.Status == HardwareJobStatus.Running) && !item.ProgramName.Equals("job-watchdog"))
				{
					num++;
				}
			}
			if (num > 0)
			{
				OutputBox.Write("WARNING: jobs are currently running or pending.");
			}
			else
			{
				OutputBox.Write("There are no jobs running or pending.");
			}
			m_hardwareService.ExecuteServiceCommand("SERVICE diagnostic-mode status: true");
		}
	}

	private void button39_Click(object sender, EventArgs e)
	{
		m_errorProvider.Clear();
		using (Manager.MakeAspect(sender))
		{
			if (string.IsNullOrEmpty(m_rollToSensorText.Text))
			{
				return;
			}
			if (int.TryParse(m_rollToSensorText.Text, out var _))
			{
				string arg = (string.IsNullOrEmpty(m_rollerTimeoutTextBox.Text) ? "5000" : m_rollerTimeoutTextBox.Text);
				string text = $"ROLLER POS={m_rollToSensorText.Text} TIMEOUT={arg} WAIT=TRUE";
				HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(m_hardwareService, text);
				if (hardwareJob != null)
				{
					string topOfStack = hardwareJob.GetTopOfStack();
					OutputBox.Write(text + " " + topOfStack);
				}
			}
			else
			{
				m_errorProvider.SetError(m_rollToSensorText, "Integer required.");
			}
		}
	}

	private void OnRumRollers(object sender, EventArgs ea)
	{
		using (Manager.MakeAspect(sender))
		{
			string instructionFromButton = GetInstructionFromButton(sender);
			string instruction = instructionFromButton;
			if (!string.IsNullOrEmpty(m_rollerTimeoutTextBox.Text))
			{
				instruction = $"{instructionFromButton} TIMEOUT={m_rollerTimeoutTextBox.Text}";
			}
			CommonFunctions.ExecuteInstruction(m_hardwareService, instruction);
		}
	}

	private void OnReadDiskInPicker(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			ReadBarcodeResult readBarcodeResult = ReadBarcodeResult.ReadBarcodeOfDiskInPicker(m_hardwareService);
			OutputBox.Write(string.Format("Found {0} barcodes ({1}), time={2} {3}", readBarcodeResult.Count, readBarcodeResult.Barcode, readBarcodeResult.ScanTime, readBarcodeResult.IsDuplicate ? "DUPLICATE" : string.Empty));
		}
	}

	private void OnLaunchCameraProperties(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			if (LegacyCamera)
			{
				string servicePath = ServicePath;
				if (string.IsNullOrEmpty(servicePath))
				{
					OutputBox.Write("Unable to locate service path; cannot launch amcap.");
				}
				else
				{
					CommonCameraFunctions.LaunchAndWaitForTuner(m_hardwareService);
				}
			}
		}
	}

	private void OnFormClosing(object sender, FormClosingEventArgs e)
	{
		CloseForm();
	}

	private void OnCloseForm(object sender, EventArgs e)
	{
		CloseForm();
	}

	private void CloseForm()
	{
		if (FormDisposed)
		{
			return;
		}
		if (ImageViewer != null)
		{
			ImageViewer.Dispose();
		}
		FormDisposed = true;
		OutputBox.Write("Cleanup Hardware.");
		Application.DoEvents();
		if (m_hardwareService != null)
		{
			m_hardwareService.ExecuteServiceCommand("SERVICE diagnostic-mode status: false");
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(string.Format(" {0} FANON", "AIRXCHGR"));
			stringBuilder.AppendLine(" VENDDOOR CLOSE");
			stringBuilder.AppendLine(" GRIPPER RENT");
			stringBuilder.AppendLine(" GRIPPER RETRACT");
			stringBuilder.AppendLine(" SENSOR PICKER-OFF");
			stringBuilder.AppendLine(" ROLLER STOP");
			stringBuilder.AppendLine(" RINGLIGHT OFF");
			stringBuilder.AppendLine(" CLEAR");
			m_hardwareService.ExecuteImmediateProgram(Encoding.ASCII.GetBytes(stringBuilder.ToString()), out var _);
			OutputBox.Write("Restart suspended jobs.");
			Application.DoEvents();
			foreach (HardwareJob stoppedJob in m_stoppedJobs)
			{
				stoppedJob.Pend();
			}
		}
		Application.Exit();
	}

	private void OnRunInit(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			using InitJob initJob = new InitJob(m_hardwareService);
			initJob.Run();
			if (HardwareJobStatus.Completed == initJob.EndStatus)
			{
				OutputBox.Write("Init succeeded.");
			}
			else
			{
				if (initJob.Errors.Count <= 0)
				{
					return;
				}
				foreach (Error error in initJob.Errors)
				{
					OutputBox.Write(error.Details);
				}
				OutputBox.Write("Init didn't succeed; errors follow:");
			}
		}
	}

	private void OnInsertBlankLine(object sender, EventArgs e)
	{
		OutputBox.Write(string.Empty);
	}

	private void OnClearOutput(object sender, EventArgs e)
	{
		OutputBox.Clear();
	}

	private void OnGotoSlot(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			int sourceDeck = SourceDeck;
			int sourceSlot = SourceSlot;
			if (-1 == sourceDeck || -1 == sourceSlot)
			{
				OutputBox.Write(Resources.SourceDeckSlotInvalid);
				return;
			}
			int num = 0;
			string text = ExecuteInstructionWithResult("VERSION AUX", logResult: false);
			if ("TIMEOUT" == text)
			{
				OutputBox.Write("Move: AUX board not responsive.");
				num++;
			}
			text = ExecuteInstructionWithResult("VERSION PICKER", logResult: false);
			if ("TIMEOUT" == text)
			{
				OutputBox.Write("Move: PCB not responsive.");
				num++;
			}
			if (num > 0)
			{
				return;
			}
			string[] array = ReadLimits();
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				if (text2.Contains("BLOCKED"))
				{
					num++;
					OutputBox.Write($"MOVE: {text2}");
				}
			}
			if (num <= 0)
			{
				MoveHelper moveHelper = new MoveHelper(m_hardwareService);
				MoveHelperResult moveHelperResult = moveHelper.MoveTo(sourceDeck, sourceSlot);
				OutputBox.Write($"MOVE DECK={sourceDeck} SLOT={sourceSlot} {moveHelperResult.Status}");
			}
		}
	}

	private void OnSyncSlot(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			int sourceDeck = SourceDeck;
			int sourceSlot = SourceSlot;
			if (-1 == sourceDeck || -1 == sourceSlot)
			{
				OutputBox.Write(Resources.SourceDeckSlotInvalid);
				return;
			}
			HardwareCommandResult hardwareCommandResult = m_hardwareService.ScheduleJob("sync-locations", "Tester Location Sync", enableDebugging: false, new HardwareJobSchedule
			{
				Priority = HardwareJobPriority.Highest
			}, out var job);
			if (!hardwareCommandResult.Success)
			{
				OutputBox.Write("Unable to communicate with HAL.");
				return;
			}
			job.Push(sourceSlot);
			job.Push(sourceDeck);
			job.Push(1);
			job.Pend();
			hardwareCommandResult = job.WaitForCompletion(240000);
			HardwareCommandResult job2 = m_hardwareService.GetJob(job.ID, out job);
			if (!job2.Success)
			{
				OutputBox.Write("Unable to talk with HAL.");
			}
			else
			{
				if (!job.GetSymbols(out var symbols).Success)
				{
					return;
				}
				{
					foreach (string key in symbols.Keys)
					{
						if (key.StartsWith("MSTESTER-SYMBOL"))
						{
							OutputBox.Write(symbols[key]);
						}
					}
					return;
				}
			}
		}
	}

	private void OnPut(object sender, EventArgs e)
	{
		int sourceDeck = SourceDeck;
		int sourceSlot = SourceSlot;
		if (-1 == sourceDeck || -1 == sourceSlot)
		{
			OutputBox.Write(Resources.SourceDeckSlotInvalid);
		}
		else
		{
			CompositeFunctions.PutItem(sender, Manager, m_hardwareService, sourceDeck, sourceSlot, OutputBox);
		}
	}

	private void OnGetAndRead(object sender, EventArgs e)
	{
		GetDVD(sender);
	}

	private void OnTransfer(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			int sourceDeck = SourceDeck;
			int sourceSlot = SourceSlot;
			if (-1 == sourceDeck || -1 == sourceSlot)
			{
				OutputBox.Write(Resources.SourceDeckSlotInvalid);
				return;
			}
			int integer = m_destinationDeckText.GetInteger("Destination Deck", OutputBox);
			int integer2 = m_destinationSlotText.GetInteger("Destination Slot", OutputBox);
			if (-1 == integer || -1 == integer2)
			{
				OutputBox.Write(Resources.DestDeckSlotInvalid);
				return;
			}
			HardwareCommandResult hardwareCommandResult = m_hardwareService.ExecuteImmediate("GRIPPER  STATUS", out var job);
			if (!hardwareCommandResult.Success)
			{
				LogServiceError(hardwareCommandResult);
				return;
			}
			string topOfStack = job.GetTopOfStack();
			if (topOfStack.Equals("FULL"))
			{
				OutputBox.Write("The gripper is full; no transfer. Sorry! Have a great day!");
				return;
			}
			TransferLocation source = new TransferLocation(sourceDeck, sourceSlot);
			TransferLocation target = new TransferLocation(integer, integer2);
			TransferDisk transferDisk = new TransferDisk(m_hardwareService);
			topOfStack = transferDisk.Transfer(source, target);
			OutputBox.Write("Transfer " + topOfStack);
			m_destinationDeckText.Text = string.Empty;
			m_destinationSlotText.Text = string.Empty;
		}
	}

	private void OnVendDVD(object sender, EventArgs e)
	{
		CompositeFunctions.VendDisk(sender, m_hardwareService, Manager, OutputBox);
	}

	private void OnPushDVDInSlot(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			using PushInDvdJob pushInDvdJob = new PushInDvdJob(m_hardwareService);
			pushInDvdJob.Run();
			int num = 0;
			foreach (ProgramResult result in pushInDvdJob.Results)
			{
				if (result.Code == "MachineError")
				{
					num++;
					OutputBox.Write(result.Message);
				}
			}
			if (num == 0)
			{
				OutputBox.Write("push-in-dvd SUCCESS");
				return;
			}
			string arg = (pushInDvdJob.Symbols.ContainsKey("ERROR-STATE") ? pushInDvdJob.Symbols["ERROR-STATE"] : "ERROR");
			OutputBox.Write($"push-in-dvd {arg}");
		}
	}

	private void OnPutInEmptySlot(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			PutInEmptySlotResult putInEmptySlotResult = new PutInEmptySlotResult(m_hardwareService);
			putInEmptySlotResult.Run();
			foreach (ProgramResult result in putInEmptySlotResult.Results)
			{
				OutputBox.Write(result.Message);
			}
		}
	}

	private void LogServiceError(HardwareCommandResult result)
	{
		CommonFunctions.DumpResult(result);
		LogHelper.Instance.Log("Service call failure.");
	}

	private void OnReadSensors(object sender, EventArgs args)
	{
		using (Manager.MakeAspect(sender))
		{
			if (!(sender is Button button))
			{
				return;
			}
			string text = button.Tag as string;
			string arg = "AUX board";
			HardwareJob hardwareJob;
			if (text.Equals("AUX"))
			{
				hardwareJob = CommonFunctions.ExecuteInstruction(m_hardwareService, "SENSOR READ-AUX-INPUTS");
			}
			else
			{
				if (!text.Equals("PICKER"))
				{
					OutputBox.Write("Unrecognized sensor type " + text);
					return;
				}
				arg = "PCB";
				hardwareJob = CommonFunctions.ExecuteInstruction(m_hardwareService, "SENSOR READ-PICKER-INPUTS");
			}
			if (hardwareJob == null)
			{
				OutputBox.Write("Command failed.");
				return;
			}
			string topOfStack = hardwareJob.GetTopOfStack();
			int result = 0;
			if (!int.TryParse(topOfStack, out result))
			{
				OutputBox.Write("Sensor read error.");
				return;
			}
			if (result == 0)
			{
				OutputBox.Write($"{arg} is not responsive.");
				return;
			}
			string[] stackEntries = hardwareJob.GetStackEntries(result + 1);
			for (int i = 1; i <= result; i++)
			{
				OutputBox.Write(stackEntries[i]);
			}
		}
	}

	private void CopyListboxTextToClipboard(object sender, EventArgs e)
	{
		if (m_outputBox.Items.Count > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < m_outputBox.Items.Count; i++)
			{
				stringBuilder.Append(m_outputBox.Items[i]);
				stringBuilder.Append(Environment.NewLine);
			}
			Clipboard.SetText(stringBuilder.ToString());
		}
	}

	private void button15_Click(object sender, EventArgs e)
	{
		string servicePath = ServicePath;
		if (string.IsNullOrEmpty(servicePath))
		{
			OutputBox.Write("Unable to locate HAL service path.");
			return;
		}
		string fileName = Path.Combine(servicePath, "halmgmtc.exe");
		Process.Start(fileName);
		if (Settings.Default.ExitOnAdvancedModeLaunch)
		{
			Application.Exit();
		}
	}

	private void button58_Click(object sender, EventArgs e)
	{
		try
		{
			DeckConfigurationForm deckConfigurationForm = new DeckConfigurationForm(m_hardwareService, allowInventoryUpdate: false);
			deckConfigurationForm.ShowDialog();
		}
		catch (Exception ex)
		{
			OutputBox.Write("Unable to run Decks form.");
			WriteToLogFile(ex.Message);
		}
	}

	private void button59_Click(object sender, EventArgs e)
	{
		Process process = null;
		try
		{
			process = new Process();
			process.StartInfo.Arguments = "c:\\Program Files\\Redbox\\KioskLogs\\ErrorLogs";
			process.StartInfo.FileName = "explorer.exe";
			process.Start();
		}
		catch (Exception)
		{
			OutputBox.Write("Unable to launch explorer.");
		}
	}

	private TextBox TextBoxFromTag(string tag)
	{
		return tag switch
		{
			"SourceDeck" => m_sourceDeckTextBox, 
			"SourceSlot" => m_sourceSlotTextBox, 
			"DestDeck" => m_destinationDeckText, 
			"DestSlot" => m_destinationSlotText, 
			_ => null, 
		};
	}

	private void button60_Click(object sender, EventArgs e)
	{
		if (!(sender is Button { Tag: string tag }))
		{
			return;
		}
		TextBox textBox = TextBoxFromTag(tag);
		if (textBox != null)
		{
			LocationNumberPad locationNumberPad = new LocationNumberPad();
			if (locationNumberPad.ShowDialog() == DialogResult.OK)
			{
				textBox.Text = locationNumberPad.Number.ToString();
			}
		}
	}

	private void button17_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			using MSPullInDvdJob mSPullInDvdJob = new MSPullInDvdJob(m_hardwareService);
			mSPullInDvdJob.Run();
			if (mSPullInDvdJob.EndStatus == HardwareJobStatus.Completed)
			{
				OutputBox.Write("pull-dvd-in SUCCESS");
				return;
			}
			foreach (ProgramResult result in mSPullInDvdJob.Results)
			{
				if (result.Code == "InvalidJobUse")
				{
					OutputBox.Write("pull-dvd-in is intended for a disk stuck in the picker.");
					OutputBox.Write("Please use the 'GET' function instead.");
					break;
				}
				if (result.Code == "MachineError")
				{
					OutputBox.Write("pull-dvd-in " + result.Message);
				}
			}
		}
	}

	private void button65_Click(object sender, EventArgs e)
	{
		if (Settings.Default.DeveloperMode)
		{
			EngineeringForm engineeringForm = new EngineeringForm(Helper, m_hardwareService, Manager);
			engineeringForm.ShowDialog();
			return;
		}
		using (Manager.MakeAspect(sender))
		{
			DisplayInventoryStats(first: false);
		}
	}

	private void button67_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary["AllowSessionDisplay"] = Settings.Default.AllowSessionDisplay.ToString();
			ConfiguredDevicesForm configuredDevicesForm = new ConfiguredDevicesForm(m_hardwareService, dictionary);
			configuredDevicesForm.ShowDialog();
			ChangeUIAfterconfig();
			dictionary.Clear();
		}
	}

	private void ChangeQlmPanelState()
	{
		if (!VMZConfigured)
		{
			ToggleQLMButtons(show: true);
			return;
		}
		ToggleQLMButtons(show: false);
		m_qlmTestButton.Text = "Put In bin";
		m_qlmTestButton.Enabled = true;
		m_qlmGroupBox.Text = "Door sensors and others.";
		m_qlmEngageButton.Enabled = DoorSensorsConfigured;
		m_qlmEngageButton.Text = "Query Door Sensors";
		m_qlmDisengageButton.Enabled = true;
		m_qlmDisengageButton.Text = "Test deck 8.";
		Button qlmCaseStatusButton = m_qlmCaseStatusButton;
		Button qlmDownButton = m_qlmDownButton;
		Button qlmStopButton = m_qlmStopButton;
		Button qlmUpButton = m_qlmUpButton;
		bool flag = (m_qlmDoorStatus.Visible = false);
		bool flag3 = (qlmUpButton.Visible = flag);
		bool flag5 = (qlmStopButton.Visible = flag3);
		bool visible = (qlmDownButton.Visible = flag5);
		qlmCaseStatusButton.Visible = visible;
	}

	private void ToggleQLMButtons(bool show)
	{
		m_qlmCaseStatusButton.Enabled = show;
		m_qlmDisengageButton.Enabled = show;
		m_qlmDownButton.Enabled = show;
		m_qlmEngageButton.Enabled = show;
		m_qlmStopButton.Enabled = show;
		m_qlmUpButton.Enabled = show;
		m_qlmDoorStatus.Enabled = show;
		m_qlmTestButton.Enabled = show;
	}

	private string ExecuteInstructionWithResult(string inst)
	{
		return ExecuteInstructionWithResult(inst, logResult: true);
	}

	private string ExecuteInstructionWithResult(string inst, bool logResult)
	{
		if (string.IsNullOrEmpty(inst))
		{
			return string.Empty;
		}
		HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(m_hardwareService, inst, 120000);
		if (hardwareJob != null)
		{
			string topOfStack = hardwareJob.GetTopOfStack();
			if (logResult)
			{
				OutputBox.Write(inst + " - " + topOfStack);
			}
			return topOfStack;
		}
		OutputBox.Write("Command failed.");
		return string.Empty;
	}

	private bool JobCompletedWithoutError(HardwareJob job)
	{
		if (job.Status != HardwareJobStatus.Completed)
		{
			return job.Status == HardwareJobStatus.Stopped;
		}
		return true;
	}

	private bool JobCompleted(HardwareJob job)
	{
		if (job.Status != HardwareJobStatus.Completed && job.Status != HardwareJobStatus.Stopped && job.Status != HardwareJobStatus.Errored)
		{
			return job.Status == HardwareJobStatus.Garbage;
		}
		return true;
	}

	private void button66_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			try
			{
				HardwareJobSchedule hardwareJobSchedule = new HardwareJobSchedule();
				hardwareJobSchedule.Priority = HardwareJobPriority.Highest;
				HardwareJobSchedule schedule = hardwareJobSchedule;
				HardwareCommandResult hardwareCommandResult = m_hardwareService.ScheduleJob("hardware-status", string.Empty, enableDebugging: false, schedule, out var job);
				if (!hardwareCommandResult.Success)
				{
					OutputBox.Write("Unable to run job hardware-status");
					return;
				}
				if (!Helper.WaitForJob(job, out var _))
				{
					OutputBox.Write("Unable to run job hardware-status");
					return;
				}
				if (!job.GetResults(out var results).Success)
				{
					OutputBox.Write($"Unable to read the results for job {job.ID}");
					return;
				}
				if (results[0].Code == "HardwareStatusOk")
				{
					OutputBox.Write("Hardware status ok.");
				}
				else
				{
					OutputBox.Write("Hardware error status follows:");
					ProgramResult[] array = results;
					foreach (ProgramResult programResult in array)
					{
						if (!(programResult.Code == "HardwareStatusInError"))
						{
							OutputBox.Write(programResult.Message);
						}
					}
				}
				m_hardwareService.TrashJob(job.ID);
			}
			catch (Exception)
			{
				OutputBox.Write("ERROR: hardware-status failed.");
			}
		}
	}

	private void button9_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			using ResetMotionControlExecutor resetMotionControlExecutor = new ResetMotionControlExecutor(m_hardwareService);
			resetMotionControlExecutor.Run();
			if (resetMotionControlExecutor.EndStatus != HardwareJobStatus.Completed)
			{
				OutputBox.Write("ERROR: reset-arcus failed.");
				return;
			}
			foreach (ProgramResult result in resetMotionControlExecutor.Results)
			{
				OutputBox.Write(result.Message);
			}
		}
	}

	private void ExecuteEngageOrOther(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			if (VMZConfigured)
			{
				ReadDoorSensors();
			}
			else
			{
				ExecuteInstructionWithResult("QLM ENGAGE");
			}
		}
	}

	private void button19_Click(object sender, EventArgs e)
	{
		GetDVD(sender);
		OnReadDiskInPicker(sender, EventArgs.Empty);
	}

	private void GetDVD(object sender)
	{
		int sourceDeck = SourceDeck;
		int sourceSlot = SourceSlot;
		if (-1 == sourceDeck || -1 == sourceSlot)
		{
			OutputBox.Write(Resources.SourceDeckSlotInvalid);
		}
		else
		{
			CompositeFunctions.GetItem(sender, Manager, sourceDeck, sourceSlot, OutputBox, m_hardwareService);
		}
	}

	private void button20_Click(object sender, EventArgs e)
	{
		m_errorProvider.Clear();
		using (Manager.MakeAspect(sender))
		{
			int sourceSlot = SourceSlot;
			if (-1 == sourceSlot)
			{
				m_errorProvider.SetError(m_sourceSlotTextBox, "Need a valid slot!");
				return;
			}
			using VerticalSync verticalSync = new VerticalSync(m_hardwareService, sourceSlot);
			verticalSync.Run();
			if (HardwareJobStatus.Completed == verticalSync.EndStatus)
			{
				OutputBox.Write("The job completed successfully.");
				return;
			}
			foreach (ProgramResult result in verticalSync.Results)
			{
				OutputBox.Write($"Failure at Deck {result.Deck} Slot {result.Slot} MSG: {result.Message}");
			}
		}
	}

	private void m_cameraWorkingButton_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(m_hardwareService, "CAMERA RESETRETURNCOUNTER");
			if (hardwareJob != null)
			{
				OutputBox.Write("Return counter reset.");
				m_cameraWorkingButton.Enabled = false;
			}
			else
			{
				OutputBox.Write("Command failed.");
			}
		}
	}

	private void m_showTimeoutLogButton_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			using LogTimeoutsExecutor logTimeoutsExecutor = new LogTimeoutsExecutor(m_hardwareService, CounterFile);
			logTimeoutsExecutor.Run();
			if (File.Exists(CounterFile))
			{
				Process.Start(CounterFile);
			}
			else
			{
				OutputBox.Write("Command failed.");
			}
		}
	}

	private void m_readLimitsButton_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			string[] array = ReadLimits();
			if (array.Length == 0)
			{
				OutputBox.Write("Unable to read limits.");
				return;
			}
			string[] array2 = array;
			foreach (string msg in array2)
			{
				OutputBox.Write(msg);
			}
		}
	}

	private string[] ReadLimits()
	{
		List<string> list = new List<string>();
		HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(m_hardwareService, "MOTIONCONTROL READLIMITS");
		if (hardwareJob == null)
		{
			OutputBox.Write("Command failed.");
			return list.ToArray();
		}
		if (hardwareJob.GetStack(out var stack).Errors.Count > 0)
		{
			OutputBox.Write("Error getting stack.");
			return list.ToArray();
		}
		string s = stack.Peek();
		try
		{
			int num = int.Parse(s);
			stack.Pop();
			for (int i = 0; i < num; i++)
			{
				list.Add(stack.Pop());
			}
		}
		catch
		{
			OutputBox.Write("Error processing stack.");
		}
		return list.ToArray();
	}

	private void ReadDoorSensors()
	{
		HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(m_hardwareService, "DOORSENSORS QUERY");
		if (hardwareJob == null)
		{
			OutputBox.Write("Command failed.");
			return;
		}
		if (hardwareJob.GetStack(out var stack).Errors.Count > 0)
		{
			OutputBox.Write("Error getting stack.");
			return;
		}
		string s = stack.Peek();
		try
		{
			int num = int.Parse(s);
			stack.Pop();
			for (int i = 0; i < num; i++)
			{
				OutputBox.Write(stack.Pop());
			}
		}
		catch
		{
			OutputBox.Write("Error processing stack.");
		}
	}

	private void button22_Click(object sender, EventArgs e)
	{
		if (m_hardwareService.ServiceUnknownSync().Success)
		{
			OutputBox.Write("Unknown sync scheduled.");
		}
		else
		{
			OutputBox.Write("Unknown sync schedule failed.");
		}
	}

	private void button21_Click(object sender, EventArgs e)
	{
		m_errorProvider.Clear();
		using (Manager.MakeAspect(sender))
		{
			if (!VMZConfigured)
			{
				using (QlmTestSyncJob qlmTestSyncJob = new QlmTestSyncJob(m_hardwareService))
				{
					qlmTestSyncJob.Run();
					foreach (ProgramResult result in qlmTestSyncJob.Results)
					{
						OutputBox.Write($"Failure at Deck {result.Deck} Slot {result.Slot} MSG: {result.Message}");
					}
					return;
				}
			}
			using PutDiskInPickerInBinJob putDiskInPickerInBinJob = new PutDiskInPickerInBinJob(m_hardwareService);
			putDiskInPickerInBinJob.Run();
			OutputBox.Write($"Job ended with status = {putDiskInPickerInBinJob.EndStatus.ToString()}");
			foreach (ProgramResult result2 in putDiskInPickerInBinJob.Results)
			{
				if (result2.Code == "PutToBinOK")
				{
					string arg = (result2.ItemID.IsUnknown() ? result2.ItemID.Metadata : result2.ItemID.Barcode);
					OutputBox.Write($"The disk ID = {arg} was put into the bin.");
				}
				else
				{
					OutputBox.Write($"Code = {result2.Code} Message = {result2.Message}");
				}
			}
		}
	}

	private Button[] FormButtons()
	{
		return new Button[68]
		{
			m_openLogsButton, m_clearOutputButton, m_readPosButton, m_runInitButton, m_homeXButton, m_homeYButton, m_resetProteusButton, m_testArcusCommButton, button57, button5,
			button47, button50, button51, button53, button54, m_qlmTestButton, button22, button66, button20, m_showTimeoutLogButton,
			m_readLimitsButton, button36, button37, button38, button39, button64, button31, button32, m_gripperSellButton, button34,
			button35, button28, button29, button30, m_qlmEngageButton, m_qlmDisengageButton, m_qlmStopButton, m_qlmCaseStatusButton, m_qlmUpButton, m_qlmDownButton,
			m_qlmDoorStatus, m_snapImageButton, m_startButtonCamera, m_cameraPreview, m_cameraWorkingButton, button19, button26, button7, button8, button10,
			button11, button12, button13, button16, button14, button17, button18, button1, button2, button3,
			button4, m_advancedMode, m_configureDevicesButton, m_takeDiskButton, m_closeSerialButton, m_deckConfigurationButton, m_takeDiskButton, m_getWithOffset
		};
	}

	private void button6_Click(object sender, EventArgs e)
	{
		if (!FanConfigured)
		{
			return;
		}
		FanState fanState = FetchState();
		using (Manager.MakeAspect(sender))
		{
			if (fanState == FanState.On)
			{
				CommonFunctions.ExecuteInstruction(m_hardwareService, string.Format("{0} FANOFF", "AIRXCHGR"));
				OutputBox.Write("Xchanger fan should be off.");
			}
			else
			{
				CommonFunctions.ExecuteInstruction(m_hardwareService, string.Format("{0} FANON", "AIRXCHGR"));
				OutputBox.Write("Xchanger fan should be on.");
			}
		}
		SetFanButtonState();
	}

	private void SetFanButtonState()
	{
		if (FanConfigured)
		{
			FanState fanState = FetchState();
			m_fanButton.BackColor = ((fanState == FanState.On) ? Color.LightGray : Color.Red);
			m_fanButton.Text = ((fanState == FanState.On) ? "Turn Off Xchgr Fan" : "Turn On Xchgr Fan");
		}
	}

	private FanState FetchState()
	{
		if (!FanConfigured)
		{
			return FanState.Off;
		}
		HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(m_hardwareService, string.Format("{0} FANSTATUS", "AIRXCHGR"));
		if (hardwareJob == null)
		{
			return FanState.On;
		}
		string topOfStack = hardwareJob.GetTopOfStack();
		return Enum<FanState>.ParseIgnoringCase(topOfStack, FanState.On);
	}

	private void m_getWithOffset_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			int sourceDeck = SourceDeck;
			int sourceSlot = SourceSlot;
			if (-1 == sourceDeck || -1 == sourceSlot)
			{
				m_errorProvider.SetError(m_sourceDeckTextBox, "Must provide a deck/slot value.");
				return;
			}
			if (string.IsNullOrEmpty(m_getOffsetTB.Text))
			{
				GetDVD(sender);
				return;
			}
			if (m_offsetGetRadioButton == null)
			{
				OutputBox.Write("Please specify an axis.");
				m_errorProvider.SetError(m_getOffsetTB, "Please specify an axis.");
			}
			m_hardwareService.ExecuteImmediate("CLEAR", out var job);
			HardwareCommandResult hardwareCommandResult = m_hardwareService.ExecuteImmediate("GRIPPER STATUS", out job);
			if (!hardwareCommandResult.Success)
			{
				CommonFunctions.DumpResult(hardwareCommandResult);
				OutputBox.Write("Unable to communicate with service.");
				return;
			}
			string topOfStack = job.GetTopOfStack();
			if ("FULL".Equals(topOfStack, StringComparison.CurrentCultureIgnoreCase))
			{
				OutputBox.Write("The gripper is full - clear picker & try again.");
				return;
			}
			Axis axis = Enum<Axis>.ParseIgnoringCase(m_offsetGetRadioButton.Tag as string, Axis.X);
			if (!int.TryParse(m_getOffsetTB.Text, out var result))
			{
				OutputBox.Write($"Could not decode {m_getOffsetTB.Text} as offset.");
				return;
			}
			if (axis == Axis.X && Math.Abs(result) > 50)
			{
				OutputBox.Write($"Offset {m_getOffsetTB.Text} exceeds 50 units.");
				return;
			}
			if (Axis.Y == axis && Math.Abs(result) > 500)
			{
				OutputBox.Write($"Offset {m_getOffsetTB.Text} exceeds 500 units.");
				return;
			}
			using PickAtOffsetExecutor pickAtOffsetExecutor = new PickAtOffsetExecutor(m_hardwareService, sourceDeck, sourceSlot, axis, result);
			pickAtOffsetExecutor.Run();
			if (HardwareJobStatus.Completed == pickAtOffsetExecutor.EndStatus)
			{
				OutputBox.Write("GET was successful.");
				return;
			}
			foreach (ProgramResult result2 in pickAtOffsetExecutor.Results)
			{
				OutputBox.Write(result2.Message);
			}
		}
	}

	private void radioButton_CheckedChanged(object sender, EventArgs e)
	{
		if (sender is RadioButton { Checked: not false } radioButton)
		{
			m_offsetGetRadioButton = radioButton;
		}
	}

	private HardwareCommandResult RunImmediate(string command)
	{
		HardwareCommandResult result = m_hardwareService.ExecuteImmediate(command, out var _);
		CommonFunctions.DumpResult(result);
		return result;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.m_testArcusCommButton = new System.Windows.Forms.Button();
		this.m_resetProteusButton = new System.Windows.Forms.Button();
		this.m_homeYButton = new System.Windows.Forms.Button();
		this.m_homeXButton = new System.Windows.Forms.Button();
		this.button44 = new System.Windows.Forms.Button();
		this.m_runInitButton = new System.Windows.Forms.Button();
		this.m_fanButton = new System.Windows.Forms.Button();
		this.groupBox3 = new System.Windows.Forms.GroupBox();
		this.m_closeSerialButton = new System.Windows.Forms.Button();
		this.button50 = new System.Windows.Forms.Button();
		this.button47 = new System.Windows.Forms.Button();
		this.button5 = new System.Windows.Forms.Button();
		this.button57 = new System.Windows.Forms.Button();
		this.groupBox4 = new System.Windows.Forms.GroupBox();
		this.button54 = new System.Windows.Forms.Button();
		this.button53 = new System.Windows.Forms.Button();
		this.button51 = new System.Windows.Forms.Button();
		this.groupBox5 = new System.Windows.Forms.GroupBox();
		this.groupBox6 = new System.Windows.Forms.GroupBox();
		this.m_rollerTimeoutTextBox = new System.Windows.Forms.TextBox();
		this.label17 = new System.Windows.Forms.Label();
		this.m_rollToSensorText = new System.Windows.Forms.TextBox();
		this.button39 = new System.Windows.Forms.Button();
		this.button38 = new System.Windows.Forms.Button();
		this.button37 = new System.Windows.Forms.Button();
		this.button36 = new System.Windows.Forms.Button();
		this.groupBox7 = new System.Windows.Forms.GroupBox();
		this.button64 = new System.Windows.Forms.Button();
		this.button35 = new System.Windows.Forms.Button();
		this.button34 = new System.Windows.Forms.Button();
		this.m_gripperSellButton = new System.Windows.Forms.Button();
		this.button32 = new System.Windows.Forms.Button();
		this.button31 = new System.Windows.Forms.Button();
		this.groupBox8 = new System.Windows.Forms.GroupBox();
		this.button30 = new System.Windows.Forms.Button();
		this.button29 = new System.Windows.Forms.Button();
		this.button28 = new System.Windows.Forms.Button();
		this.m_qlmGroupBox = new System.Windows.Forms.GroupBox();
		this.m_qlmDownButton = new System.Windows.Forms.Button();
		this.m_qlmUpButton = new System.Windows.Forms.Button();
		this.m_qlmDoorStatus = new System.Windows.Forms.Button();
		this.m_qlmCaseStatusButton = new System.Windows.Forms.Button();
		this.m_qlmStopButton = new System.Windows.Forms.Button();
		this.m_qlmDisengageButton = new System.Windows.Forms.Button();
		this.m_qlmEngageButton = new System.Windows.Forms.Button();
		this.groupBox10 = new System.Windows.Forms.GroupBox();
		this.button63 = new System.Windows.Forms.Button();
		this.button62 = new System.Windows.Forms.Button();
		this.button61 = new System.Windows.Forms.Button();
		this.button60 = new System.Windows.Forms.Button();
		this.m_destinationSlotText = new System.Windows.Forms.TextBox();
		this.m_destinationDeckText = new System.Windows.Forms.TextBox();
		this.label11 = new System.Windows.Forms.Label();
		this.label10 = new System.Windows.Forms.Label();
		this.button18 = new System.Windows.Forms.Button();
		this.button17 = new System.Windows.Forms.Button();
		this.button16 = new System.Windows.Forms.Button();
		this.button14 = new System.Windows.Forms.Button();
		this.button13 = new System.Windows.Forms.Button();
		this.button12 = new System.Windows.Forms.Button();
		this.button11 = new System.Windows.Forms.Button();
		this.button10 = new System.Windows.Forms.Button();
		this.button8 = new System.Windows.Forms.Button();
		this.button7 = new System.Windows.Forms.Button();
		this.m_sourceSlotTextBox = new System.Windows.Forms.TextBox();
		this.m_sourceDeckTextBox = new System.Windows.Forms.TextBox();
		this.groupBox11 = new System.Windows.Forms.GroupBox();
		this.m_getOffXRadio = new System.Windows.Forms.RadioButton();
		this.m_getOffYRadio = new System.Windows.Forms.RadioButton();
		this.m_getOffsetTB = new System.Windows.Forms.TextBox();
		this.m_getWithOffset = new System.Windows.Forms.Button();
		this.m_sensorCheckBox = new System.Windows.Forms.CheckBox();
		this.label16 = new System.Windows.Forms.Label();
		this.label15 = new System.Windows.Forms.Label();
		this.button4 = new System.Windows.Forms.Button();
		this.button3 = new System.Windows.Forms.Button();
		this.button2 = new System.Windows.Forms.Button();
		this.m_encoderUnitsTextBox = new System.Windows.Forms.TextBox();
		this.button1 = new System.Windows.Forms.Button();
		this.m_readPosButton = new System.Windows.Forms.Button();
		this.groupBox12 = new System.Windows.Forms.GroupBox();
		this.m_cameraWorkingButton = new System.Windows.Forms.Button();
		this.button19 = new System.Windows.Forms.Button();
		this.m_cameraPreview = new System.Windows.Forms.Button();
		this.button26 = new System.Windows.Forms.Button();
		this.m_startButtonCamera = new System.Windows.Forms.Button();
		this.m_snapImageButton = new System.Windows.Forms.Button();
		this.m_outputBox = new System.Windows.Forms.ListBox();
		this.m_clearOutputButton = new System.Windows.Forms.Button();
		this.label14 = new System.Windows.Forms.Label();
		this.m_advancedMode = new System.Windows.Forms.Button();
		this.m_versionTextBox = new System.Windows.Forms.TextBox();
		this.label19 = new System.Windows.Forms.Label();
		this.label20 = new System.Windows.Forms.Label();
		this.m_kioskIDTextBox = new System.Windows.Forms.TextBox();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.button22 = new System.Windows.Forms.Button();
		this.m_qlmTestButton = new System.Windows.Forms.Button();
		this.m_readLimitsButton = new System.Windows.Forms.Button();
		this.m_showTimeoutLogButton = new System.Windows.Forms.Button();
		this.button20 = new System.Windows.Forms.Button();
		this.button66 = new System.Windows.Forms.Button();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_deckConfigurationButton = new System.Windows.Forms.Button();
		this.m_takeDiskButton = new System.Windows.Forms.Button();
		this.m_openLogsButton = new System.Windows.Forms.Button();
		this.m_configureDevicesButton = new System.Windows.Forms.Button();
		this.groupBox2.SuspendLayout();
		this.groupBox3.SuspendLayout();
		this.groupBox4.SuspendLayout();
		this.groupBox6.SuspendLayout();
		this.groupBox7.SuspendLayout();
		this.groupBox8.SuspendLayout();
		this.m_qlmGroupBox.SuspendLayout();
		this.groupBox10.SuspendLayout();
		this.groupBox11.SuspendLayout();
		this.groupBox12.SuspendLayout();
		this.groupBox1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.groupBox2.Controls.Add(this.m_testArcusCommButton);
		this.groupBox2.Controls.Add(this.m_resetProteusButton);
		this.groupBox2.Controls.Add(this.m_homeYButton);
		this.groupBox2.Controls.Add(this.m_homeXButton);
		this.groupBox2.Controls.Add(this.button44);
		this.groupBox2.Controls.Add(this.m_runInitButton);
		this.groupBox2.Location = new System.Drawing.Point(13, 219);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(309, 130);
		this.groupBox2.TabIndex = 1;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "Init";
		this.m_testArcusCommButton.BackColor = System.Drawing.Color.LightGray;
		this.m_testArcusCommButton.Enabled = false;
		this.m_testArcusCommButton.Location = new System.Drawing.Point(204, 21);
		this.m_testArcusCommButton.Name = "m_testArcusCommButton";
		this.m_testArcusCommButton.Size = new System.Drawing.Size(93, 43);
		this.m_testArcusCommButton.TabIndex = 6;
		this.m_testArcusCommButton.Tag = "MOTIONCONTROL COMSTATUS";
		this.m_testArcusCommButton.Text = "Test Proteus Comm";
		this.m_testArcusCommButton.UseVisualStyleBackColor = false;
		this.m_testArcusCommButton.Click += new System.EventHandler(OnExecuteInstructionWithResult);
		this.m_resetProteusButton.BackColor = System.Drawing.Color.LightGray;
		this.m_resetProteusButton.Enabled = false;
		this.m_resetProteusButton.Location = new System.Drawing.Point(204, 79);
		this.m_resetProteusButton.Name = "m_resetProteusButton";
		this.m_resetProteusButton.Size = new System.Drawing.Size(93, 45);
		this.m_resetProteusButton.TabIndex = 5;
		this.m_resetProteusButton.Text = "Reset Proteus";
		this.m_resetProteusButton.UseVisualStyleBackColor = false;
		this.m_resetProteusButton.Click += new System.EventHandler(button9_Click);
		this.m_homeYButton.BackColor = System.Drawing.Color.LightGray;
		this.m_homeYButton.Enabled = false;
		this.m_homeYButton.Location = new System.Drawing.Point(105, 79);
		this.m_homeYButton.Name = "m_homeYButton";
		this.m_homeYButton.Size = new System.Drawing.Size(75, 45);
		this.m_homeYButton.TabIndex = 3;
		this.m_homeYButton.Tag = "HOMEY";
		this.m_homeYButton.Text = "Home Y";
		this.m_homeYButton.UseVisualStyleBackColor = false;
		this.m_homeYButton.Click += new System.EventHandler(OnExecuteInstructionWithResult);
		this.m_homeXButton.BackColor = System.Drawing.Color.LightGray;
		this.m_homeXButton.Enabled = false;
		this.m_homeXButton.Location = new System.Drawing.Point(6, 79);
		this.m_homeXButton.Name = "m_homeXButton";
		this.m_homeXButton.Size = new System.Drawing.Size(75, 45);
		this.m_homeXButton.TabIndex = 2;
		this.m_homeXButton.Tag = "HOMEX";
		this.m_homeXButton.Text = "Home X";
		this.m_homeXButton.UseVisualStyleBackColor = false;
		this.m_homeXButton.Click += new System.EventHandler(OnExecuteInstructionWithResult);
		this.button44.BackColor = System.Drawing.Color.GreenYellow;
		this.button44.Location = new System.Drawing.Point(105, 19);
		this.button44.Name = "button44";
		this.button44.Size = new System.Drawing.Size(75, 45);
		this.button44.TabIndex = 1;
		this.button44.Text = "Exit";
		this.button44.UseVisualStyleBackColor = false;
		this.button44.Click += new System.EventHandler(OnCloseForm);
		this.m_runInitButton.BackColor = System.Drawing.Color.LightGray;
		this.m_runInitButton.Enabled = false;
		this.m_runInitButton.Location = new System.Drawing.Point(6, 19);
		this.m_runInitButton.Name = "m_runInitButton";
		this.m_runInitButton.Size = new System.Drawing.Size(75, 45);
		this.m_runInitButton.TabIndex = 0;
		this.m_runInitButton.Text = "Init";
		this.m_runInitButton.UseVisualStyleBackColor = false;
		this.m_runInitButton.Click += new System.EventHandler(OnRunInit);
		this.m_fanButton.BackColor = System.Drawing.Color.LightGray;
		this.m_fanButton.Enabled = false;
		this.m_fanButton.Location = new System.Drawing.Point(6, 76);
		this.m_fanButton.Name = "m_fanButton";
		this.m_fanButton.Size = new System.Drawing.Size(93, 42);
		this.m_fanButton.TabIndex = 6;
		this.m_fanButton.Text = "Turn Off Xchgr Fan";
		this.m_fanButton.UseVisualStyleBackColor = false;
		this.m_fanButton.Click += new System.EventHandler(button6_Click);
		this.groupBox3.Controls.Add(this.m_fanButton);
		this.groupBox3.Controls.Add(this.m_closeSerialButton);
		this.groupBox3.Controls.Add(this.button50);
		this.groupBox3.Controls.Add(this.button47);
		this.groupBox3.Controls.Add(this.button5);
		this.groupBox3.Controls.Add(this.button57);
		this.groupBox3.Location = new System.Drawing.Point(13, 355);
		this.groupBox3.Name = "groupBox3";
		this.groupBox3.Size = new System.Drawing.Size(309, 135);
		this.groupBox3.TabIndex = 2;
		this.groupBox3.TabStop = false;
		this.groupBox3.Text = "Boards";
		this.m_closeSerialButton.BackColor = System.Drawing.Color.LightGray;
		this.m_closeSerialButton.Enabled = false;
		this.m_closeSerialButton.Location = new System.Drawing.Point(210, 19);
		this.m_closeSerialButton.Name = "m_closeSerialButton";
		this.m_closeSerialButton.Size = new System.Drawing.Size(87, 45);
		this.m_closeSerialButton.TabIndex = 4;
		this.m_closeSerialButton.Tag = "SERIALBOARD CLOSEPORT";
		this.m_closeSerialButton.Text = "Close Serial Port";
		this.m_closeSerialButton.UseVisualStyleBackColor = false;
		this.m_closeSerialButton.Click += new System.EventHandler(OnExecuteInstructionWithResult);
		this.button50.BackColor = System.Drawing.Color.LightGray;
		this.button50.Enabled = false;
		this.button50.Location = new System.Drawing.Point(210, 75);
		this.button50.Name = "button50";
		this.button50.Size = new System.Drawing.Size(93, 45);
		this.button50.TabIndex = 3;
		this.button50.Tag = "SERIALBOARD RESET";
		this.button50.Text = "Reset";
		this.button50.UseVisualStyleBackColor = false;
		this.button50.Click += new System.EventHandler(OnExecuteInstructionWithResult);
		this.button47.BackColor = System.Drawing.Color.LightGray;
		this.button47.Enabled = false;
		this.button47.Location = new System.Drawing.Point(111, 76);
		this.button47.Name = "button47";
		this.button47.Size = new System.Drawing.Size(93, 45);
		this.button47.TabIndex = 0;
		this.button47.Tag = "";
		this.button47.Text = "Board Test";
		this.button47.UseVisualStyleBackColor = false;
		this.button47.Click += new System.EventHandler(OnTestBoards);
		this.button5.BackColor = System.Drawing.Color.LightGray;
		this.button5.Enabled = false;
		this.button5.Location = new System.Drawing.Point(111, 19);
		this.button5.Name = "button5";
		this.button5.Size = new System.Drawing.Size(93, 45);
		this.button5.TabIndex = 0;
		this.button5.Tag = "AUX";
		this.button5.Text = "Read Aux Inputs";
		this.button5.UseVisualStyleBackColor = false;
		this.button5.Click += new System.EventHandler(OnReadSensors);
		this.button57.BackColor = System.Drawing.Color.LightGray;
		this.button57.Enabled = false;
		this.button57.Location = new System.Drawing.Point(6, 19);
		this.button57.Name = "button57";
		this.button57.Size = new System.Drawing.Size(93, 45);
		this.button57.TabIndex = 1;
		this.button57.Tag = "PICKER";
		this.button57.Text = "Read Picker Controller Inputs";
		this.button57.UseVisualStyleBackColor = false;
		this.button57.Click += new System.EventHandler(OnReadSensors);
		this.groupBox4.Controls.Add(this.button54);
		this.groupBox4.Controls.Add(this.button53);
		this.groupBox4.Controls.Add(this.button51);
		this.groupBox4.Location = new System.Drawing.Point(13, 496);
		this.groupBox4.Name = "groupBox4";
		this.groupBox4.Size = new System.Drawing.Size(309, 93);
		this.groupBox4.TabIndex = 3;
		this.groupBox4.TabStop = false;
		this.groupBox4.Text = "Vend Door";
		this.button54.BackColor = System.Drawing.Color.LightGray;
		this.button54.Enabled = false;
		this.button54.Location = new System.Drawing.Point(204, 25);
		this.button54.Name = "button54";
		this.button54.Size = new System.Drawing.Size(75, 45);
		this.button54.TabIndex = 3;
		this.button54.Tag = "VENDDOOR STATUS";
		this.button54.Text = "Check Status";
		this.button54.UseVisualStyleBackColor = false;
		this.button54.Click += new System.EventHandler(OnExecuteInstructionWithResult);
		this.button53.BackColor = System.Drawing.Color.LightGray;
		this.button53.Enabled = false;
		this.button53.Location = new System.Drawing.Point(105, 25);
		this.button53.Name = "button53";
		this.button53.Size = new System.Drawing.Size(75, 45);
		this.button53.TabIndex = 2;
		this.button53.Tag = "VENDDOOR CLOSE";
		this.button53.Text = "Close";
		this.button53.UseVisualStyleBackColor = false;
		this.button53.Click += new System.EventHandler(OnExecuteInstructionWithResult);
		this.button51.BackColor = System.Drawing.Color.LightGray;
		this.button51.Enabled = false;
		this.button51.Location = new System.Drawing.Point(6, 25);
		this.button51.Name = "button51";
		this.button51.Size = new System.Drawing.Size(75, 45);
		this.button51.TabIndex = 0;
		this.button51.Tag = "VENDDOOR RENT";
		this.button51.Text = "Rent";
		this.button51.UseVisualStyleBackColor = false;
		this.button51.Click += new System.EventHandler(OnExecuteInstructionWithResult);
		this.groupBox5.Location = new System.Drawing.Point(328, 9);
		this.groupBox5.Name = "groupBox5";
		this.groupBox5.Size = new System.Drawing.Size(373, 146);
		this.groupBox5.TabIndex = 4;
		this.groupBox5.TabStop = false;
		this.groupBox5.Text = "Sensors";
		this.groupBox6.Controls.Add(this.m_rollerTimeoutTextBox);
		this.groupBox6.Controls.Add(this.label17);
		this.groupBox6.Controls.Add(this.m_rollToSensorText);
		this.groupBox6.Controls.Add(this.button39);
		this.groupBox6.Controls.Add(this.button38);
		this.groupBox6.Controls.Add(this.button37);
		this.groupBox6.Controls.Add(this.button36);
		this.groupBox6.Location = new System.Drawing.Point(328, 161);
		this.groupBox6.Name = "groupBox6";
		this.groupBox6.Size = new System.Drawing.Size(373, 119);
		this.groupBox6.TabIndex = 5;
		this.groupBox6.TabStop = false;
		this.groupBox6.Text = "Rollers";
		this.m_rollerTimeoutTextBox.Location = new System.Drawing.Point(243, 35);
		this.m_rollerTimeoutTextBox.MaxLength = 5;
		this.m_rollerTimeoutTextBox.Name = "m_rollerTimeoutTextBox";
		this.m_rollerTimeoutTextBox.Size = new System.Drawing.Size(87, 20);
		this.m_rollerTimeoutTextBox.TabIndex = 6;
		this.label17.AutoSize = true;
		this.label17.Location = new System.Drawing.Point(240, 20);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(101, 13);
		this.label17.TabIndex = 5;
		this.label17.Text = "Roll To Timeout(ms)";
		this.m_rollToSensorText.Location = new System.Drawing.Point(240, 93);
		this.m_rollToSensorText.Name = "m_rollToSensorText";
		this.m_rollToSensorText.Size = new System.Drawing.Size(50, 20);
		this.m_rollToSensorText.TabIndex = 4;
		this.button39.BackColor = System.Drawing.Color.LightGray;
		this.button39.Enabled = false;
		this.button39.Location = new System.Drawing.Point(146, 68);
		this.button39.Name = "button39";
		this.button39.Size = new System.Drawing.Size(88, 45);
		this.button39.TabIndex = 3;
		this.button39.Text = "Roll To Sensor";
		this.button39.UseVisualStyleBackColor = false;
		this.button39.Click += new System.EventHandler(button39_Click);
		this.button38.BackColor = System.Drawing.Color.LightGray;
		this.button38.Enabled = false;
		this.button38.Location = new System.Drawing.Point(23, 69);
		this.button38.Name = "button38";
		this.button38.Size = new System.Drawing.Size(105, 45);
		this.button38.TabIndex = 2;
		this.button38.Tag = "ROLLER STOP";
		this.button38.Text = "Stop";
		this.button38.UseVisualStyleBackColor = false;
		this.button38.Click += new System.EventHandler(OnExecuteInstruction);
		this.button37.BackColor = System.Drawing.Color.LightGray;
		this.button37.Enabled = false;
		this.button37.Location = new System.Drawing.Point(146, 18);
		this.button37.Name = "button37";
		this.button37.Size = new System.Drawing.Size(88, 45);
		this.button37.TabIndex = 1;
		this.button37.Tag = "ROLLER OUT";
		this.button37.Text = "Out";
		this.button37.UseVisualStyleBackColor = false;
		this.button37.Click += new System.EventHandler(OnExecuteInstruction);
		this.button36.BackColor = System.Drawing.Color.LightGray;
		this.button36.Enabled = false;
		this.button36.Location = new System.Drawing.Point(23, 18);
		this.button36.Name = "button36";
		this.button36.Size = new System.Drawing.Size(105, 45);
		this.button36.TabIndex = 0;
		this.button36.Tag = "ROLLER IN";
		this.button36.Text = "In";
		this.button36.UseVisualStyleBackColor = false;
		this.button36.Click += new System.EventHandler(OnExecuteInstruction);
		this.groupBox7.Controls.Add(this.button64);
		this.groupBox7.Controls.Add(this.button35);
		this.groupBox7.Controls.Add(this.button34);
		this.groupBox7.Controls.Add(this.m_gripperSellButton);
		this.groupBox7.Controls.Add(this.button32);
		this.groupBox7.Controls.Add(this.button31);
		this.groupBox7.Location = new System.Drawing.Point(328, 286);
		this.groupBox7.Name = "groupBox7";
		this.groupBox7.Size = new System.Drawing.Size(373, 125);
		this.groupBox7.TabIndex = 6;
		this.groupBox7.TabStop = false;
		this.groupBox7.Text = "Gripper";
		this.button64.BackColor = System.Drawing.Color.LightGray;
		this.button64.Enabled = false;
		this.button64.Location = new System.Drawing.Point(10, 19);
		this.button64.Name = "button64";
		this.button64.Size = new System.Drawing.Size(97, 45);
		this.button64.TabIndex = 5;
		this.button64.Tag = "GRIPPER PEEK";
		this.button64.Text = "Check for DVD In Slot";
		this.button64.UseVisualStyleBackColor = false;
		this.button64.Click += new System.EventHandler(OnExecuteInstructionWithResult);
		this.button35.BackColor = System.Drawing.Color.LightGray;
		this.button35.Enabled = false;
		this.button35.Location = new System.Drawing.Point(241, 70);
		this.button35.Name = "button35";
		this.button35.Size = new System.Drawing.Size(105, 45);
		this.button35.TabIndex = 4;
		this.button35.Tag = "GRIPPER CLOSE";
		this.button35.Text = "Close";
		this.button35.UseVisualStyleBackColor = false;
		this.button35.Click += new System.EventHandler(OnExecuteInstructionWithResult);
		this.button34.BackColor = System.Drawing.Color.LightGray;
		this.button34.Enabled = false;
		this.button34.Location = new System.Drawing.Point(10, 70);
		this.button34.Name = "button34";
		this.button34.Size = new System.Drawing.Size(97, 45);
		this.button34.TabIndex = 3;
		this.button34.Tag = "GRIPPER RENT";
		this.button34.Text = "Rent";
		this.button34.UseVisualStyleBackColor = false;
		this.button34.Click += new System.EventHandler(OnExecuteInstructionWithResult);
		this.m_gripperSellButton.BackColor = System.Drawing.Color.LightGray;
		this.m_gripperSellButton.Enabled = false;
		this.m_gripperSellButton.Location = new System.Drawing.Point(125, 70);
		this.m_gripperSellButton.Name = "m_gripperSellButton";
		this.m_gripperSellButton.Size = new System.Drawing.Size(96, 45);
		this.m_gripperSellButton.TabIndex = 2;
		this.m_gripperSellButton.Tag = "GRIPPER OPEN";
		this.m_gripperSellButton.Text = "Sell";
		this.m_gripperSellButton.UseVisualStyleBackColor = false;
		this.m_gripperSellButton.Click += new System.EventHandler(OnExecuteInstructionWithResult);
		this.button32.BackColor = System.Drawing.Color.LightGray;
		this.button32.Enabled = false;
		this.button32.Location = new System.Drawing.Point(241, 19);
		this.button32.Name = "button32";
		this.button32.Size = new System.Drawing.Size(105, 45);
		this.button32.TabIndex = 1;
		this.button32.Tag = "GRIPPER RETRACT";
		this.button32.Text = "Retract";
		this.button32.UseVisualStyleBackColor = false;
		this.button32.Click += new System.EventHandler(OnExecuteInstructionWithResult);
		this.button31.BackColor = System.Drawing.Color.LightGray;
		this.button31.Enabled = false;
		this.button31.Location = new System.Drawing.Point(125, 19);
		this.button31.Name = "button31";
		this.button31.Size = new System.Drawing.Size(97, 45);
		this.button31.TabIndex = 0;
		this.button31.Tag = "GRIPPER EXTEND";
		this.button31.Text = "Extend";
		this.button31.UseVisualStyleBackColor = false;
		this.button31.Click += new System.EventHandler(OnExecuteInstructionWithResult);
		this.groupBox8.Controls.Add(this.button30);
		this.groupBox8.Controls.Add(this.button29);
		this.groupBox8.Controls.Add(this.button28);
		this.groupBox8.Location = new System.Drawing.Point(328, 417);
		this.groupBox8.Name = "groupBox8";
		this.groupBox8.Size = new System.Drawing.Size(373, 73);
		this.groupBox8.TabIndex = 7;
		this.groupBox8.TabStop = false;
		this.groupBox8.Text = "Track";
		this.button30.BackColor = System.Drawing.Color.LightGray;
		this.button30.Enabled = false;
		this.button30.Location = new System.Drawing.Point(249, 19);
		this.button30.Name = "button30";
		this.button30.Size = new System.Drawing.Size(97, 45);
		this.button30.TabIndex = 2;
		this.button30.Tag = "TRACK STATUS";
		this.button30.Text = "Status";
		this.button30.UseVisualStyleBackColor = false;
		this.button30.Click += new System.EventHandler(OnExecuteInstructionWithResult);
		this.button29.BackColor = System.Drawing.Color.LightGray;
		this.button29.Enabled = false;
		this.button29.Location = new System.Drawing.Point(125, 19);
		this.button29.Name = "button29";
		this.button29.Size = new System.Drawing.Size(108, 45);
		this.button29.TabIndex = 1;
		this.button29.Tag = "TRACK CLOSE";
		this.button29.Text = "Close";
		this.button29.UseVisualStyleBackColor = false;
		this.button29.Click += new System.EventHandler(OnExecuteInstructionWithResult);
		this.button28.BackColor = System.Drawing.Color.LightGray;
		this.button28.Enabled = false;
		this.button28.Location = new System.Drawing.Point(10, 19);
		this.button28.Name = "button28";
		this.button28.Size = new System.Drawing.Size(97, 45);
		this.button28.TabIndex = 0;
		this.button28.Tag = "TRACK OPEN";
		this.button28.Text = "Open";
		this.button28.UseVisualStyleBackColor = false;
		this.button28.Click += new System.EventHandler(OnExecuteInstructionWithResult);
		this.m_qlmGroupBox.Controls.Add(this.m_qlmDownButton);
		this.m_qlmGroupBox.Controls.Add(this.m_qlmUpButton);
		this.m_qlmGroupBox.Controls.Add(this.m_qlmDoorStatus);
		this.m_qlmGroupBox.Controls.Add(this.m_qlmCaseStatusButton);
		this.m_qlmGroupBox.Controls.Add(this.m_qlmStopButton);
		this.m_qlmGroupBox.Controls.Add(this.m_qlmDisengageButton);
		this.m_qlmGroupBox.Controls.Add(this.m_qlmEngageButton);
		this.m_qlmGroupBox.Location = new System.Drawing.Point(328, 497);
		this.m_qlmGroupBox.Name = "m_qlmGroupBox";
		this.m_qlmGroupBox.Size = new System.Drawing.Size(373, 118);
		this.m_qlmGroupBox.TabIndex = 8;
		this.m_qlmGroupBox.TabStop = false;
		this.m_qlmGroupBox.Text = "QLM";
		this.m_qlmDownButton.BackColor = System.Drawing.Color.LightGray;
		this.m_qlmDownButton.Enabled = false;
		this.m_qlmDownButton.Location = new System.Drawing.Point(90, 70);
		this.m_qlmDownButton.Name = "m_qlmDownButton";
		this.m_qlmDownButton.Size = new System.Drawing.Size(75, 40);
		this.m_qlmDownButton.TabIndex = 6;
		this.m_qlmDownButton.Tag = "";
		this.m_qlmDownButton.Text = "Down";
		this.m_qlmDownButton.UseVisualStyleBackColor = false;
		this.m_qlmDownButton.Click += new System.EventHandler(OnDropQLM);
		this.m_qlmUpButton.BackColor = System.Drawing.Color.LightGray;
		this.m_qlmUpButton.Enabled = false;
		this.m_qlmUpButton.Location = new System.Drawing.Point(10, 70);
		this.m_qlmUpButton.Name = "m_qlmUpButton";
		this.m_qlmUpButton.Size = new System.Drawing.Size(75, 40);
		this.m_qlmUpButton.TabIndex = 5;
		this.m_qlmUpButton.Tag = "QLM LIFT";
		this.m_qlmUpButton.Text = "Up";
		this.m_qlmUpButton.UseVisualStyleBackColor = false;
		this.m_qlmUpButton.Click += new System.EventHandler(OnExecuteInstruction);
		this.m_qlmDoorStatus.BackColor = System.Drawing.Color.LightGray;
		this.m_qlmDoorStatus.Enabled = false;
		this.m_qlmDoorStatus.Location = new System.Drawing.Point(262, 65);
		this.m_qlmDoorStatus.Name = "m_qlmDoorStatus";
		this.m_qlmDoorStatus.Size = new System.Drawing.Size(75, 45);
		this.m_qlmDoorStatus.TabIndex = 4;
		this.m_qlmDoorStatus.Tag = "QLMDOOR STATUS";
		this.m_qlmDoorStatus.Text = "Door Status";
		this.m_qlmDoorStatus.UseVisualStyleBackColor = false;
		this.m_qlmDoorStatus.Visible = false;
		this.m_qlmDoorStatus.Click += new System.EventHandler(OnExecuteInstructionWithResult);
		this.m_qlmCaseStatusButton.BackColor = System.Drawing.Color.LightGray;
		this.m_qlmCaseStatusButton.Enabled = false;
		this.m_qlmCaseStatusButton.Location = new System.Drawing.Point(255, 18);
		this.m_qlmCaseStatusButton.Name = "m_qlmCaseStatusButton";
		this.m_qlmCaseStatusButton.Size = new System.Drawing.Size(75, 45);
		this.m_qlmCaseStatusButton.TabIndex = 3;
		this.m_qlmCaseStatusButton.Tag = "QLM STATUS";
		this.m_qlmCaseStatusButton.Text = "Case Status";
		this.m_qlmCaseStatusButton.UseVisualStyleBackColor = false;
		this.m_qlmCaseStatusButton.Click += new System.EventHandler(OnExecuteInstructionWithResult);
		this.m_qlmStopButton.BackColor = System.Drawing.Color.LightGray;
		this.m_qlmStopButton.Enabled = false;
		this.m_qlmStopButton.Location = new System.Drawing.Point(172, 19);
		this.m_qlmStopButton.Name = "m_qlmStopButton";
		this.m_qlmStopButton.Size = new System.Drawing.Size(75, 45);
		this.m_qlmStopButton.TabIndex = 2;
		this.m_qlmStopButton.Tag = "QLM HALT";
		this.m_qlmStopButton.Text = "Stop";
		this.m_qlmStopButton.UseVisualStyleBackColor = false;
		this.m_qlmStopButton.Click += new System.EventHandler(OnExecuteInstruction);
		this.m_qlmDisengageButton.BackColor = System.Drawing.Color.LightGray;
		this.m_qlmDisengageButton.Enabled = false;
		this.m_qlmDisengageButton.Location = new System.Drawing.Point(91, 19);
		this.m_qlmDisengageButton.Name = "m_qlmDisengageButton";
		this.m_qlmDisengageButton.Size = new System.Drawing.Size(75, 45);
		this.m_qlmDisengageButton.TabIndex = 1;
		this.m_qlmDisengageButton.Tag = "QLM DISENGAGE";
		this.m_qlmDisengageButton.Text = "Disengage";
		this.m_qlmDisengageButton.UseVisualStyleBackColor = false;
		this.m_qlmDisengageButton.Click += new System.EventHandler(OnRunQlmDisengage);
		this.m_qlmEngageButton.BackColor = System.Drawing.Color.LightGray;
		this.m_qlmEngageButton.Enabled = false;
		this.m_qlmEngageButton.Location = new System.Drawing.Point(10, 19);
		this.m_qlmEngageButton.Name = "m_qlmEngageButton";
		this.m_qlmEngageButton.Size = new System.Drawing.Size(75, 45);
		this.m_qlmEngageButton.TabIndex = 0;
		this.m_qlmEngageButton.Tag = "";
		this.m_qlmEngageButton.Text = "Engage";
		this.m_qlmEngageButton.UseVisualStyleBackColor = false;
		this.m_qlmEngageButton.Click += new System.EventHandler(ExecuteEngageOrOther);
		this.groupBox10.Controls.Add(this.button63);
		this.groupBox10.Controls.Add(this.button62);
		this.groupBox10.Controls.Add(this.button61);
		this.groupBox10.Controls.Add(this.button60);
		this.groupBox10.Controls.Add(this.m_destinationSlotText);
		this.groupBox10.Controls.Add(this.m_destinationDeckText);
		this.groupBox10.Controls.Add(this.label11);
		this.groupBox10.Controls.Add(this.label10);
		this.groupBox10.Controls.Add(this.button18);
		this.groupBox10.Controls.Add(this.button17);
		this.groupBox10.Controls.Add(this.button16);
		this.groupBox10.Controls.Add(this.button14);
		this.groupBox10.Controls.Add(this.button13);
		this.groupBox10.Controls.Add(this.button12);
		this.groupBox10.Controls.Add(this.button11);
		this.groupBox10.Controls.Add(this.button10);
		this.groupBox10.Controls.Add(this.button8);
		this.groupBox10.Controls.Add(this.button7);
		this.groupBox10.Controls.Add(this.m_sourceSlotTextBox);
		this.groupBox10.Controls.Add(this.m_sourceDeckTextBox);
		this.groupBox10.Location = new System.Drawing.Point(720, 12);
		this.groupBox10.Name = "groupBox10";
		this.groupBox10.Size = new System.Drawing.Size(271, 358);
		this.groupBox10.TabIndex = 9;
		this.groupBox10.TabStop = false;
		this.groupBox10.Text = "Movements";
		this.button63.BackColor = System.Drawing.Color.LightGray;
		this.button63.Location = new System.Drawing.Point(133, 89);
		this.button63.Name = "button63";
		this.button63.Size = new System.Drawing.Size(56, 29);
		this.button63.TabIndex = 25;
		this.button63.Tag = "DestSlot";
		this.button63.Text = "Slot";
		this.button63.UseVisualStyleBackColor = false;
		this.button63.Click += new System.EventHandler(button60_Click);
		this.button62.BackColor = System.Drawing.Color.LightGray;
		this.button62.Location = new System.Drawing.Point(12, 89);
		this.button62.Name = "button62";
		this.button62.Size = new System.Drawing.Size(60, 29);
		this.button62.TabIndex = 24;
		this.button62.Tag = "DestDeck";
		this.button62.Text = "Deck";
		this.button62.UseVisualStyleBackColor = false;
		this.button62.Click += new System.EventHandler(button60_Click);
		this.button61.BackColor = System.Drawing.Color.LightGray;
		this.button61.Location = new System.Drawing.Point(133, 32);
		this.button61.Name = "button61";
		this.button61.Size = new System.Drawing.Size(56, 32);
		this.button61.TabIndex = 23;
		this.button61.Tag = "SourceSlot";
		this.button61.Text = "Slot";
		this.button61.UseVisualStyleBackColor = false;
		this.button61.Click += new System.EventHandler(button60_Click);
		this.button60.BackColor = System.Drawing.Color.LightGray;
		this.button60.Location = new System.Drawing.Point(13, 32);
		this.button60.Name = "button60";
		this.button60.Size = new System.Drawing.Size(59, 32);
		this.button60.TabIndex = 22;
		this.button60.Tag = "SourceDeck";
		this.button60.Text = "Deck";
		this.button60.UseVisualStyleBackColor = false;
		this.button60.Click += new System.EventHandler(button60_Click);
		this.m_destinationSlotText.Location = new System.Drawing.Point(203, 94);
		this.m_destinationSlotText.Name = "m_destinationSlotText";
		this.m_destinationSlotText.Size = new System.Drawing.Size(49, 20);
		this.m_destinationSlotText.TabIndex = 21;
		this.m_destinationDeckText.Location = new System.Drawing.Point(81, 95);
		this.m_destinationDeckText.Name = "m_destinationDeckText";
		this.m_destinationDeckText.Size = new System.Drawing.Size(40, 20);
		this.m_destinationDeckText.TabIndex = 20;
		this.label11.AutoSize = true;
		this.label11.Location = new System.Drawing.Point(11, 73);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(102, 13);
		this.label11.TabIndex = 17;
		this.label11.Text = "Transfer Destination";
		this.label10.AutoSize = true;
		this.label10.Location = new System.Drawing.Point(18, 19);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(41, 13);
		this.label10.TabIndex = 16;
		this.label10.Text = "Source";
		this.button18.BackColor = System.Drawing.Color.LightGray;
		this.button18.Enabled = false;
		this.button18.Location = new System.Drawing.Point(133, 308);
		this.button18.Name = "button18";
		this.button18.Size = new System.Drawing.Size(119, 40);
		this.button18.TabIndex = 15;
		this.button18.Text = "Put in Empty Slot";
		this.button18.UseVisualStyleBackColor = false;
		this.button18.Click += new System.EventHandler(OnPutInEmptySlot);
		this.button17.BackColor = System.Drawing.Color.LightGray;
		this.button17.Enabled = false;
		this.button17.Location = new System.Drawing.Point(14, 308);
		this.button17.Name = "button17";
		this.button17.Size = new System.Drawing.Size(102, 40);
		this.button17.TabIndex = 14;
		this.button17.Tag = "";
		this.button17.Text = "Pull DVD Into Picker";
		this.button17.UseVisualStyleBackColor = false;
		this.button17.Click += new System.EventHandler(button17_Click);
		this.button16.BackColor = System.Drawing.Color.LightGray;
		this.button16.Enabled = false;
		this.button16.Location = new System.Drawing.Point(14, 262);
		this.button16.Name = "button16";
		this.button16.Size = new System.Drawing.Size(102, 40);
		this.button16.TabIndex = 13;
		this.button16.Text = "Push DVD In Slot";
		this.button16.UseVisualStyleBackColor = false;
		this.button16.Click += new System.EventHandler(OnPushDVDInSlot);
		this.button14.BackColor = System.Drawing.Color.LightGray;
		this.button14.Enabled = false;
		this.button14.Location = new System.Drawing.Point(133, 262);
		this.button14.Name = "button14";
		this.button14.Size = new System.Drawing.Size(119, 40);
		this.button14.TabIndex = 11;
		this.button14.Text = "Vend DVD in Picker";
		this.button14.UseVisualStyleBackColor = false;
		this.button14.Click += new System.EventHandler(OnVendDVD);
		this.button13.BackColor = System.Drawing.Color.LightGray;
		this.button13.Enabled = false;
		this.button13.Location = new System.Drawing.Point(14, 216);
		this.button13.Name = "button13";
		this.button13.Size = new System.Drawing.Size(102, 40);
		this.button13.TabIndex = 10;
		this.button13.Tag = "MOVEVEND";
		this.button13.Text = "Move to Vend";
		this.button13.UseVisualStyleBackColor = false;
		this.button13.Click += new System.EventHandler(OnExecuteInstructionWithResult);
		this.button12.BackColor = System.Drawing.Color.LightGray;
		this.button12.Enabled = false;
		this.button12.Location = new System.Drawing.Point(133, 216);
		this.button12.Name = "button12";
		this.button12.Size = new System.Drawing.Size(119, 40);
		this.button12.TabIndex = 9;
		this.button12.Text = "Transfer";
		this.button12.UseVisualStyleBackColor = false;
		this.button12.Click += new System.EventHandler(OnTransfer);
		this.button11.BackColor = System.Drawing.Color.LightGray;
		this.button11.Enabled = false;
		this.button11.Location = new System.Drawing.Point(14, 170);
		this.button11.Name = "button11";
		this.button11.Size = new System.Drawing.Size(102, 40);
		this.button11.TabIndex = 8;
		this.button11.Text = "Get";
		this.button11.UseVisualStyleBackColor = false;
		this.button11.Click += new System.EventHandler(OnGetAndRead);
		this.button10.BackColor = System.Drawing.Color.LightGray;
		this.button10.Enabled = false;
		this.button10.Location = new System.Drawing.Point(133, 124);
		this.button10.Name = "button10";
		this.button10.Size = new System.Drawing.Size(119, 40);
		this.button10.TabIndex = 7;
		this.button10.Text = "Put";
		this.button10.UseVisualStyleBackColor = false;
		this.button10.Click += new System.EventHandler(OnPut);
		this.button8.BackColor = System.Drawing.Color.LightGray;
		this.button8.Enabled = false;
		this.button8.Location = new System.Drawing.Point(133, 170);
		this.button8.Name = "button8";
		this.button8.Size = new System.Drawing.Size(119, 40);
		this.button8.TabIndex = 5;
		this.button8.Text = "Sync Slot";
		this.button8.UseVisualStyleBackColor = false;
		this.button8.Click += new System.EventHandler(OnSyncSlot);
		this.button7.BackColor = System.Drawing.Color.LightGray;
		this.button7.Enabled = false;
		this.button7.Location = new System.Drawing.Point(14, 124);
		this.button7.Name = "button7";
		this.button7.Size = new System.Drawing.Size(102, 40);
		this.button7.TabIndex = 4;
		this.button7.Text = "Go To";
		this.button7.UseVisualStyleBackColor = false;
		this.button7.Click += new System.EventHandler(OnGotoSlot);
		this.m_sourceSlotTextBox.Location = new System.Drawing.Point(203, 39);
		this.m_sourceSlotTextBox.Name = "m_sourceSlotTextBox";
		this.m_sourceSlotTextBox.Size = new System.Drawing.Size(49, 20);
		this.m_sourceSlotTextBox.TabIndex = 3;
		this.m_sourceDeckTextBox.Location = new System.Drawing.Point(81, 39);
		this.m_sourceDeckTextBox.Name = "m_sourceDeckTextBox";
		this.m_sourceDeckTextBox.Size = new System.Drawing.Size(38, 20);
		this.m_sourceDeckTextBox.TabIndex = 0;
		this.groupBox11.Controls.Add(this.m_getOffXRadio);
		this.groupBox11.Controls.Add(this.m_getOffYRadio);
		this.groupBox11.Controls.Add(this.m_getOffsetTB);
		this.groupBox11.Controls.Add(this.m_getWithOffset);
		this.groupBox11.Controls.Add(this.m_sensorCheckBox);
		this.groupBox11.Controls.Add(this.label16);
		this.groupBox11.Controls.Add(this.label15);
		this.groupBox11.Controls.Add(this.button4);
		this.groupBox11.Controls.Add(this.button3);
		this.groupBox11.Controls.Add(this.button2);
		this.groupBox11.Controls.Add(this.m_encoderUnitsTextBox);
		this.groupBox11.Controls.Add(this.button1);
		this.groupBox11.Location = new System.Drawing.Point(720, 381);
		this.groupBox11.Name = "groupBox11";
		this.groupBox11.Size = new System.Drawing.Size(271, 193);
		this.groupBox11.TabIndex = 10;
		this.groupBox11.TabStop = false;
		this.groupBox11.Text = "X/Y Moves";
		this.m_getOffXRadio.AutoSize = true;
		this.m_getOffXRadio.Location = new System.Drawing.Point(93, 149);
		this.m_getOffXRadio.Name = "m_getOffXRadio";
		this.m_getOffXRadio.Size = new System.Drawing.Size(32, 17);
		this.m_getOffXRadio.TabIndex = 18;
		this.m_getOffXRadio.TabStop = true;
		this.m_getOffXRadio.Tag = "X";
		this.m_getOffXRadio.Text = "X";
		this.m_getOffXRadio.UseVisualStyleBackColor = true;
		this.m_getOffXRadio.CheckedChanged += new System.EventHandler(radioButton_CheckedChanged);
		this.m_getOffYRadio.AutoSize = true;
		this.m_getOffYRadio.Location = new System.Drawing.Point(93, 170);
		this.m_getOffYRadio.Name = "m_getOffYRadio";
		this.m_getOffYRadio.Size = new System.Drawing.Size(32, 17);
		this.m_getOffYRadio.TabIndex = 17;
		this.m_getOffYRadio.TabStop = true;
		this.m_getOffYRadio.Tag = "Y";
		this.m_getOffYRadio.Text = "Y";
		this.m_getOffYRadio.UseVisualStyleBackColor = true;
		this.m_getOffYRadio.CheckedChanged += new System.EventHandler(radioButton_CheckedChanged);
		this.m_getOffsetTB.Location = new System.Drawing.Point(6, 165);
		this.m_getOffsetTB.Name = "m_getOffsetTB";
		this.m_getOffsetTB.Size = new System.Drawing.Size(81, 20);
		this.m_getOffsetTB.TabIndex = 16;
		this.m_getWithOffset.BackColor = System.Drawing.Color.LightGray;
		this.m_getWithOffset.Enabled = false;
		this.m_getWithOffset.Location = new System.Drawing.Point(177, 146);
		this.m_getWithOffset.Name = "m_getWithOffset";
		this.m_getWithOffset.Size = new System.Drawing.Size(84, 41);
		this.m_getWithOffset.TabIndex = 15;
		this.m_getWithOffset.Text = "Get With Offset";
		this.m_getWithOffset.UseVisualStyleBackColor = false;
		this.m_getWithOffset.Click += new System.EventHandler(m_getWithOffset_Click);
		this.m_sensorCheckBox.AutoSize = true;
		this.m_sensorCheckBox.Location = new System.Drawing.Point(6, 134);
		this.m_sensorCheckBox.Name = "m_sensorCheckBox";
		this.m_sensorCheckBox.Size = new System.Drawing.Size(133, 17);
		this.m_sensorCheckBox.TabIndex = 14;
		this.m_sensorCheckBox.Text = "Without Sensor Check";
		this.m_sensorCheckBox.UseVisualStyleBackColor = true;
		this.label16.AutoSize = true;
		this.label16.Location = new System.Drawing.Point(174, 37);
		this.label16.Name = "label16";
		this.label16.Size = new System.Drawing.Size(94, 13);
		this.label16.TabIndex = 11;
		this.label16.Text = "Counter-clockwise";
		this.label15.AutoSize = true;
		this.label15.Location = new System.Drawing.Point(11, 37);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(55, 13);
		this.label15.TabIndex = 10;
		this.label15.Text = "Clockwise";
		this.button4.BackColor = System.Drawing.Color.LightGray;
		this.button4.Enabled = false;
		this.button4.Location = new System.Drawing.Point(87, 91);
		this.button4.Name = "button4";
		this.button4.Size = new System.Drawing.Size(75, 40);
		this.button4.TabIndex = 4;
		this.button4.Tag = "Down";
		this.button4.Text = "Down";
		this.button4.UseVisualStyleBackColor = false;
		this.button4.Click += new System.EventHandler(OnDirectionMove);
		this.button3.BackColor = System.Drawing.Color.LightGray;
		this.button3.Enabled = false;
		this.button3.Location = new System.Drawing.Point(87, 18);
		this.button3.Name = "button3";
		this.button3.Size = new System.Drawing.Size(75, 40);
		this.button3.TabIndex = 3;
		this.button3.Tag = "Up";
		this.button3.Text = "Up";
		this.button3.UseVisualStyleBackColor = false;
		this.button3.Click += new System.EventHandler(OnDirectionMove);
		this.button2.BackColor = System.Drawing.Color.LightGray;
		this.button2.Enabled = false;
		this.button2.Location = new System.Drawing.Point(177, 54);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(75, 40);
		this.button2.TabIndex = 2;
		this.button2.Tag = "Right";
		this.button2.Text = "Right";
		this.button2.UseVisualStyleBackColor = false;
		this.button2.Click += new System.EventHandler(OnDirectionMove);
		this.m_encoderUnitsTextBox.Location = new System.Drawing.Point(87, 65);
		this.m_encoderUnitsTextBox.Name = "m_encoderUnitsTextBox";
		this.m_encoderUnitsTextBox.Size = new System.Drawing.Size(75, 20);
		this.m_encoderUnitsTextBox.TabIndex = 1;
		this.button1.BackColor = System.Drawing.Color.LightGray;
		this.button1.Enabled = false;
		this.button1.Location = new System.Drawing.Point(0, 55);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(75, 40);
		this.button1.TabIndex = 0;
		this.button1.Tag = "Left";
		this.button1.Text = "Left";
		this.button1.UseVisualStyleBackColor = false;
		this.button1.Click += new System.EventHandler(OnDirectionMove);
		this.m_readPosButton.BackColor = System.Drawing.Color.LightGray;
		this.m_readPosButton.Enabled = false;
		this.m_readPosButton.Location = new System.Drawing.Point(118, 161);
		this.m_readPosButton.Name = "m_readPosButton";
		this.m_readPosButton.Size = new System.Drawing.Size(99, 45);
		this.m_readPosButton.TabIndex = 7;
		this.m_readPosButton.Tag = "";
		this.m_readPosButton.Text = "Read Positions";
		this.m_readPosButton.UseVisualStyleBackColor = false;
		this.m_readPosButton.Click += new System.EventHandler(OnReadPositions);
		this.groupBox12.Controls.Add(this.m_cameraWorkingButton);
		this.groupBox12.Controls.Add(this.button19);
		this.groupBox12.Controls.Add(this.m_cameraPreview);
		this.groupBox12.Controls.Add(this.button26);
		this.groupBox12.Controls.Add(this.m_startButtonCamera);
		this.groupBox12.Controls.Add(this.m_snapImageButton);
		this.groupBox12.Location = new System.Drawing.Point(328, 621);
		this.groupBox12.Name = "groupBox12";
		this.groupBox12.Size = new System.Drawing.Size(370, 107);
		this.groupBox12.TabIndex = 11;
		this.groupBox12.TabStop = false;
		this.groupBox12.Text = "Camera";
		this.m_cameraWorkingButton.BackColor = System.Drawing.Color.LightGray;
		this.m_cameraWorkingButton.Enabled = false;
		this.m_cameraWorkingButton.Location = new System.Drawing.Point(128, 62);
		this.m_cameraWorkingButton.Name = "m_cameraWorkingButton";
		this.m_cameraWorkingButton.Size = new System.Drawing.Size(93, 40);
		this.m_cameraWorkingButton.TabIndex = 5;
		this.m_cameraWorkingButton.Text = "Reset CCF Counter";
		this.m_cameraWorkingButton.UseVisualStyleBackColor = false;
		this.m_cameraWorkingButton.Click += new System.EventHandler(m_cameraWorkingButton_Click);
		this.button19.BackColor = System.Drawing.Color.LightGray;
		this.button19.Enabled = false;
		this.button19.Location = new System.Drawing.Point(262, 13);
		this.button19.Name = "button19";
		this.button19.Size = new System.Drawing.Size(93, 40);
		this.button19.TabIndex = 4;
		this.button19.Text = "Get and Read";
		this.button19.UseVisualStyleBackColor = false;
		this.button19.Click += new System.EventHandler(button19_Click);
		this.m_cameraPreview.BackColor = System.Drawing.Color.LightGray;
		this.m_cameraPreview.Enabled = false;
		this.m_cameraPreview.Location = new System.Drawing.Point(128, 13);
		this.m_cameraPreview.Name = "m_cameraPreview";
		this.m_cameraPreview.Size = new System.Drawing.Size(93, 40);
		this.m_cameraPreview.TabIndex = 3;
		this.m_cameraPreview.Text = "Camera Settings and Preview";
		this.m_cameraPreview.UseVisualStyleBackColor = false;
		this.m_cameraPreview.Click += new System.EventHandler(OnLaunchCameraProperties);
		this.button26.BackColor = System.Drawing.Color.LightGray;
		this.button26.Enabled = false;
		this.button26.Location = new System.Drawing.Point(262, 62);
		this.button26.Name = "button26";
		this.button26.Size = new System.Drawing.Size(93, 40);
		this.button26.TabIndex = 2;
		this.button26.Text = "Read Disk In Picker";
		this.button26.UseVisualStyleBackColor = false;
		this.button26.Click += new System.EventHandler(OnReadDiskInPicker);
		this.m_startButtonCamera.BackColor = System.Drawing.Color.LightGray;
		this.m_startButtonCamera.Enabled = false;
		this.m_startButtonCamera.Location = new System.Drawing.Point(10, 62);
		this.m_startButtonCamera.Name = "m_startButtonCamera";
		this.m_startButtonCamera.Size = new System.Drawing.Size(93, 40);
		this.m_startButtonCamera.TabIndex = 1;
		this.m_startButtonCamera.Tag = "";
		this.m_startButtonCamera.Text = "Turn Ringlight On";
		this.m_startButtonCamera.UseVisualStyleBackColor = false;
		this.m_startButtonCamera.Click += new System.EventHandler(OnStartCamera);
		this.m_snapImageButton.BackColor = System.Drawing.Color.LightGray;
		this.m_snapImageButton.Enabled = false;
		this.m_snapImageButton.Location = new System.Drawing.Point(10, 16);
		this.m_snapImageButton.Name = "m_snapImageButton";
		this.m_snapImageButton.Size = new System.Drawing.Size(93, 40);
		this.m_snapImageButton.TabIndex = 0;
		this.m_snapImageButton.Tag = "";
		this.m_snapImageButton.Text = "Snap";
		this.m_snapImageButton.UseVisualStyleBackColor = false;
		this.m_snapImageButton.Click += new System.EventHandler(OnDoSnap);
		this.m_outputBox.Font = new System.Drawing.Font("Arial", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.m_outputBox.FormattingEnabled = true;
		this.m_outputBox.HorizontalScrollbar = true;
		this.m_outputBox.ItemHeight = 14;
		this.m_outputBox.Location = new System.Drawing.Point(13, 25);
		this.m_outputBox.Name = "m_outputBox";
		this.m_outputBox.Size = new System.Drawing.Size(301, 130);
		this.m_outputBox.TabIndex = 12;
		this.m_clearOutputButton.BackColor = System.Drawing.Color.LightGray;
		this.m_clearOutputButton.Enabled = false;
		this.m_clearOutputButton.Location = new System.Drawing.Point(13, 161);
		this.m_clearOutputButton.Name = "m_clearOutputButton";
		this.m_clearOutputButton.Size = new System.Drawing.Size(99, 45);
		this.m_clearOutputButton.TabIndex = 14;
		this.m_clearOutputButton.Text = "Clear Output";
		this.m_clearOutputButton.UseVisualStyleBackColor = false;
		this.m_clearOutputButton.Click += new System.EventHandler(OnClearOutput);
		this.label14.AutoSize = true;
		this.label14.Location = new System.Drawing.Point(10, 9);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(39, 13);
		this.label14.TabIndex = 15;
		this.label14.Text = "Output";
		this.m_advancedMode.BackColor = System.Drawing.Color.LightGray;
		this.m_advancedMode.Enabled = false;
		this.m_advancedMode.Location = new System.Drawing.Point(720, 580);
		this.m_advancedMode.Name = "m_advancedMode";
		this.m_advancedMode.Size = new System.Drawing.Size(132, 40);
		this.m_advancedMode.TabIndex = 16;
		this.m_advancedMode.Text = "Advanced Mode";
		this.m_advancedMode.UseVisualStyleBackColor = false;
		this.m_advancedMode.Click += new System.EventHandler(button15_Click);
		this.m_versionTextBox.Location = new System.Drawing.Point(859, 676);
		this.m_versionTextBox.Name = "m_versionTextBox";
		this.m_versionTextBox.ReadOnly = true;
		this.m_versionTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_versionTextBox.TabIndex = 17;
		this.label19.AutoSize = true;
		this.label19.Location = new System.Drawing.Point(717, 683);
		this.label19.Name = "label19";
		this.label19.Size = new System.Drawing.Size(78, 13);
		this.label19.TabIndex = 18;
		this.label19.Text = "Tester Version:";
		this.label20.AutoSize = true;
		this.label20.Location = new System.Drawing.Point(717, 709);
		this.label20.Name = "label20";
		this.label20.Size = new System.Drawing.Size(50, 13);
		this.label20.TabIndex = 19;
		this.label20.Text = "Kiosk ID:";
		this.m_kioskIDTextBox.Location = new System.Drawing.Point(859, 702);
		this.m_kioskIDTextBox.Name = "m_kioskIDTextBox";
		this.m_kioskIDTextBox.ReadOnly = true;
		this.m_kioskIDTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_kioskIDTextBox.TabIndex = 20;
		this.m_kioskIDTextBox.Text = "UNKNOWN";
		this.groupBox1.Controls.Add(this.button22);
		this.groupBox1.Controls.Add(this.m_qlmTestButton);
		this.groupBox1.Controls.Add(this.m_readLimitsButton);
		this.groupBox1.Controls.Add(this.m_showTimeoutLogButton);
		this.groupBox1.Controls.Add(this.button20);
		this.groupBox1.Controls.Add(this.button66);
		this.groupBox1.Location = new System.Drawing.Point(13, 596);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(309, 132);
		this.groupBox1.TabIndex = 21;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Other";
		this.button22.BackColor = System.Drawing.Color.LightGray;
		this.button22.Enabled = false;
		this.button22.Location = new System.Drawing.Point(107, 17);
		this.button22.Name = "button22";
		this.button22.Size = new System.Drawing.Size(75, 46);
		this.button22.TabIndex = 7;
		this.button22.Text = "Sync Unknowns";
		this.button22.UseVisualStyleBackColor = false;
		this.button22.Click += new System.EventHandler(button22_Click);
		this.m_qlmTestButton.BackColor = System.Drawing.Color.LightGray;
		this.m_qlmTestButton.Enabled = false;
		this.m_qlmTestButton.Location = new System.Drawing.Point(8, 17);
		this.m_qlmTestButton.Name = "m_qlmTestButton";
		this.m_qlmTestButton.Size = new System.Drawing.Size(75, 46);
		this.m_qlmTestButton.TabIndex = 6;
		this.m_qlmTestButton.Text = "Qlm Slot Test";
		this.m_qlmTestButton.UseVisualStyleBackColor = false;
		this.m_qlmTestButton.Click += new System.EventHandler(button21_Click);
		this.m_readLimitsButton.BackColor = System.Drawing.Color.LightGray;
		this.m_readLimitsButton.Enabled = false;
		this.m_readLimitsButton.Location = new System.Drawing.Point(204, 69);
		this.m_readLimitsButton.Name = "m_readLimitsButton";
		this.m_readLimitsButton.Size = new System.Drawing.Size(75, 57);
		this.m_readLimitsButton.TabIndex = 5;
		this.m_readLimitsButton.Text = "Read Controller Limits";
		this.m_readLimitsButton.UseVisualStyleBackColor = false;
		this.m_readLimitsButton.Click += new System.EventHandler(m_readLimitsButton_Click);
		this.m_showTimeoutLogButton.BackColor = System.Drawing.Color.LightGray;
		this.m_showTimeoutLogButton.Enabled = false;
		this.m_showTimeoutLogButton.Location = new System.Drawing.Point(105, 69);
		this.m_showTimeoutLogButton.Name = "m_showTimeoutLogButton";
		this.m_showTimeoutLogButton.Size = new System.Drawing.Size(75, 57);
		this.m_showTimeoutLogButton.TabIndex = 4;
		this.m_showTimeoutLogButton.Text = "Show Timeouts";
		this.m_showTimeoutLogButton.UseVisualStyleBackColor = false;
		this.m_showTimeoutLogButton.Click += new System.EventHandler(m_showTimeoutLogButton_Click);
		this.button20.BackColor = System.Drawing.Color.LightGray;
		this.button20.Enabled = false;
		this.button20.Location = new System.Drawing.Point(6, 69);
		this.button20.Name = "button20";
		this.button20.Size = new System.Drawing.Size(75, 57);
		this.button20.TabIndex = 3;
		this.button20.Text = "Vertical Slot Test";
		this.button20.UseVisualStyleBackColor = false;
		this.button20.Click += new System.EventHandler(button20_Click);
		this.button66.BackColor = System.Drawing.Color.LightGray;
		this.button66.Enabled = false;
		this.button66.Location = new System.Drawing.Point(204, 18);
		this.button66.Name = "button66";
		this.button66.Size = new System.Drawing.Size(75, 45);
		this.button66.TabIndex = 2;
		this.button66.Text = "Hardware Check";
		this.button66.UseVisualStyleBackColor = false;
		this.button66.Click += new System.EventHandler(button66_Click);
		this.m_errorProvider.ContainerControl = this;
		this.m_deckConfigurationButton.BackColor = System.Drawing.Color.LightGray;
		this.m_deckConfigurationButton.Enabled = false;
		this.m_deckConfigurationButton.Location = new System.Drawing.Point(859, 580);
		this.m_deckConfigurationButton.Name = "m_deckConfigurationButton";
		this.m_deckConfigurationButton.Size = new System.Drawing.Size(132, 40);
		this.m_deckConfigurationButton.TabIndex = 23;
		this.m_deckConfigurationButton.Text = "Deck Configuration";
		this.m_deckConfigurationButton.UseVisualStyleBackColor = false;
		this.m_deckConfigurationButton.Click += new System.EventHandler(button58_Click);
		this.m_takeDiskButton.BackColor = System.Drawing.Color.LightGray;
		this.m_takeDiskButton.Enabled = false;
		this.m_takeDiskButton.Location = new System.Drawing.Point(721, 626);
		this.m_takeDiskButton.Name = "m_takeDiskButton";
		this.m_takeDiskButton.Size = new System.Drawing.Size(132, 44);
		this.m_takeDiskButton.TabIndex = 27;
		this.m_takeDiskButton.Text = "Engineering Tests";
		this.m_takeDiskButton.UseVisualStyleBackColor = false;
		this.m_takeDiskButton.Click += new System.EventHandler(button65_Click);
		this.m_openLogsButton.BackColor = System.Drawing.Color.LightGray;
		this.m_openLogsButton.Enabled = false;
		this.m_openLogsButton.Location = new System.Drawing.Point(223, 161);
		this.m_openLogsButton.Name = "m_openLogsButton";
		this.m_openLogsButton.Size = new System.Drawing.Size(99, 45);
		this.m_openLogsButton.TabIndex = 28;
		this.m_openLogsButton.Text = "Open Error Logs";
		this.m_openLogsButton.UseVisualStyleBackColor = false;
		this.m_openLogsButton.Click += new System.EventHandler(button59_Click);
		this.m_configureDevicesButton.BackColor = System.Drawing.Color.LightGray;
		this.m_configureDevicesButton.Enabled = false;
		this.m_configureDevicesButton.Location = new System.Drawing.Point(859, 626);
		this.m_configureDevicesButton.Name = "m_configureDevicesButton";
		this.m_configureDevicesButton.Size = new System.Drawing.Size(132, 44);
		this.m_configureDevicesButton.TabIndex = 29;
		this.m_configureDevicesButton.Text = "Configure Devices";
		this.m_configureDevicesButton.UseVisualStyleBackColor = false;
		this.m_configureDevicesButton.Click += new System.EventHandler(button67_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.WhiteSmoke;
		base.ClientSize = new System.Drawing.Size(1013, 746);
		base.Controls.Add(this.m_configureDevicesButton);
		base.Controls.Add(this.m_openLogsButton);
		base.Controls.Add(this.m_takeDiskButton);
		base.Controls.Add(this.m_deckConfigurationButton);
		base.Controls.Add(this.m_readPosButton);
		base.Controls.Add(this.groupBox1);
		base.Controls.Add(this.m_kioskIDTextBox);
		base.Controls.Add(this.label20);
		base.Controls.Add(this.label19);
		base.Controls.Add(this.m_versionTextBox);
		base.Controls.Add(this.m_advancedMode);
		base.Controls.Add(this.label14);
		base.Controls.Add(this.m_clearOutputButton);
		base.Controls.Add(this.m_outputBox);
		base.Controls.Add(this.groupBox12);
		base.Controls.Add(this.groupBox11);
		base.Controls.Add(this.groupBox10);
		base.Controls.Add(this.m_qlmGroupBox);
		base.Controls.Add(this.groupBox8);
		base.Controls.Add(this.groupBox7);
		base.Controls.Add(this.groupBox6);
		base.Controls.Add(this.groupBox5);
		base.Controls.Add(this.groupBox4);
		base.Controls.Add(this.groupBox3);
		base.Controls.Add(this.groupBox2);
		base.Name = "Form1";
		this.Text = "HAL Tester";
		this.groupBox2.ResumeLayout(false);
		this.groupBox3.ResumeLayout(false);
		this.groupBox4.ResumeLayout(false);
		this.groupBox6.ResumeLayout(false);
		this.groupBox6.PerformLayout();
		this.groupBox7.ResumeLayout(false);
		this.groupBox8.ResumeLayout(false);
		this.m_qlmGroupBox.ResumeLayout(false);
		this.groupBox10.ResumeLayout(false);
		this.groupBox10.PerformLayout();
		this.groupBox11.ResumeLayout(false);
		this.groupBox11.PerformLayout();
		this.groupBox12.ResumeLayout(false);
		this.groupBox1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
