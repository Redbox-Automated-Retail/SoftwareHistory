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

	private TextBox txtCustProfileNum;

	private Label label1;

	private TextBox txtMobilePhoneNumber;

	private Label label2;

	private CheckBox cbPromptForPerks;

	private CheckBox cbPromptForEarlyId;

	private CheckBox cbTextClubEnabled;

	public MarketingOptinPreferencesPage()
	{
		InitializeComponent();
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		MarketingOptinServicesProxy instance = MarketingOptinServicesProxy.Instance;
		instance.CustomerProfileNumber = txtCustProfileNum.Text;
		instance.MobilePhoneNumber = txtMobilePhoneNumber.Text;
		instance.PromptForEarlyId = cbPromptForEarlyId.Checked;
		instance.PromptForPerks = cbPromptForPerks.Checked;
		instance.TextClubEnabled = cbTextClubEnabled.Checked;
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
		MarketingOptinServicesProxy instance = MarketingOptinServicesProxy.Instance;
		m_confirmationStatusComboBox.SelectedIndex = (int)instance.ConfirmationStatusCode;
		m_liveCallCheckBox.Checked = instance.LiveCall;
		m_commFailureCheckBox.Checked = instance.SimulateCommunicationsFailure;
		txtCustProfileNum.Text = instance.CustomerProfileNumber;
		txtMobilePhoneNumber.Text = instance.MobilePhoneNumber;
		cbPromptForEarlyId.Checked = instance.PromptForEarlyId;
		cbPromptForPerks.Checked = instance.PromptForPerks;
		cbTextClubEnabled.Checked = instance.TextClubEnabled;
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
		txtCustProfileNum.Enabled = !liveCall;
		txtMobilePhoneNumber.Enabled = !liveCall;
		cbPromptForEarlyId.Enabled = !liveCall;
		cbPromptForPerks.Enabled = !liveCall;
		cbTextClubEnabled.Enabled = !liveCall;
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
		this.txtCustProfileNum = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.txtMobilePhoneNumber = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.cbPromptForPerks = new System.Windows.Forms.CheckBox();
		this.cbPromptForEarlyId = new System.Windows.Forms.CheckBox();
		this.cbTextClubEnabled = new System.Windows.Forms.CheckBox();
		base.SuspendLayout();
		this.m_confirmationStatusComboBox.FormattingEnabled = true;
		this.m_confirmationStatusComboBox.Location = new System.Drawing.Point(144, 75);
		this.m_confirmationStatusComboBox.Name = "m_confirmationStatusComboBox";
		this.m_confirmationStatusComboBox.Size = new System.Drawing.Size(262, 21);
		this.m_confirmationStatusComboBox.TabIndex = 0;
		this.m_confirmationStatusComboBox.SelectedIndexChanged += new System.EventHandler(OnConfirmationStatusSelectedIndexChanged);
		this.m_confirmationStatusLabel.AutoSize = true;
		this.m_confirmationStatusLabel.Location = new System.Drawing.Point(12, 78);
		this.m_confirmationStatusLabel.Name = "m_confirmationStatusLabel";
		this.m_confirmationStatusLabel.Size = new System.Drawing.Size(101, 13);
		this.m_confirmationStatusLabel.TabIndex = 1;
		this.m_confirmationStatusLabel.Text = "Confirmation Status:";
		this.m_commFailureCheckBox.AutoSize = true;
		this.m_commFailureCheckBox.Location = new System.Drawing.Point(144, 53);
		this.m_commFailureCheckBox.Name = "m_commFailureCheckBox";
		this.m_commFailureCheckBox.Size = new System.Drawing.Size(132, 17);
		this.m_commFailureCheckBox.TabIndex = 2;
		this.m_commFailureCheckBox.Text = "Communication Failure";
		this.m_commFailureCheckBox.UseVisualStyleBackColor = true;
		this.m_commFailureCheckBox.CheckedChanged += new System.EventHandler(OnCommFailureCheckChanged);
		this.m_liveCallCheckBox.AutoSize = true;
		this.m_liveCallCheckBox.Location = new System.Drawing.Point(15, 53);
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
		this.txtCustProfileNum.Location = new System.Drawing.Point(144, 102);
		this.txtCustProfileNum.Name = "txtCustProfileNum";
		this.txtCustProfileNum.Size = new System.Drawing.Size(262, 20);
		this.txtCustProfileNum.TabIndex = 57;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(12, 104);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(126, 13);
		this.label1.TabIndex = 56;
		this.label1.Text = "Customer Profile Number:";
		this.txtMobilePhoneNumber.Location = new System.Drawing.Point(144, 128);
		this.txtMobilePhoneNumber.Name = "txtMobilePhoneNumber";
		this.txtMobilePhoneNumber.Size = new System.Drawing.Size(262, 20);
		this.txtMobilePhoneNumber.TabIndex = 59;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(12, 130);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(115, 13);
		this.label2.TabIndex = 58;
		this.label2.Text = "Mobile Phone Number:";
		this.cbPromptForPerks.AutoSize = true;
		this.cbPromptForPerks.Location = new System.Drawing.Point(144, 154);
		this.cbPromptForPerks.Name = "cbPromptForPerks";
		this.cbPromptForPerks.Size = new System.Drawing.Size(107, 17);
		this.cbPromptForPerks.TabIndex = 60;
		this.cbPromptForPerks.Text = "Prompt For Perks";
		this.cbPromptForPerks.UseVisualStyleBackColor = true;
		this.cbPromptForEarlyId.AutoSize = true;
		this.cbPromptForEarlyId.Location = new System.Drawing.Point(144, 177);
		this.cbPromptForEarlyId.Name = "cbPromptForEarlyId";
		this.cbPromptForEarlyId.Size = new System.Drawing.Size(117, 17);
		this.cbPromptForEarlyId.TabIndex = 61;
		this.cbPromptForEarlyId.Text = "Prompt For Early ID";
		this.cbPromptForEarlyId.UseVisualStyleBackColor = true;
		this.cbTextClubEnabled.AutoSize = true;
		this.cbTextClubEnabled.Location = new System.Drawing.Point(144, 200);
		this.cbTextClubEnabled.Name = "cbTextClubEnabled";
		this.cbTextClubEnabled.Size = new System.Drawing.Size(113, 17);
		this.cbTextClubEnabled.TabIndex = 62;
		this.cbTextClubEnabled.Text = "Text Club Enabled";
		this.cbTextClubEnabled.UseVisualStyleBackColor = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.cbTextClubEnabled);
		base.Controls.Add(this.cbPromptForEarlyId);
		base.Controls.Add(this.cbPromptForPerks);
		base.Controls.Add(this.txtMobilePhoneNumber);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.txtCustProfileNum);
		base.Controls.Add(this.label1);
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
