using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.BrokerServices.Proxy;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy;

namespace Redbox.BrokerServices.KernelExtension;

public class GeneralPreferencePage : UserControl, IPreferencePageHost
{
	private IContainer components;

	private TextBox m_brokerServicesTimeoutTextBox;

	private Label label4;

	private TextBox m_brokerServicesUrlTextBox;

	private Label label5;

	private ErrorProvider m_errorProvider;

	private CheckBox m_liveCallCheckBox;

	private CheckBox m_simulateServerBusyCheckBox;

	private CheckBox m_simulateCommunicationsErrorCheckBox;

	private Label m_headerLabel;

	private Panel panel1;

	private Button btnCancel;

	private Label label2;

	private TextBox txtRefNumber;

	private Button btnValidate;

	public GeneralPreferencePage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		return true;
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		m_liveCallCheckBox.Checked = ReservationServicesProxy.Instance.LiveCall;
		m_brokerServicesUrlTextBox.Text = BrokerServicesProxy.Instance.Url;
		m_brokerServicesTimeoutTextBox.Text = BrokerServicesProxy.Instance.Timeout.ToString();
		m_simulateServerBusyCheckBox.Checked = ReservationServicesProxy.Instance.SimulateServerBusyError;
		m_simulateCommunicationsErrorCheckBox.Checked = ReservationServicesProxy.Instance.SimulateCommunicationsFailure;
	}

	private void OnLeaveBrokerServicesUrlTextBox(object sender, EventArgs e)
	{
		string url = BrokerServicesProxy.Instance.Url;
		if (!string.IsNullOrEmpty(m_brokerServicesUrlTextBox.Text))
		{
			url = m_brokerServicesUrlTextBox.Text;
		}
		BrokerServicesProxy.Instance.Url = url;
	}

	private void OnLeaveBrokerServicesTimeoutTextBox(object sender, EventArgs e)
	{
		if (int.TryParse(m_brokerServicesTimeoutTextBox.Text, out var result))
		{
			BrokerServicesProxy.Instance.Timeout = result;
		}
	}

	private void OnLiveCallCheckChanged(object sender, EventArgs e)
	{
		ReservationServicesProxy.Instance.LiveCall = m_liveCallCheckBox.Checked;
	}

	private void OnSimulateServerBusyCheckChanged(object sender, EventArgs e)
	{
		ReservationServicesProxy.Instance.SimulateServerBusyError = m_simulateServerBusyCheckBox.Checked;
	}

	private void OnSimulateCommunicationsErrorCheckChanged(object sender, EventArgs e)
	{
		ReservationServicesProxy.Instance.SimulateCommunicationsFailure = m_simulateCommunicationsErrorCheckBox.Checked;
	}

	private void btnCancel_Click(object sender, EventArgs e)
	{
		ReservationServicesProxy.Instance.CancelReservation(long.Parse(txtRefNumber.Text));
	}

	private void btnValidate_Click(object sender, EventArgs e)
	{
		ReservationServicesProxy.Instance.ValidateReservation(StoreServicesProxy.Instance.StoreNumber, long.Parse(txtRefNumber.Text), null);
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
		this.m_brokerServicesTimeoutTextBox = new System.Windows.Forms.TextBox();
		this.label4 = new System.Windows.Forms.Label();
		this.m_brokerServicesUrlTextBox = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_liveCallCheckBox = new System.Windows.Forms.CheckBox();
		this.m_simulateServerBusyCheckBox = new System.Windows.Forms.CheckBox();
		this.m_simulateCommunicationsErrorCheckBox = new System.Windows.Forms.CheckBox();
		this.m_headerLabel = new System.Windows.Forms.Label();
		this.panel1 = new System.Windows.Forms.Panel();
		this.btnCancel = new System.Windows.Forms.Button();
		this.label2 = new System.Windows.Forms.Label();
		this.txtRefNumber = new System.Windows.Forms.TextBox();
		this.btnValidate = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		this.panel1.SuspendLayout();
		base.SuspendLayout();
		this.m_brokerServicesTimeoutTextBox.Location = new System.Drawing.Point(107, 108);
		this.m_brokerServicesTimeoutTextBox.MaxLength = 6;
		this.m_brokerServicesTimeoutTextBox.Name = "m_brokerServicesTimeoutTextBox";
		this.m_brokerServicesTimeoutTextBox.Size = new System.Drawing.Size(100, 20);
		this.m_brokerServicesTimeoutTextBox.TabIndex = 4;
		this.m_brokerServicesTimeoutTextBox.Leave += new System.EventHandler(OnLeaveBrokerServicesTimeoutTextBox);
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(14, 111);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(87, 13);
		this.label4.TabIndex = 3;
		this.label4.Text = "Service Timeout:";
		this.m_brokerServicesUrlTextBox.Location = new System.Drawing.Point(107, 81);
		this.m_brokerServicesUrlTextBox.Name = "m_brokerServicesUrlTextBox";
		this.m_brokerServicesUrlTextBox.Size = new System.Drawing.Size(399, 20);
		this.m_brokerServicesUrlTextBox.TabIndex = 2;
		this.m_brokerServicesUrlTextBox.Leave += new System.EventHandler(OnLeaveBrokerServicesUrlTextBox);
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(30, 84);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(71, 13);
		this.label5.TabIndex = 1;
		this.label5.Text = "Service URL:";
		this.m_errorProvider.ContainerControl = this;
		this.m_liveCallCheckBox.AutoSize = true;
		this.m_liveCallCheckBox.Location = new System.Drawing.Point(107, 58);
		this.m_liveCallCheckBox.Name = "m_liveCallCheckBox";
		this.m_liveCallCheckBox.Size = new System.Drawing.Size(66, 17);
		this.m_liveCallCheckBox.TabIndex = 0;
		this.m_liveCallCheckBox.Text = "Live Call";
		this.m_liveCallCheckBox.UseVisualStyleBackColor = true;
		this.m_liveCallCheckBox.CheckedChanged += new System.EventHandler(OnLiveCallCheckChanged);
		this.m_simulateServerBusyCheckBox.AutoSize = true;
		this.m_simulateServerBusyCheckBox.Location = new System.Drawing.Point(107, 157);
		this.m_simulateServerBusyCheckBox.Name = "m_simulateServerBusyCheckBox";
		this.m_simulateServerBusyCheckBox.Size = new System.Drawing.Size(126, 17);
		this.m_simulateServerBusyCheckBox.TabIndex = 6;
		this.m_simulateServerBusyCheckBox.Text = "Simulate Server Busy";
		this.m_simulateServerBusyCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateServerBusyCheckBox.CheckedChanged += new System.EventHandler(OnSimulateServerBusyCheckChanged);
		this.m_simulateCommunicationsErrorCheckBox.AutoSize = true;
		this.m_simulateCommunicationsErrorCheckBox.Location = new System.Drawing.Point(107, 134);
		this.m_simulateCommunicationsErrorCheckBox.Name = "m_simulateCommunicationsErrorCheckBox";
		this.m_simulateCommunicationsErrorCheckBox.Size = new System.Drawing.Size(171, 17);
		this.m_simulateCommunicationsErrorCheckBox.TabIndex = 5;
		this.m_simulateCommunicationsErrorCheckBox.Text = "Simulate Communications Error";
		this.m_simulateCommunicationsErrorCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateCommunicationsErrorCheckBox.CheckedChanged += new System.EventHandler(OnSimulateCommunicationsErrorCheckChanged);
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkMagenta;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 7;
		this.m_headerLabel.Text = "  Broker Service: General";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panel1.Controls.Add(this.btnValidate);
		this.panel1.Controls.Add(this.btnCancel);
		this.panel1.Controls.Add(this.label2);
		this.panel1.Controls.Add(this.txtRefNumber);
		this.panel1.Location = new System.Drawing.Point(17, 356);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(509, 56);
		this.panel1.TabIndex = 8;
		this.btnCancel.Location = new System.Drawing.Point(263, 3);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(119, 23);
		this.btnCancel.TabIndex = 4;
		this.btnCancel.Text = "Cancel Reservation";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnCancel.Click += new System.EventHandler(btnCancel_Click);
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(10, 18);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(100, 13);
		this.label2.TabIndex = 3;
		this.label2.Text = "Reference Number:";
		this.txtRefNumber.Location = new System.Drawing.Point(114, 18);
		this.txtRefNumber.Name = "txtRefNumber";
		this.txtRefNumber.Size = new System.Drawing.Size(100, 20);
		this.txtRefNumber.TabIndex = 1;
		this.btnValidate.Location = new System.Drawing.Point(263, 31);
		this.btnValidate.Name = "btnValidate";
		this.btnValidate.Size = new System.Drawing.Size(119, 23);
		this.btnValidate.TabIndex = 5;
		this.btnValidate.Text = "Validate Reservation";
		this.btnValidate.UseVisualStyleBackColor = true;
		this.btnValidate.Click += new System.EventHandler(btnValidate_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.panel1);
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.m_simulateServerBusyCheckBox);
		base.Controls.Add(this.m_simulateCommunicationsErrorCheckBox);
		base.Controls.Add(this.m_liveCallCheckBox);
		base.Controls.Add(this.m_brokerServicesTimeoutTextBox);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.m_brokerServicesUrlTextBox);
		base.Controls.Add(this.label5);
		base.Name = "GeneralPreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
