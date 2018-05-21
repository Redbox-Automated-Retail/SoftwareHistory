using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.KioskServices.Proxy;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model.Personalization;

namespace Redbox.KioskServices.KernelExtension;

public class PersonalizationPreferencePage : UserControl, IPreferencePageHost
{
	private IContainer components;

	private Label m_headerLabel;

	private ComboBox cb_KioskLoginResponseErrors;

	private Label label1;

	private Label lbl_KioskLoginResponseErrors;

	private CheckBox cb_LiveCall;

	private Label label3;

	private Label label4;

	private Label label5;

	private Label label6;

	private Label label7;

	private Label label8;

	private Label label9;

	private TextBox tb_CustProfileNum;

	private TextBox tb_firstName;

	private TextBox tb_loginEmail;

	private TextBox tb_phoneNum;

	private TextBox tb_postalCode;

	private TextBox tb_loyaltyPointBal;

	private TextBox tb_loyaltyTier;

	private DateTimePicker dtp_birthDate;

	private CheckBox cb_simulateSuccess;

	private TextBox tb_loyaltyTierCounter;

	private Label label2;

	private TextBox tb_pointsExpiring;

	private Label label10;

	private DateTimePicker dtp_PointsExpirationDate;

	private Label label11;

	private Label label12;

	private ComboBox cbUpdateAccountError;

	private Panel panel1;

	private Label label14;

	private TextBox tb_Pin;

	private Label label13;

	private CheckBox cbTextClubOptIn;

	private CheckBox cbPunchCardOptIn;

	private Button btnUpdateAccount;

	public PersonalizationPreferencePage()
	{
		InitializeComponent();
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		cb_KioskLoginResponseErrors.DataSource = Enum.GetValues(typeof(KioskLoginResponseErrors));
		cbUpdateAccountError.DataSource = Enum.GetValues(typeof(UpdateAccountResponseErrors));
		PersonalizationServicesProxySimulator instance = PersonalizationServicesProxySimulator.Instance;
		if (instance != null)
		{
			cb_LiveCall.Checked = instance.LiveCall;
			cb_simulateSuccess.Checked = instance.SimulateSuccess;
			cb_KioskLoginResponseErrors.SelectedItem = instance.ResponseCode;
			tb_CustProfileNum.Text = instance.CustomerProfileNumber;
			tb_firstName.Text = instance.FirstName;
			tb_loginEmail.Text = instance.LoginEmail;
			tb_loyaltyPointBal.Text = instance.LoyaltyPointBalance.ToString();
			tb_loyaltyTier.Text = instance.LoyaltyTier;
			tb_phoneNum.Text = instance.PhoneNumber;
			tb_postalCode.Text = instance.PostalCode;
			tb_loyaltyTierCounter.Text = instance.LoyaltyTierCounter.ToString();
			tb_pointsExpiring.Text = instance.PointsExpiring.ToString();
		}
	}

	public bool SaveValues(IPreferencePage preferencePage)
	{
		PersonalizationServicesProxySimulator instance = PersonalizationServicesProxySimulator.Instance;
		if (instance == null)
		{
			return false;
		}
		instance.BirthDate = dtp_birthDate.Value;
		instance.CustomerProfileNumber = tb_CustProfileNum.Text;
		instance.FirstName = tb_firstName.Text;
		instance.LoginEmail = tb_loginEmail.Text;
		int result = 0;
		instance.LoyaltyPointBalance = (int.TryParse(tb_loyaltyPointBal.Text, out result) ? result : 0);
		instance.LoyaltyTier = tb_loyaltyTier.Text;
		instance.PhoneNumber = tb_phoneNum.Text;
		instance.PostalCode = tb_postalCode.Text;
		instance.LoyaltyTierCounter = (int.TryParse(tb_loyaltyTierCounter.Text, out var result2) ? result2 : 0);
		instance.PointsExpiring = (int.TryParse(tb_pointsExpiring.Text, out var result3) ? result3 : 0);
		instance.PointsExpirationDate = dtp_PointsExpirationDate.Value;
		return true;
	}

