using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.KernelExtension;

public class DigitalServicesPreferencePage : UserControl, IPreferencePageHost
{
	private bool m_isLoaded;

	private IContainer components;

	private CheckBox m_liveCallCheckBox;

	private CheckBox m_simulateCommunicationsFailureCheckBox;

	private CheckBox m_simulateServerBusyCheckBox;

	private ErrorProvider m_errorProvider;

	private ComboBox m_digitalResponseTypeComboBox;

	private Label m_digitalResponseTypeLabel;

	private Label m_totalCreditsLabel;

	private TextBox m_totalCreditsTextBox;

	private CheckBox m_hasProfileCheckBox;

	private Label m_headerLabel;

	private Label label5;

	private TextBox m_emailAddressTextBox;

	private Label label4;

	private TextBox m_cpLastNameTextBox;

	private Label label3;

	private TextBox m_cpFirstNameTextBox;

	private CheckBox m_hasCustomerProfileCheckBox;

	private CheckBox m_canCreateProfileCheckBox;

	public DigitalServicesPreferencePage()
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
		m_liveCallCheckBox.Checked = DigitalServicesProxy.Instance.LiveCall;
		m_simulateCommunicationsFailureCheckBox.Checked = DigitalServicesProxy.Instance.SimulateCommunicationsFailure;
		m_simulateServerBusyCheckBox.Checked = DigitalServicesProxy.Instance.SimulateServerBusyError;
		m_digitalResponseTypeComboBox.Items.Clear();
		string[] names = Enum.GetNames(typeof(DigitalServicesResponse));
		foreach (string item in names)
		{
			m_digitalResponseTypeComboBox.Items.Add(item);
		}
		m_digitalResponseTypeComboBox.SelectedIndex = m_digitalResponseTypeComboBox.FindString(DigitalServicesProxy.Instance.DigitalServicesResponse.ToString());
		m_hasProfileCheckBox.Checked = DigitalServicesProxy.Instance.HasProfile;
		m_totalCreditsTextBox.Text = DigitalServicesProxy.Instance.TotalCreditsAvailable.ToString();
		m_cpFirstNameTextBox.Text = DigitalServicesProxy.Instance.CPFirstName;
		m_cpLastNameTextBox.Text = DigitalServicesProxy.Instance.CPLastName;
		m_emailAddressTextBox.Text = DigitalServicesProxy.Instance.EmailAddress;
		m_hasCustomerProfileCheckBox.Checked = DigitalServicesProxy.Instance.HasCustomerProfile;
		m_canCreateProfileCheckBox.Checked = DigitalServicesProxy.Instance.CanCreateProfile;
		m_isLoaded = true;
	}

	private void m_liveCallCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			DigitalServicesProxy.Instance.LiveCall = m_liveCallCheckBox.Checked;
		}
	}

	private void m_simulateCommunicationsFailureCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			DigitalServicesProxy.Instance.SimulateCommunicationsFailure = m_simulateCommunicationsFailureCheckBox.Checked;
		}
	}

	private void m_simulateServerBusyCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			DigitalServicesProxy.Instance.SimulateServerBusyError = m_simulateServerBusyCheckBox.Checked;
		}
	}

	private void m_digitalResponseTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			DigitalServicesProxy.Instance.DigitalServicesResponse = Enum<DigitalServicesResponse>.ParseIgnoringCase(m_digitalResponseTypeComboBox.SelectedItem.ToString(), DigitalServicesResponse.Valid);
		}
	}

	private void m_hasProfileCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			DigitalServicesProxy.Instance.HasProfile = m_hasProfileCheckBox.Checked;
		}
	}

	private void m_totalCreditsTextBox_Leave(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			m_errorProvider.SetError(m_totalCreditsTextBox, string.Empty);
			if (!int.TryParse(m_totalCreditsTextBox.Text, out var result) || result < 0)
			{
				m_errorProvider.SetError(m_totalCreditsTextBox, "Total credits available must be a valid integer value.");
			}
			else
			{
				DigitalServicesProxy.Instance.TotalCreditsAvailable = result;
			}
		}
	}

	private void CPFirstNameTextBox_Leave(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			DigitalServicesProxy.Instance.CPFirstName = m_cpFirstNameTextBox.Text;
		}
	}

	private void CPLastNameTextBox_Leave(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			DigitalServicesProxy.Instance.CPLastName = m_cpLastNameTextBox.Text;
		}
	}

	private void EmailAddressTextBox_Leave(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			DigitalServicesProxy.Instance.EmailAddress = m_emailAddressTextBox.Text;
		}
	}

	private void OnHasCustomerProfileCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			DigitalServicesProxy.Instance.HasCustomerProfile = m_hasCustomerProfileCheckBox.Checked;
		}
	}

	private void OnCanCreateProfileCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			DigitalServicesProxy.Instance.CanCreateProfile = m_canCreateProfileCheckBox.Checked;
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
		this.m_liveCallCheckBox = new System.Windows.Forms.CheckBox();
		this.m_simulateCommunicationsFailureCheckBox = new System.Windows.Forms.CheckBox();
		this.m_simulateServerBusyCheckBox = new System.Windows.Forms.CheckBox();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_digitalResponseTypeComboBox = new System.Windows.Forms.ComboBox();
		this.m_digitalResponseTypeLabel = new System.Windows.Forms.Label();
		this.m_totalCreditsLabel = new System.Windows.Forms.Label();
		this.m_totalCreditsTextBox = new System.Windows.Forms.TextBox();
		this.m_hasProfileCheckBox = new System.Windows.Forms.CheckBox();
		this.m_headerLabel = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.m_emailAddressTextBox = new System.Windows.Forms.TextBox();
		this.label4 = new System.Windows.Forms.Label();
		this.m_cpLastNameTextBox = new System.Windows.Forms.TextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.m_cpFirstNameTextBox = new System.Windows.Forms.TextBox();
		this.m_hasCustomerProfileCheckBox = new System.Windows.Forms.CheckBox();
		this.m_canCreateProfileCheckBox = new System.Windows.Forms.CheckBox();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_liveCallCheckBox.AutoSize = true;
		this.m_liveCallCheckBox.Location = new System.Drawing.Point(33, 62);
		this.m_liveCallCheckBox.Name = "m_liveCallCheckBox";
		this.m_liveCallCheckBox.Size = new System.Drawing.Size(66, 17);
		this.m_liveCallCheckBox.TabIndex = 6;
		this.m_liveCallCheckBox.Text = "Live Call";
		this.m_liveCallCheckBox.UseVisualStyleBackColor = true;
		this.m_liveCallCheckBox.CheckedChanged += new System.EventHandler(m_liveCallCheckBox_CheckedChanged);
		this.m_simulateCommunicationsFailureCheckBox.AutoSize = true;
		this.m_simulateCommunicationsFailureCheckBox.Location = new System.Drawing.Point(33, 85);
		this.m_simulateCommunicationsFailureCheckBox.Name = "m_simulateCommunicationsFailureCheckBox";
		this.m_simulateCommunicationsFailureCheckBox.Size = new System.Drawing.Size(180, 17);
		this.m_simulateCommunicationsFailureCheckBox.TabIndex = 9;
		this.m_simulateCommunicationsFailureCheckBox.Text = "Simulate Communications Failure";
		this.m_simulateCommunicationsFailureCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateCommunicationsFailureCheckBox.CheckedChanged += new System.EventHandler(m_simulateCommunicationsFailureCheckBox_CheckedChanged);
		this.m_simulateServerBusyCheckBox.AutoSize = true;
		this.m_simulateServerBusyCheckBox.Location = new System.Drawing.Point(33, 108);
		this.m_simulateServerBusyCheckBox.Name = "m_simulateServerBusyCheckBox";
		this.m_simulateServerBusyCheckBox.Size = new System.Drawing.Size(126, 17);
		this.m_simulateServerBusyCheckBox.TabIndex = 10;
		this.m_simulateServerBusyCheckBox.Text = "Simulate Server Busy";
		this.m_simulateServerBusyCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateServerBusyCheckBox.CheckedChanged += new System.EventHandler(m_simulateServerBusyCheckBox_CheckedChanged);
		this.m_errorProvider.ContainerControl = this;
		this.m_digitalResponseTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_digitalResponseTypeComboBox.FormattingEnabled = true;
		this.m_digitalResponseTypeComboBox.Location = new System.Drawing.Point(169, 144);
		this.m_digitalResponseTypeComboBox.Name = "m_digitalResponseTypeComboBox";
		this.m_digitalResponseTypeComboBox.Size = new System.Drawing.Size(279, 21);
		this.m_digitalResponseTypeComboBox.TabIndex = 13;
		this.m_digitalResponseTypeComboBox.SelectedIndexChanged += new System.EventHandler(m_digitalResponseTypeComboBox_SelectedIndexChanged);
		this.m_digitalResponseTypeLabel.AutoSize = true;
		this.m_digitalResponseTypeLabel.Location = new System.Drawing.Point(31, 147);
		this.m_digitalResponseTypeLabel.Name = "m_digitalResponseTypeLabel";
		this.m_digitalResponseTypeLabel.Size = new System.Drawing.Size(120, 13);
		this.m_digitalResponseTypeLabel.TabIndex = 14;
		this.m_digitalResponseTypeLabel.Text = "Credits Response Type:";
		this.m_totalCreditsLabel.AutoSize = true;
		this.m_totalCreditsLabel.Location = new System.Drawing.Point(166, 213);
		this.m_totalCreditsLabel.Name = "m_totalCreditsLabel";
		this.m_totalCreditsLabel.Size = new System.Drawing.Size(115, 13);
		this.m_totalCreditsLabel.TabIndex = 15;
		this.m_totalCreditsLabel.Text = "Total Credits Available:";
		this.m_totalCreditsTextBox.Location = new System.Drawing.Point(287, 210);
		this.m_totalCreditsTextBox.Name = "m_totalCreditsTextBox";
		this.m_totalCreditsTextBox.Size = new System.Drawing.Size(32, 20);
		this.m_totalCreditsTextBox.TabIndex = 16;
		this.m_totalCreditsTextBox.Leave += new System.EventHandler(m_totalCreditsTextBox_Leave);
		this.m_hasProfileCheckBox.AutoSize = true;
		this.m_hasProfileCheckBox.Location = new System.Drawing.Point(169, 184);
		this.m_hasProfileCheckBox.Name = "m_hasProfileCheckBox";
		this.m_hasProfileCheckBox.Size = new System.Drawing.Size(77, 17);
		this.m_hasProfileCheckBox.TabIndex = 18;
		this.m_hasProfileCheckBox.Text = "Has Profile";
		this.m_hasProfileCheckBox.UseVisualStyleBackColor = true;
		this.m_hasProfileCheckBox.CheckedChanged += new System.EventHandler(m_hasProfileCheckBox_CheckedChanged);
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkCyan;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 37;
		this.m_headerLabel.Text = "  Kiosk Services: Credit System";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(53, 345);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(73, 13);
		this.label5.TabIndex = 60;
		this.label5.Text = "Email Address";
		this.m_emailAddressTextBox.Location = new System.Drawing.Point(172, 342);
		this.m_emailAddressTextBox.Name = "m_emailAddressTextBox";
		this.m_emailAddressTextBox.Size = new System.Drawing.Size(317, 20);
		this.m_emailAddressTextBox.TabIndex = 59;
		this.m_emailAddressTextBox.Leave += new System.EventHandler(EmailAddressTextBox_Leave);
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(53, 322);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(75, 13);
		this.label4.TabIndex = 58;
		this.label4.Text = "CP Last Name";
		this.m_cpLastNameTextBox.Location = new System.Drawing.Point(172, 319);
		this.m_cpLastNameTextBox.Name = "m_cpLastNameTextBox";
		this.m_cpLastNameTextBox.Size = new System.Drawing.Size(317, 20);
		this.m_cpLastNameTextBox.TabIndex = 57;
		this.m_cpLastNameTextBox.Leave += new System.EventHandler(CPLastNameTextBox_Leave);
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(53, 299);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(74, 13);
		this.label3.TabIndex = 56;
		this.label3.Text = "CP First Name";
		this.m_cpFirstNameTextBox.Location = new System.Drawing.Point(172, 296);
		this.m_cpFirstNameTextBox.Name = "m_cpFirstNameTextBox";
		this.m_cpFirstNameTextBox.Size = new System.Drawing.Size(317, 20);
		this.m_cpFirstNameTextBox.TabIndex = 55;
		this.m_cpFirstNameTextBox.Leave += new System.EventHandler(CPFirstNameTextBox_Leave);
		this.m_hasCustomerProfileCheckBox.AutoSize = true;
		this.m_hasCustomerProfileCheckBox.Location = new System.Drawing.Point(33, 274);
		this.m_hasCustomerProfileCheckBox.Name = "m_hasCustomerProfileCheckBox";
		this.m_hasCustomerProfileCheckBox.Size = new System.Drawing.Size(124, 17);
		this.m_hasCustomerProfileCheckBox.TabIndex = 54;
		this.m_hasCustomerProfileCheckBox.Text = "Has Customer Profile";
		this.m_hasCustomerProfileCheckBox.UseVisualStyleBackColor = true;
		this.m_hasCustomerProfileCheckBox.CheckedChanged += new System.EventHandler(OnHasCustomerProfileCheckChanged);
		this.m_canCreateProfileCheckBox.AutoSize = true;
		this.m_canCreateProfileCheckBox.Location = new System.Drawing.Point(33, 425);
		this.m_canCreateProfileCheckBox.Name = "m_canCreateProfileCheckBox";
		this.m_canCreateProfileCheckBox.Size = new System.Drawing.Size(111, 17);
		this.m_canCreateProfileCheckBox.TabIndex = 62;
		this.m_canCreateProfileCheckBox.Text = "Can Create Profile";
		this.m_canCreateProfileCheckBox.UseVisualStyleBackColor = true;
		this.m_canCreateProfileCheckBox.CheckedChanged += new System.EventHandler(OnCanCreateProfileCheckChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_canCreateProfileCheckBox);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.m_emailAddressTextBox);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.m_cpLastNameTextBox);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.m_cpFirstNameTextBox);
		base.Controls.Add(this.m_hasCustomerProfileCheckBox);
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.m_hasProfileCheckBox);
		base.Controls.Add(this.m_totalCreditsTextBox);
		base.Controls.Add(this.m_totalCreditsLabel);
		base.Controls.Add(this.m_digitalResponseTypeLabel);
		base.Controls.Add(this.m_digitalResponseTypeComboBox);
		base.Controls.Add(this.m_simulateServerBusyCheckBox);
		base.Controls.Add(this.m_simulateCommunicationsFailureCheckBox);
		base.Controls.Add(this.m_liveCallCheckBox);
		base.Name = "DigitalServicesPreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
