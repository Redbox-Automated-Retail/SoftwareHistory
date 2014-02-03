using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
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

	private CheckBox m_queueBasedRentalLimitCheckBox;

	private CheckBox m_enableOfflineBadCardListCheckBox;

	private ErrorProvider m_errorProvider;

	private TextBox m_maximumItemsTextBox;

	private Label label2;

	private CheckBox m_enforceShoppingCartLimitCheckBox;

	private ComboBox m_authorizeResponseTypeComboBox;

	private Label label1;

	private CheckBox m_simulateServerBusyCheckBox;

	private CheckBox m_simulateCommunicationsFailureCheckBox;

	private CheckBox m_liveCallCheckBox;

	private Label m_headerLabel;

	private Button m_deleteOfferButton;

	private ListBox m_offersListBox;

	private Button m_addOfferButton;

	private TextBox m_addOfferTextBox;

	private Label m_OffersLabel;

	private CheckBox m_customerProfileCheckBox;

	private Label label4;

	private TextBox m_cpLastNameTextBox;

	private Label label3;

	private TextBox m_cpFirstNameTextBox;

	private Label label5;

	private TextBox m_EmailAddressTextBox;

	private ComboBox m_confirmationStatusComboBox;

	private Label label6;

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
		m_maximumItemsTextBox.Text = MerchantServicesProxy.Instance.MaximumShoppingCartItems.ToString();
		m_queueBasedRentalLimitCheckBox.Checked = MerchantServicesProxy.Instance.EnableQueueBasedRentalLimit;
		m_enableOfflineBadCardListCheckBox.Checked = MerchantServicesProxy.Instance.EnableOfflineBadCardList;
		m_enforceShoppingCartLimitCheckBox.Checked = MerchantServicesProxy.Instance.EnforceShoppingCartItemLimit;
		m_simulateCommunicationsFailureCheckBox.Checked = MerchantServicesProxy.Instance.SimulateCommunicationsFailure;
		m_authorizeResponseTypeComboBox.Items.Clear();
		string[] names = Enum.GetNames(typeof(AuthorizeResponse));
		foreach (string item in names)
		{
			m_authorizeResponseTypeComboBox.Items.Add(item);
		}
		m_authorizeResponseTypeComboBox.SelectedIndex = m_authorizeResponseTypeComboBox.FindString(MerchantServicesProxy.Instance.AuthorizeResponse.ToString());
		m_customerProfileCheckBox.Checked = MerchantServicesProxy.Instance.HasCustomerProfile;
		m_cpFirstNameTextBox.Text = MerchantServicesProxy.Instance.CPFirstName;
		m_cpLastNameTextBox.Text = MerchantServicesProxy.Instance.CPLastName;
		m_EmailAddressTextBox.Text = MerchantServicesProxy.Instance.EmailAddress;
		m_confirmationStatusComboBox.Items.Clear();
		string[] names2 = Enum.GetNames(typeof(ConfirmationStatus));
		foreach (string item2 in names2)
		{
			m_confirmationStatusComboBox.Items.Add(item2);
		}
		m_confirmationStatusComboBox.SelectedIndex = m_confirmationStatusComboBox.FindString(MerchantServicesProxy.Instance.ConfirmationStatus.ToString());
		m_offersListBox.Items.Clear();
		string[] userOffers = MerchantServicesProxy.Instance.UserOffers;
		foreach (string item3 in userOffers)
		{
			m_offersListBox.Items.Add(item3);
		}
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

	private void OnEnforceLimitCheckBoxChanged(object sender, EventArgs e)
	{
		MerchantServicesProxy.Instance.EnforceShoppingCartItemLimit = m_enforceShoppingCartLimitCheckBox.Checked;
		m_maximumItemsTextBox.Enabled = m_enforceShoppingCartLimitCheckBox.Checked;
		m_queueBasedRentalLimitCheckBox.Enabled = m_enforceShoppingCartLimitCheckBox.Checked;
	}

	private void OnBadCardListCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.EnableOfflineBadCardList = m_enableOfflineBadCardListCheckBox.Checked;
		}
	}

	private void OnMaximumItemsLeave(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			m_errorProvider.SetError(m_maximumItemsTextBox, string.Empty);
			if (!int.TryParse(m_maximumItemsTextBox.Text, out var result) || result < 0)
			{
				m_errorProvider.SetError(m_maximumItemsTextBox, "Maximum items must be a valid integer value.");
			}
			else
			{
				MerchantServicesProxy.Instance.MaximumShoppingCartItems = result;
			}
		}
	}

	private void OnQueueBasedRentalLimitCheckChanged(object sender, EventArgs e)
	{
		if (m_isLoaded)
		{
			MerchantServicesProxy.Instance.EnableQueueBasedRentalLimit = m_queueBasedRentalLimitCheckBox.Checked;
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
			MerchantServicesProxy.Instance.ConfirmationStatus = Enum<ConfirmationStatus>.ParseIgnoringCase(m_confirmationStatusComboBox.SelectedItem.ToString(), ConfirmationStatus.DoNotShow);
		}
	}

	private void m_addOfferButton_Click(object sender, EventArgs e)
	{
		if (m_isLoaded && !string.IsNullOrEmpty(m_addOfferTextBox.Text))
		{
			m_offersListBox.Items.Add(m_addOfferTextBox.Text);
			MerchantServicesProxy.Instance.UserOffers = (from object offer in m_offersListBox.Items
				select offer.ToString()).ToArray();
		}
	}

	private void m_deleteOfferButton_Click(object sender, EventArgs e)
	{
		if (m_isLoaded && m_offersListBox.SelectedIndex != -1)
		{
			m_offersListBox.Items.RemoveAt(m_offersListBox.SelectedIndex);
			MerchantServicesProxy.Instance.UserOffers = (from object offer in m_offersListBox.Items
				select offer.ToString()).ToArray();
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
		this.m_queueBasedRentalLimitCheckBox = new System.Windows.Forms.CheckBox();
		this.m_enableOfflineBadCardListCheckBox = new System.Windows.Forms.CheckBox();
		this.m_errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
		this.m_maximumItemsTextBox = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.m_enforceShoppingCartLimitCheckBox = new System.Windows.Forms.CheckBox();
		this.m_authorizeResponseTypeComboBox = new System.Windows.Forms.ComboBox();
		this.label1 = new System.Windows.Forms.Label();
		this.m_simulateServerBusyCheckBox = new System.Windows.Forms.CheckBox();
		this.m_simulateCommunicationsFailureCheckBox = new System.Windows.Forms.CheckBox();
		this.m_liveCallCheckBox = new System.Windows.Forms.CheckBox();
		this.m_headerLabel = new System.Windows.Forms.Label();
		this.m_OffersLabel = new System.Windows.Forms.Label();
		this.m_addOfferTextBox = new System.Windows.Forms.TextBox();
		this.m_addOfferButton = new System.Windows.Forms.Button();
		this.m_offersListBox = new System.Windows.Forms.ListBox();
		this.m_deleteOfferButton = new System.Windows.Forms.Button();
		this.m_customerProfileCheckBox = new System.Windows.Forms.CheckBox();
		this.m_cpFirstNameTextBox = new System.Windows.Forms.TextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.m_cpLastNameTextBox = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.m_EmailAddressTextBox = new System.Windows.Forms.TextBox();
		this.label6 = new System.Windows.Forms.Label();
		this.m_confirmationStatusComboBox = new System.Windows.Forms.ComboBox();
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).BeginInit();
		base.SuspendLayout();
		this.m_queueBasedRentalLimitCheckBox.AutoSize = true;
		this.m_queueBasedRentalLimitCheckBox.Enabled = false;
		this.m_queueBasedRentalLimitCheckBox.Location = new System.Drawing.Point(30, 180);
		this.m_queueBasedRentalLimitCheckBox.Name = "m_queueBasedRentalLimitCheckBox";
		this.m_queueBasedRentalLimitCheckBox.Size = new System.Drawing.Size(206, 17);
		this.m_queueBasedRentalLimitCheckBox.TabIndex = 16;
		this.m_queueBasedRentalLimitCheckBox.Text = "Enable queue-based rental limit check";
		this.m_queueBasedRentalLimitCheckBox.UseVisualStyleBackColor = true;
		this.m_queueBasedRentalLimitCheckBox.CheckedChanged += new System.EventHandler(OnQueueBasedRentalLimitCheckChanged);
		this.m_enableOfflineBadCardListCheckBox.AutoSize = true;
		this.m_enableOfflineBadCardListCheckBox.Location = new System.Drawing.Point(30, 157);
		this.m_enableOfflineBadCardListCheckBox.Name = "m_enableOfflineBadCardListCheckBox";
		this.m_enableOfflineBadCardListCheckBox.Size = new System.Drawing.Size(150, 17);
		this.m_enableOfflineBadCardListCheckBox.TabIndex = 17;
		this.m_enableOfflineBadCardListCheckBox.Text = "Enable offline bad card list";
		this.m_enableOfflineBadCardListCheckBox.UseVisualStyleBackColor = true;
		this.m_enableOfflineBadCardListCheckBox.CheckedChanged += new System.EventHandler(OnBadCardListCheckChanged);
		this.m_errorProvider.ContainerControl = this;
		this.m_maximumItemsTextBox.Enabled = false;
		this.m_maximumItemsTextBox.Location = new System.Drawing.Point(416, 177);
		this.m_maximumItemsTextBox.Name = "m_maximumItemsTextBox";
		this.m_maximumItemsTextBox.Size = new System.Drawing.Size(70, 20);
		this.m_maximumItemsTextBox.TabIndex = 15;
		this.m_maximumItemsTextBox.Leave += new System.EventHandler(OnMaximumItemsLeave);
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(323, 181);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(82, 13);
		this.label2.TabIndex = 14;
		this.label2.Text = "Maximum Items:";
		this.m_enforceShoppingCartLimitCheckBox.AutoSize = true;
		this.m_enforceShoppingCartLimitCheckBox.Location = new System.Drawing.Point(30, 203);
		this.m_enforceShoppingCartLimitCheckBox.Name = "m_enforceShoppingCartLimitCheckBox";
		this.m_enforceShoppingCartLimitCheckBox.Size = new System.Drawing.Size(281, 17);
		this.m_enforceShoppingCartLimitCheckBox.TabIndex = 13;
		this.m_enforceShoppingCartLimitCheckBox.Text = "Enforce shopping cart limit in offline/stand alone mode";
		this.m_enforceShoppingCartLimitCheckBox.UseVisualStyleBackColor = true;
		this.m_enforceShoppingCartLimitCheckBox.CheckedChanged += new System.EventHandler(OnEnforceLimitCheckBoxChanged);
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
		this.m_OffersLabel.AutoSize = true;
		this.m_OffersLabel.Location = new System.Drawing.Point(31, 394);
		this.m_OffersLabel.Name = "m_OffersLabel";
		this.m_OffersLabel.Size = new System.Drawing.Size(38, 13);
		this.m_OffersLabel.TabIndex = 42;
		this.m_OffersLabel.Text = "Offers:";
		this.m_addOfferTextBox.Location = new System.Drawing.Point(75, 391);
		this.m_addOfferTextBox.Name = "m_addOfferTextBox";
		this.m_addOfferTextBox.Size = new System.Drawing.Size(330, 20);
		this.m_addOfferTextBox.TabIndex = 43;
		this.m_addOfferButton.Location = new System.Drawing.Point(416, 391);
		this.m_addOfferButton.Name = "m_addOfferButton";
		this.m_addOfferButton.Size = new System.Drawing.Size(75, 23);
		this.m_addOfferButton.TabIndex = 44;
		this.m_addOfferButton.Text = "Add Offer";
		this.m_addOfferButton.UseVisualStyleBackColor = true;
		this.m_addOfferButton.Click += new System.EventHandler(m_addOfferButton_Click);
		this.m_offersListBox.FormattingEnabled = true;
		this.m_offersListBox.Location = new System.Drawing.Point(75, 417);
		this.m_offersListBox.Name = "m_offersListBox";
		this.m_offersListBox.ScrollAlwaysVisible = true;
		this.m_offersListBox.Size = new System.Drawing.Size(330, 69);
		this.m_offersListBox.TabIndex = 45;
		this.m_deleteOfferButton.Location = new System.Drawing.Point(416, 463);
		this.m_deleteOfferButton.Name = "m_deleteOfferButton";
		this.m_deleteOfferButton.Size = new System.Drawing.Size(75, 23);
		this.m_deleteOfferButton.TabIndex = 46;
		this.m_deleteOfferButton.Text = "Delete Offer";
		this.m_deleteOfferButton.UseVisualStyleBackColor = true;
		this.m_deleteOfferButton.Click += new System.EventHandler(m_deleteOfferButton_Click);
		this.m_customerProfileCheckBox.AutoSize = true;
		this.m_customerProfileCheckBox.Location = new System.Drawing.Point(30, 238);
		this.m_customerProfileCheckBox.Name = "m_customerProfileCheckBox";
		this.m_customerProfileCheckBox.Size = new System.Drawing.Size(124, 17);
		this.m_customerProfileCheckBox.TabIndex = 47;
		this.m_customerProfileCheckBox.Text = "Has Customer Profile";
		this.m_customerProfileCheckBox.UseVisualStyleBackColor = true;
		this.m_customerProfileCheckBox.CheckedChanged += new System.EventHandler(OnHasCustomerProfileCheckChanged);
		this.m_cpFirstNameTextBox.Location = new System.Drawing.Point(169, 260);
		this.m_cpFirstNameTextBox.Name = "m_cpFirstNameTextBox";
		this.m_cpFirstNameTextBox.Size = new System.Drawing.Size(317, 20);
		this.m_cpFirstNameTextBox.TabIndex = 48;
		this.m_cpFirstNameTextBox.Leave += new System.EventHandler(CPFirstNameTextBox_Leave);
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(50, 263);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(74, 13);
		this.label3.TabIndex = 49;
		this.label3.Text = "CP First Name";
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(50, 286);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(75, 13);
		this.label4.TabIndex = 51;
		this.label4.Text = "CP Last Name";
		this.m_cpLastNameTextBox.Location = new System.Drawing.Point(169, 283);
		this.m_cpLastNameTextBox.Name = "m_cpLastNameTextBox";
		this.m_cpLastNameTextBox.Size = new System.Drawing.Size(317, 20);
		this.m_cpLastNameTextBox.TabIndex = 50;
		this.m_cpLastNameTextBox.Leave += new System.EventHandler(CPLastNameTextBox_Leave);
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(50, 309);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(73, 13);
		this.label5.TabIndex = 53;
		this.label5.Text = "Email Address";
		this.m_EmailAddressTextBox.Location = new System.Drawing.Point(169, 306);
		this.m_EmailAddressTextBox.Name = "m_EmailAddressTextBox";
		this.m_EmailAddressTextBox.Size = new System.Drawing.Size(317, 20);
		this.m_EmailAddressTextBox.TabIndex = 52;
		this.m_EmailAddressTextBox.Leave += new System.EventHandler(EmailAddressTextBox_Leave);
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(31, 345);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(101, 13);
		this.label6.TabIndex = 54;
		this.label6.Text = "Confirmation Status:";
		this.m_confirmationStatusComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.m_confirmationStatusComboBox.FormattingEnabled = true;
		this.m_confirmationStatusComboBox.Location = new System.Drawing.Point(169, 342);
		this.m_confirmationStatusComboBox.Name = "m_confirmationStatusComboBox";
		this.m_confirmationStatusComboBox.Size = new System.Drawing.Size(317, 21);
		this.m_confirmationStatusComboBox.TabIndex = 55;
		this.m_confirmationStatusComboBox.SelectedIndexChanged += new System.EventHandler(OnSelectedConfirmationStatusChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.m_confirmationStatusComboBox);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.m_EmailAddressTextBox);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.m_cpLastNameTextBox);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.m_cpFirstNameTextBox);
		base.Controls.Add(this.m_customerProfileCheckBox);
		base.Controls.Add(this.m_deleteOfferButton);
		base.Controls.Add(this.m_offersListBox);
		base.Controls.Add(this.m_addOfferButton);
		base.Controls.Add(this.m_addOfferTextBox);
		base.Controls.Add(this.m_OffersLabel);
		base.Controls.Add(this.m_headerLabel);
		base.Controls.Add(this.m_queueBasedRentalLimitCheckBox);
		base.Controls.Add(this.m_enableOfflineBadCardListCheckBox);
		base.Controls.Add(this.m_maximumItemsTextBox);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.m_enforceShoppingCartLimitCheckBox);
		base.Controls.Add(this.m_authorizeResponseTypeComboBox);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.m_simulateServerBusyCheckBox);
		base.Controls.Add(this.m_simulateCommunicationsFailureCheckBox);
		base.Controls.Add(this.m_liveCallCheckBox);
		base.Name = "MerchantServicesPreferencePage";
		base.Size = new System.Drawing.Size(542, 506);
		((System.ComponentModel.ISupportInitialize)this.m_errorProvider).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
