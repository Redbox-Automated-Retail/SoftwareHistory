using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy;
using Redbox.KioskServices.Proxy.ComponentModel;

namespace Redbox.KioskServices.KernelExtension;

public class MerchantServicesPreferencePage : UserControl, IPreferencePageHost
{
	private bool m_isLoaded;

	private IContainer components;

	private CheckBox m_enableOfflineBadCardListCheckBox;

	private ErrorProvider m_errorProvider;

	private ComboBox m_authorizeResponseTypeComboBox;

	private Label label1;

	private CheckBox m_simulateServerBusyCheckBox;

	private CheckBox m_simulateCommunicationsFailureCheckBox;

	private CheckBox m_liveCallCheckBox;

	private Label m_headerLabel;

	private CheckBox m_customerProfileCheckBox;

	private Label label4;

	private TextBox m_cpLastNameTextBox;

	private Label label3;

	private TextBox m_cpFirstNameTextBox;

	private Label label5;

	private TextBox m_EmailAddressTextBox;

	private ComboBox m_confirmationStatusComboBox;

	private Label label6;

	private CheckBox m_isRBICheckBox;

	private TextBox m_transactionIdTextBox;

	private Label label7;

	private TextBox m_accountNumberTextBox;

	private Label label2;

	private TextBox m_cpAccountNumberTextBox;

	private Label label8;

	private Label label9;

	private TextBox m_CancelledReservations;

	private TextBox m_InvalidReservations;

	private Label label10;

	private Label lblTransactionEntities;

	private TextBox txtTransactionEntities;

