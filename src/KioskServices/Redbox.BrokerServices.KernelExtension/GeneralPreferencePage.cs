using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.BrokerServices.Proxy;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

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

	private Button button1;

	private Label m_reservationCount;

	private Label label1;

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
		Dictionary<string, object> obj = ServiceLocator.Instance.GetService<IDataStoreService>()?.GetAll("local", "Reservation");
		m_reservationCount.Text = obj?.Count.ToString() ?? "0";
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

	private void OnCreateReservationClick(object sender, EventArgs e)
	{
		_ = m_liveCallCheckBox.Checked;
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
		this.button1 = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.m_reservationCount = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
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
		this.button1.Location = new System.Drawing.Point(365, 466);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(141, 23);
		this.button1.TabIndex = 8;
		this.button1.Text = "Create Reservation";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(OnCreateReservationClick);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(367, 441);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(98, 13);
		this.label1.TabIndex = 9;
		this.label1.Text = "Reservation Count:";
		this.m_reservationCount.AutoSize = true;
		this.m_reservationCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f);
		this.m_reservationCount.Location = new System.Drawing.Point(471, 436);
		this.m_reservationCount.Name = "m_reservationCount";
		this.m_reservationCount.Size = new System.Drawing.Size(18, 20);
		this.m_reservationCount.TabIndex = 10;
		this.m_reservationCount.Text = "0";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_reservationCount);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.button1);
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
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
