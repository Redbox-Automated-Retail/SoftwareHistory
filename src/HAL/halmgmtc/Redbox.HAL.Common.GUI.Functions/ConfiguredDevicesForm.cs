using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Redbox.HAL.Client;
using Redbox.HAL.Core;

namespace Redbox.HAL.Common.GUI.Functions;

public class ConfiguredDevicesForm : Form
{
	private bool m_cortexConfigured;

	private IContainer components;

	private TabControl m_devicesTab;

	private TabPage m_quickReturnTab;

	private TabPage m_auxRelayBoardPanel;

	private TabPage m_airExchangerTab;

	private Label m_boardStatusLabel;

	private Button m_powerOffAuxUSBButton;

	private Button m_powerAuxBoardButton;

	private Button m_configureAuxBoardButton;

	private Label m_iceQubeExchangerLabel;

	private Button m_configureQubeButton;

	private Button m_resetQubeBoard;

	private Button m_qubeStatusButton;

	private TextBox m_qubeTextBox;

	private Label label2;

	private Label m_powerRelayConfiguredLabel;

	private Label label1;

	private TextBox m_auxRelayOutput;

	private Button m_powerResetButton;

	private Button m_resetPersistentCounter;

	private Button m_exitButton;

	private TabPage m_cortexTab;

	private Button m_installCortexButton;

	private Button m_uninstallCortexButton;

	private Label label3;

	private ListBox m_cortexStatusBox;

	private Button m_startCortexButton;

	private ExternalDeviceConfiguration DeviceConfiguration { get; set; }

	private HardwareService Service { get; set; }

	private TextWriter LogFile { get; set; }

	private ButtonAspectsManager Manager { get; set; }

	public ConfiguredDevicesForm(HardwareService service, TextWriter log)
	{
		InitializeComponent();
		Service = service;
		DeviceConfiguration = new ExternalDeviceConfiguration(service, loadDevices: true);
		UserControl value = new QRInstallHelper(Service)
		{
			Dock = DockStyle.Fill
		};
		m_quickReturnTab.Controls.Add(value);
		m_devicesTab.Selected += OnTabSelectedChange;
		Manager = new ButtonAspectsManager();
	}

	private void ConfigureAuxRelayBoard(bool configured)
	{
		m_powerRelayConfiguredLabel.Text = string.Format("The board {0} configured.", configured ? "is" : "is not");
		m_powerAuxBoardButton.Enabled = configured;
		m_powerOffAuxUSBButton.Enabled = configured;
		m_powerResetButton.Enabled = configured;
		m_configureAuxBoardButton.Text = (configured ? "Remove" : "Configure");
	}

