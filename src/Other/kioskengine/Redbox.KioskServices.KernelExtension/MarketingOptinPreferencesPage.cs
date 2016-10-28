using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.KernelExtension;

public class MarketingOptinPreferencesPage : UserControl, IPreferencePageHost
{
	private bool m_isLoaded;

	private IContainer components;

	private ComboBox m_confirmationStatusComboBox;

	private Label m_confirmationStatusLabel;

	private CheckBox m_commFailureCheckBox;

	private CheckBox m_liveCallCheckBox;

	private Label m_headerLabel;

	public MarketingOptinPreferencesPage()
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
		m_confirmationStatusComboBox.Items.Clear();
		string[] names = Enum.GetNames(typeof(ConfirmationStatus));
		foreach (string item in names)
		{
			m_confirmationStatusComboBox.Items.Add(item);
		}
		m_confirmationStatusComboBox.SelectedIndex = (int)MarketingOptinServicesProxy.Instance.ConfirmationStatusCode;
		m_liveCallCheckBox.Checked = MarketingOptinServicesProxy.Instance.LiveCall;
		m_commFailureCheckBox.Checked = MarketingOptinServicesProxy.Instance.SimulateCommunicationsFailure;
		UpdateSimulatedOptions();
		m_isLoaded = true;
	}

	private void OnLiveCallCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MarketingOptinServicesProxy.Instance.LiveCall = m_liveCallCheckBox.Checked;
			UpdateSimulatedOptions();
		}
	}

	private void UpdateSimulatedOptions()
	{
		bool liveCall = MarketingOptinServicesProxy.Instance.LiveCall;
		m_confirmationStatusComboBox.Enabled = !liveCall;
		m_commFailureCheckBox.Enabled = !liveCall;
	}

	private void OnCommFailureCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MarketingOptinServicesProxy.Instance.SimulateCommunicationsFailure = m_commFailureCheckBox.Checked;
		}
	}

	private void OnConfirmationStatusSelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MarketingOptinServicesProxy.Instance.ConfirmationStatusCode = (ConfirmationStatus)Enum.Parse(typeof(ConfirmationStatus), (string)m_confirmationStatusComboBox.SelectedItem, ignoreCase: true);
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
		this.m_confirmationStatusComboBox = new System.Windows.Forms.ComboBox();
		this.m_confirmationStatusLabel = new System.Windows.Forms.Label();
		this.m_commFailureCheckBox = new System.Windows.Forms.CheckBox();
		this.m_liveCallCheckBox = new System.Windows.Forms.CheckBox();
		this.m_headerLabel = new System.Windows.Forms.Label();
		base.SuspendLayout();
		this.m_confirmationStatusComboBox.FormattingEnabled = true;
		this.m_confirmationStatusComboBox.Location = new System.Drawing.Point(137, 75);
		this.m_confirmationStatusComboBox.Name = "m_confirmationStatusComboBox";
		this.m_confirmationStatusComboBox.Size = new System.Drawing.Size(262, 21);
		this.m_confirmationStatusComboBox.TabIndex = 0;
		this.m_confirmationStatusComboBox.SelectedIndexChanged += new System.EventHandler(OnConfirmationStatusSelectedIndexChanged);
		this.m_confirmationStatusLabel.AutoSize = true;
		this.m_confirmationStatusLabel.Location = new System.Drawing.Point(37, 78);
		this.m_confirmationStatusLabel.Name = "m_confirmationStatusLabel";
		this.m_confirmationStatusLabel.Size = new System.Drawing.Size(101, 13);
		this.m_confirmationStatusLabel.TabIndex = 1;
		this.m_confirmationStatusLabel.Text = "Confirmation Status:";
		this.m_commFailureCheckBox.AutoSize = true;
		this.m_commFailureCheckBox.Location = new System.Drawing.Point(137, 102);
		this.m_commFailureCheckBox.Name = "m_commFailureCheckBox";
		this.m_commFailureCheckBox.Size = new System.Drawing.Size(132, 17);
		this.m_commFailureCheckBox.TabIndex = 2;
		this.m_commFailureCheckBox.Text = "Communication Failure";
		this.m_commFailureCheckBox.UseVisualStyleBackColor = true;
		this.m_commFailureCheckBox.CheckedChanged += new System.EventHandler(OnCommFailureCheckChanged);
		this.m_liveCallCheckBox.AutoSize = true;
		this.m_liveCallCheckBox.Location = new System.Drawing.Point(40, 53);
		this.m_liveCallCheckBox.Name = "m_liveCallCheckBox";
		this.m_liveCallCheckBox.Size = new System.Drawing.Size(66, 17);
		this.m_liveCallCheckBox.TabIndex = 3;
		this.m_liveCallCheckBox.Text = "Live Call";
		this.m_liveCallCheckBox.UseVisualStyleBackColor = true;
		this.m_liveCallCheckBox.CheckedChanged += new System.EventHandler(OnLiveCallCheckChanged);
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkCyan;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(519, 38);
		this.m_headerLabel.TabIndex = 41;
		this.m_headerLabel.Text = "  Kiosk Services: Marketing Optin";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.m_liveCallCheckBox);
		base.Controls.Add(this.m_commFailureCheckBox);
		base.Controls.Add(this.m_confirmationStatusLabel);
		base.Controls.Add(this.m_confirmationStatusComboBox);
		base.Name = "MarketingOptinPreferencesPage";
		base.Size = new System.Drawing.Size(519, 382);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
