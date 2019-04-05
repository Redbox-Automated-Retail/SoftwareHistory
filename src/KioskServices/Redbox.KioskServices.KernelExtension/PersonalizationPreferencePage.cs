using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.Rental.Model.Personalization;

namespace Redbox.KioskServices.KernelExtension;

public class PersonalizationPreferencePage : UserControl, IPreferencePageHost
{
	private IContainer components;

	private Label m_headerLabel;

	private ComboBox cb_KioskLoginResponseErrors;

	private Label lbl_KioskLoginResponseErrors;

	private Label label3;

	private Label label5;

	private Label label6;

	private Label label7;

	private Label label8;

	private Label label9;

	private TextBox tb_firstName;

	private TextBox tb_phoneNum;

	private TextBox tb_postalCode;

	private TextBox tb_loyaltyPointBal;

	private TextBox tb_loyaltyTier;

	private DateTimePicker dtp_birthDate;

	private TextBox tb_loyaltyTierCounter;

	private Label label2;

	private TextBox tb_pointsExpiring;

	private Label label10;

	private DateTimePicker dtp_PointsExpirationDate;

	private Label label11;

	private CheckBox cb_promptForPerks;

	private CheckBox cbPromptForEarlyId;

	private CheckBox cbTextClubEnabled;

	private Label label1;

	private TextBox txtRecoTitlesOfferCode;

	private Label label4;

	private TextBox txtRecoTitleIds;

	private Label label12;

	private RadioButton rbGroupIds;

	private RadioButton rbTitleIds;

	public PersonalizationPreferencePage()
	{
		InitializeComponent();
	}

