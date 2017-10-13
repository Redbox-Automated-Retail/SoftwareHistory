using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy;

namespace Redbox.KioskServices.KernelExtension;

public class HealthServicesPreferencePage : UserControl, IPreferencePageHost
{
	private bool m_isLoaded;

	private IContainer components;

	private Button m_pingButton;

	private TextBox m_communicationsStatusTextBox;

	private Label label5;

	private TextBox m_remoteServerStatusTextBox;

	private Label label4;

	private CheckBox m_simulateServerBusyCheckBox;

	private CheckBox m_simulateCommunicationsFailureCheckBox;

	private TextBox m_recipientsTextBox;

	private Label label3;

	private TextBox m_smtpServerTextBox;

	private Label label2;

	private TextBox m_pingPeriodTextBox;

	private Label label1;

	private CheckBox m_liveCallCheckBox;

	private GroupBox groupBox1;

	private Label label6;

	private ErrorProvider m_errorProvider;

	private Timer m_timer;

	private Label m_headerLabel;

	public HealthServicesPreferencePage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		m_isLoaded = false;
		m_liveCallCheckBox.Checked = HealthServicesProxy.Instance.LiveCall;
		m_pingPeriodTextBox.Text = HealthServicesProxy.Instance.PingPeriod.ToString();
		m_smtpServerTextBox.Text = HealthServicesProxy.Instance.SmtpServer;
		m_recipientsTextBox.Text = HealthServicesProxy.Instance.Recipients;
		m_remoteServerStatusTextBox.Text = HealthServicesProxy.Instance.RemoteServicesStatus;
		m_simulateServerBusyCheckBox.Checked = HealthServicesProxy.Instance.SimulateServerBusyError;
		m_communicationsStatusTextBox.Text = (HealthServicesProxy.Instance.IsOnline ? "Online" : "Offline");
		m_simulateCommunicationsFailureCheckBox.Checked = HealthServicesProxy.Instance.SimulateCommunicationsFailure;
		m_isLoaded = true;
	}

	private void OnPing(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			HealthServicesProxy.Instance.Ping(StoreServicesProxy.Instance.StoreNumber);
		}
	}

	private void OnLeavePingPeriodTextBox(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			m_errorProvider.SetError(m_pingPeriodTextBox, string.Empty);
			if (!int.TryParse(m_pingPeriodTextBox.Text, out var result) || result < 0)
			{
				m_errorProvider.SetError(m_pingPeriodTextBox, "Ping period must be a valid integer value.");
			}
			else
			{
				HealthServicesProxy.Instance.PingPeriod = result;
			}
		}
	}

	private void OnLiveCallCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			HealthServicesProxy.Instance.LiveCall = m_liveCallCheckBox.Checked;
		}
	}

	private void OnLeaveSmtpServer(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			HealthServicesProxy.Instance.SmtpServer = m_smtpServerTextBox.Text;
		}
	}

	private void OnLeaveRecipients(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			HealthServicesProxy.Instance.Recipients = m_recipientsTextBox.Text;
		}
	}

	private void OnSimulateServerBusyCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			HealthServicesProxy.Instance.SimulateServerBusyError = m_simulateServerBusyCheckBox.Checked;
		}
	}

	private void OnSimulateCommunicationsFailureChecked(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			HealthServicesProxy.Instance.SimulateCommunicationsFailure = m_simulateCommunicationsFailureCheckBox.Checked;
		}
	}

	private void OnTick(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			m_remoteServerStatusTextBox.Text = HealthServicesProxy.Instance.RemoteServicesStatus;
			m_communicationsStatusTextBox.Text = (HealthServicesProxy.Instance.IsOnline ? "Online" : "Offline");
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
		this.components = new System.ComponentModel.Container();
		this.m_pingButton = new System.Windows.Forms.Button();
		this.m_communicationsStatusTextBox = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.m_remoteServerStatusTextBox = new System.Windows.Forms.TextBox();
		this.label4 = new System.Windows.Forms.Label();
		this.m_simulateServerBusyCheckBox = new System.Windows.Forms.CheckBox();
		this.m_simulateCommunicationsFailureCheckBox = new System.Windows.Forms.CheckBox();
		this.m_recipientsTextBox = new System.Windows.Forms.TextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.m_smtpServerTextBox = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.m_pingPeriodTextBox = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.m_liveCallCheckBox = new System.Windows.Forms.CheckBox();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.label6 = new System.Windows.Forms.Label();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_timer = new System.Windows.Forms.Timer(this.components);
		this.m_headerLabel = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_pingButton.Location = new System.Drawing.Point(170, 239);
		this.m_pingButton.Name = "m_pingButton";
		this.m_pingButton.Size = new System.Drawing.Size(152, 23);
		this.m_pingButton.TabIndex = 9;
		this.m_pingButton.Text = "Ping";
		this.m_pingButton.UseVisualStyleBackColor = true;
		this.m_pingButton.Click += new System.EventHandler(OnPing);
		this.m_communicationsStatusTextBox.Location = new System.Drawing.Point(170, 213);
		this.m_communicationsStatusTextBox.Name = "m_communicationsStatusTextBox";
		this.m_communicationsStatusTextBox.ReadOnly = true;
		this.m_communicationsStatusTextBox.Size = new System.Drawing.Size(152, 20);
		this.m_communicationsStatusTextBox.TabIndex = 8;
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(49, 216);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(115, 13);
		this.label5.TabIndex = 7;
		this.label5.Text = "Communication Status:";
		this.m_remoteServerStatusTextBox.Location = new System.Drawing.Point(170, 187);
		this.m_remoteServerStatusTextBox.Name = "m_remoteServerStatusTextBox";
		this.m_remoteServerStatusTextBox.ReadOnly = true;
		this.m_remoteServerStatusTextBox.Size = new System.Drawing.Size(152, 20);
		this.m_remoteServerStatusTextBox.TabIndex = 6;
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(40, 190);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(124, 13);
		this.label4.TabIndex = 5;
		this.label4.Text = "Remote Services Status:";
		this.m_simulateServerBusyCheckBox.AutoSize = true;
		this.m_simulateServerBusyCheckBox.Location = new System.Drawing.Point(38, 102);
		this.m_simulateServerBusyCheckBox.Name = "m_simulateServerBusyCheckBox";
		this.m_simulateServerBusyCheckBox.Size = new System.Drawing.Size(126, 17);
		this.m_simulateServerBusyCheckBox.TabIndex = 2;
		this.m_simulateServerBusyCheckBox.Text = "Simulate Server Busy";
		this.m_simulateServerBusyCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateServerBusyCheckBox.CheckedChanged += new System.EventHandler(OnSimulateServerBusyCheckChanged);
		this.m_simulateCommunicationsFailureCheckBox.AutoSize = true;
		this.m_simulateCommunicationsFailureCheckBox.Location = new System.Drawing.Point(38, 79);
		this.m_simulateCommunicationsFailureCheckBox.Name = "m_simulateCommunicationsFailureCheckBox";
		this.m_simulateCommunicationsFailureCheckBox.Size = new System.Drawing.Size(180, 17);
		this.m_simulateCommunicationsFailureCheckBox.TabIndex = 1;
		this.m_simulateCommunicationsFailureCheckBox.Text = "Simulate Communications Failure";
		this.m_simulateCommunicationsFailureCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateCommunicationsFailureCheckBox.CheckedChanged += new System.EventHandler(OnSimulateCommunicationsFailureChecked);
		this.m_recipientsTextBox.Location = new System.Drawing.Point(170, 324);
		this.m_recipientsTextBox.Name = "m_recipientsTextBox";
		this.m_recipientsTextBox.Size = new System.Drawing.Size(342, 20);
		this.m_recipientsTextBox.TabIndex = 15;
		this.m_recipientsTextBox.Leave += new System.EventHandler(OnLeaveRecipients);
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(104, 327);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(60, 13);
		this.label3.TabIndex = 14;
		this.label3.Text = "Recipients:";
		this.m_smtpServerTextBox.Location = new System.Drawing.Point(170, 298);
		this.m_smtpServerTextBox.Name = "m_smtpServerTextBox";
		this.m_smtpServerTextBox.Size = new System.Drawing.Size(342, 20);
		this.m_smtpServerTextBox.TabIndex = 13;
		this.m_smtpServerTextBox.Leave += new System.EventHandler(OnLeaveSmtpServer);
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(90, 301);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(74, 13);
		this.label2.TabIndex = 12;
		this.label2.Text = "SMTP Server:";
		this.m_pingPeriodTextBox.Location = new System.Drawing.Point(170, 161);
		this.m_pingPeriodTextBox.Name = "m_pingPeriodTextBox";
		this.m_pingPeriodTextBox.Size = new System.Drawing.Size(152, 20);
		this.m_pingPeriodTextBox.TabIndex = 4;
		this.m_pingPeriodTextBox.Leave += new System.EventHandler(OnLeavePingPeriodTextBox);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(35, 164);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(129, 13);
		this.label1.TabIndex = 3;
		this.label1.Text = "Ping Period (milliseconds):";
		this.m_liveCallCheckBox.AutoSize = true;
		this.m_liveCallCheckBox.Location = new System.Drawing.Point(38, 56);
		this.m_liveCallCheckBox.Name = "m_liveCallCheckBox";
		this.m_liveCallCheckBox.Size = new System.Drawing.Size(66, 17);
		this.m_liveCallCheckBox.TabIndex = 0;
		this.m_liveCallCheckBox.Text = "Live Call";
		this.m_liveCallCheckBox.UseVisualStyleBackColor = true;
		this.m_liveCallCheckBox.CheckedChanged += new System.EventHandler(OnLiveCallCheckChanged);
		this.groupBox1.Location = new System.Drawing.Point(38, 279);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(495, 3);
		this.groupBox1.TabIndex = 10;
		this.groupBox1.TabStop = false;
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(50, 275);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(122, 13);
		this.label6.TabIndex = 11;
		this.label6.Text = "Local Test Configuration";
		this.m_errorProvider.ContainerControl = this;
		this.m_timer.Interval = 5000;
		this.m_timer.Tick += new System.EventHandler(OnTick);
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkCyan;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 39;
		this.m_headerLabel.Text = "  Kiosk Services: Health";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.groupBox1);
		base.Controls.Add(this.m_pingButton);
		base.Controls.Add(this.m_communicationsStatusTextBox);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.m_remoteServerStatusTextBox);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.m_simulateServerBusyCheckBox);
		base.Controls.Add(this.m_simulateCommunicationsFailureCheckBox);
		base.Controls.Add(this.m_recipientsTextBox);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.m_smtpServerTextBox);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.m_pingPeriodTextBox);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_liveCallCheckBox);
		base.Name = "HealthServicesPreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