	private void ConfigureCortexPanel()
	{
		HardwareCommandResult configuration = Service.GetConfiguration("camera");
		if (!configuration.Success)
		{
			return;
		}
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.LoadXml(configuration.CommandMessages[0]);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An exception was thrown when trying to load configuration from Hal", e, LogEntryType.Error);
			return;
		}
		foreach (XmlNode childNode in xmlDocument.DocumentElement.ChildNodes)
		{
			if (childNode.Name == "property" && childNode.GetAttributeValue<string>("name") == "ScannerService")
			{
				m_cortexConfigured = childNode.GetAttributeValue<string>("value") == "Cortex";
				break;
			}
		}
		m_installCortexButton.Enabled = !m_cortexConfigured;
		m_uninstallCortexButton.Enabled = m_cortexConfigured;
		WriteToBox(string.Format("The camera {0} enabled.", m_cortexConfigured ? "is" : "is not"));
	}

	private void ConfigureIceQubeBoard(bool configured)
	{
		m_iceQubeExchangerLabel.Text = string.Format("The board {0} configured.", configured ? "is" : "is not");
		m_configureQubeButton.Text = (configured ? "Remove" : "Configure");
		m_resetQubeBoard.Enabled = configured;
		m_qubeStatusButton.Enabled = configured;
		m_resetPersistentCounter.Enabled = configured;
	}

	private void OnTabSelectedChange(object sender, EventArgs e)
	{
		if (Enum.IsDefined(typeof(PanelIndex), m_devicesTab.SelectedIndex))
		{
			PanelIndex panelIndex = (PanelIndex)Enum.ToObject(typeof(PanelIndex), m_devicesTab.SelectedIndex);
			WriteToOutput($"Change to panel {panelIndex.ToString()}");
			switch (panelIndex)
			{
			case PanelIndex.AuxBoardRelay:
				ConfigureAuxRelayBoard(AuxPowerBoardConfigured());
				break;
			case PanelIndex.IceQubeBoard:
				ConfigureIceQubeBoard(IceQubeBoardConfigured());
				break;
			case PanelIndex.CortexCamera:
				ConfigureCortexPanel();
				break;
			case PanelIndex.QuickReturn:
				break;
			}
		}
	}

	private void WriteToOutput(string msg)
	{
		if (LogFile != null)
		{
			LogFile.WriteLine(msg);
		}
	}

	private bool IceQubeBoardConfigured()
	{
		HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(Service, string.Format("{0} ISCONFIGURED", "AIRXCHGR"));
		if (hardwareJob == null)
		{
			m_iceQubeExchangerLabel.Text = "Command failed - unknown configuration.";
			return false;
		}
		string topOfStack = CommonFunctions.GetTopOfStack(hardwareJob);
		bool flag = false;
		try
		{
			return bool.Parse(topOfStack);
		}
		catch
		{
			return false;
		}
	}

	private bool AuxPowerBoardConfigured()
	{
		HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(Service, string.Format("{0} ISCONFIGURED", "AUXPOWERRELAYBOARD"));
		if (hardwareJob == null)
		{
			m_powerRelayConfiguredLabel.Text = "Command failed - unknown configuration.";
			return false;
		}
		string topOfStack = CommonFunctions.GetTopOfStack(hardwareJob);
		bool flag = false;
		try
		{
			return bool.Parse(topOfStack);
		}
		catch
		{
			return false;
		}
	}

	private void m_configureQubeButton_Click(object sender, EventArgs e)
	{
		bool flag = IceQubeBoardConfigured();
		HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(Service, string.Format("{0} {1}", "AIRXCHGR", flag ? "DISABLE" : "ENABLE"));
		if (hardwareJob == null)
		{
			m_iceQubeExchangerLabel.Text = "Command failed - unknown configuration.";
			return;
		}
		string topOfStack = CommonFunctions.GetTopOfStack(hardwareJob);
		try
		{
			flag = bool.Parse(topOfStack);
		}
		catch
		{
			flag = false;
		}
		ConfigureIceQubeBoard(flag);
	}

	private void m_configureAuxBoardButton_Click(object sender, EventArgs e)
	{
		bool flag = AuxPowerBoardConfigured();
		HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(Service, string.Format("{0} {1}", "AUXPOWERRELAYBOARD", flag ? "DISABLE" : "ENABLE"));
		if (hardwareJob == null)
		{
			m_iceQubeExchangerLabel.Text = "Command failed - unknown configuration.";
			return;
		}
		string topOfStack = CommonFunctions.GetTopOfStack(hardwareJob);
		try
		{
			flag = bool.Parse(topOfStack);
		}
		catch
		{
			flag = false;
		}
		ConfigureAuxRelayBoard(flag);
	}

	private void OnExecuteAuxBoardCommand(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			string tagFromButton = GetTagFromButton(sender);
			if (string.IsNullOrEmpty(tagFromButton))
			{
				m_auxRelayOutput.Text = "Instruction failure.";
				return;
			}
			string instruction = string.Format("{0} {1}", "AUXPOWERRELAYBOARD", tagFromButton);
			HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(Service, instruction);
			if (hardwareJob == null)
			{
				m_auxRelayOutput.Text = "Instruction failed.";
			}
			else
			{
				m_auxRelayOutput.Text = CommonFunctions.GetTopOfStack(hardwareJob);
			}
		}
	}

	private void OnExecuteIceQubeBoardCommand(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			string tagFromButton = GetTagFromButton(sender);
			if (string.IsNullOrEmpty(tagFromButton))
			{
				m_qubeTextBox.Text = "Instruction failure.";
				return;
			}
			string instruction = string.Format("{0} {1}", "AIRXCHGR", tagFromButton);
			HardwareJob hardwareJob = CommonFunctions.ExecuteInstruction(Service, instruction);
			if (hardwareJob == null)
			{
				m_qubeTextBox.Text = "Instruction failed.";
			}
			else
			{
				m_qubeTextBox.Text = CommonFunctions.GetTopOfStack(hardwareJob);
			}
		}
	}

	private string GetTagFromButton(object sender)
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

	private void m_exitButton_Click(object sender, EventArgs e)
	{
		base.DialogResult = DialogResult.OK;
		Close();
	}

	private void m_installCortexButton_Click(object sender, EventArgs e)
	{
		ToggleCortex(sender, install: true);
	}

	private void m_uninstallCortexButton_Click(object sender, EventArgs e)
	{
		ToggleCortex(sender, install: false);
	}

	private void ToggleCortex(object sender, bool install)
	{
		using (Manager.MakeAspect(sender))
		{
			string statement = string.Format("SETCFG \"ScannerService\" \"{0}\" TYPE=CAMERA", install ? "Cortex" : "Legacy");
			if (Service.ExecuteImmediate(statement, out var job).Success)
			{
				WriteToBox("Successfully changed camera configuration.");
				PerfFunctions.Wait(2000);
				WriteToBox("Probing ports for device...");
				if (install)
				{
					job = CommonFunctions.ExecuteInstruction(Service, "CAMERA SCANFORPORT");
					if (job != null)
					{
						string topOfStack = CommonFunctions.GetTopOfStack(job);
						if (topOfStack == "NONE")
						{
							WriteToBox("Device probe failed; install unsuccessful.");
						}
						else
						{
							statement = $"SETCFG \"SnapDecodePort\" \"{topOfStack}\" TYPE=CAMERA";
							if (Service.ExecuteImmediate(statement, out job).Success)
							{
								WriteToBox($"The camera was successfully installed on port {topOfStack}");
							}
							else
							{
								WriteToBox("There was an error during installation.");
							}
						}
					}
				}
				else
				{
					Service.ExecuteImmediate("SETCFG \"SnapDecodePort\" \"NONE\" TYPE=CAMERA", out job);
				}
				m_installCortexButton.Enabled = !install;
				Button uninstallCortexButton = m_uninstallCortexButton;
				bool enabled = (m_startCortexButton.Enabled = install);
				uninstallCortexButton.Enabled = enabled;
			}
			else
			{
				WriteToBox("Unable to change camera configuration.");
			}
		}
	}

	private void WriteToBox(string msg)
	{
		if (m_cortexStatusBox.InvokeRequired)
		{
			SetTextCallback method = WriteToBox;
			Invoke(method, msg);
		}
		else
		{
			string item = $"{DateTime.Now.ToLongTimeString()} {msg}";
			m_cortexStatusBox.Items.Insert(0, item);
		}
	}

	private void button1_Click(object sender, EventArgs e)
	{
		using (Manager.MakeAspect(sender))
		{
			WriteToBox("Waiting for camera to start - could take up to 15s.");
			HardwareJob job = CommonFunctions.ExecuteInstruction(Service, "CAMERA START");
			WriteToBox(CommonFunctions.GetTopOfStack(job).Equals("true", StringComparison.CurrentCultureIgnoreCase) ? "Camera started." : "Camera DIDN'T START.");
		}
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
		this.m_devicesTab = new System.Windows.Forms.TabControl();
		this.m_quickReturnTab = new System.Windows.Forms.TabPage();
		this.m_auxRelayBoardPanel = new System.Windows.Forms.TabPage();
		this.m_powerResetButton = new System.Windows.Forms.Button();
		this.m_auxRelayOutput = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.m_powerRelayConfiguredLabel = new System.Windows.Forms.Label();
		this.m_configureAuxBoardButton = new System.Windows.Forms.Button();
		this.m_powerOffAuxUSBButton = new System.Windows.Forms.Button();
		this.m_powerAuxBoardButton = new System.Windows.Forms.Button();
		this.m_boardStatusLabel = new System.Windows.Forms.Label();
		this.m_airExchangerTab = new System.Windows.Forms.TabPage();
		this.m_resetPersistentCounter = new System.Windows.Forms.Button();
		this.m_qubeTextBox = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.m_qubeStatusButton = new System.Windows.Forms.Button();
		this.m_resetQubeBoard = new System.Windows.Forms.Button();
		this.m_configureQubeButton = new System.Windows.Forms.Button();
		this.m_iceQubeExchangerLabel = new System.Windows.Forms.Label();
		this.m_cortexTab = new System.Windows.Forms.TabPage();
		this.m_startCortexButton = new System.Windows.Forms.Button();
		this.m_cortexStatusBox = new System.Windows.Forms.ListBox();
		this.label3 = new System.Windows.Forms.Label();
		this.m_uninstallCortexButton = new System.Windows.Forms.Button();
		this.m_installCortexButton = new System.Windows.Forms.Button();
		this.m_exitButton = new System.Windows.Forms.Button();
		this.m_devicesTab.SuspendLayout();
		this.m_auxRelayBoardPanel.SuspendLayout();
		this.m_airExchangerTab.SuspendLayout();
		this.m_cortexTab.SuspendLayout();
		base.SuspendLayout();
		this.m_devicesTab.Controls.Add(this.m_quickReturnTab);
		this.m_devicesTab.Controls.Add(this.m_auxRelayBoardPanel);
		this.m_devicesTab.Controls.Add(this.m_airExchangerTab);
		this.m_devicesTab.Controls.Add(this.m_cortexTab);
		this.m_devicesTab.Location = new System.Drawing.Point(2, 12);
		this.m_devicesTab.Name = "m_devicesTab";
		this.m_devicesTab.Padding = new System.Drawing.Point(10, 20);
		this.m_devicesTab.SelectedIndex = 0;
		this.m_devicesTab.Size = new System.Drawing.Size(951, 626);
		this.m_devicesTab.TabIndex = 0;
		this.m_quickReturnTab.BackColor = System.Drawing.Color.LightGray;
		this.m_quickReturnTab.Location = new System.Drawing.Point(4, 56);
		this.m_quickReturnTab.Name = "m_quickReturnTab";
		this.m_quickReturnTab.Padding = new System.Windows.Forms.Padding(3);
		this.m_quickReturnTab.Size = new System.Drawing.Size(943, 566);
		this.m_quickReturnTab.TabIndex = 0;
		this.m_quickReturnTab.Text = "Quick Return Device";
		this.m_auxRelayBoardPanel.BackColor = System.Drawing.Color.LightGray;
		this.m_auxRelayBoardPanel.Controls.Add(this.m_powerResetButton);
		this.m_auxRelayBoardPanel.Controls.Add(this.m_auxRelayOutput);
		this.m_auxRelayBoardPanel.Controls.Add(this.label1);
		this.m_auxRelayBoardPanel.Controls.Add(this.m_powerRelayConfiguredLabel);
		this.m_auxRelayBoardPanel.Controls.Add(this.m_configureAuxBoardButton);
		this.m_auxRelayBoardPanel.Controls.Add(this.m_powerOffAuxUSBButton);
		this.m_auxRelayBoardPanel.Controls.Add(this.m_powerAuxBoardButton);
		this.m_auxRelayBoardPanel.Controls.Add(this.m_boardStatusLabel);
		this.m_auxRelayBoardPanel.Location = new System.Drawing.Point(4, 56);
		this.m_auxRelayBoardPanel.Name = "m_auxRelayBoardPanel";
		this.m_auxRelayBoardPanel.Padding = new System.Windows.Forms.Padding(3);
		this.m_auxRelayBoardPanel.Size = new System.Drawing.Size(943, 566);
		this.m_auxRelayBoardPanel.TabIndex = 1;
		this.m_auxRelayBoardPanel.Text = "Aux Relay Board";
		this.m_powerResetButton.Location = new System.Drawing.Point(690, 102);
		this.m_powerResetButton.Name = "m_powerResetButton";
		this.m_powerResetButton.Size = new System.Drawing.Size(200, 120);
		this.m_powerResetButton.TabIndex = 7;
		this.m_powerResetButton.Tag = "RESET";
		this.m_powerResetButton.Text = "Reset";
		this.m_powerResetButton.UseVisualStyleBackColor = true;
		this.m_powerResetButton.Click += new System.EventHandler(OnExecuteAuxBoardCommand);
		this.m_auxRelayOutput.BackColor = System.Drawing.Color.White;
		this.m_auxRelayOutput.Location = new System.Drawing.Point(80, 261);
		this.m_auxRelayOutput.Name = "m_auxRelayOutput";
		this.m_auxRelayOutput.ReadOnly = true;
		this.m_auxRelayOutput.Size = new System.Drawing.Size(181, 20);
		this.m_auxRelayOutput.TabIndex = 6;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(32, 268);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(42, 13);
		this.label1.TabIndex = 5;
		this.label1.Text = "Output:";
		this.m_powerRelayConfiguredLabel.AutoSize = true;
		this.m_powerRelayConfiguredLabel.Location = new System.Drawing.Point(23, 27);
		this.m_powerRelayConfiguredLabel.Name = "m_powerRelayConfiguredLabel";
		this.m_powerRelayConfiguredLabel.Size = new System.Drawing.Size(35, 13);
		this.m_powerRelayConfiguredLabel.TabIndex = 4;
		this.m_powerRelayConfiguredLabel.Text = "label1";
		this.m_configureAuxBoardButton.Location = new System.Drawing.Point(22, 102);
		this.m_configureAuxBoardButton.Name = "m_configureAuxBoardButton";
		this.m_configureAuxBoardButton.Size = new System.Drawing.Size(200, 120);
		this.m_configureAuxBoardButton.TabIndex = 3;
		this.m_configureAuxBoardButton.Text = "button1";
		this.m_configureAuxBoardButton.UseVisualStyleBackColor = true;
		this.m_configureAuxBoardButton.Click += new System.EventHandler(m_configureAuxBoardButton_Click);
		this.m_powerOffAuxUSBButton.Location = new System.Drawing.Point(461, 102);
		this.m_powerOffAuxUSBButton.Name = "m_powerOffAuxUSBButton";
		this.m_powerOffAuxUSBButton.Size = new System.Drawing.Size(200, 120);
		this.m_powerOffAuxUSBButton.TabIndex = 2;
		this.m_powerOffAuxUSBButton.Tag = "KILLPOWER";
		this.m_powerOffAuxUSBButton.Text = "Power Off";
		this.m_powerOffAuxUSBButton.UseVisualStyleBackColor = true;
		this.m_powerOffAuxUSBButton.Click += new System.EventHandler(OnExecuteAuxBoardCommand);
		this.m_powerAuxBoardButton.Location = new System.Drawing.Point(242, 102);
		this.m_powerAuxBoardButton.Name = "m_powerAuxBoardButton";
		this.m_powerAuxBoardButton.Size = new System.Drawing.Size(200, 120);
		this.m_powerAuxBoardButton.TabIndex = 1;
		this.m_powerAuxBoardButton.Tag = "POWERON";
		this.m_powerAuxBoardButton.Text = "Power On";
		this.m_powerAuxBoardButton.UseVisualStyleBackColor = true;
		this.m_powerAuxBoardButton.Click += new System.EventHandler(OnExecuteAuxBoardCommand);
		this.m_boardStatusLabel.AutoSize = true;
		this.m_boardStatusLabel.Location = new System.Drawing.Point(19, 24);
		this.m_boardStatusLabel.Name = "m_boardStatusLabel";
		this.m_boardStatusLabel.Size = new System.Drawing.Size(0, 13);
		this.m_boardStatusLabel.TabIndex = 0;
		this.m_airExchangerTab.BackColor = System.Drawing.Color.LightGray;
		this.m_airExchangerTab.Controls.Add(this.m_resetPersistentCounter);
		this.m_airExchangerTab.Controls.Add(this.m_qubeTextBox);
		this.m_airExchangerTab.Controls.Add(this.label2);
		this.m_airExchangerTab.Controls.Add(this.m_qubeStatusButton);
		this.m_airExchangerTab.Controls.Add(this.m_resetQubeBoard);
		this.m_airExchangerTab.Controls.Add(this.m_configureQubeButton);
		this.m_airExchangerTab.Controls.Add(this.m_iceQubeExchangerLabel);
		this.m_airExchangerTab.Location = new System.Drawing.Point(4, 56);
		this.m_airExchangerTab.Name = "m_airExchangerTab";
		this.m_airExchangerTab.Padding = new System.Windows.Forms.Padding(3);
		this.m_airExchangerTab.Size = new System.Drawing.Size(943, 566);
		this.m_airExchangerTab.TabIndex = 3;
		this.m_airExchangerTab.Text = "Ice Qube Air Exchanger";
		this.m_resetPersistentCounter.Location = new System.Drawing.Point(644, 102);
		this.m_resetPersistentCounter.Name = "m_resetPersistentCounter";
		this.m_resetPersistentCounter.Size = new System.Drawing.Size(200, 120);
		this.m_resetPersistentCounter.TabIndex = 6;
		this.m_resetPersistentCounter.Tag = "RESETFAILURECOUNTER";
		this.m_resetPersistentCounter.Text = "Clear Error Status";
		this.m_resetPersistentCounter.UseVisualStyleBackColor = true;
		this.m_resetPersistentCounter.Click += new System.EventHandler(OnExecuteIceQubeBoardCommand);
		this.m_qubeTextBox.BackColor = System.Drawing.Color.White;
		this.m_qubeTextBox.Location = new System.Drawing.Point(84, 276);
		this.m_qubeTextBox.Name = "m_qubeTextBox";
		this.m_qubeTextBox.ReadOnly = true;
		this.m_qubeTextBox.Size = new System.Drawing.Size(301, 20);
		this.m_qubeTextBox.TabIndex = 5;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(23, 279);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(42, 13);
		this.label2.TabIndex = 4;
		this.label2.Text = "Output:";
		this.m_qubeStatusButton.Location = new System.Drawing.Point(232, 102);
		this.m_qubeStatusButton.Name = "m_qubeStatusButton";
		this.m_qubeStatusButton.Size = new System.Drawing.Size(200, 120);
		this.m_qubeStatusButton.TabIndex = 3;
		this.m_qubeStatusButton.Tag = "BOARDSTATUS";
		this.m_qubeStatusButton.Text = "Exchanger Board Status";
		this.m_qubeStatusButton.UseVisualStyleBackColor = true;
		this.m_qubeStatusButton.Click += new System.EventHandler(OnExecuteIceQubeBoardCommand);
		this.m_resetQubeBoard.Location = new System.Drawing.Point(438, 102);
		this.m_resetQubeBoard.Name = "m_resetQubeBoard";
		this.m_resetQubeBoard.Size = new System.Drawing.Size(200, 120);
		this.m_resetQubeBoard.TabIndex = 2;
		this.m_resetQubeBoard.Tag = "RESET";
		this.m_resetQubeBoard.Text = "Reset Board";
		this.m_resetQubeBoard.UseVisualStyleBackColor = true;
		this.m_resetQubeBoard.Click += new System.EventHandler(OnExecuteIceQubeBoardCommand);
		this.m_configureQubeButton.Location = new System.Drawing.Point(26, 102);
		this.m_configureQubeButton.Name = "m_configureQubeButton";
		this.m_configureQubeButton.Size = new System.Drawing.Size(200, 120);
		this.m_configureQubeButton.TabIndex = 1;
		this.m_configureQubeButton.Text = "button1";
		this.m_configureQubeButton.UseVisualStyleBackColor = true;
		this.m_configureQubeButton.Click += new System.EventHandler(m_configureQubeButton_Click);
		this.m_iceQubeExchangerLabel.AutoSize = true;
		this.m_iceQubeExchangerLabel.Location = new System.Drawing.Point(23, 27);
		this.m_iceQubeExchangerLabel.Name = "m_iceQubeExchangerLabel";
		this.m_iceQubeExchangerLabel.Size = new System.Drawing.Size(35, 13);
		this.m_iceQubeExchangerLabel.TabIndex = 0;
		this.m_iceQubeExchangerLabel.Text = "label1";
		this.m_cortexTab.BackColor = System.Drawing.Color.LightGray;
		this.m_cortexTab.Controls.Add(this.m_startCortexButton);
		this.m_cortexTab.Controls.Add(this.m_cortexStatusBox);
		this.m_cortexTab.Controls.Add(this.label3);
		this.m_cortexTab.Controls.Add(this.m_uninstallCortexButton);
		this.m_cortexTab.Controls.Add(this.m_installCortexButton);
		this.m_cortexTab.Location = new System.Drawing.Point(4, 56);
		this.m_cortexTab.Name = "m_cortexTab";
		this.m_cortexTab.Padding = new System.Windows.Forms.Padding(3);
		this.m_cortexTab.Size = new System.Drawing.Size(943, 566);
		this.m_cortexTab.TabIndex = 4;
		this.m_cortexTab.Text = "Cortex Camera";
		this.m_startCortexButton.Enabled = false;
		this.m_startCortexButton.Location = new System.Drawing.Point(375, 51);
		this.m_startCortexButton.Name = "m_startCortexButton";
		this.m_startCortexButton.Size = new System.Drawing.Size(150, 75);
		this.m_startCortexButton.TabIndex = 4;
		this.m_startCortexButton.Text = "Start Camera";
		this.m_startCortexButton.UseVisualStyleBackColor = true;
		this.m_startCortexButton.Click += new System.EventHandler(button1_Click);
		this.m_cortexStatusBox.FormattingEnabled = true;
		this.m_cortexStatusBox.Location = new System.Drawing.Point(72, 161);
		this.m_cortexStatusBox.Name = "m_cortexStatusBox";
		this.m_cortexStatusBox.Size = new System.Drawing.Size(453, 290);
		this.m_cortexStatusBox.TabIndex = 3;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(16, 161);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(40, 13);
		this.label3.TabIndex = 2;
		this.label3.Text = "Status:";
		this.m_uninstallCortexButton.Location = new System.Drawing.Point(189, 51);
		this.m_uninstallCortexButton.Name = "m_uninstallCortexButton";
		this.m_uninstallCortexButton.Size = new System.Drawing.Size(150, 75);
		this.m_uninstallCortexButton.TabIndex = 1;
		this.m_uninstallCortexButton.Text = "Uninstall";
		this.m_uninstallCortexButton.UseVisualStyleBackColor = true;
		this.m_uninstallCortexButton.Click += new System.EventHandler(m_uninstallCortexButton_Click);
		this.m_installCortexButton.Location = new System.Drawing.Point(19, 51);
		this.m_installCortexButton.Name = "m_installCortexButton";
		this.m_installCortexButton.Size = new System.Drawing.Size(150, 75);
		this.m_installCortexButton.TabIndex = 0;
		this.m_installCortexButton.Text = "Install";
		this.m_installCortexButton.UseVisualStyleBackColor = true;
		this.m_installCortexButton.Click += new System.EventHandler(m_installCortexButton_Click);
		this.m_exitButton.BackColor = System.Drawing.Color.GreenYellow;
		this.m_exitButton.Location = new System.Drawing.Point(12, 644);
		this.m_exitButton.Name = "m_exitButton";
		this.m_exitButton.Size = new System.Drawing.Size(135, 59);
		this.m_exitButton.TabIndex = 1;
		this.m_exitButton.Text = "Exit";
		this.m_exitButton.UseVisualStyleBackColor = false;
		this.m_exitButton.Click += new System.EventHandler(m_exitButton_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(975, 715);
		base.Controls.Add(this.m_exitButton);
		base.Controls.Add(this.m_devicesTab);
		base.MaximizeBox = false;
		base.Name = "ConfiguredDevicesForm";
		this.Text = "ConfiguredDevicesForm";
		this.m_devicesTab.ResumeLayout(false);
		this.m_auxRelayBoardPanel.ResumeLayout(false);
		this.m_auxRelayBoardPanel.PerformLayout();
		this.m_airExchangerTab.ResumeLayout(false);
		this.m_airExchangerTab.PerformLayout();
		this.m_cortexTab.ResumeLayout(false);
		this.m_cortexTab.PerformLayout();
		base.ResumeLayout(false);
	}
}