	private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
	{
		PersonalizationServicesProxySimulator instance = PersonalizationServicesProxySimulator.Instance;
		if (instance != null)
		{
			instance.ResponseCode = (KioskLoginResponseErrors)cb_KioskLoginResponseErrors.SelectedItem;
		}
	}

	private void OnLiveCallChanged(object sender, EventArgs e)
	{
		PersonalizationServicesProxySimulator instance = PersonalizationServicesProxySimulator.Instance;
		if (instance != null)
		{
			instance.LiveCall = cb_LiveCall.Checked;
		}
		ServiceLocator.Instance.RemoveService(typeof(IPersonalizationServices));
		if (instance.LiveCall)
		{
			ServiceLocator.Instance.AddService(typeof(IPersonalizationServices), PersonalizationServicesProxy.Instance);
		}
		else
		{
			ServiceLocator.Instance.AddService(typeof(IPersonalizationServices), PersonalizationServicesProxySimulator.Instance);
		}
	}

	private void OnSimulateSuccessChanged(object sender, EventArgs e)
	{
		PersonalizationServicesProxySimulator instance = PersonalizationServicesProxySimulator.Instance;
		if (instance != null)
		{
			instance.SimulateSuccess = cb_simulateSuccess.Checked;
		}
	}

	private void cbUpdateAccountError_SelectedIndexChanged(object sender, EventArgs e)
	{
		PersonalizationServicesProxySimulator instance = PersonalizationServicesProxySimulator.Instance;
		if (instance != null)
		{
			instance.UpdateAccountResponseCode = (UpdateAccountResponseErrors)cbUpdateAccountError.SelectedItem;
		}
	}

	private void btnUpdateAccount_Click(object sender, EventArgs e)
	{
		ServiceLocator.Instance.GetService<IPersonalizationService>().UpdateAccount(resultCallback: updateAcountCallback, pin: CardHelper.CopyToSecure(tb_Pin.Text), customerProfileNumber: tb_CustProfileNum.Text, emailAddress: tb_loginEmail.Text, phoneNumber: tb_phoneNum.Text, punchcardOptIn: cbPunchCardOptIn.Checked, textClubOptIn: cbTextClubOptIn.Checked);
	}