	public MerchantServicesPreferencePage()
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
		m_liveCallCheckBox.Checked = MerchantServicesProxy.Instance.LiveCall;
		m_simulateServerBusyCheckBox.Checked = MerchantServicesProxy.Instance.SimulateServerBusyError;
		m_enableOfflineBadCardListCheckBox.Checked = MerchantServicesProxy.Instance.EnableOfflineBadCardList;
		m_simulateCommunicationsFailureCheckBox.Checked = MerchantServicesProxy.Instance.SimulateCommunicationsFailure;
		m_authorizeResponseTypeComboBox.Items.Clear();
		string[] names = Enum.GetNames(typeof(AuthorizeResponse));
		foreach (string item in names)
		{
			m_authorizeResponseTypeComboBox.Items.Add(item);
		}
		m_authorizeResponseTypeComboBox.SelectedIndex = m_authorizeResponseTypeComboBox.FindString(MerchantServicesProxy.Instance.AuthorizeResponse.ToString());
		m_accountNumberTextBox.Text = MerchantServicesProxy.Instance.AccountNumber;
		m_transactionIdTextBox.Text = MerchantServicesProxy.Instance.TransactionId;
		m_customerProfileCheckBox.Checked = MerchantServicesProxy.Instance.HasCustomerProfile;
		m_cpAccountNumberTextBox.Text = MerchantServicesProxy.Instance.CPAccountNumber;
		m_cpFirstNameTextBox.Text = MerchantServicesProxy.Instance.CPFirstName;
		m_cpLastNameTextBox.Text = MerchantServicesProxy.Instance.CPLastName;
		m_EmailAddressTextBox.Text = MerchantServicesProxy.Instance.EmailAddress;
		m_isRBICheckBox.Checked = MerchantServicesProxy.Instance.IsRBI;
		m_confirmationStatusComboBox.Items.Clear();
		names = Enum.GetNames(typeof(ConfirmationStatus));
		foreach (string item2 in names)
		{
			m_confirmationStatusComboBox.Items.Add(item2);
		}
		m_confirmationStatusComboBox.SelectedIndex = m_confirmationStatusComboBox.FindString(MerchantServicesProxy.Instance.ConfirmationStatus.ToString());
		m_CancelledReservations.Text = MerchantServicesProxy.Instance.CancelledReservations;
		m_InvalidReservations.Text = MerchantServicesProxy.Instance.InvalidReservations;
		m_isLoaded = true;
	}

	private void OnLiveCallCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.LiveCall = m_liveCallCheckBox.Checked;
		}
	}

	private void OnSelectedResponseTypeChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.AuthorizeResponse = Enum<AuthorizeResponse>.ParseIgnoringCase(m_authorizeResponseTypeComboBox.SelectedItem.ToString(), AuthorizeResponse.Approved);
		}
	}

	private void OnSimulateServerBusyCheckChange(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.SimulateServerBusyError = m_simulateServerBusyCheckBox.Checked;
		}
	}

	private void OnSimulateCommunicationsFailureCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.SimulateCommunicationsFailure = m_simulateCommunicationsFailureCheckBox.Checked;
		}
	}

	private void OnBadCardListCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.EnableOfflineBadCardList = m_enableOfflineBadCardListCheckBox.Checked;
		}
	}

	private void OnHasCustomerProfileCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.HasCustomerProfile = m_customerProfileCheckBox.Checked;
		}
	}

	private void CPFirstNameTextBox_Leave(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.CPFirstName = m_cpFirstNameTextBox.Text;
		}
	}

	private void CPLastNameTextBox_Leave(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.CPLastName = m_cpLastNameTextBox.Text;
		}
	}

	private void EmailAddressTextBox_Leave(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.EmailAddress = m_EmailAddressTextBox.Text;
		}
	}

	private void OnSelectedConfirmationStatusChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.ConfirmationStatus = Enum<ConfirmationStatus>.ParseIgnoringCase(m_confirmationStatusComboBox.SelectedItem.ToString(), ConfirmationStatus.DontShow);
		}
	}

	private void OnIsRBICheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.IsRBI = m_isRBICheckBox.Checked;
		}
	}

	private void m_cpAccountNumberTextBox_Leave(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.CPAccountNumber = (string.IsNullOrWhiteSpace(m_cpAccountNumberTextBox.Text) ? string.Empty : m_cpAccountNumberTextBox.Text);
		}
	}

	private void m_transactionIdTextBox_Leave(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.TransactionId = (string.IsNullOrWhiteSpace(m_transactionIdTextBox.Text) ? string.Empty : m_transactionIdTextBox.Text);
		}
	}

	private void m_accountNumberTextBox_Leave(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.AccountNumber = (string.IsNullOrWhiteSpace(m_accountNumberTextBox.Text) ? string.Empty : m_accountNumberTextBox.Text);
		}
	}

	private void m_CancelledReservations_Leave(object sender, EventArgs e)
	{
		if (!m_isLoaded)
		{
			return;
		}
		string[] array = m_CancelledReservations.Text.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (!long.TryParse(array[i], out var _))
			{
				MessageBox.Show("Cancelled reservations must be a comma-delimited list of reference numbers.");
				return;
			}
		}
		MerchantServicesProxy.Instance.CancelledReservations = (string.IsNullOrWhiteSpace(m_CancelledReservations.Text) ? string.Empty : m_CancelledReservations.Text);
	}

	private void m_InvalidReservations_Leave(object sender, EventArgs e)
	{
		if (!m_isLoaded)
		{
			return;
		}
		string[] array = m_InvalidReservations.Text.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (!long.TryParse(array[i], out var _))
			{
				MessageBox.Show("Invalid reservations must be a comma-delimited list of reference numbers.");
				return;
			}
		}
		MerchantServicesProxy.Instance.InvalidReservations = (string.IsNullOrWhiteSpace(m_InvalidReservations.Text) ? string.Empty : m_InvalidReservations.Text);
	}

	private void TransactionEntitiesTextBox_Leave(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.TransactionEntities = txtTransactionEntities.Text;
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
		this.m_enableOfflineBadCardListCheckBox = new System.Windows.Forms.CheckBox();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_authorizeResponseTypeComboBox = new System.Windows.Forms.ComboBox();
		this.label1 = new System.Windows.Forms.Label();
		this.m_simulateServerBusyCheckBox = new System.Windows.Forms.CheckBox();
		this.m_simulateCommunicationsFailureCheckBox = new System.Windows.Forms.CheckBox();
		this.m_liveCallCheckBox = new System.Windows.Forms.CheckBox();
		this.m_headerLabel = new System.Windows.Forms.Label();
		this.m_customerProfileCheckBox = new System.Windows.Forms.CheckBox();
		this.m_cpFirstNameTextBox = new System.Windows.Forms.TextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.m_cpLastNameTextBox = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.m_EmailAddressTextBox = new System.Windows.Forms.TextBox();
		this.label6 = new System.Windows.Forms.Label();
		this.m_confirmationStatusComboBox = new System.Windows.Forms.ComboBox();
		this.m_isRBICheckBox = new System.Windows.Forms.CheckBox();
		this.label2 = new System.Windows.Forms.Label();
		this.m_accountNumberTextBox = new System.Windows.Forms.TextBox();
		this.m_transactionIdTextBox = new System.Windows.Forms.TextBox();
		this.label7 = new System.Windows.Forms.Label();
		this.label8 = new System.Windows.Forms.Label();
		this.m_cpAccountNumberTextBox = new System.Windows.Forms.TextBox();
		this.label9 = new System.Windows.Forms.Label();
		this.m_CancelledReservations = new System.Windows.Forms.TextBox();
		this.label10 = new System.Windows.Forms.Label();
		this.m_InvalidReservations = new System.Windows.Forms.TextBox();
		this.txtTransactionEntities = new System.Windows.Forms.TextBox();
		this.lblTransactionEntities = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_enableOfflineBadCardListCheckBox.AutoSize = true;
		this.m_enableOfflineBadCardListCheckBox.Location = new System.Drawing.Point(30, 157);
		this.m_enableOfflineBadCardListCheckBox.Name = "m_enableOfflineBadCardListCheckBox";
		this.m_enableOfflineBadCardListCheckBox.Size = new System.Drawing.Size(150, 17);
		this.m_enableOfflineBadCardListCheckBox.TabIndex = 17;
		this.m_enableOfflineBadCardListCheckBox.Text = "Enable offline bad card list";
		this.m_enableOfflineBadCardListCheckBox.UseVisualStyleBackColor = true;
		this.m_enableOfflineBadCardListCheckBox.CheckedChanged += new System.EventHandler(OnBadCardListCheckChanged);
		this.m_errorProvider.ContainerControl = this;
		this.m_authorizeResponseTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_authorizeResponseTypeComboBox.FormattingEnabled = true;
		this.m_authorizeResponseTypeComboBox.Location = new System.Drawing.Point(169, 130);
		this.m_authorizeResponseTypeComboBox.Name = "m_authorizeResponseTypeComboBox";
		this.m_authorizeResponseTypeComboBox.Size = new System.Drawing.Size(317, 21);
		this.m_authorizeResponseTypeComboBox.TabIndex = 12;
		this.m_authorizeResponseTypeComboBox.SelectedIndexChanged += new System.EventHandler(OnSelectedResponseTypeChanged);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(31, 133);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(132, 13);
		this.label1.TabIndex = 11;
		this.label1.Text = "Authorize Response Type:";
		this.m_simulateServerBusyCheckBox.AutoSize = true;
		this.m_simulateServerBusyCheckBox.Location = new System.Drawing.Point(30, 99);
		this.m_simulateServerBusyCheckBox.Name = "m_simulateServerBusyCheckBox";
		this.m_simulateServerBusyCheckBox.Size = new System.Drawing.Size(126, 17);
		this.m_simulateServerBusyCheckBox.TabIndex = 19;
		this.m_simulateServerBusyCheckBox.Text = "Simulate Server Busy";
		this.m_simulateServerBusyCheckBox.UseVisualStyleBackColor = true;
		this.m_simulateServerBusyCheckBox.CheckedChanged += new System.EventHandler(OnSimulateServerBusyCheckChange);
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
		this.m_headerLabel.Text = "  Kiosk Services: Payment Auth. && Customer Lookup";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.m_customerProfileCheckBox.AutoSize = true;
		this.m_customerProfileCheckBox.Location = new System.Drawing.Point(30, 226);
		this.m_customerProfileCheckBox.Name = "m_customerProfileCheckBox";
		this.m_customerProfileCheckBox.Size = new System.Drawing.Size(124, 17);
		this.m_customerProfileCheckBox.TabIndex = 47;
		this.m_customerProfileCheckBox.Text = "Has Customer Profile";
		this.m_customerProfileCheckBox.UseVisualStyleBackColor = true;
		this.m_customerProfileCheckBox.CheckedChanged += new System.EventHandler(OnHasCustomerProfileCheckChanged);
		this.m_cpFirstNameTextBox.Location = new System.Drawing.Point(169, 267);
		this.m_cpFirstNameTextBox.Name = "m_cpFirstNameTextBox";
		this.m_cpFirstNameTextBox.Size = new System.Drawing.Size(317, 20);
		this.m_cpFirstNameTextBox.TabIndex = 48;
		this.m_cpFirstNameTextBox.Leave += new System.EventHandler(CPFirstNameTextBox_Leave);
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(49, 270);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(74, 13);
		this.label3.TabIndex = 49;
		this.label3.Text = "CP First Name";
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(50, 294);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(75, 13);
		this.label4.TabIndex = 51;
		this.label4.Text = "CP Last Name";
		this.m_cpLastNameTextBox.Location = new System.Drawing.Point(169, 291);
		this.m_cpLastNameTextBox.Name = "m_cpLastNameTextBox";
		this.m_cpLastNameTextBox.Size = new System.Drawing.Size(317, 20);
		this.m_cpLastNameTextBox.TabIndex = 50;
		this.m_cpLastNameTextBox.Leave += new System.EventHandler(CPLastNameTextBox_Leave);
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(50, 317);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(73, 13);
		this.label5.TabIndex = 53;
		this.label5.Text = "Email Address";
		this.m_EmailAddressTextBox.Location = new System.Drawing.Point(169, 314);
		this.m_EmailAddressTextBox.Name = "m_EmailAddressTextBox";
		this.m_EmailAddressTextBox.Size = new System.Drawing.Size(317, 20);
		this.m_EmailAddressTextBox.TabIndex = 52;
		this.m_EmailAddressTextBox.Leave += new System.EventHandler(EmailAddressTextBox_Leave);
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(31, 351);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(101, 13);
		this.label6.TabIndex = 54;
		this.label6.Text = "Confirmation Status:";
		this.m_confirmationStatusComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_confirmationStatusComboBox.FormattingEnabled = true;
		this.m_confirmationStatusComboBox.Location = new System.Drawing.Point(169, 348);
		this.m_confirmationStatusComboBox.Name = "m_confirmationStatusComboBox";
		this.m_confirmationStatusComboBox.Size = new System.Drawing.Size(317, 21);
		this.m_confirmationStatusComboBox.TabIndex = 55;
		this.m_confirmationStatusComboBox.SelectedIndexChanged += new System.EventHandler(OnSelectedConfirmationStatusChanged);
		this.m_isRBICheckBox.AutoSize = true;
		this.m_isRBICheckBox.Location = new System.Drawing.Point(53, 333);
		this.m_isRBICheckBox.Name = "m_isRBICheckBox";
		this.m_isRBICheckBox.Size = new System.Drawing.Size(52, 17);
		this.m_isRBICheckBox.TabIndex = 64;
		this.m_isRBICheckBox.Text = "IsRBI";
		this.m_isRBICheckBox.UseVisualStyleBackColor = true;
		this.m_isRBICheckBox.CheckedChanged += new System.EventHandler(OnIsRBICheckChanged);
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(30, 181);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(87, 13);
		this.label2.TabIndex = 65;
		this.label2.Text = "Account Number";
		this.m_accountNumberTextBox.Location = new System.Drawing.Point(169, 178);
		this.m_accountNumberTextBox.Name = "m_accountNumberTextBox";
		this.m_accountNumberTextBox.Size = new System.Drawing.Size(317, 20);
		this.m_accountNumberTextBox.TabIndex = 66;
		this.m_accountNumberTextBox.Leave += new System.EventHandler(m_accountNumberTextBox_Leave);
		this.m_transactionIdTextBox.Location = new System.Drawing.Point(169, 202);
		this.m_transactionIdTextBox.Name = "m_transactionIdTextBox";
		this.m_transactionIdTextBox.Size = new System.Drawing.Size(317, 20);
		this.m_transactionIdTextBox.TabIndex = 68;
		this.m_transactionIdTextBox.Leave += new System.EventHandler(m_transactionIdTextBox_Leave);
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(30, 205);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(77, 13);
		this.label7.TabIndex = 67;
		this.label7.Text = "Transaction ID";
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(49, 246);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(104, 13);
		this.label8.TabIndex = 69;
		this.label8.Text = "CP Account Number";
		this.m_cpAccountNumberTextBox.Location = new System.Drawing.Point(169, 243);
		this.m_cpAccountNumberTextBox.Name = "m_cpAccountNumberTextBox";
		this.m_cpAccountNumberTextBox.Size = new System.Drawing.Size(317, 20);
		this.m_cpAccountNumberTextBox.TabIndex = 70;
		this.m_cpAccountNumberTextBox.Leave += new System.EventHandler(m_cpAccountNumberTextBox_Leave);
		this.label9.AutoSize = true;
		this.label9.Location = new System.Drawing.Point(30, 378);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(119, 13);
		this.label9.TabIndex = 72;
		this.label9.Text = "Cancelled Reservations";
		this.m_CancelledReservations.Location = new System.Drawing.Point(169, 375);
		this.m_CancelledReservations.Name = "m_CancelledReservations";
		this.m_CancelledReservations.Size = new System.Drawing.Size(317, 20);
		this.m_CancelledReservations.TabIndex = 71;
		this.m_CancelledReservations.Leave += new System.EventHandler(m_CancelledReservations_Leave);
		this.label10.AutoSize = true;
		this.label10.Location = new System.Drawing.Point(30, 406);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(103, 13);
		this.label10.TabIndex = 73;
		this.label10.Text = "Invalid Reservations";
		this.m_InvalidReservations.Location = new System.Drawing.Point(169, 403);
		this.m_InvalidReservations.Name = "m_InvalidReservations";
		this.m_InvalidReservations.Size = new System.Drawing.Size(317, 20);
		this.m_InvalidReservations.TabIndex = 74;
		this.m_InvalidReservations.Leave += new System.EventHandler(m_InvalidReservations_Leave);
		this.txtTransactionEntities.Location = new System.Drawing.Point(169, 429);
		this.txtTransactionEntities.Name = "txtTransactionEntities";
		this.txtTransactionEntities.Size = new System.Drawing.Size(317, 20);
		this.txtTransactionEntities.TabIndex = 75;
		string text = "//RC//".Replace('/', '"');
		this.txtTransactionEntities.Text = "[{\"DiscId\":16336447,\"Name\":\"TivoQueryId\",\"Value\":\"132ghlp4rasdf23gqrfjasdfio\",\"TitleId\":6686}, {\"DiscId\":16336447,\"Name\":\"OfferCode\",\"Value\":" + text + ",\"TitleId\":6686}]";
		this.txtTransactionEntities.Leave += new System.EventHandler(TransactionEntitiesTextBox_Leave);
		this.lblTransactionEntities.AutoSize = true;
		this.lblTransactionEntities.Location = new System.Drawing.Point(31, 432);
		this.lblTransactionEntities.Name = "lblTransactionEntities";
		this.lblTransactionEntities.Size = new System.Drawing.Size(97, 13);
		this.lblTransactionEntities.TabIndex = 76;
		this.lblTransactionEntities.Text = "TransactionEntities";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.lblTransactionEntities);
		base.Controls.Add(this.txtTransactionEntities);
		base.Controls.Add(this.m_InvalidReservations);
		base.Controls.Add(this.label10);
		base.Controls.Add(this.label9);
		base.Controls.Add(this.m_CancelledReservations);
		base.Controls.Add(this.m_cpAccountNumberTextBox);
		base.Controls.Add(this.label8);
		base.Controls.Add(this.m_transactionIdTextBox);
		base.Controls.Add(this.label7);
		base.Controls.Add(this.m_accountNumberTextBox);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.m_isRBICheckBox);
		base.Controls.Add(this.m_confirmationStatusComboBox);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.m_EmailAddressTextBox);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.m_cpLastNameTextBox);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.m_cpFirstNameTextBox);
		base.Controls.Add(this.m_customerProfileCheckBox);
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.m_enableOfflineBadCardListCheckBox);
		base.Controls.Add(this.m_authorizeResponseTypeComboBox);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_simulateServerBusyCheckBox);
		base.Controls.Add(this.m_simulateCommunicationsFailureCheckBox);
		base.Controls.Add(this.m_liveCallCheckBox);
		base.Name = "MerchantServicesPreferencePage";
		base.Size = new System.Drawing.Size(542, 533);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
