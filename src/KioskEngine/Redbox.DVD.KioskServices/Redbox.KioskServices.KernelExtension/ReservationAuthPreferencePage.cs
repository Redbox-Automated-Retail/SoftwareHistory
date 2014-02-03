using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.KernelExtension;

public class ReservationAuthPreferencePage : UserControl, IPreferencePageHost
{
	private bool m_isLoaded;

	private IContainer components;

	private Label m_headerLabel;

	private CheckBox m_simulateServerBusyCheckBox;

	private CheckBox m_simulateCommunicationsFailureCheckBox;

	private CheckBox m_liveCallCheckBox;

	private ComboBox m_authorizeResponseTypeComboBox;

	private Label m_responseTypeLabel;

	public ReservationAuthPreferencePage()
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
		m_authorizeResponseTypeComboBox.Items.Clear();
		string[] names = Enum.GetNames(typeof(AuthorizeResponse));
		foreach (string item in names)
		{
			m_authorizeResponseTypeComboBox.Items.Add(item);
		}
		m_liveCallCheckBox.Checked = MerchantServicesProxy.Instance.ResvAuthLiveCall;
		m_simulateServerBusyCheckBox.Checked = MerchantServicesProxy.Instance.ResvAuthSimulateServerBusyError;
		m_simulateCommunicationsFailureCheckBox.Checked = MerchantServicesProxy.Instance.ResvAuthSimulateCommunicationsFailure;
		m_authorizeResponseTypeComboBox.SelectedIndex = m_authorizeResponseTypeComboBox.FindString(MerchantServicesProxy.Instance.ResvAuthAuthorizeResponse.ToString());
		m_isLoaded = true;
	}

	private void OnLiveCallCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.ResvAuthLiveCall = m_liveCallCheckBox.Checked;
		}
	}

	private void OnSimulateServerBusyCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.ResvAuthSimulateServerBusyError = m_simulateServerBusyCheckBox.Checked;
		}
	}

	private void OnSimulateCommunicationsFailureCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.ResvAuthSimulateCommunicationsFailure = m_simulateCommunicationsFailureCheckBox.Checked;
		}
	}

	private void OnResponseTypeChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.ResvAuthAuthorizeResponse = Enum<AuthorizeResponse>.ParseIgnoringCase(m_authorizeResponseTypeComboBox.SelectedItem.ToString(), AuthorizeResponse.Approved);
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
		this.m_headerLabel = new System.Windows.Forms.Label();
		this.m_simulateServerBusyCheckBox = new System.Windows.Forms.CheckBox();
		this.m_simulateCommunicationsFailureCheckBox = new System.Windows.Forms.CheckBox();
		this.m_liveCallCheckBox = new System.Windows.Forms.CheckBox();
		this.m_authorizeResponseTypeComboBox = new System.Windows.Forms.ComboBox();
		this.m_responseTypeLabel = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkCyan;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 37;
		this.m_headerLabel.Text = "  Kiosk Services: Reservation Authentication";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.m_simulateServerBusyCheckBox.AutoSize = true;
		this.m_simulateServerBusyCheckBox.Location = new System.Drawing.Point(18, 101);
		this.m_simulateServerBusyCheckBox.Name = "m_simulateServerBusyCheckBox";
		this.m_simulateServerBusyCheckBox.Size = new System.Drawing.Size(126, 17);
		this.m_simulateServerBusyCheckBox.TabIndex = 40;
		this.m_simulateServerBusyCheckBox.Text = "Simulate Server Busy";
		this.m_simulateServerBusyCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateServerBusyCheckBox.CheckedChanged += new System.EventHandler(OnSimulateServerBusyCheckChanged);
		this.m_simulateCommunicationsFailureCheckBox.AutoSize = true;
		this.m_simulateCommunicationsFailureCheckBox.Location = new System.Drawing.Point(18, 78);
		this.m_simulateCommunicationsFailureCheckBox.Name = "m_simulateCommunicationsFailureCheckBox";
		this.m_simulateCommunicationsFailureCheckBox.Size = new System.Drawing.Size(180, 17);
		this.m_simulateCommunicationsFailureCheckBox.TabIndex = 39;
		this.m_simulateCommunicationsFailureCheckBox.Text = "Simulate Communications Failure";
		this.m_simulateCommunicationsFailureCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateCommunicationsFailureCheckBox.CheckedChanged += new System.EventHandler(OnSimulateCommunicationsFailureCheckChanged);
		this.m_liveCallCheckBox.AutoSize = true;
		this.m_liveCallCheckBox.Location = new System.Drawing.Point(18, 55);
		this.m_liveCallCheckBox.Name = "m_liveCallCheckBox";
		this.m_liveCallCheckBox.Size = new System.Drawing.Size(66, 17);
		this.m_liveCallCheckBox.TabIndex = 38;
		this.m_liveCallCheckBox.Text = "Live Call";
		this.m_liveCallCheckBox.UseVisualStyleBackColor = true;
		this.m_liveCallCheckBox.CheckedChanged += new System.EventHandler(OnLiveCallCheckChanged);
		this.m_authorizeResponseTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_authorizeResponseTypeComboBox.FormattingEnabled = true;
		this.m_authorizeResponseTypeComboBox.Location = new System.Drawing.Point(156, 124);
		this.m_authorizeResponseTypeComboBox.Name = "m_authorizeResponseTypeComboBox";
		this.m_authorizeResponseTypeComboBox.Size = new System.Drawing.Size(284, 21);
		this.m_authorizeResponseTypeComboBox.TabIndex = 42;
		this.m_authorizeResponseTypeComboBox.SelectedIndexChanged += new System.EventHandler(OnResponseTypeChanged);
		this.m_responseTypeLabel.AutoSize = true;
		this.m_responseTypeLabel.Location = new System.Drawing.Point(18, 127);
		this.m_responseTypeLabel.Name = "m_responseTypeLabel";
		this.m_responseTypeLabel.Size = new System.Drawing.Size(132, 13);
		this.m_responseTypeLabel.TabIndex = 41;
		this.m_responseTypeLabel.Text = "Authorize Response Type:";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_authorizeResponseTypeComboBox);
		base.Controls.Add(this.m_responseTypeLabel);
		base.Controls.Add(this.m_simulateServerBusyCheckBox);
		base.Controls.Add(this.m_simulateCommunicationsFailureCheckBox);
		base.Controls.Add(this.m_liveCallCheckBox);
		base.Controls.Add(this.m_headerLabel);
		base.Name = "ReservationAuthPreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
