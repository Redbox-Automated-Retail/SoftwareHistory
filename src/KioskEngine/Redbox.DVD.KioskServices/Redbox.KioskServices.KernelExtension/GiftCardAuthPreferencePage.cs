using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.KernelExtension;

public class GiftCardAuthPreferencePage : UserControl, IPreferencePageHost
{
	private bool m_isLoaded;

	private IContainer components;

	private ErrorProvider m_errorProvider;

	private CheckBox m_simulateServerBusyCheckBox;

	private CheckBox m_simulateCommunicationsFailureCheckBox;

	private CheckBox m_liveCallCheckBox;

	private Label m_headerLabel;

	private Label label1;

	private ComboBox m_gcauthorizeResponseTypeComboBox;

	private CheckBox m_existsCheckBox;

	private ComboBox m_gcstatusTypeComboBox;

	private Label label2;

	private TextBox m_balanceTextBox;

	private Label label3;

	private TextBox m_maxDiscsTextBox;

	private Label label4;

	private TextBox m_maxGamesTextBox;

	private Label label5;

	public GiftCardAuthPreferencePage()
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
		m_liveCallCheckBox.Checked = MerchantServicesProxy.Instance.GCLiveCall;
		m_simulateCommunicationsFailureCheckBox.Checked = MerchantServicesProxy.Instance.GCSimulateCommunicationsFailure;
		m_simulateServerBusyCheckBox.Checked = MerchantServicesProxy.Instance.GCSimulateServerBusyError;
		m_gcauthorizeResponseTypeComboBox.Items.Clear();
		string[] names = Enum.GetNames(typeof(GCAuthorizeResponse));
		foreach (string item in names)
		{
			m_gcauthorizeResponseTypeComboBox.Items.Add(item);
		}
		m_gcauthorizeResponseTypeComboBox.SelectedIndex = m_gcauthorizeResponseTypeComboBox.FindString(MerchantServicesProxy.Instance.GCAuthorizeResponse.ToString());
		m_existsCheckBox.Checked = MerchantServicesProxy.Instance.Exists;
		m_gcstatusTypeComboBox.Items.Clear();
		string[] names2 = Enum.GetNames(typeof(GiftCardStatus));
		foreach (string item2 in names2)
		{
			m_gcstatusTypeComboBox.Items.Add(item2);
		}
		m_gcstatusTypeComboBox.SelectedIndex = m_gcstatusTypeComboBox.FindString(MerchantServicesProxy.Instance.GCStatusType.ToString());
		m_balanceTextBox.Text = MerchantServicesProxy.Instance.Balance;
		m_maxDiscsTextBox.Text = MerchantServicesProxy.Instance.MaxDiscs.ToString();
		m_maxGamesTextBox.Text = MerchantServicesProxy.Instance.MaxGames.ToString();
		m_isLoaded = true;
	}

	private void OnLiveCallCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.GCLiveCall = m_liveCallCheckBox.Checked;
		}
	}

	private void OnSimulateCommunicationsFailureCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.GCSimulateCommunicationsFailure = m_simulateCommunicationsFailureCheckBox.Checked;
		}
	}

	private void OnSimulateServerBusyCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.GCSimulateServerBusyError = m_simulateServerBusyCheckBox.Checked;
		}
	}

	private void OnSelectedGCResponseTypeChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.GCAuthorizeResponse = Enum<GCAuthorizeResponse>.ParseIgnoringCase(m_gcauthorizeResponseTypeComboBox.SelectedItem.ToString(), GCAuthorizeResponse.Approved);
		}
	}

	private void OnExistsCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.Exists = m_existsCheckBox.Checked;
		}
	}

	private void OnSelectedGCStatusTypeChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.GCStatusType = Enum<GiftCardStatus>.ParseIgnoringCase(m_gcstatusTypeComboBox.SelectedItem.ToString(), GiftCardStatus.Unregistered);
		}
	}

	private void OnLeaveBalance(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.Balance = m_balanceTextBox.Text;
		}
	}

	private void OnLeaveMaxDiscs(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			m_errorProvider.SetError(m_maxDiscsTextBox, string.Empty);
			if (!int.TryParse(m_maxDiscsTextBox.Text, out var result) || result < 0)
			{
				m_errorProvider.SetError(m_maxDiscsTextBox, "Max discs must be a valid integer value.");
			}
			else
			{
				MerchantServicesProxy.Instance.MaxDiscs = result;
			}
		}
	}

	private void OnLeaveMaxGames(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			m_errorProvider.SetError(m_maxGamesTextBox, string.Empty);
			if (!int.TryParse(m_maxGamesTextBox.Text, out var result) || result < 0)
			{
				m_errorProvider.SetError(m_maxGamesTextBox, "Max games must be a valid integer value.");
			}
			else
			{
				MerchantServicesProxy.Instance.MaxGames = result;
			}
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
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_simulateServerBusyCheckBox = new System.Windows.Forms.CheckBox();
		this.m_simulateCommunicationsFailureCheckBox = new System.Windows.Forms.CheckBox();
		this.m_liveCallCheckBox = new System.Windows.Forms.CheckBox();
		this.m_headerLabel = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.m_gcauthorizeResponseTypeComboBox = new System.Windows.Forms.ComboBox();
		this.m_existsCheckBox = new System.Windows.Forms.CheckBox();
		this.m_gcstatusTypeComboBox = new System.Windows.Forms.ComboBox();
		this.label2 = new System.Windows.Forms.Label();
		this.m_balanceTextBox = new System.Windows.Forms.TextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.m_maxDiscsTextBox = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.m_maxGamesTextBox = new System.Windows.Forms.TextBox();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_errorProvider.ContainerControl = this;
		this.m_simulateServerBusyCheckBox.AutoSize = true;
		this.m_simulateServerBusyCheckBox.Location = new System.Drawing.Point(30, 99);
		this.m_simulateServerBusyCheckBox.Name = "m_simulateServerBusyCheckBox";
		this.m_simulateServerBusyCheckBox.Size = new System.Drawing.Size(126, 17);
		this.m_simulateServerBusyCheckBox.TabIndex = 19;
		this.m_simulateServerBusyCheckBox.Text = "Simulate Server Busy";
		this.m_simulateServerBusyCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateServerBusyCheckBox.CheckedChanged += new System.EventHandler(OnSimulateServerBusyCheckChanged);
		this.m_simulateCommunicationsFailureCheckBox.AutoSize = true;
		this.m_simulateCommunicationsFailureCheckBox.Location = new System.Drawing.Point(30, 76);
		this.m_simulateCommunicationsFailureCheckBox.Name = "m_simulateCommunicationsFailureCheckBox";
		this.m_simulateCommunicationsFailureCheckBox.Size = new System.Drawing.Size(180, 17);
		this.m_simulateCommunicationsFailureCheckBox.TabIndex = 18;
		this.m_simulateCommunicationsFailureCheckBox.Text = "Simulate Communications Failure";
		this.m_simulateCommunicationsFailureCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateCommunicationsFailureCheckBox.CheckedChanged += new System.EventHandler(OnSimulateCommunicationsFailureCheckChanged);
		this.m_liveCallCheckBox.AutoSize = true;
		this.m_liveCallCheckBox.Location = new System.Drawing.Point(30, 53);
		this.m_liveCallCheckBox.Name = "m_liveCallCheckBox";
		this.m_liveCallCheckBox.Size = new System.Drawing.Size(66, 17);
		this.m_liveCallCheckBox.TabIndex = 10;
		this.m_liveCallCheckBox.Text = "Live Call";
		this.m_liveCallCheckBox.UseVisualStyleBackColor = true;
		this.m_liveCallCheckBox.CheckedChanged += new System.EventHandler(OnLiveCallCheckChanged);
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkCyan;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 41;
		this.m_headerLabel.Text = "  Kiosk Services: Gift Card Authorize";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(31, 133);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(150, 13);
		this.label1.TabIndex = 56;
		this.label1.Text = "GC Authorize Response Type:";
		this.m_gcauthorizeResponseTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_gcauthorizeResponseTypeComboBox.FormattingEnabled = true;
		this.m_gcauthorizeResponseTypeComboBox.Location = new System.Drawing.Point(186, 130);
		this.m_gcauthorizeResponseTypeComboBox.Name = "m_gcauthorizeResponseTypeComboBox";
		this.m_gcauthorizeResponseTypeComboBox.Size = new System.Drawing.Size(300, 21);
		this.m_gcauthorizeResponseTypeComboBox.TabIndex = 57;
		this.m_gcauthorizeResponseTypeComboBox.SelectedIndexChanged += new System.EventHandler(OnSelectedGCResponseTypeChanged);
		this.m_existsCheckBox.AutoSize = true;
		this.m_existsCheckBox.Location = new System.Drawing.Point(30, 165);
		this.m_existsCheckBox.Name = "m_existsCheckBox";
		this.m_existsCheckBox.Size = new System.Drawing.Size(53, 17);
		this.m_existsCheckBox.TabIndex = 58;
		this.m_existsCheckBox.Text = "Exists";
		this.m_existsCheckBox.UseVisualStyleBackColor = true;
		this.m_existsCheckBox.CheckedChanged += new System.EventHandler(OnExistsCheckChanged);
		this.m_gcstatusTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_gcstatusTypeComboBox.FormattingEnabled = true;
		this.m_gcstatusTypeComboBox.Location = new System.Drawing.Point(186, 185);
		this.m_gcstatusTypeComboBox.Name = "m_gcstatusTypeComboBox";
		this.m_gcstatusTypeComboBox.Size = new System.Drawing.Size(300, 21);
		this.m_gcstatusTypeComboBox.TabIndex = 60;
		this.m_gcstatusTypeComboBox.SelectedIndexChanged += new System.EventHandler(OnSelectedGCStatusTypeChanged);
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(31, 188);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(37, 13);
		this.label2.TabIndex = 59;
		this.label2.Text = "Status";
		this.m_balanceTextBox.Location = new System.Drawing.Point(186, 208);
		this.m_balanceTextBox.Name = "m_balanceTextBox";
		this.m_balanceTextBox.Size = new System.Drawing.Size(80, 20);
		this.m_balanceTextBox.TabIndex = 62;
		this.m_balanceTextBox.Leave += new System.EventHandler(OnLeaveBalance);
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(30, 211);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(49, 13);
		this.label3.TabIndex = 61;
		this.label3.Text = "Balance:";
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(30, 237);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(59, 13);
		this.label4.TabIndex = 63;
		this.label4.Text = "Max Discs:";
		this.m_maxDiscsTextBox.Location = new System.Drawing.Point(186, 234);
		this.m_maxDiscsTextBox.Name = "m_maxDiscsTextBox";
		this.m_maxDiscsTextBox.Size = new System.Drawing.Size(80, 20);
		this.m_maxDiscsTextBox.TabIndex = 64;
		this.m_maxDiscsTextBox.Leave += new System.EventHandler(OnLeaveMaxDiscs);
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(30, 263);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(66, 13);
		this.label5.TabIndex = 65;
		this.label5.Text = "Max Games:";
		this.m_maxGamesTextBox.Location = new System.Drawing.Point(186, 260);
		this.m_maxGamesTextBox.Name = "m_maxGamesTextBox";
		this.m_maxGamesTextBox.Size = new System.Drawing.Size(80, 20);
		this.m_maxGamesTextBox.TabIndex = 66;
		this.m_maxGamesTextBox.Leave += new System.EventHandler(OnLeaveMaxGames);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_maxGamesTextBox);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.m_maxDiscsTextBox);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.m_balanceTextBox);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.m_gcstatusTypeComboBox);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.m_existsCheckBox);
		base.Controls.Add(this.m_gcauthorizeResponseTypeComboBox);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.m_simulateServerBusyCheckBox);
		base.Controls.Add(this.m_simulateCommunicationsFailureCheckBox);
		base.Controls.Add(this.m_liveCallCheckBox);
		base.Name = "GiftCardAuthPreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