	public void LoadValues(IPreferencePage preferencePage)
	{
		cb_KioskLoginResponseErrors.DataSource = Enum.GetValues(typeof(KioskLoginResponseErrors));
		PersonalizationServicesProxySimulator instance = PersonalizationServicesProxySimulator.Instance;
		if (instance != null)
		{
			cb_KioskLoginResponseErrors.SelectedItem = instance.ResponseCode;
			tb_firstName.Text = instance.FirstName;
			tb_loyaltyPointBal.Text = instance.LoyaltyPointBalance.ToString();
			tb_loyaltyTier.Text = instance.LoyaltyTier;
			tb_phoneNum.Text = instance.PhoneNumber;
			tb_postalCode.Text = instance.PostalCode;
			tb_loyaltyTierCounter.Text = instance.LoyaltyTierCounter.ToString();
			tb_pointsExpiring.Text = instance.PointsExpiring.ToString();
			cbPromptForEarlyId.Checked = instance.PromptForEarlyId;
			cb_promptForPerks.Checked = instance.PromptForPerks;
			cbTextClubEnabled.Checked = instance.TextClubEnabled;
			txtRecoTitlesOfferCode.Text = instance.RecommendedTitlesOfferCode;
			txtRecoTitleIds.Text = instance.RecommendedTitleIds;
			rbGroupIds.Checked = instance.RecommendedTitlesIdType == RecommendedTitlesIdType.ProductGroupID;
			rbTitleIds.Checked = instance.RecommendedTitlesIdType == RecommendedTitlesIdType.TitleID;
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
		instance.FirstName = tb_firstName.Text;
		int result = 0;
		instance.LoyaltyPointBalance = (int.TryParse(tb_loyaltyPointBal.Text, out result) ? result : 0);
		instance.LoyaltyTier = tb_loyaltyTier.Text;
		instance.PhoneNumber = tb_phoneNum.Text;
		instance.PostalCode = tb_postalCode.Text;
		instance.LoyaltyTierCounter = (int.TryParse(tb_loyaltyTierCounter.Text, out var result2) ? result2 : 0);
		instance.PointsExpiring = (int.TryParse(tb_pointsExpiring.Text, out var result3) ? result3 : 0);
		instance.PointsExpirationDate = dtp_PointsExpirationDate.Value;
		instance.PromptForEarlyId = cbPromptForEarlyId.Checked;
		instance.PromptForPerks = cb_promptForPerks.Checked;
		instance.TextClubEnabled = cbTextClubEnabled.Checked;
		instance.RecommendedTitlesOfferCode = txtRecoTitlesOfferCode.Text;
		instance.RecommendedTitleIds = txtRecoTitleIds.Text;
		instance.RecommendedTitlesIdType = (rbTitleIds.Checked ? RecommendedTitlesIdType.TitleID : RecommendedTitlesIdType.ProductGroupID);
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
		this.lbl_KioskLoginResponseErrors = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.label6 = new System.Windows.Forms.Label();
		this.label7 = new System.Windows.Forms.Label();
		this.label8 = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		this.tb_firstName = new System.Windows.Forms.TextBox();
		this.tb_phoneNum = new System.Windows.Forms.TextBox();
		this.tb_postalCode = new System.Windows.Forms.TextBox();
		this.tb_loyaltyPointBal = new System.Windows.Forms.TextBox();
		this.tb_loyaltyTier = new System.Windows.Forms.TextBox();
		this.dtp_birthDate = new System.Windows.Forms.DateTimePicker();
		this.tb_loyaltyTierCounter = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.tb_pointsExpiring = new System.Windows.Forms.TextBox();
		this.label10 = new System.Windows.Forms.Label();
		this.dtp_PointsExpirationDate = new System.Windows.Forms.DateTimePicker();
		this.label11 = new System.Windows.Forms.Label();
		this.cb_promptForPerks = new System.Windows.Forms.CheckBox();
		this.cbPromptForEarlyId = new System.Windows.Forms.CheckBox();
		this.cbTextClubEnabled = new System.Windows.Forms.CheckBox();
		this.label1 = new System.Windows.Forms.Label();
		this.txtRecoTitlesOfferCode = new System.Windows.Forms.TextBox();
		this.label4 = new System.Windows.Forms.Label();
		this.txtRecoTitleIds = new System.Windows.Forms.TextBox();
		this.label12 = new System.Windows.Forms.Label();
		this.rbGroupIds = new System.Windows.Forms.RadioButton();
		this.rbTitleIds = new System.Windows.Forms.RadioButton();
		base.SuspendLayout();
		this.m_headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
		this.m_headerLabel.BackColor = System.Drawing.Color.DarkCyan;
		this.m_headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.m_headerLabel.ForeColor = System.Drawing.Color.White;
		this.m_headerLabel.Location = new System.Drawing.Point(0, 0);
		this.m_headerLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.m_headerLabel.Name = "m_headerLabel";
		this.m_headerLabel.Size = new System.Drawing.Size(723, 47);
		this.m_headerLabel.TabIndex = 42;
		this.m_headerLabel.Text = "Personalization Services: Login";
		this.m_headerLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
		this.cb_KioskLoginResponseErrors.FormattingEnabled = true;
		this.cb_KioskLoginResponseErrors.Location = new System.Drawing.Point(251, 94);
		this.cb_KioskLoginResponseErrors.Margin = new System.Windows.Forms.Padding(4);
		this.cb_KioskLoginResponseErrors.Name = "cb_KioskLoginResponseErrors";
		this.cb_KioskLoginResponseErrors.Size = new System.Drawing.Size(313, 24);
		this.cb_KioskLoginResponseErrors.TabIndex = 43;
		this.cb_KioskLoginResponseErrors.SelectionChangeCommitted += new System.EventHandler(comboBox1_SelectedIndexChanged);
		this.lbl_KioskLoginResponseErrors.AutoSize = true;
		this.lbl_KioskLoginResponseErrors.Location = new System.Drawing.Point(20, 97);
		this.lbl_KioskLoginResponseErrors.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.lbl_KioskLoginResponseErrors.Name = "lbl_KioskLoginResponseErrors";
		this.lbl_KioskLoginResponseErrors.Size = new System.Drawing.Size(196, 17);
		this.lbl_KioskLoginResponseErrors.TabIndex = 45;
		this.lbl_KioskLoginResponseErrors.Text = "Kiosk Login Response Errors:";
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(20, 139);
		this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(80, 17);
		this.label3.TabIndex = 48;
		this.label3.Text = "First Name:";
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(20, 174);
		this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(107, 17);
		this.label5.TabIndex = 50;
		this.label5.Text = "Phone Number:";
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(20, 208);
		this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(88, 17);
		this.label6.TabIndex = 51;
		this.label6.Text = "Postal Code:";
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(20, 246);
		this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(75, 17);
		this.label7.TabIndex = 52;
		this.label7.Text = "Birth Date:";
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(20, 278);
		this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(148, 17);
		this.label8.TabIndex = 53;
		this.label8.Text = "Loyalty Point Balance:";
		this.label9.AutoSize = true;
		this.label9.Location = new System.Drawing.Point(20, 308);
		this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(86, 17);
		this.label9.TabIndex = 54;
		this.label9.Text = "Loyalty Tier:";
		this.tb_firstName.Location = new System.Drawing.Point(251, 136);
		this.tb_firstName.Margin = new System.Windows.Forms.Padding(4);
		this.tb_firstName.Name = "tb_firstName";
		this.tb_firstName.Size = new System.Drawing.Size(313, 22);
		this.tb_firstName.TabIndex = 57;
		this.tb_phoneNum.Location = new System.Drawing.Point(251, 170);
		this.tb_phoneNum.Margin = new System.Windows.Forms.Padding(4);
		this.tb_phoneNum.Name = "tb_phoneNum";
		this.tb_phoneNum.Size = new System.Drawing.Size(313, 22);
		this.tb_phoneNum.TabIndex = 59;
		this.tb_postalCode.Location = new System.Drawing.Point(251, 205);
		this.tb_postalCode.Margin = new System.Windows.Forms.Padding(4);
		this.tb_postalCode.Name = "tb_postalCode";
		this.tb_postalCode.Size = new System.Drawing.Size(313, 22);
		this.tb_postalCode.TabIndex = 60;
		this.tb_loyaltyPointBal.Location = new System.Drawing.Point(251, 275);
		this.tb_loyaltyPointBal.Margin = new System.Windows.Forms.Padding(4);
		this.tb_loyaltyPointBal.Name = "tb_loyaltyPointBal";
		this.tb_loyaltyPointBal.Size = new System.Drawing.Size(313, 22);
		this.tb_loyaltyPointBal.TabIndex = 62;
		this.tb_loyaltyTier.Location = new System.Drawing.Point(251, 304);
		this.tb_loyaltyTier.Margin = new System.Windows.Forms.Padding(4);
		this.tb_loyaltyTier.Name = "tb_loyaltyTier";
		this.tb_loyaltyTier.Size = new System.Drawing.Size(313, 22);
		this.tb_loyaltyTier.TabIndex = 63;
		this.dtp_birthDate.Location = new System.Drawing.Point(251, 239);
		this.dtp_birthDate.Margin = new System.Windows.Forms.Padding(4);
		this.dtp_birthDate.Name = "dtp_birthDate";
		this.dtp_birthDate.Size = new System.Drawing.Size(313, 22);
		this.dtp_birthDate.TabIndex = 64;
		this.tb_loyaltyTierCounter.Location = new System.Drawing.Point(251, 336);
		this.tb_loyaltyTierCounter.Margin = new System.Windows.Forms.Padding(4);
		this.tb_loyaltyTierCounter.Name = "tb_loyaltyTierCounter";
		this.tb_loyaltyTierCounter.Size = new System.Drawing.Size(313, 22);
		this.tb_loyaltyTierCounter.TabIndex = 66;
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(20, 340);
		this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(140, 17);
		this.label2.TabIndex = 67;
		this.label2.Text = "Loyalty Tier Counter:";
		this.tb_pointsExpiring.Location = new System.Drawing.Point(251, 372);
		this.tb_pointsExpiring.Margin = new System.Windows.Forms.Padding(4);
		this.tb_pointsExpiring.Name = "tb_pointsExpiring";
		this.tb_pointsExpiring.Size = new System.Drawing.Size(313, 22);
		this.tb_pointsExpiring.TabIndex = 68;
		this.label10.AutoSize = true;
		this.label10.Location = new System.Drawing.Point(20, 376);
		this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(105, 17);
		this.label10.TabIndex = 69;
		this.label10.Text = "Points Expiring:";
		this.dtp_PointsExpirationDate.Location = new System.Drawing.Point(251, 408);
		this.dtp_PointsExpirationDate.Margin = new System.Windows.Forms.Padding(4);
		this.dtp_PointsExpirationDate.Name = "dtp_PointsExpirationDate";
		this.dtp_PointsExpirationDate.Size = new System.Drawing.Size(313, 22);
		this.dtp_PointsExpirationDate.TabIndex = 70;
		this.label11.AutoSize = true;
		this.label11.Location = new System.Drawing.Point(20, 415);
		this.label11.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(151, 17);
		this.label11.TabIndex = 71;
		this.label11.Text = "Points Expiration Date:";
		this.cb_promptForPerks.AutoSize = true;
		this.cb_promptForPerks.Location = new System.Drawing.Point(169, 456);
		this.cb_promptForPerks.Margin = new System.Windows.Forms.Padding(4);
		this.cb_promptForPerks.Name = "cb_promptForPerks";
		this.cb_promptForPerks.Size = new System.Drawing.Size(140, 21);
		this.cb_promptForPerks.TabIndex = 72;
		this.cb_promptForPerks.Text = "Prompt For Perks";
		this.cb_promptForPerks.UseVisualStyleBackColor = true;
		this.cbPromptForEarlyId.AutoSize = true;
		this.cbPromptForEarlyId.Location = new System.Drawing.Point(339, 456);
		this.cbPromptForEarlyId.Margin = new System.Windows.Forms.Padding(4);
		this.cbPromptForEarlyId.Name = "cbPromptForEarlyId";
		this.cbPromptForEarlyId.Size = new System.Drawing.Size(153, 21);
		this.cbPromptForEarlyId.TabIndex = 73;
		this.cbPromptForEarlyId.Text = "Prompt For Early ID";
		this.cbPromptForEarlyId.UseVisualStyleBackColor = true;
		this.cbTextClubEnabled.AutoSize = true;
		this.cbTextClubEnabled.Location = new System.Drawing.Point(516, 456);
		this.cbTextClubEnabled.Margin = new System.Windows.Forms.Padding(4);
		this.cbTextClubEnabled.Name = "cbTextClubEnabled";
		this.cbTextClubEnabled.Size = new System.Drawing.Size(145, 21);
		this.cbTextClubEnabled.TabIndex = 74;
		this.cbTextClubEnabled.Text = "Text Club Enabled";
		this.cbTextClubEnabled.UseVisualStyleBackColor = true;
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(20, 507);
		this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(218, 17);
		this.label1.TabIndex = 76;
		this.label1.Text = "Recommended Titles Offer Code:";
		this.txtRecoTitlesOfferCode.Location = new System.Drawing.Point(251, 503);
		this.txtRecoTitlesOfferCode.Margin = new System.Windows.Forms.Padding(4);
		this.txtRecoTitlesOfferCode.Name = "txtRecoTitlesOfferCode";
		this.txtRecoTitlesOfferCode.Size = new System.Drawing.Size(313, 22);
		this.txtRecoTitlesOfferCode.TabIndex = 75;
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(20, 541);
		this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(204, 17);
		this.label4.TabIndex = 78;
		this.label4.Text = "Recommended Title/Group Ids:";
		this.txtRecoTitleIds.Location = new System.Drawing.Point(251, 537);
		this.txtRecoTitleIds.Margin = new System.Windows.Forms.Padding(4);
		this.txtRecoTitleIds.Name = "txtRecoTitleIds";
		this.txtRecoTitleIds.Size = new System.Drawing.Size(313, 22);
		this.txtRecoTitleIds.TabIndex = 77;
		this.label12.AutoSize = true;
		this.label12.Location = new System.Drawing.Point(572, 541);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(146, 17);
		this.label12.TabIndex = 79;
		this.label12.Text = "(Comma delimited list)";
		this.rbGroupIds.AutoSize = true;
		this.rbGroupIds.Location = new System.Drawing.Point(251, 566);
		this.rbGroupIds.Name = "rbGroupIds";
		this.rbGroupIds.Size = new System.Drawing.Size(144, 21);
		this.rbGroupIds.TabIndex = 82;
		this.rbGroupIds.TabStop = true;
		this.rbGroupIds.Text = "Product Group Ids";
		this.rbGroupIds.UseVisualStyleBackColor = true;
		this.rbTitleIds.AutoSize = true;
		this.rbTitleIds.Location = new System.Drawing.Point(401, 566);
		this.rbTitleIds.Name = "rbTitleIds";
		this.rbTitleIds.Size = new System.Drawing.Size(78, 21);
		this.rbTitleIds.TabIndex = 83;
		this.rbTitleIds.TabStop = true;
		this.rbTitleIds.Text = "Title Ids";
		this.rbTitleIds.UseVisualStyleBackColor = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(8f, 16f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.rbTitleIds);
		base.Controls.Add(this.rbGroupIds);
		base.Controls.Add(this.label12);
		base.Controls.Add(this.label4);
		base.Controls.Add(this.txtRecoTitleIds);
		base.Controls.Add(this.label1);
		base.Controls.Add(this.txtRecoTitlesOfferCode);
		base.Controls.Add(this.cbTextClubEnabled);
		base.Controls.Add(this.cbPromptForEarlyId);
		base.Controls.Add(this.cb_promptForPerks);
		base.Controls.Add(this.label11);
		base.Controls.Add(this.dtp_PointsExpirationDate);
		base.Controls.Add(this.label10);
		base.Controls.Add(this.tb_pointsExpiring);
		base.Controls.Add(this.label2);
		base.Controls.Add(this.tb_loyaltyTierCounter);
		base.Controls.Add(this.dtp_birthDate);
		base.Controls.Add(this.tb_loyaltyTier);
		base.Controls.Add(this.tb_loyaltyPointBal);
		base.Controls.Add(this.tb_postalCode);
		base.Controls.Add(this.tb_phoneNum);
		base.Controls.Add(this.tb_firstName);
		base.Controls.Add(this.label9);
		base.Controls.Add(this.label8);
		base.Controls.Add(this.label7);
		base.Controls.Add(this.label6);
		base.Controls.Add(this.label5);
		base.Controls.Add(this.label3);
		base.Controls.Add(this.lbl_KioskLoginResponseErrors);
		base.Controls.Add(this.cb_KioskLoginResponseErrors);
		base.Controls.Add(this.m_headerLabel);
		base.Margin = new System.Windows.Forms.Padding(4);
		base.Name = "PersonalizationPreferencePage";
		base.Size = new System.Drawing.Size(723, 656);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