	private void updateAcountCallback(IUpdateAccountResult result)
	{
		if (result.Success)
		{
			MessageBox.Show("Call was successful");
		}
		else
		{
			MessageBox.Show($"Call was unsuccessful: {result.UpdateAccountResponseError} {result.ErrorMessage} ");
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
		this.cb_KioskLoginResponseErrors = new System.Windows.Forms.ComboBox();
		this.label1 = new System.Windows.Forms.Label();
		this.lbl_KioskLoginResponseErrors = new System.Windows.Forms.Label();
		this.cb_LiveCall = new System.Windows.Forms.CheckBox();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.label7 = new System.Windows.Forms.Label();
		this.label8 = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		this.tb_CustProfileNum = new System.Windows.Forms.TextBox();
		this.tb_firstName = new System.Windows.Forms.TextBox();
		this.tb_loginEmail = new System.Windows.Forms.TextBox();
		this.tb_phoneNum = new System.Windows.Forms.TextBox();
		this.tb_postalCode = new System.Windows.Forms.TextBox();
		this.tb_loyaltyPointBal = new System.Windows.Forms.TextBox();
		this.tb_loyaltyTier = new System.Windows.Forms.TextBox();
		this.dtp_birthDate = new System.Windows.Forms.DateTimePicker();
		this.cb_simulateSuccess = new System.Windows.Forms.CheckBox();
		this.tb_loyaltyTierCounter = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.tb_pointsExpiring = new System.Windows.Forms.TextBox();
		this.label10 = new System.Windows.Forms.Label();
		this.dtp_PointsExpirationDate = new System.Windows.Forms.DateTimePicker();
		this.label11 = new System.Windows.Forms.Label();
		this.label12 = new System.Windows.Forms.Label();
		this.cbUpdateAccountError = new System.Windows.Forms.ComboBox();
		this.panel1 = new System.Windows.Forms.Panel();
		this.tb_Pin = new System.Windows.Forms.TextBox();
		this.label13 = new System.Windows.Forms.Label();
		this.cbTextClubOptIn = new System.Windows.Forms.CheckBox();
		this.cbPunchCardOptIn = new System.Windows.Forms.CheckBox();
		this.btnUpdateAccount = new System.Windows.Forms.Button();
		this.label14 = new System.Windows.Forms.Label();
		this.panel1.SuspendLayout();
		base.SuspendLayout();
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkCyan;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(542, 38);
		this.m_headerLabel.TabIndex = 42;
		this.m_headerLabel.Text = "Personalization Services: Login";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cb_KioskLoginResponseErrors.FormattingEnabled = true;
		this.cb_KioskLoginResponseErrors.Location = new System.Drawing.Point(188, 76);
		this.cb_KioskLoginResponseErrors.Name = "cb_KioskLoginResponseErrors";
		this.cb_KioskLoginResponseErrors.Size = new System.Drawing.Size(236, 21);
		this.cb_KioskLoginResponseErrors.TabIndex = 43;
		this.cb_KioskLoginResponseErrors.SelectionChangeCommitted += new System.EventHandler(comboBox1_SelectedIndexChanged);
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(15, 110);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(126, 13);
		this.label1.TabIndex = 44;
		this.label1.Text = "Customer Profile Number:";
		this.lbl_KioskLoginResponseErrors.AutoSize = true;
		this.lbl_KioskLoginResponseErrors.Location = new System.Drawing.Point(15, 79);
		this.lbl_KioskLoginResponseErrors.Name = "lbl_KioskLoginResponseErrors";
		this.lbl_KioskLoginResponseErrors.Size = new System.Drawing.Size(146, 13);
		this.lbl_KioskLoginResponseErrors.TabIndex = 45;
		this.lbl_KioskLoginResponseErrors.Text = "Kiosk Login Response Errors:";
		this.cb_LiveCall.AutoSize = true;
		this.cb_LiveCall.Location = new System.Drawing.Point(188, 53);
		this.cb_LiveCall.Name = "cb_LiveCall";
		this.cb_LiveCall.Size = new System.Drawing.Size(66, 17);
		this.cb_LiveCall.TabIndex = 46;
		this.cb_LiveCall.Text = "Live Call";
		this.cb_LiveCall.UseVisualStyleBackColor = true;
		this.cb_LiveCall.CheckedChanged += new System.EventHandler(OnLiveCallChanged);
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(15, 136);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(60, 13);
		this.label3.TabIndex = 48;
		this.label3.Text = "First Name:";
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(15, 164);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(64, 13);
		this.label4.TabIndex = 49;
		this.label4.Text = "Login Email:";
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(15, 191);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(81, 13);
		this.label5.TabIndex = 50;
		this.label5.Text = "Phone Number:";
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(15, 219);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(67, 13);
		this.label6.TabIndex = 51;
		this.label6.Text = "Postal Code:";
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(15, 250);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(57, 13);
		this.label7.TabIndex = 52;
		this.label7.Text = "Birth Date:";
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(15, 276);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(112, 13);
		this.label8.TabIndex = 53;
		this.label8.Text = "Loyalty Point Balance:";
		this.label9.AutoSize = true;
		this.label9.Location = new System.Drawing.Point(15, 300);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(64, 13);
		this.label9.TabIndex = 54;
		this.label9.Text = "Loyalty Tier:";
		this.tb_CustProfileNum.Location = new System.Drawing.Point(188, 107);
		this.tb_CustProfileNum.Name = "tb_CustProfileNum";
		this.tb_CustProfileNum.Size = new System.Drawing.Size(236, 20);
		this.tb_CustProfileNum.TabIndex = 55;
		this.tb_firstName.Location = new System.Drawing.Point(188, 133);
		this.tb_firstName.Name = "tb_firstName";
		this.tb_firstName.Size = new System.Drawing.Size(236, 20);
		this.tb_firstName.TabIndex = 57;
		this.tb_loginEmail.Location = new System.Drawing.Point(188, 161);
		this.tb_loginEmail.Name = "tb_loginEmail";
		this.tb_loginEmail.Size = new System.Drawing.Size(236, 20);
		this.tb_loginEmail.TabIndex = 58;
		this.tb_phoneNum.Location = new System.Drawing.Point(188, 188);
		this.tb_phoneNum.Name = "tb_phoneNum";
		this.tb_phoneNum.Size = new System.Drawing.Size(236, 20);
		this.tb_phoneNum.TabIndex = 59;
		this.tb_postalCode.Location = new System.Drawing.Point(188, 216);
		this.tb_postalCode.Name = "tb_postalCode";
		this.tb_postalCode.Size = new System.Drawing.Size(236, 20);
		this.tb_postalCode.TabIndex = 60;
		this.tb_loyaltyPointBal.Location = new System.Drawing.Point(188, 273);
		this.tb_loyaltyPointBal.Name = "tb_loyaltyPointBal";
		this.tb_loyaltyPointBal.Size = new System.Drawing.Size(236, 20);
		this.tb_loyaltyPointBal.TabIndex = 62;
		this.tb_loyaltyTier.Location = new System.Drawing.Point(188, 297);
		this.tb_loyaltyTier.Name = "tb_loyaltyTier";
		this.tb_loyaltyTier.Size = new System.Drawing.Size(236, 20);
		this.tb_loyaltyTier.TabIndex = 63;
		this.dtp_birthDate.Location = new System.Drawing.Point(188, 244);
		this.dtp_birthDate.Name = "dtp_birthDate";
		this.dtp_birthDate.Size = new System.Drawing.Size(236, 20);
		this.dtp_birthDate.TabIndex = 64;
		this.cb_simulateSuccess.AutoSize = true;
		this.cb_simulateSuccess.Location = new System.Drawing.Point(314, 53);
		this.cb_simulateSuccess.Name = "cb_simulateSuccess";
		this.cb_simulateSuccess.Size = new System.Drawing.Size(110, 17);
		this.cb_simulateSuccess.TabIndex = 65;
		this.cb_simulateSuccess.Text = "Simulate Success";
		this.cb_simulateSuccess.UseVisualStyleBackColor = true;
		this.cb_simulateSuccess.CheckedChanged += new System.EventHandler(OnSimulateSuccessChanged);
		this.tb_loyaltyTierCounter.Location = new System.Drawing.Point(188, 323);
		this.tb_loyaltyTierCounter.Name = "tb_loyaltyTierCounter";
		this.tb_loyaltyTierCounter.Size = new System.Drawing.Size(236, 20);
		this.tb_loyaltyTierCounter.TabIndex = 66;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(15, 326);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(104, 13);
		this.label2.TabIndex = 67;
		this.label2.Text = "Loyalty Tier Counter:";
		this.tb_pointsExpiring.Location = new System.Drawing.Point(188, 352);
		this.tb_pointsExpiring.Name = "tb_pointsExpiring";
		this.tb_pointsExpiring.Size = new System.Drawing.Size(236, 20);
		this.tb_pointsExpiring.TabIndex = 68;
		this.label10.AutoSize = true;
		this.label10.Location = new System.Drawing.Point(15, 355);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(79, 13);
		this.label10.TabIndex = 69;
		this.label10.Text = "Points Expiring:";
		this.dtp_PointsExpirationDate.Location = new System.Drawing.Point(188, 381);
		this.dtp_PointsExpirationDate.Name = "dtp_PointsExpirationDate";
		this.dtp_PointsExpirationDate.Size = new System.Drawing.Size(236, 20);
		this.dtp_PointsExpirationDate.TabIndex = 70;
		this.label11.AutoSize = true;
		this.label11.Location = new System.Drawing.Point(15, 387);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(114, 13);
		this.label11.TabIndex = 71;
		this.label11.Text = "Points Expiration Date:";
		this.label12.AutoSize = true;
		this.label12.Location = new System.Drawing.Point(15, 413);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(164, 13);
		this.label12.TabIndex = 73;
		this.label12.Text = "Update Account Response Error:";
		this.cbUpdateAccountError.FormattingEnabled = true;
		this.cbUpdateAccountError.Location = new System.Drawing.Point(188, 410);
		this.cbUpdateAccountError.Name = "cbUpdateAccountError";
		this.cbUpdateAccountError.Size = new System.Drawing.Size(236, 21);
		this.cbUpdateAccountError.TabIndex = 72;
		this.cbUpdateAccountError.SelectedIndexChanged += new System.EventHandler(cbUpdateAccountError_SelectedIndexChanged);
		this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.panel1.Controls.Add(this.label14);
		this.panel1.Controls.Add(this.tb_Pin);
		this.panel1.Controls.Add(this.label13);
		this.panel1.Controls.Add(this.cbTextClubOptIn);
		this.panel1.Controls.Add(this.cbPunchCardOptIn);
		this.panel1.Controls.Add(this.btnUpdateAccount);
		this.panel1.Location = new System.Drawing.Point(51, 437);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(409, 68);
		this.panel1.TabIndex = 79;
		this.tb_Pin.Location = new System.Drawing.Point(193, 43);
		this.tb_Pin.Name = "tb_Pin";
		this.tb_Pin.Size = new System.Drawing.Size(55, 20);
		this.tb_Pin.TabIndex = 83;
		this.label13.AutoSize = true;
		this.label13.Location = new System.Drawing.Point(155, 46);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(28, 13);
		this.label13.TabIndex = 82;
		this.label13.Text = "PIN:";
		this.cbTextClubOptIn.AutoSize = true;
		this.cbTextClubOptIn.Location = new System.Drawing.Point(158, 23);
		this.cbTextClubOptIn.Name = "cbTextClubOptIn";
		this.cbTextClubOptIn.Size = new System.Drawing.Size(103, 17);
		this.cbTextClubOptIn.TabIndex = 81;
		this.cbTextClubOptIn.Text = "Text Club Opt In";
		this.cbTextClubOptIn.UseVisualStyleBackColor = true;
		this.cbPunchCardOptIn.AutoSize = true;
		this.cbPunchCardOptIn.Location = new System.Drawing.Point(158, 3);
		this.cbPunchCardOptIn.Name = "cbPunchCardOptIn";
		this.cbPunchCardOptIn.Size = new System.Drawing.Size(110, 17);
		this.cbPunchCardOptIn.TabIndex = 80;
		this.cbPunchCardOptIn.Text = "Punchcard Opt In";
		this.cbPunchCardOptIn.UseVisualStyleBackColor = true;
		this.btnUpdateAccount.Location = new System.Drawing.Point(275, 41);
		this.btnUpdateAccount.Name = "btnUpdateAccount";
		this.btnUpdateAccount.Size = new System.Drawing.Size(117, 23);
		this.btnUpdateAccount.TabIndex = 79;
		this.btnUpdateAccount.Text = "Call UpdateAccount()";
		this.btnUpdateAccount.UseVisualStyleBackColor = true;
		this.btnUpdateAccount.Click += new System.EventHandler(btnUpdateAccount_Click);
		this.label14.AutoSize = true;
		this.label14.Location = new System.Drawing.Point(3, 0);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(123, 13);
		this.label14.TabIndex = 84;
		this.label14.Text = "Temporary Test Harness";
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.panel1);
		base.Controls.Add(this.label12);
		base.Controls.Add(this.cbUpdateAccountError);
		base.Controls.Add(this.label11);
		base.Controls.Add(this.dtp_PointsExpirationDate);
		base.Controls.Add(this.label10);
		base.Controls.Add(this.tb_pointsExpiring);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.tb_loyaltyTierCounter);
		base.Controls.Add(this.cb_simulateSuccess);
		base.Controls.Add(this.dtp_birthDate);
		base.Controls.Add(this.tb_loyaltyTier);
		base.Controls.Add(this.tb_loyaltyPointBal);
		base.Controls.Add(this.tb_postalCode);
		base.Controls.Add(this.tb_phoneNum);
		base.Controls.Add(this.tb_loginEmail);
		base.Controls.Add(this.tb_firstName);
		base.Controls.Add(this.tb_CustProfileNum);
		base.Controls.Add(this.label9);
		base.Controls.Add(this.label8);
		base.Controls.Add(this.label7);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.cb_LiveCall);
		base.Controls.Add(this.lbl_KioskLoginResponseErrors);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.cb_KioskLoginResponseErrors);
		base.Controls.Add(this.m_headerLabel);
		base.Name = "PersonalizationPreferencePage";
		base.Size = new System.Drawing.Size(542, 533);
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
